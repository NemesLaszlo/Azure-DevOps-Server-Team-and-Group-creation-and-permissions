using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using Team_and_Group_Assist.Connection_Adapter;
using Team_and_Group_Assist.Domain;
using Team_and_Group_Assist.Permission_Handler;

namespace Team_and_Group_Assist.Team_and_Group_Handler
{
    public class GroupManager
    {
        private Logger.Logger _Logger;
        private ConnectionAdapter _connectionAdapter;

        private Dictionary<string, int> _groupSpecificPermissions = Permissions._projectPermissionAllow;

        // Only some group needs this only one Tagging permission (Create tag definition) - White list for this groups
        private ICollection<string> _tagDefinitionPermissionGranted = Permissions._taggingPermissionGroups;
        private int _groupCreateTagDefinitionPermission = Permissions._taggingPermissionAllow;

        public GroupManager(ConnectionAdapter connectionAdapter, Logger.Logger Logger)
        {
            _connectionAdapter = connectionAdapter;
            _Logger = Logger;
        }

        /// <summary>
        /// Create groups on the Server by the Excel data
        /// Then set up the permissions and members (members data comes from the Excel too)
        /// </summary>
        /// <param name="groupList">Collection of the groups</param>
        public void CreateGroupAndSetUp(ICollection<Group> groupList)
        {
            foreach (Group group in groupList)
            {
                try
                {
                    // Create a group on the Server
                    _connectionAdapter.groupSecurityService.CreateApplicationGroup(_connectionAdapter.projectInfo.Uri, group._groupName, "Created by a Tool");

                    _Logger.Info("Grpup: " + group._groupName + " creation was successful");
                    _Logger.Flush();

                    // Set the group permissions
                    SetGroupPermissions(group._groupName);

                    // Add the AD groups to the group
                    if (group._adGroups.Count != 0)
                    {
                        AddADGroups(group._groupName, group._adGroups);
                    }

                }
                catch (Exception ex)
                {
                    _Logger.Error("Problem: " + ex.Message);
                    _Logger.Flush();
                }        
            }
        }

        /// <summary>
        /// Set the defined permissions to the group
        /// </summary>
        /// <param name="groupName">Name of the group</param>
        private void SetGroupPermissions(string groupName)
        {
            try
            {
                // Get the group identity
                Identity SIDS = _connectionAdapter.groupSecurityService.ReadIdentity(SearchFactor.AccountName, groupName, QueryMembership.Expanded);
                IdentityDescriptor id = new IdentityDescriptor("Microsoft.TeamFoundation.Identity", SIDS.Sid);

                // Unofficial (there is no official docs) security token format table: https://roadtoalm.com/2014/07/28/add-permissions-with-tfssecuritythe-ultimate-reference/
                // Token for the permission settings - 'Projects'
                string securityTokenProject = "$PROJECT:" + _connectionAdapter.projectInfo.Uri;
                // TeamProjectGuid for the 'Tagging' token
                string TeamProjectGuid = _connectionAdapter.projectInfo.Uri.ToString().Split('/').Last();
                // Token for the permission settings - 'Tagging'
                string securityTokenTagging = "/" + TeamProjectGuid;

                // Group name sufix tag
                string groupNameTag = groupName.Split('-')[1];
                // Get the current team permissions by the name sufix
                int _currentGroupPermissions = _groupSpecificPermissions[groupNameTag];

                // Check the this specific group needs tagging permission ( "Create tag definition" )
                bool getTaggingPermission = false;
                if (_tagDefinitionPermissionGranted.Contains(groupNameTag))
                {
                    getTaggingPermission = true;
                }

                foreach (SecurityNamespace sn in _connectionAdapter.securityList)
                {

                    // Check the security namespace to set the corrent permissions from the corrent Display
                    if (sn.Description.DisplayName == "Project")
                    {
                        sn.SetPermissions(securityTokenProject, id, _currentGroupPermissions, 0, true);
                        _Logger.Info("Group: " + groupName + " Project permissions added");
                        _Logger.Flush();
                    }
                    if (sn.Description.DisplayName == "Tagging" && getTaggingPermission)
                    {
                        sn.SetPermissions(securityTokenTagging, id, _groupCreateTagDefinitionPermission, 0, true);
                        _Logger.Info("Group: " + groupName + " Tagging permissions added");
                        _Logger.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("Problem: " + ex.Message);
                _Logger.Flush();
            }
        }

        /// <summary>
        /// Add the AD group(s) to the group
        /// </summary>
        /// <param name="groupName">Add AD groups to this group</param>
        /// <param name="adGroups">AD group collection</param>
        private void AddADGroups(string groupName, ICollection<string> adGroups)
        {
            foreach (string adGroup in adGroups)
            {
                try
                {
                    // Group identity
                    TeamFoundationIdentity tfsGroupIdentity = _connectionAdapter.identityManagementService.ReadIdentity(IdentitySearchFactor.AccountName,
                                                                groupName,
                                                                MembershipQuery.None,
                                                                ReadIdentityOptions.IncludeReadFromSource);

                    // AD group indetity
                    TeamFoundationIdentity userIdentity = _connectionAdapter.identityManagementService.ReadIdentity(IdentitySearchFactor.AccountName,
                                                            adGroup,
                                                            MembershipQuery.None,
                                                            ReadIdentityOptions.IncludeReadFromSource);

                    // Add the AD group to the group
                    _connectionAdapter.identityManagementService.AddMemberToApplicationGroup(tfsGroupIdentity.Descriptor, userIdentity.Descriptor);

                    _Logger.Info("AD Group: " + adGroup + " added successfully to group: " + groupName);
                    _Logger.Flush();
                }
                catch (Exception ex)
                {
                    _Logger.Error("Problem with the AD group identity, AD group: " +  adGroup + " Error: " + ex.Message);
                    _Logger.Flush();
                }
            }
        }
    }
}

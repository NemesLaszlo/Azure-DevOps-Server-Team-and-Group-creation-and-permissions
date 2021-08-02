using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using Team_and_Group_Assist.Connection_Adapter;
using Team_and_Group_Assist.Domain;

namespace Team_and_Group_Assist.Team_and_Group_Handler
{
    public class TeamManager
    {
        private Logger.Logger _Logger;
        private ConnectionAdapter _connectionAdapter;

        public TeamManager(ConnectionAdapter connectionAdapter, Logger.Logger Logger)
        {
            _connectionAdapter = connectionAdapter;
            _Logger = Logger;
        }

        /// <summary>
        /// Create a team on the Server by the name which comes from the Excel
        /// Then set up the administrators and members
        /// </summary>
        /// <param name="teamName">Name to create a team</param>
        /// <param name="groupList">Groups to set them members of the team and pick the TC group as administrator</param>
        public void CreateTeamAndSetUp(string teamName, ICollection<Group> groupList)
        {
            try
            {
                // Get the Team Configurator group name - this going to be a Administrator of the Team
                string administratorGroupName = groupList.Where(group => group._groupName.Contains("TC")).Select(group => group._groupName).FirstOrDefault();

                // Create a team
                TeamFoundationTeam newTeam = _connectionAdapter.teamService.CreateTeam(_connectionAdapter.projectInfo.Uri, teamName, "This Team created by a Tool", null);
                _Logger.Info("Team: " + newTeam.Name + " creation was successful");
                _Logger.Flush();

                // Set the Team Administrators
                SetTeamAdministrators(newTeam, administratorGroupName);

                // Set team member Groups
                SetTeamMembers(newTeam.Name, groupList);
            }
            catch (Exception ex)
            {
                _Logger.Error("Problem: " + ex.Message);
                _Logger.Flush();
            }

        }

        /// <summary>
        /// Set the Team Administrators
        /// </summary>
        /// <param name="team">Team to set administrators</param>
        /// <param name="administratorGroupName">Set this group to administrator of the team</param>
        private void SetTeamAdministrators(TeamFoundationTeam team, string administratorGroupName)
        {
            try
            {
                // Current logged in user Identity to delete from Administrators
                TeamFoundationIdentity userIdentity = _connectionAdapter.identityManagementService.ReadIdentity(IdentitySearchFactor.AccountName,
                                                            _connectionAdapter.loggedInUser,
                                                            MembershipQuery.None,
                                                            ReadIdentityOptions.IncludeReadFromSource);

                // Get the Group identity - this group going to get a Team Administrator role 
                Identity SIDS = _connectionAdapter.groupSecurityService.ReadIdentity(SearchFactor.AccountName, administratorGroupName, QueryMembership.Expanded);
                IdentityDescriptor teamConfiguratorTeam = new IdentityDescriptor("Microsoft.TeamFoundation.Identity", SIDS.Sid);

                // Get a security token
                string token = IdentityHelper.CreateSecurityToken(team.Identity);
                // Set the permission - this group gets admin permission on this team
                _connectionAdapter.securityNamespace.SetPermissions(token, teamConfiguratorTeam, 15, 0, true);
                // Remove the current logged in user from the Administrators (needs to create the Team)
                _connectionAdapter.securityNamespace.RemovePermissions(token, userIdentity.Descriptor, 15);
                _Logger.Info("Team: " + team.Name + " got a new Administrator: " + administratorGroupName);
                _Logger.Info("Team: " + team.Name + " logged in user removed from Administrators: " + userIdentity.DisplayName);
                _Logger.Flush();
            }
            catch (Exception ex)
            {
                _Logger.Error("Problem: Administrator group Sufix was not correct, check this first! Extended error message: " + ex.Message);
                _Logger.Flush();
            }
        }

        /// <summary>
        /// Add groups to the selected team as members
        /// </summary>
        /// <param name="teamName">Add members to this team</param>
        /// <param name="groups">Groups to add to the team</param>
        private void SetTeamMembers(string teamName, ICollection<Group> groups)
        {
            foreach (Group group in groups)
            {
                try
                {
                    // Team identity
                    TeamFoundationIdentity tfsTeamIdentity = _connectionAdapter.identityManagementService.ReadIdentity(IdentitySearchFactor.AccountName,
                                                                teamName,
                                                                MembershipQuery.None,
                                                                ReadIdentityOptions.IncludeReadFromSource);

                    // Group indetity
                    TeamFoundationIdentity groupIdentity = _connectionAdapter.identityManagementService.ReadIdentity(IdentitySearchFactor.AccountName,
                                                            group._groupName,
                                                            MembershipQuery.None,
                                                            ReadIdentityOptions.IncludeReadFromSource);

                    // Add the Group to the team
                    _connectionAdapter.identityManagementService.AddMemberToApplicationGroup(tfsTeamIdentity.Descriptor, groupIdentity.Descriptor);

                    _Logger.Info("Group: " + group._groupName + " added successfully to team: " + teamName);
                    _Logger.Flush();
                }
                catch (Exception ex)
                {
                    _Logger.Error("Problem: " + ex.Message);
                    _Logger.Flush();
                }      
            }
        }
    }
}

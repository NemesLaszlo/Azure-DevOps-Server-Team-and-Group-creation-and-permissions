using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Framework.Client;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Team_and_Group_Assist.Connection_Adapter
{
    public class ConnectionAdapter
    {
        public IGroupSecurityService groupSecurityService { get; private set; }
        public IIdentityManagementService identityManagementService { get; private set; }
        public TfsTeamService teamService { get; private set; }
        public TeamSettingsConfigurationService teamSettingsConfigurationService { get; private set; }
        public SecurityNamespace securityNamespace { get; private set; }
        public List<SecurityNamespace> securityList { get; private set; }
        public ProjectInfo projectInfo { get; private set; }
        public string loggedInUser { get; private set; }

        private Uri ConnUri;
        private TfsTeamProjectCollection Tpc;
        private Logger.Logger _Logger;

        /// <summary>
        ///  Connection constructor
        /// </summary>
        /// <param name="AzureConnection">The server URL for the connection</param>
        /// <param name="TeamProjectName">The Project name on the server - for the operations</param>
        /// <param name="log">Logger</param>
        public ConnectionAdapter(string AzureConnection, string TeamProjectName, Logger.Logger Logger)
        {
            _Logger = Logger;
            Connection(AzureConnection, TeamProjectName);
        }

        /// <summary>
        /// Connect to the Azure DevOps Server
        /// </summary>
        /// <param name="AzureConnection">The server URL for the connection</param>
        /// <param name="TeamProjectName">The Project name on the server - for the operations</param>
        private void Connection(string AzureConnection, string TeamProjectName)
        {
            try
            {
                if (!(string.IsNullOrEmpty(AzureConnection) || string.IsNullOrEmpty(TeamProjectName)))
                {
                    ConnUri = new Uri(AzureConnection);
                    Tpc = new TfsTeamProjectCollection(ConnUri);

                    ICommonStructureService icss = Tpc.GetService<ICommonStructureService>();
                    projectInfo = icss.GetProjectFromName(TeamProjectName);

                    // Get the group security service
                    groupSecurityService = Tpc.GetService<IGroupSecurityService>();

                    // Get the identity management service
                    identityManagementService = Tpc.GetService<IIdentityManagementService>();

                    // Get the team service
                    teamService = Tpc.GetService<TfsTeamService>();
                    teamSettingsConfigurationService = Tpc.GetService<TeamSettingsConfigurationService>();

                    // Get the Security Namespaces
                    ISecurityService securityService = Tpc.GetService<ISecurityService>();
                    securityNamespace = securityService.GetSecurityNamespace(FrameworkSecurity.IdentitiesNamespaceId);
                    ReadOnlyCollection<SecurityNamespace> securityNamespaces = securityService.GetSecurityNamespaces();
                    securityList = securityNamespaces.ToList<SecurityNamespace>();

                    // Version Control Server to get the current logged in user
                    VersionControlServer versionControl = Tpc.GetService<VersionControlServer>();
                    loggedInUser = versionControl.AuthorizedUser;

                    string message = string.Format("Connection was successful to {0} and the ProjectName is {1}", AzureConnection, TeamProjectName);
                    _Logger.Info(message);
                    _Logger.Flush();
                }
                else
                {
                    _Logger.Error("Connection problem, check the connection data!");
                    _Logger.Flush();
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("Problem: " + ex.Message);
                _Logger.Flush();
                Environment.Exit(-1);
            }
        }
    }
}

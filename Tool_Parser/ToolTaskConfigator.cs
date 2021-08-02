using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using Team_and_Group_Assist.Domain;
using Team_and_Group_Assist.Excel_Process;
using Team_and_Group_Assist.Connection_Adapter;
using Team_and_Group_Assist.Team_and_Group_Handler;
using System.Diagnostics;

namespace Team_and_Group_Assist.Tool_Parser
{
    public static class ToolTaskConfigator
    {
        // Connection informations to the Azure DevOps Server - Url with the collection and TeamProjectName
        private static Dictionary<string, string> _connectionData = new Dictionary<string, string>();
        // Excel file path
        private static string _groupInformationsExcelPath = string.Empty;
        // Team name from the excel file for the Team creation
        private static string _teamName = string.Empty;

        /// <summary>
        /// Custom Logging Initializer
        /// </summary>
        /// <returns>Costom Logger class</returns>
        private static Logger.Logger LoggerInit()
        {
            string initializeData = string.Empty;
            ConfigurationSection diagnosticsSection = (ConfigurationSection)ConfigurationManager.GetSection("system.diagnostics");
            ConfigurationElement traceSection = diagnosticsSection.ElementInformation.Properties["trace"].Value as ConfigurationElement;
            ConfigurationElementCollection listeners = traceSection.ElementInformation.Properties["listeners"].Value as ConfigurationElementCollection;
            foreach (ConfigurationElement listener in listeners)
            {

                initializeData = listener.ElementInformation.Properties["initializeData"].Value.ToString();

            }

            // Check the path with the .log extension
            if (initializeData.Contains(".log"))
            {
                Logger.Logger log = new Logger.Logger(initializeData);
                return log;
            }
            return null;
        }

        /// <summary>
        /// Config Connection to the Azure DevOps Server and Set the excel path to the Group informations
        /// </summary>
        private static void ConfigDataInit()
        {
            // Connection Initializer
            NameValueCollection ConnectionInformations = ConfigurationManager.GetSection("Connection") as NameValueCollection;
            foreach (string info in ConnectionInformations.AllKeys)
            {
                Console.WriteLine(info + ": " + ConnectionInformations[info]);
                _connectionData.Add(info, ConnectionInformations[info]);
            }

            // Group informations from excel - Path to the Excel file
            NameValueCollection WorkItemListsInformations = ConfigurationManager.GetSection("TaskInfo") as NameValueCollection;
            foreach (string info in WorkItemListsInformations.AllKeys)
            {
                Console.WriteLine(info + ": " + WorkItemListsInformations[info]);
                _groupInformationsExcelPath = WorkItemListsInformations[info];
            }
        }

        /// <summary>
        /// Build a DataSet from the Excel
        /// Get the Team Name and build a list from the groups from the excel dataset
        /// </summary>
        /// <param name="filePath">Path to the excel file</param>
        /// <returns>List of Groups or null</returns>
        private static List<Group> BuildGroupListFromExcelDataSet(string filePath)
        {
            // Result Group list
            List<Group> GroupList = new List<Group>();

            // Dataset with Datatables from the Excel file
            DataSet dataset = ExcelDataSetHandler.DatasetImportFromExcel(filePath);

            foreach (DataRow row in dataset.Tables["Sheet1"].Rows)
            {
                object[] rowData = row.ItemArray;

                // Set the Team Name - it is only in the first cell of the first row
                if (string.IsNullOrEmpty(_teamName))
                {
                    _teamName = rowData[0].ToString();
                }

                // Parse the group informations
                string teamName = _teamName;
                string roleSufix = rowData[2].ToString();
                string[] adGroups = Array.Empty<string>();
                if (!string.IsNullOrEmpty(rowData[3].ToString()))
                {
                    adGroups = rowData[3].ToString().Split(',');
                }

                // Create a group and add it to the list
                if (!string.IsNullOrEmpty(teamName) && !string.IsNullOrEmpty(roleSufix))
                {
                    GroupList.Add(new Group(teamName, roleSufix, adGroups));
                }
            }

            if (GroupList.Count != 0)
            {
                return GroupList;
            }
            return null;
        }

        /// <summary>
        /// Execute the tool
        /// </summary>
        public static void ConsoleRun()
        {
            ConfigDataInit();

            Logger.Logger Logger = LoggerInit();
            // Exit and information message about the log path problem
            if (Logger == null)
            {
                Console.WriteLine("Wrong 'log path' please add the full path of the space to save the log files, " +
                    "with the desired name of this file with a '.log' extension!");
                return;
            }

            // Build a Group List with configured Names
            Console.WriteLine("Excel process starts ...");
            List<Group> groupList = BuildGroupListFromExcelDataSet(_groupInformationsExcelPath);
            if (groupList == null)
            {
                Console.WriteLine("Excel Process problem, please check the excel file!");
                return;
            }
            Console.WriteLine("Excel process done");

            Console.WriteLine("Team and Groups creation and setup starts ...");
            //Connect to the server
            ConnectionAdapter ConnectionAdapter = new ConnectionAdapter(_connectionData["AzureConnection"], _connectionData["TeamProjectName"], Logger);
            Console.WriteLine("Team and Groups creation and setup processing ...");
            Stopwatch watch = Stopwatch.StartNew();
            // Create and set up groups (create, permissions, members)
            GroupManager groupManager = new GroupManager(ConnectionAdapter, Logger);
            groupManager.CreateGroupAndSetUp(groupList);
            // Create and set up the team (create, administrators, members)
            TeamManager teamManager = new TeamManager(ConnectionAdapter, Logger);
            teamManager.CreateTeamAndSetUp(_teamName, groupList);
            watch.Stop();
            TimeSpan timeSpan = watch.Elapsed;
            Console.WriteLine($"Total processing time: {Math.Floor(timeSpan.TotalMinutes)}:{timeSpan:ss\\.ff}");
            Console.WriteLine("Done, press any key to exit");
        }
    }
}

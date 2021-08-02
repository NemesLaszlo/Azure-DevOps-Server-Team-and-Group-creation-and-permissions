using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team_and_Group_Assist.Permission_Handler
{
    public static class Permissions
    {
        // Permissions with thei Bit values to set it up
        private static Dictionary<string, int> _projectDisplaynamePermissions = new Dictionary<string, int>(){
            {"View project-level information", 1},
            {"Edit project-level information", 2},
            {"Delete team project", 4},
            {"Create test runs", 8},
            {"Administer a build", 16},
            {"Start a build", 32},
            {"Edit build quality", 64},
            {"Write to build operational store", 128},
            {"Delete test runs", 256},
            {"View test runs", 512},
            {"Manage test environments", 2048},
            {"Manage test configurations", 4096},
            {"Delete and restore work items", 8192},
            {"Move work items out of this project", 16384},
            {"Permanently delete work items", 32768},
            {"Rename team project", 65536},
            {"Manage project properties", 131072},
            {"Manage system project properties ", 262144},
            {"Bypass project property cache", 524288},
            {"Bypass rules on work item updates", 1048576},
            {"Suppress notifications for work item updates", 2097152},
            {"Update project visibility", 4194304}
        };

        private static Dictionary<string, int> _taggingDisplaynamePermissions = new Dictionary<string, int>(){
            {"Enumerate tag definitions", 1},
            {"Create tag definition", 2},
            {"Update tag definition", 4},
            {"Delete tag definition ", 8}
        };

        // Team Configurator -----------------------------------------------------------------------------------------------------------
        private static int TC_ProjectPermissionAllow = (_projectDisplaynamePermissions["Create test runs"] |
                                                       _projectDisplaynamePermissions["Delete and restore work items"] |
                                                       _projectDisplaynamePermissions["Delete test runs"] |
                                                       _projectDisplaynamePermissions["Manage test configurations"] |
                                                       _projectDisplaynamePermissions["Manage test environments"] |
                                                       _projectDisplaynamePermissions["Move work items out of this project"] |
                                                       _projectDisplaynamePermissions["View project-level information"]);

        // Team Branchmaster -----------------------------------------------------------------------------------------------------------
        private static int TBrM_ProjectPermissionAllow = (_projectDisplaynamePermissions["View project-level information"]);

        // Team Buildmaster  -----------------------------------------------------------------------------------------------------------
        private static int TBM_ProjectPermissionAllow = (_projectDisplaynamePermissions["View project-level information"]);

        // Team Writer -----------------------------------------------------------------------------------------------------------
        private static int TW_ProjectPermissionAllow = (_projectDisplaynamePermissions["Create test runs"] |
                                                       _projectDisplaynamePermissions["Delete test runs"] |
                                                       _projectDisplaynamePermissions["Move work items out of this project"] |
                                                       _projectDisplaynamePermissions["View project-level information"] |
                                                       _projectDisplaynamePermissions["View test runs"]);

        // Team Reader -----------------------------------------------------------------------------------------------------------
        private static int TR_ProjectPermissionAllow = (_projectDisplaynamePermissions["View project-level information"] |
                                                       _projectDisplaynamePermissions["View test runs"]);

        // Team WIT Configurator -----------------------------------------------------------------------------------------------------------
        private static int TWC_ProjectPermissionAllow = (_projectDisplaynamePermissions["Move work items out of this project"] |
                                                        _projectDisplaynamePermissions["View project-level information"]);

        // Team WIT Writer -----------------------------------------------------------------------------------------------------------
        private static int TWW_ProjectPermissionAllow = (_projectDisplaynamePermissions["Move work items out of this project"] |
                                                        _projectDisplaynamePermissions["View project-level information"]);

        // Team WIT Reader -----------------------------------------------------------------------------------------------------------
        private static int TWR_ProjectPermissionAllow = (_projectDisplaynamePermissions["View project-level information"]);

        // Team WIT Test Manager -----------------------------------------------------------------------------------------------------------
        private static int TWTM_ProjectPermissionAllow = (_projectDisplaynamePermissions["Create test runs"] |
                                                         _projectDisplaynamePermissions["Delete test runs"] |
                                                         _projectDisplaynamePermissions["View project-level information"] |
                                                         _projectDisplaynamePermissions["View test runs"]);

        // Tagging permission "While list" - these groups receive this permission
        public static ICollection<string> _taggingPermissionGroups = new List<string>
        {
            "TC", "TW", "TWC", "TWW", "TWTM"
        };

        // Tagging DisplayName Permission - "Create tag Definition"
        public static int _taggingPermissionAllow = (_taggingDisplaynamePermissions["Create tag definition"]);

        // Collection of the Project DisplayName Permissions
        public static Dictionary<string, int> _projectPermissionAllow = new Dictionary<string, int>
        {
            {"TC", TC_ProjectPermissionAllow},
            {"TBrM", TBrM_ProjectPermissionAllow},
            {"TBM",TBM_ProjectPermissionAllow},
            {"TW", TW_ProjectPermissionAllow},
            {"TR", TR_ProjectPermissionAllow},
            {"TWC", TWC_ProjectPermissionAllow},
            {"TWW", TWW_ProjectPermissionAllow},
            {"TWR", TWR_ProjectPermissionAllow},
            {"TWTM",TWTM_ProjectPermissionAllow}
        };
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Team_and_Group_Assist.Domain
{
    public class Group
    {
        // Name of the group with the role sufix part
        public string _groupName { get; private set; }
        // AD groups of the Group
        public ICollection<string> _adGroups { get; private set; }

        public Group(string teamName, string roleSufix, string[] adGroups)
        {
            _groupName = "Team " + teamName + "-" + roleSufix;
            _adGroups = adGroups.ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoveaAutoFramework.JIRA
{
    public class Version
    {
        public long value { get; set; }
        public bool Archived { get; set; }
        public string Label { get; set; }
    }
}

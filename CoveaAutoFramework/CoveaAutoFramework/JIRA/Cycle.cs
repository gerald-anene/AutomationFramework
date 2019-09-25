using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoveaAutoFramework.JIRA
{
    public class Cycle
    {
        public long CycleId { get; set; }
        public int TotalExecutions { get; set; }
        public string Description { get; set; }
        public int TotalExecuted { get; set; }
        public string Started { get; set; }
        public string Expand { get; set; }
        public string ProjectKey { get; set; }
        public int VersionId { get; set; }
        public string Environment { get; set; }
        public string Build { get; set; }
        public string CreatedBy { get; set; }
        public string Name { get; set; }
        public string ModifiedBy { get; set; }
        public long ProjectId { get; set; }
        public string CreatedByDisplay { get; set; }
        public string StartDate { get; set; }
        public ExecutionSummaries ExecutionSummaries { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoveaAutoFramework.JIRA
{
    public class TestExecution
    {
        public long Id { get; set; }
        public int ExecutionStatus { get; set; }
        public long IssueId { get; set; }
        public string IssueKey { get; set; }
        public string Summary { get; set; }
        public string IssueDescription { get; set; }
        public string Label { get; set; }

        public override string ToString()
        {
            return IssueKey;
        }
    }
}

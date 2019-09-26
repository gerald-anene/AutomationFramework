using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject;
//using CoveaAutoFramework.ApiClients;
using CoveaAutoFramework.JIRA;
//using CoveaAutoFramework.Config;
using CoveaFramework.ApiClients;
using CoveaFramework.Config;

namespace CoveaAutoFramework.FeatureAttributes
{
    public class FeatureSourceAttribute : IEnumerable
    {
        private const string AUTOMATED = "AUTOMATED";

        [Inject]
        public IZapi ZapiClient { get; set; }

        private Task<TestExecutions> testExecutions;


        public FeatureSourceAttribute()
        {
            Factory.Instance.Inject(this);
            testExecutions = ZapiClient.GetTests(Settings.JiraProject, Settings.JiraVersion, Settings.JiraCycle);
            testExecutions.Wait();

        }

        public IEnumerator GetEnumerator()
        {

            return testExecutions.Result.Executions.Where(e => e.Label.Contains(AUTOMATED)).GetEnumerator();
        }


    }
}

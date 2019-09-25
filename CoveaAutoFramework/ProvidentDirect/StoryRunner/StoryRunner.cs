using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using NBehave.Narrator.Framework;
using System.Text.RegularExpressions;
using NBehave.Narrator.Framework.EventListeners;
using Ninject;
using CoveaFramework.ApiClients;
using CoveaAutoFramework.JIRA;
using CoveaFramework.Config;
using CoveaAutoFramework.FeatureAttributes;
using CoveaAutoFramework.Logging;
//using CoveaFramework.generalSteps;
//using ProvidentProjects.generalSteps;


namespace CoveaAutoFramework.StoryRunners
{
    public class StoryRunner
    {
        private const string SCENARIO_FORMAT = "Scenario: {0} [{1}]";
        private const string HTML_TAG_PATTERN = "\\<.*?>";
        private const string END_OF_TEST = "end-of-test";

        [Inject]
        public IZapi ZapiClient { get; set; }

        public StoryRunner()
        {
            Factory.Instance.Inject(this);
        }

        [TestCaseSource(typeof(FeatureSourceAttribute))]
        public void Story(TestExecution testExecution)
        {
            var featureBuilder = new StringBuilder();
            featureBuilder.AppendLine(string.Format(SCENARIO_FORMAT, testExecution.Summary, testExecution.IssueKey));

            string description = Regex.Replace(testExecution.IssueDescription, HTML_TAG_PATTERN, "");
            description = description.Substring(0, description.IndexOf(END_OF_TEST));
            featureBuilder.AppendLine(description);

            List<IEventListener> eventListeners = new List<IEventListener>();
            eventListeners.Add(EventListeners.ColorfulConsoleOutputEventListener());

            eventListeners.Add(new LoggingStoryReporter());
            if (Settings.UpdateExecutionStatus)
            {
                eventListeners.Add(new ZapiStoryReporter(ZapiClient, testExecution.Id, Settings.PassExecutionStatus, Settings.FailExecutionStatus, Settings.unexecutedStatus, Settings.workInProgress));
            }
            eventListeners.Add(new InconclusiveStoryReporter());

            featureBuilder.ToString().ExecuteText(typeof(GeneralSteps).Assembly, eventListeners.ToArray());
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Factory.Instance.Kernel.Dispose();
        }
    }
}

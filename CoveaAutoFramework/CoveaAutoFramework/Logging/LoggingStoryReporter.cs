using log4net;
using NBehave.Narrator.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoveaAutoFramework.Logging
{
    public class LoggingStoryReporter : EventListener
    {
        private static ILog logger = LogManager.GetLogger(typeof(LoggingStoryReporter));

        public override void ScenarioStarted(string scenarioTitle)
        {
            logger.InfoFormat("Running scenario [{0}]", scenarioTitle);
        }

        public override void ScenarioFinished(ScenarioResult result)
        {
            logger.InfoFormat("Scenario finished [{0}]", result.ScenarioTitle);

            var stepResults = result.StepResults.ToList();

            foreach (var stepResult in stepResults)
            {

                var stepName = stepResult.StringStep;
                var message = stepResult.Message;
                var resultType = stepResult.Result.GetType().Name.ToUpper();
                var logEntry = string.Format("{0}: {1} - {2}", resultType, stepName, message);

                if (stepResult.Result is Passed)
                {
                    logger.Info(logEntry);
                }
                else
                {
                    logger.Warn(logEntry);

                }
            }

            if (result.HasFailedSteps())
            {
                logger.ErrorFormat("SCENARIO FAILED: {0}", result.ScenarioTitle);
            }
        }
    }
}

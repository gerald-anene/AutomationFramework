using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using NBehave.Narrator.Framework;
//using CoveaAutoFramework.ApiClient;
using CoveaFramework.ApiClients;

namespace CoveaAutoFramework.Logging
{
    public class ZapiStoryReporter : EventListener
    {
        private static ILog logger = LogManager.GetLogger(typeof(ZapiStoryReporter));

        private const string SUCCESSFUL_RESULT = "SUCCESSFUL";
        private const string UN_SUCCESSFUL_RESULT = "UN-SUCCESSFUL";
        private const string AUTOMATED_EXECUTION = "Automated execution.";
        private const string FAILURE_COMMENT_FORMAT = "Failing step [{0}]\nReason [{1}]";
        private const string EXECUTION_ID = "execution id";
        private const string STEP_HAS_NOT_YET_BEEN_IMPLEMENTED = "Step has not yet been implemented.";
        private const string UNEXECUTED_MESSAGE = "The execution status was reset to unexecuted";

        private const int MAX_COMMENT_LENGTH = 750;

        private readonly IZapi zapi;
        private readonly int passStatus;
        private readonly int failStatus;
        private readonly int unexecutedStatus;
        private readonly long executionId;
        private readonly int wipStatus;
       

        public ZapiStoryReporter(IZapi zapi, long executionId, int passStatus, int failStatus, int unexecutedStatus, int wip)
        {
            this.zapi = zapi;
            this.passStatus = passStatus;
            this.failStatus = failStatus;
            this.unexecutedStatus = unexecutedStatus;
            this.executionId = executionId;
            this.wipStatus = wip;
            
        }

        public override void ScenarioFinished(ScenarioResult result)
        {
            var hasFailures = result.HasFailedSteps();
            var pending = result.StepResults.Where(r => r.Result is Pending || r.Result is PendingNotImplemented);
            var hasPending = pending.Any();

            var isSuccessful = !(hasFailures || hasPending);

            var failingStep = "";
            var failureMessage = "";

            if (!isSuccessful)
            {
                if (hasFailures)
                {
                    var step = result.StepResults.First(r => r.Result is Failed);
                    failingStep = step.StringStep.Step;
                    failureMessage = result.Message;
                }
                else if (hasPending)
                {
                    failingStep = pending.First().StringStep.Step;
                    failureMessage = STEP_HAS_NOT_YET_BEEN_IMPLEMENTED;
                }
            }

            var resultType = isSuccessful ? SUCCESSFUL_RESULT : UN_SUCCESSFUL_RESULT;

            //  var resultInt = isSuccessful ? passStatus :failStatus;

            var resultInt = 0;

            if(isSuccessful)
            {
                resultInt = passStatus;
            }
            else if(hasPending)
            {
                resultInt = wipStatus;
            }else if(hasFailures)
            {
                resultInt = failStatus;
            }
            
            
            var unexecuted = unexecutedStatus;

            var comment = isSuccessful ? AUTOMATED_EXECUTION : string.Format(FAILURE_COMMENT_FORMAT, failingStep, failureMessage);
            var unexecutedComment = UNEXECUTED_MESSAGE;
            logger.InfoFormat("Writing {0} execution result to Jira for scenario: {1}", resultType, result.ScenarioTitle);


            zapi.UpdateTestExecution(executionId, unexecuted, unexecutedComment.Length > MAX_COMMENT_LENGTH ? unexecutedComment.Substring(0, MAX_COMMENT_LENGTH) : unexecutedComment).Wait();



            zapi.UpdateTestExecution(executionId, resultInt, comment.Length > MAX_COMMENT_LENGTH ? comment.Substring(0, MAX_COMMENT_LENGTH) : comment).Wait();

        }
    }
}

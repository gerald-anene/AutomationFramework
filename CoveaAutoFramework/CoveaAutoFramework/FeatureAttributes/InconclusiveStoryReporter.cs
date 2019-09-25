using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NBehave.Narrator.Framework;
using NUnit.Framework;

namespace CoveaAutoFramework.FeatureAttributes
{
    public class InconclusiveStoryReporter : EventListener
    {
        public override void ScenarioFinished(ScenarioResult result)
        {
            var pending = result.StepResults.Where(r => r.Result is Pending || r.Result is PendingNotImplemented);
            if (pending.Any())
            {
                Assert.Ignore("Some steps were pending.", pending);
            }
        }
    }
}

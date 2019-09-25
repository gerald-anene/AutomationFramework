using CoveaFramework.JIRA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoveaFramework.ApiClients
{
    public interface IZapi : IDisposable
    {
        /// <summary>
        /// Obtain the test executions associated with the given Jira project, version and cycle.
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="versionName"></param>
        /// <param name="cycleName"></param>
        /// <returns></returns>
        Task<TestExecutions> GetTests(string projectName, string versionName, string cycleName);

        /// <summary>
        /// Update the status and comments associated with the test execution having the given id.
        /// </summary>
        /// <param name="executionId"></param>
        /// <param name="result"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        Task UpdateTestExecution(long executionId, int result, string comment);
    }

}


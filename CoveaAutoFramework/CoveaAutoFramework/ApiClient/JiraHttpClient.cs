using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Formatting;
using Newtonsoft.Json.Serialization;
//using CoveaFramework.JIRA;
using CoveaAutoFramework.JIRA;

namespace CoveaFramework.ApiClients
{
    public class JiraHttpClient : HttpClient, IZapi
    {
        private const string PROJECTS_RESOURCE = "rest/zapi/latest/util/project-list";
        private const string VERSIONS_RESOURCE = "rest/zapi/latest/util/versionBoard-list";
        private const string CYCLES_RESOURCE = "rest/zapi/latest/cycle";
        private const string EXECUTIONS_RESOURCE = "rest/zapi/latest/execution";
        private const string EXECUTION_RESOURCE_FORMAT = "rest/zapi/latest/execution/{0}/execute";

        private const string PROJECT_ID_PARAM = "projectId";
        private const string VERSION_ID_PARAM = "versionId";
        private const string CYCLE_ID_PARAM = "cycleId";
        private const string AUTHORIZATION = "Authorization";

        private const string PROJECT_NAME_NOT_SUPPLIED = "Project name not supplied";
        private const string PROJECT_NAME_NOT_FOUND_FORMAT = "Project name not found {0}";
        private const string VERSION_NAME_NOT_SUPPLIED = "Version name not supplied";
        private const string VERSION_NAME_NOT_FOUND_IN_PROJECT_FORMAT = "Version name {0} not found in project {1}";
        private const string CYCLE_NAME_NOT_SUPPLIED = "Cycle name not supplied";
        private const string CYCLE_NAME_NOT_FOUND_IN_VERSION_FORMAT = "Cycle name {0} not found in version {1}";
        private const string ERROR_UPDATING_STATUS_FOR_EXECUTION_FORMAT = "Error updating status for execution id {0} response status [{1}: {2}]";

        private const string RECORDS_COUNT = "recordsCount";

        private readonly JsonMediaTypeFormatter defaultJsonFormatter;


        public JiraHttpClient(string baseUrl, string username, string password)
        {
            BaseAddress = new Uri(baseUrl);
            DefaultRequestHeaders.Accept.Clear();
            DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(
                        string.Format("{0}:{1}", username, password))));



            var settings = new JsonSerializerSettings();
            // make sure properties keys are camel cased
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            defaultJsonFormatter = new JsonMediaTypeFormatter { SerializerSettings = settings };
        }

        async Task<TestExecutions> IZapi.GetTests(string projectName, string versionName, string cycleName)
        {
            var project = await GetProject(projectName);
            var version = await GetVersion(project.value, versionName);
            var cycle = await GetCycle(project.value, version.value, cycleName);

            return await GetTests(cycle.CycleId);
        }

        public async Task UpdateTestExecution(long executionId, int result, string comment)
        {
            var statusUpdate = new StatusUpdate
            {
                Status = result.ToString(),
                Comment = comment
            };

            var response = await this.PutAsync(string.Format(EXECUTION_RESOURCE_FORMAT, executionId), statusUpdate, defaultJsonFormatter).ConfigureAwait(false);
            var test = response.Content.ReadAsStringAsync().Result;
            response.EnsureSuccessStatusCode();
            
        }

        private async Task<Project> GetProject(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name", PROJECT_NAME_NOT_SUPPLIED);
            }

            var projects = await Get<Projects>(PROJECTS_RESOURCE);

            var project = projects.Options.SingleOrDefault(p => p.Label == name);

            if (project == null)
            {
                throw new InvalidOperationException(string.Format(PROJECT_NAME_NOT_FOUND_FORMAT, name));
            }

            return project;
        }

        private async Task<CoveaAutoFramework.JIRA.Version> GetVersion(long projectId, string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name", VERSION_NAME_NOT_SUPPLIED);
            }

            var versions = await Get<Versions>(VERSIONS_RESOURCE, new NameValueCollection { { PROJECT_ID_PARAM, projectId.ToString() } });

            var version = versions.ReleasedVersions.Union(versions.UnreleasedVersions)
                .SingleOrDefault(v => v.Label == name);

            if (version == null)
            {
                throw new InvalidOperationException(string.Format(VERSION_NAME_NOT_FOUND_IN_PROJECT_FORMAT, name, projectId));
            }

            return version;
        }

        private async Task<Cycle> GetCycle(long projectId, long versionId, string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name", CYCLE_NAME_NOT_SUPPLIED);
            }

            var cyclesDict = await Get<Dictionary<string, JToken>>(CYCLES_RESOURCE,
                new NameValueCollection { { PROJECT_ID_PARAM, projectId.ToString() }, { VERSION_ID_PARAM, versionId.ToString() } });

            // filter out the unwanted 
            var cycles = cyclesDict.Where(c => c.Key != RECORDS_COUNT).Select(c =>
            {
                var cycle2 = c.Value.ToObject<Cycle>();
                cycle2.CycleId = long.Parse(c.Key);
                return cycle2;
            });

            var cycle = cycles.SingleOrDefault(c => c.Name == name);

            if (cycle == null)
            {
                throw new InvalidOperationException(string.Format(CYCLE_NAME_NOT_FOUND_IN_VERSION_FORMAT, name, versionId));
            }

            return cycle;
        }

        private Task<TestExecutions> GetTests(long cycleId)
        {
            return Get<TestExecutions>(EXECUTIONS_RESOURCE, new NameValueCollection { { CYCLE_ID_PARAM, cycleId.ToString() } });
        }

        private async Task<T> Get<T>(string url, NameValueCollection query = null)
        {
            var queryString = "";

            if (query != null)
            {
                // ParseQueryString returns an internal subclass of NameValueCollection
                var queryCollection = HttpUtility.ParseQueryString(BaseAddress.Query);
                query.AllKeys.ToList().ForEach(k => queryCollection[k] = query[k]);

                // the internal implementation overrides ToString, returning a well formatted query string
                queryString = "?" + queryCollection.ToString();
            }

            var response = await GetAsync(url + queryString);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<T>().ConfigureAwait(false);
        }
    }
}

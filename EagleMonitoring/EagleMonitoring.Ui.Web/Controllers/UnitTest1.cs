using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Test.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;

namespace Eagle.Server.Framework.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            var tfsProject = "GlobalRoam";
            var baseUrl = new Uri("http://tfs:8080/tfs/GRCollection");

            var credentials = CredentialWindow.GetCredentials();

            var vssCredentials = new VssCredentials(new WindowsCredential(credentials));

            var buildClient = new BuildHttpClient(baseUrl, vssCredentials);
            var definitions = await buildClient.GetDefinitionsAsync(project: tfsProject);
            var definitionId = definitions.First(d => d.Name.Equals("Products.ez2viewAustralia_CI", StringComparison.InvariantCultureIgnoreCase)).Id;
            var definition = (BuildDefinition)await buildClient.GetDefinitionAsync(project: tfsProject, definitionId: definitionId);
            var builds = await buildClient.GetBuildsAsync(project: tfsProject, definitions: new[] { definitionId });
            var buildId = builds.First().Id;
            var build = await buildClient.GetBuildAsync(project: tfsProject, buildId: buildId);
            var artifacts = await buildClient.GetArtifactsAsync(project: tfsProject, buildId: buildId);
            var logs = await buildClient.GetBuildLogsAsync(project: tfsProject, buildId: buildId);
            var timeline = await buildClient.GetBuildTimelineAsync(project: tfsProject, buildId: buildId);
            var tags = await buildClient.GetBuildTagsAsync(project: tfsProject, buildId: buildId);

            var testManagementClient = new TestManagementHttpClient(baseUrl, vssCredentials);
            var testRuns = await testManagementClient.GetTestRunsAsync(projectId: tfsProject, buildUri: build.Uri.ToString());
            var testRunId = testRuns.First().Id;
            var testRun = await testManagementClient.GetTestRunByIdAsync(tfsProject, testRunId);
        }

        private static async Task DirectlyAccessBuildDefinitions(ICredentials credentials)
        {
            // Credentials should be a NetworkCredential(userName, password, domain) for a domain account
            // This will allow the HttpClient to use NTLM (NT LAN Manager) HTTP authentication (which is a multi-step process)
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                Credentials = credentials
            };

            // Documentation for VSO (visual studio online), which is very similar to TFS:
            // https://www.visualstudio.com/integrate/api/build/builds
            // Some examples of the API:
            // http://tfs:8080/tfs/GRCollection/0bd228f0-c6e0-4694-b721-a046a1f26bd7/_apis/build/Definitions
            // http://tfs:8080/tfs/GRCollection/0bd228f0-c6e0-4694-b721-a046a1f26bd7/_apis/build/Builds?project=0bd228f0-c6e0-4694-b721-a046a1f26bd7&definitions=146&statusFilter=3&%24top=10
            // http://tfs:8080/tfs/GRCollection/0bd228f0-c6e0-4694-b721-a046a1f26bd7/_apis/build/Builds?%24top=25&statusFilter=2&type=2
            // http://tfs:8080/tfs/GRCollection/0bd228f0-c6e0-4694-b721-a046a1f26bd7/_apis/build/Builds/10061

            using (var client = new HttpClient(handler))
            {
                var result = await client.GetAsync("http://tfs:8080/tfs/GRCollection/0bd228f0-c6e0-4694-b721-a046a1f26bd7/_apis/build/Definitions");
            }
        }
    }
}

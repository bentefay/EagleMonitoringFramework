using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.VisualStudio.Services.Common;

namespace Emf.Web.Ui.Models
{
    public class TfsBuildDefinitionRepository
    {
        private const string _tfsProject = "GlobalRoam";
        private static readonly Uri _baseUrl = new Uri("http://tfs.gr.local:8080/tfs/GRCollection");
        private readonly BuildHttpClient _buildClient;

        public TfsBuildDefinitionRepository(VssCredentials credentials)
        {
            _buildClient = new BuildHttpClient(_baseUrl, credentials);
        }

        public async Task<IReadOnlyList<BuildDefinitionReferenceDto>> GetDefinitions(CancellationToken cancellationToken)
        {
            var definitionReferences = await _buildClient.GetDefinitionsAsync(project: _tfsProject, cancellationToken: cancellationToken);

            return definitionReferences.Select(d => new BuildDefinitionReferenceDto { Id = d.Id, Name = d.Name }).ToList();
        }

        //public async Task GetOtherStuff(int definitionId)
        //{
        //    var definition = (BuildDefinition)await _buildClient.GetDefinitionAsync(project: _tfsProject, definitionId: definitionId);

        //    var builds = await _buildClient.GetBuildsAsync(project: _tfsProject, definitions: new[] { definitionId });
        //    var buildId = builds.First().Id;
        //    var build = await _buildClient.GetBuildAsync(project: _tfsProject, buildId: buildId);
        //    var artifacts = await _buildClient.GetArtifactsAsync(project: _tfsProject, buildId: buildId);
        //    var logs = await _buildClient.GetBuildLogsAsync(project: _tfsProject, buildId: buildId);
        //    var timeline = await _buildClient.GetBuildTimelineAsync(project: _tfsProject, buildId: buildId);
        //    var tags = await _buildClient.GetBuildTagsAsync(project: _tfsProject, buildId: buildId);

        //    var testManagementClient = new TestManagementHttpClient(_baseUrl, _vssCredentials);
        //    var testRuns = await testManagementClient.GetTestRunsAsync(projectId: _tfsProject, buildUri: build.Uri.ToString());
        //    var testRunId = testRuns.First().Id;
        //    var testRun = await testManagementClient.GetTestRunByIdAsync(_tfsProject, testRunId);
        //}

        //private static async Task DirectlyAccessBuildDefinitions(ICredentials credentials)
        //{
        //    // Credentials should be a NetworkCredential(userName, password, domain) for a domain account
        //    // This will allow the HttpClient to use NTLM (NT LAN Manager) HTTP authentication (which is a multi-step process)
        //    var handler = new HttpClientHandler
        //    {
        //        AllowAutoRedirect = true,
        //        Credentials = credentials
        //    };

        //    // Documentation for VSO (visual studio online), which is very similar to TFS:
        //    // https://www.visualstudio.com/integrate/api/build/builds
        //    // Some examples of the API:
        //    // http://tfs:8080/tfs/GRCollection/0bd228f0-c6e0-4694-b721-a046a1f26bd7/_apis/build/Definitions
        //    // http://tfs:8080/tfs/GRCollection/0bd228f0-c6e0-4694-b721-a046a1f26bd7/_apis/build/Builds?project=0bd228f0-c6e0-4694-b721-a046a1f26bd7&definitions=146&statusFilter=3&%24top=10
        //    // http://tfs:8080/tfs/GRCollection/0bd228f0-c6e0-4694-b721-a046a1f26bd7/_apis/build/Builds?%24top=25&statusFilter=2&type=2
        //    // http://tfs:8080/tfs/GRCollection/0bd228f0-c6e0-4694-b721-a046a1f26bd7/_apis/build/Builds/10061

        //    using (var client = new HttpClient(handler))
        //    {
        //        var result = await client.GetAsync("http://tfs:8080/tfs/GRCollection/0bd228f0-c6e0-4694-b721-a046a1f26bd7/_apis/build/Definitions");
        //    }
        //}
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.VisualStudio.Services.Common;

namespace Eagle.Server.UI.Web2.Models
{
    public class BuildRepository
    {
        private static readonly Uri _baseUrl = new Uri("http://tfs:8080/tfs/GRCollection");
        private const string _tfsProject = "GlobalRoam";

        private readonly BuildHttpClient _buildClient;
        private readonly TestManagementHttpClient _testManagementClient;

        public BuildRepository(ICredentials credentials)
        {
            var vssCredentials = new VssCredentials(new WindowsCredential(credentials));
            _buildClient = new BuildHttpClient(_baseUrl, vssCredentials);
            _testManagementClient = new TestManagementHttpClient(_baseUrl, vssCredentials);
        }

        public async Task<IReadOnlyList<BuildDefinitionDto>> GetBuilds(CancellationToken token)
        {
            var definitionSummaries = await _buildClient.GetDefinitionsAsync(project: _tfsProject, cancellationToken: token);

            // var definitions = await Task.WhenAll(definitionIds.Select(d => _buildClient.GetDefinitionAsync(project: _tfsProject, definitionId: d, cancellationToken: token)));
            // var buildDefinitions = definitions.OfType<BuildDefinition>().ToList();
            // var xamlBuildDefinitions = definitions.OfType<XamlBuildDefinition>().ToList();

            // var buildSummaries = await _buildClient.GetBuildsAsync(project: _tfsProject, definitions: definitionSummaries.Select(d => d.Id), maxBuildsPerDefinition: 1, cancellationToken: token);

            // var definitionAndLatestBuild = definitionSummaries.GroupJoin(buildSummaries, d => d.Id, b => b.Definition.Id, (d, b) => new { Definition = d, Build = b, }) 

            // var builds = buildSummaries.Select(async b => await _buildClient.GetBuildAsync(project: _tfsProject, buildId: b.Id, cancellationToken: token));

            // var artifacts = await _buildClient.GetArtifactsAsync(project: _tfsProject, buildId: buildId, cancellationToken: token);
            // var logs = await _buildClient.GetBuildLogsAsync(project: _tfsProject, buildId: buildId, cancellationToken: token);
            // var timeline = await _buildClient.GetBuildTimelineAsync(project: _tfsProject, buildId: buildId, cancellationToken: token);
            // var tags = await _buildClient.GetBuildTagsAsync(project: _tfsProject, buildId: buildId, cancellationToken: token);

            //var detailedBuilds = await Task.WhenAll(buildSummaries.Select(async b => new
            //{
            //    BuildSummary = b,
            //    LatestTestRun = (await _testManagementClient.GetTestRunsAsync(projectId: _tfsProject, buildUri: b.Uri.ToString(), top: 1)).FirstOrDefault()
            //}));

            // var testRunSummaries = testRunSummariesForBuilds.Select(t => t.FirstOrDefault()).Where(t => t != null);
            // var testRuns = await Task.WhenAll(testRunSummaries.Select(t => _testManagementClient.GetTestRunByIdAsync(_tfsProject, t.Id)));
            var buildDefinitions = new List<BuildDefinitionDto>();

            return buildDefinitions;
        }
    }

    public class BuildDefinitionDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BuildDto LatestBuild { get; set; }
    }

    public class BuildDto
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public BuildTestResultsDto TestResults { get; set; }
    }

    public class BuildTestResultsDto
    {
        public int TestsPassed { get; set; }
        public int TestsTotal { get; set; }
    }
}

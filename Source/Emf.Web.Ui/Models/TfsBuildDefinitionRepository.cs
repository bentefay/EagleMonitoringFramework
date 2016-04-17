using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Emf.Web.Ui.Services.Settings;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using VssCredentials = Microsoft.VisualStudio.Services.Common.VssCredentials;

namespace Emf.Web.Ui.Models
{
    public class TfsBuildDefinitionRepository
    {
        private const string _tfsProject = "GlobalRoam";
        private static readonly Uri _baseUrl = new Uri("http://tfs.gr.local:8080/tfs/GRCollection");
        private static readonly Settings _settings = new Settings(_baseUrl.ToString(), _tfsProject);

        private readonly SettingStore<Dictionary<int, BuildDefinition>> _buildDefinitionStore;
        private readonly SettingStore<BuildCollection> _buildStore;
        private readonly BuildHttpClient _buildClient;
        private readonly TestManagementHttpClient _testManagementClient;

        public TfsBuildDefinitionRepository(VssCredentials credentials, SettingStore<Dictionary<int, BuildDefinition>> buildDefinitionStore, SettingStore<BuildCollection> buildStore)
        {
            _buildDefinitionStore = buildDefinitionStore;
            _buildStore = buildStore;
            _buildClient = new BuildHttpClient(_baseUrl, credentials);
            _testManagementClient = new TestManagementHttpClient(_baseUrl, credentials);
        }

        public Settings GetSettings() => _settings;

        public async Task<IReadOnlyList<BuildDefinitionReference>> GetLatestDefinitionReferences(CancellationToken cancellationToken)
        {
            var definitionReferences = await _buildClient.GetDefinitionsAsync(project: _tfsProject, cancellationToken: cancellationToken);

            return definitionReferences.Select(r => new BuildDefinitionReference(r.Id, r.Name, r.Revision, r.Type)).ToList();
        }

        public async Task<IReadOnlyDictionary<int, BuildDefinition>> GetLatestDefinitions(IReadOnlyList<BuildDefinitionReference> latestReferences, CancellationToken cancellationToken)
        {
            var existingDefinitionsLookup = _buildDefinitionStore.GetOrCreate(() => new Dictionary<int, BuildDefinition>());

            var anyChanges = false;

            foreach (var latestReference in latestReferences)
            {
                BuildDefinition oldFullBuildDefinition;
                if (existingDefinitionsLookup.TryGetValue(latestReference.Id, out oldFullBuildDefinition) && latestReference.Revision <= oldFullBuildDefinition.Reference.Revision)
                    continue;

                var baseDefinition = await _buildClient.GetDefinitionAsync(project: _tfsProject, definitionId: latestReference.Id, cancellationToken: cancellationToken);

                existingDefinitionsLookup[latestReference.Id] = GetBuildDefinition(latestReference, baseDefinition);
                anyChanges = true;
            }

            var idsToBeDeleted = existingDefinitionsLookup.Select(b => b.Key).Except(latestReferences.Select(b => b.Id)).ToArray();

            if (idsToBeDeleted.Any())
                anyChanges = true;

            foreach (var idToBeDeleted in idsToBeDeleted)
                existingDefinitionsLookup.Remove(idToBeDeleted);

            if (anyChanges)
                _buildDefinitionStore.Set(existingDefinitionsLookup);

            return existingDefinitionsLookup;
        }

        private static BuildDefinition GetBuildDefinition(BuildDefinitionReference latestDefinitionReference, DefinitionReference baseDefinition)
        {
            var xamlDefinition = baseDefinition as XamlBuildDefinition;
            if (xamlDefinition != null)
                return new BuildDefinition(latestDefinitionReference, xamlDefinition);

            var definition = baseDefinition as Microsoft.TeamFoundation.Build.WebApi.BuildDefinition;
            if (definition != null)
                return new BuildDefinition(latestDefinitionReference, definition);

            throw new Exception($"{nameof(baseDefinition)} has unexpected type {baseDefinition.GetType()}");
        }

        public async Task<IDictionary<int, Build>> GetLatestBuilds(IReadOnlyDictionary<int, BuildDefinitionReference> latestReferences, CancellationToken cancellationToken)
        {
            if (!latestReferences.Any())
                return new Dictionary<int, Build>();

            var buildCollection = _buildStore.GetOrCreate(() => new BuildCollection());

            await AddBuilds(latestReferences, cancellationToken, buildCollection);

            var latestBuildFinishTime = buildCollection.BuildsByDefinitionId.Values.Select(b => b.FinishTime ?? b.StartTime).DefaultIfEmpty(null).Max();

            if (latestBuildFinishTime != buildCollection.LatestBuildFinishTime)
            {
                buildCollection.LatestBuildFinishTime = latestBuildFinishTime;
                _buildStore.Set(buildCollection);
            }

            return buildCollection.BuildsByDefinitionId;
        }

        private async Task AddBuilds(IReadOnlyDictionary<int, BuildDefinitionReference> latestReferences, CancellationToken cancellationToken, BuildCollection buildCollection)
        {
            var completedBuilds = await _buildClient.GetBuildsAsync(
                _tfsProject,
                minFinishTime: buildCollection.LatestBuildFinishTime,
                cancellationToken: cancellationToken);

            var completedBuildsByDefinition = completedBuilds
                .Where(b => latestReferences.ContainsKey(b.Definition.Id))
                .Where(b =>
                {
                    if (buildCollection.LatestBuildFinishTime == null || (b.StartTime == null && b.FinishTime == null))
                        return true;
                    return (b.FinishTime ?? b.StartTime) > buildCollection.LatestBuildFinishTime;
                })
                .ToLookup(b => b.Definition.Id)
                .ToDictionary(
                    g => g.Key,
                    g => g.Where(b => b.Uri != null)
                        .OrderByDescending(b => b.FinishTime ?? b.StartTime)
                        .ToArray());

            foreach (var completedBuildsForDefinition in completedBuildsByDefinition)
            {
                BuildDefinitionReference buildReference;
                if (!latestReferences.TryGetValue(completedBuildsForDefinition.Key, out buildReference))
                    continue;

                var latestCompletedBuild = completedBuildsForDefinition.Value.FirstOrDefault();

                if (latestCompletedBuild == null)
                    continue;

                var testRuns = await _testManagementClient.GetTestRunsAsync(projectId: _tfsProject, buildUri: latestCompletedBuild.Uri.ToString(), includeRunDetails: true);
                buildCollection.BuildsByDefinitionId[completedBuildsForDefinition.Key] = new Build(buildReference, latestCompletedBuild, testRuns);
            }
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

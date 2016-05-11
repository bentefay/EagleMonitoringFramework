using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Emf.Web.Ui.Models
{
    public class TfsMonitoringService : IDisposable
    {
        private readonly TfsBuildDefinitionRepository _repository;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly TimeSpan _checkForDefinitionsInterval;
        private readonly TimeSpan _checkForBuildsInterval;
        private readonly ObservableRepository<int, BuildDefinitionReference> _buildDefinitions;
        private readonly ObservableRepository<int, Build> _builds;
        private readonly ObservableRepository<int, ConnectionSettings> _settings;

        public TfsMonitoringService(TfsBuildDefinitionRepository repository, TimeSpan checkForDefinitionsInterval, TimeSpan checkForBuildsInterval)
        {
            _repository = repository;
            _checkForDefinitionsInterval = checkForDefinitionsInterval;
            _checkForBuildsInterval = checkForBuildsInterval;
            _buildDefinitions = new ObservableRepository<int, BuildDefinitionReference>(d => d.Id, BuildDefinitionReference.ChangedComparer);
            _builds = new ObservableRepository<int, Build>(b => b.Definition.Id, Build.ChangedComparer);
            _settings = new ObservableRepository<int, ConnectionSettings>(s => 0, Models.ConnectionSettings.ChangedComparer);
            _settings.AddOrUpdate(new [] { _repository.GetSettings() });
        }

        public async void Start()
        {
            await UpdateDefinitions(_tokenSource.Token);
            await UpdateBuilds(_tokenSource.Token);

            await Task.WhenAll(
                RepeatTask.Every(UpdateDefinitions, _checkForDefinitionsInterval, _tokenSource.Token),
                RepeatTask.Every(UpdateBuilds, _checkForBuildsInterval, _tokenSource.Token));
        }

        private async Task UpdateDefinitions(CancellationToken token)
        {
            var buildDefinitions = await _repository.GetLatestDefinitionReferences(_tokenSource.Token);

            lock (_buildDefinitions)
                _buildDefinitions.SetTo(buildDefinitions);
        }

        private async Task UpdateBuilds(CancellationToken token)
        {
            Dictionary<int, BuildDefinitionReference> buildDefinitions;
            lock (_buildDefinitions)
                buildDefinitions = _buildDefinitions.Map.ToDictionary(p => p.Key, p => p.Value);

            var builds = await _repository.GetLatestBuilds(buildDefinitions, _tokenSource.Token);

            _builds.SetTo(builds.Values);
        }

        public IObservableRepository<int, BuildDefinitionReference> BuildDefinitions => _buildDefinitions;
        public IObservableRepository<int, Build> Builds => _builds;
        public IObservableRepository<int, ConnectionSettings> Settings => _settings;

        public void Dispose()
        {
            _tokenSource.Dispose();
        }
    }
}

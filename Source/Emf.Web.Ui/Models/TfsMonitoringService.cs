using System;
using System.Threading;
using System.Threading.Tasks;

namespace Emf.Web.Ui.Models
{
    public class TfsMonitoringService : IDisposable
    {
        private readonly TfsBuildDefinitionRepository _repository;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly TimeSpan _interval;
        private readonly ObservableRepository<int, BuildDefinitionReferenceDto> _buildDefinitionReferences;

        public TfsMonitoringService(TfsBuildDefinitionRepository repository, TimeSpan interval)
        {
            _repository = repository;
            _interval = interval;
            _buildDefinitionReferences = new ObservableRepository<int, BuildDefinitionReferenceDto>(d => d.Id);
        }

        public Task Start()
        {
            return RepeatTask.Every(OnTimerTick, _interval, _tokenSource.Token);
        }

        private async Task OnTimerTick(CancellationToken token)
        {
            var buildDefinitions = await _repository.GetDefinitions(_tokenSource.Token);

            _buildDefinitionReferences.AddOrUpdate(buildDefinitions);
        }

        public IObservableRepository<int, BuildDefinitionReferenceDto> BuildDefinitionReferences => _buildDefinitionReferences;

        public void Dispose()
        {
            _tokenSource.Dispose();
        }
    }
}

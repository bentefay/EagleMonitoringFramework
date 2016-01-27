using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Emf.Web.Ui.Models
{
    public class TfsMonitoringService : IDisposable
    {
        private readonly TfsBuildDefinitionRepository _repository;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly TimeSpan _interval;

        public TfsMonitoringService(TfsBuildDefinitionRepository repository, TimeSpan interval)
        {
            _repository = repository;
            _interval = interval;
        }

        public Task Start()
        {
            return RepeatTask.Every(new Action<CancellationToken>(OnTimerTick), _interval, _tokenSource.Token);
        }

        private void OnTimerTick(CancellationToken token)
        {
            var buildDefinitions = _repository.GetDefinitions(_tokenSource.Token);
        }

        public void Dispose()
        {
            _tokenSource.Dispose();
        }
    }
}

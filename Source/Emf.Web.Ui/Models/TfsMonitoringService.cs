using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

    public class ObservableRepository<T, TKey>
    {
        private readonly object _lock = new object();
        private readonly Func<T, TKey> _getKey;
        private readonly Dictionary<TKey, T> _map;
        private readonly Subject<ObservableRepositoryEvent<T, TKey>> _subject = new Subject<ObservableRepositoryEvent<T, TKey>>();

        public ObservableRepository(Func<T, TKey> keyGetter)
        {
            _getKey = keyGetter;
            _map = new Dictionary<TKey, T>();
        }

        public IObservable<ObservableRepositoryEvent<T, TKey>> GetChanges()
        {
            return Observable.Create<ObservableRepositoryEvent<T, TKey>>(o =>
            {
                lock (_lock)
                {
                    o.OnNext(new ObservableRepositoryEvent<T, TKey>(newOrUpdateditems: _map.Values.ToList()));
                    return _subject.Subscribe(o);
                }
            });
        }

        public void AddOrUpdate(IEnumerable<T> items)
        {
            lock (_lock)
            {
                var itemList = items.ToList();

                foreach (var item in itemList)
                    _map[_getKey(item)] = item;

                _subject.OnNext(new ObservableRepositoryEvent<T, TKey>(newOrUpdateditems: itemList));
            }
        }

        public void Remove(IEnumerable<TKey> keys)
        {
            lock (_lock)
            {
                var removedKeys = keys.Where(key => _map.Remove(key)).ToList();

                _subject.OnNext(new ObservableRepositoryEvent<T, TKey>(deletedItems: removedKeys));
            }
        }
    }

    public struct ObservableRepositoryEvent<T, TKey>
    {
        private static readonly IReadOnlyList<T> _emptyItemsList = new List<T>();
        private static readonly IReadOnlyList<TKey> _emptyKeysList = new List<TKey>();

        public ObservableRepositoryEvent(IReadOnlyList<T> newOrUpdateditems = null, IReadOnlyList<TKey> deletedItems = null)
        {
            NewOrUpdatedItems = newOrUpdateditems ?? _emptyItemsList;
            DeletedItems = deletedItems ?? _emptyKeysList;
        }

        public IReadOnlyList<T> NewOrUpdatedItems { get; }
        public IReadOnlyList<TKey> DeletedItems { get; }
    }
}

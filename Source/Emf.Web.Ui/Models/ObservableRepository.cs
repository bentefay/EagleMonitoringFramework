using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Emf.Web.Ui.Models
{
    public class ObservableRepository<TValue, TKey>
    {
        private readonly object _lock = new object();
        private readonly Func<TValue, TKey> _getKey;
        private readonly Dictionary<TKey, TValue> _map;
        private readonly Subject<ObservableRepositoryEvent<TValue, TKey>> _subject = new Subject<ObservableRepositoryEvent<TValue, TKey>>();

        public ObservableRepository(Func<TValue, TKey> keyGetter)
        {
            _getKey = keyGetter;
            _map = new Dictionary<TKey, TValue>();
        }

        public IObservable<ObservableRepositoryEvent<TValue, TKey>> GetChanges()
        {
            return Observable.Create<ObservableRepositoryEvent<TValue, TKey>>(o =>
            {
                lock (_lock)
                {
                    var items = _map.Select(p => KeyValue.Create(p.Key, p.Value)).ToList();
                    if (items.Any())
                        o.OnNext(new ObservableRepositoryEvent<TValue, TKey>(items: items, reset: true));
                    return _subject.Subscribe(o);
                }
            });
        }

        public void AddOrUpdate(IEnumerable<TValue> items)
        {
            lock (_lock)
            {
                var pairList = items.Select(i => KeyValue.Create(_getKey(i), i)).ToList();

                if (!pairList.Any())
                    return;

                foreach (var pair in pairList)
                    _map[pair.Key] = pair.Value;

                _subject.OnNext(new ObservableRepositoryEvent<TValue, TKey>(newOrUpdateditems: pairList));
            }
        }

        public void Remove(IEnumerable<TKey> keys)
        {
            lock (_lock)
            {
                var removedKeys = keys.Where(key => _map.Remove(key)).ToList();

                if (!removedKeys.Any())
                    return;

                _subject.OnNext(new ObservableRepositoryEvent<TValue, TKey>(deletedItems: removedKeys));
            }
        }
    }

    public struct KeyValue
    {
        public static KeyValue<TKey, TValue> Create<TKey, TValue> (TKey key, TValue value)
        {
            return new KeyValue<TKey, TValue>(key, value);
        }

        public KeyValue(object key, object value)
        {
            Key = key;
            Value = value;
        }

        public object Key { get; }
        public object Value { get; }

    }

    public struct KeyValue<TKey, TValue>
    {
        public KeyValue(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public TKey Key { get; }
        public TValue Value { get; }

        public static implicit operator KeyValue(KeyValue<TKey, TValue> pair)
        {
            return new KeyValue(pair.Key, pair.Value);
        }
    }


    public interface IObservableRepositoryEvent
    {
        IReadOnlyList<KeyValue> NewOrUpdatedItems { get; }
        IReadOnlyList<object> DeletedItemKeys { get; }
        bool Reset { get; }
    }

    public struct ObservableRepositoryEvent<TValue, TKey> : IObservableRepositoryEvent
    {
        private static readonly IReadOnlyList<KeyValue<TKey, TValue>> _emptyItemsList = new List<KeyValue<TKey, TValue>>();
        private static readonly IReadOnlyList<TKey> _emptyKeysList = new List<TKey>();

        public ObservableRepositoryEvent(IReadOnlyList<KeyValue<TKey, TValue>> newOrUpdateditems = null, IReadOnlyList<TKey> deletedItems = null)
        {
            NewOrUpdatedItems = newOrUpdateditems ?? _emptyItemsList;
            DeletedItems = deletedItems ?? _emptyKeysList;
            Reset = false;
        }

        public ObservableRepositoryEvent(IReadOnlyList<KeyValue<TKey, TValue>> items, bool reset)
        {
            NewOrUpdatedItems = items ?? _emptyItemsList;
            DeletedItems = _emptyKeysList;
            Reset = reset;
        }

        public IReadOnlyList<KeyValue<TKey, TValue>> NewOrUpdatedItems { get; }
        public IReadOnlyList<TKey> DeletedItems { get; }
        public bool Reset { get; }

        IReadOnlyList<KeyValue> IObservableRepositoryEvent.NewOrUpdatedItems => NewOrUpdatedItems.Cast<KeyValue>().ToList();
        IReadOnlyList<object> IObservableRepositoryEvent.DeletedItemKeys => DeletedItems.Cast<object>().ToList();
    }
}
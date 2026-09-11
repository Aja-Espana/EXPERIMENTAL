using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mandible.Systems
{
    [Serializable]
    public class ObservedList<T> : IList<T>, ISerializationCallbackReceiver
    {
        public delegate void ChangedDelegate(int index, T oldValue, T newValue);

        [SerializeField] private List<T> _list = new();

        public event ChangedDelegate Changed;
        public event Action Updated;

        public void OnAfterDeserialize() { }

        public void OnBeforeSerialize() { }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
            _list.Add(item);
            Updated?.Invoke();
        }

        public void Clear()
        {
            _list.Clear();
            Updated?.Invoke();
        }

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public bool Remove(T item)
        {
            bool output = _list.Remove(item);
            Updated?.Invoke();
            return output;
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public int IndexOf(T item) => _list.IndexOf(item);

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            Updated?.Invoke();
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
            Updated?.Invoke();
        }

        public T this[int index]
        {
            get => _list[index];
            set
            {
                var oldValue = _list[index];
                _list[index] = value;
                Changed?.Invoke(index, oldValue, value);
                Updated?.Invoke();
            }
        }
    }
}

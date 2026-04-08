using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Assets._Scripts.Patterns
{
    public class Pooling<T> where T : Component
    {
        private T _prefab;
        private Transform _parent;

        private Queue<T> _pool = new();
        private UnityAction<T> _onCreate;

        public Pooling()
        {
            _prefab = null;
            _parent = null;
        }

        public Pooling(T prefab, int initAmount, Transform parent = null, UnityAction<T> onCreate = null)
        {
            _prefab = prefab;
            _parent = parent;
            _onCreate = onCreate;

            while (prefab != null && _pool.Count < initAmount)
            {
                var newItem = CreateItem();
                _pool.Enqueue(newItem);
            }
        }

        private T CreateItem()
        {
            if (_prefab == null) return null; 
            var newItem = Object.Instantiate(_prefab, _parent);
            newItem.gameObject.SetActive(false);
            _onCreate?.Invoke(newItem);
            return newItem;
        }

        public T GetItem(UnityAction<T> onGet = null)
        {
            T toGet;
            if (_pool.Count == 0)
                toGet = CreateItem();
            else
                toGet = _pool.Dequeue();
            toGet.gameObject.SetActive(true);
            onGet?.Invoke(toGet);
            return toGet;
        }

        public void ReturnItem(T toReturn, UnityAction<T> onReturned = null)
        {
            toReturn.gameObject.SetActive(false);
            _pool.Enqueue(toReturn);
            onReturned?.Invoke(toReturn);
        }
    }
}
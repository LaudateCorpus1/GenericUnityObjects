﻿namespace GenericUnityObjects.Util.Database
{
    using System;
    using System.Collections.Generic;
    using TypeReferences;
    using UnityEngine;
    using UnityEngine.Assertions;

    /// <summary>
    /// A serializable dictionary of type Dictionary&lt;TypeReference[], TypeReference>.
    /// </summary>
    [Serializable]
    internal class TypeDictionary : ISerializationCallbackReceiver
    {
        private readonly Dictionary<TypeReference[], TypeReference> _dict =
            new Dictionary<TypeReference[], TypeReference>(new TypeReferenceArrayComparer());

        [SerializeField] private TypeReferenceCollection[] _keys = { };

        [SerializeField] private TypeReference[] _values = { };

        public void Add(Type[] key, Type value) => _dict.Add(key.CastToTypeReference(), new TypeReference(value, suppressLogs: true));

        public bool ContainsKey(Type[] key)
        {
            var refKey = key.CastToTypeReference();

            if (! _dict.ContainsKey(refKey))
                return false;

            return _dict[refKey].Type != null;
        }

        public bool TryGetValue(TypeReference[] key, out TypeReference value) => _dict.TryGetValue(key, out value);

        public bool TryGetValue(Type[] key, out Type value)
        {
            bool result = TryGetValue(key.CastToTypeReference(), out TypeReference typeRef);
            value = typeRef;

            return result && value != null;
        }

        public void OnAfterDeserialize()
        {
            int keysLength = _keys.Length;
            int valuesLength = _values.Length;

            Assert.IsTrue(keysLength == valuesLength);
            Assert.IsTrue(_dict.Count == 0);

            for (int i = 0; i < keysLength; ++i)
            {
                if (_values[i].TypeIsMissing())
                    continue;

                _dict[_keys[i]] = _values[i];
            }

            TypeReferenceCollection.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(TypeReferenceCollection oldCollection, TypeReferenceCollection newCollection)
        {
            if ( ! _dict.TryGetValue(oldCollection, out TypeReference value))
                return;

            _dict.Remove(oldCollection);
            _dict.Add(newCollection, value);
        }

        public void OnBeforeSerialize()
        {
            int dictLength = _dict.Count;
            _keys = new TypeReferenceCollection[dictLength];
            _values = new TypeReference[dictLength];

            int keysIndex = 0;
            foreach (var pair in _dict)
            {
                _keys[keysIndex] = pair.Key;
                _values[keysIndex] = pair.Value;
                ++keysIndex;
            }
        }
    }

    /// <summary>
    /// A TypeReference[] container that is used because TypeReference[][] cannot be serialized by Unity.
    /// </summary>
    [Serializable]
    internal class TypeReferenceCollection : ISerializationCallbackReceiver
    {
        [SerializeField] private TypeReference[] _array;

        public TypeReferenceCollection(TypeReference[] collection) => _array = collection;

        public TypeReferenceCollection() : this((TypeReference[]) null) { }

        public TypeReferenceCollection(Type[] collection) : this(collection.CastToTypeReference()) { }

        public static implicit operator TypeReferenceCollection(Type[] typeCollection) =>
            new TypeReferenceCollection(typeCollection);

        public static implicit operator TypeReferenceCollection(TypeReference[] typeRefCollection) =>
            new TypeReferenceCollection(typeRefCollection);

        public static implicit operator TypeReference[](TypeReferenceCollection typeRefCollection) =>
            typeRefCollection._array;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            TypeReference.TypeRestoredFromGUID += OnElementChanged;
        }

        public static event Action<TypeReferenceCollection, TypeReferenceCollection> CollectionChanged;

        private void OnElementChanged(TypeReference typeRef)
        {
            var previousArray = new TypeReference[_array.Length];

            for (int i = 0; i < _array.Length; i++)
            {
                var currentTypeRef = _array[i];

                if (currentTypeRef == typeRef)
                {
                    previousArray[i] = new TypeReference();
                }
                else
                {
                    previousArray[i] = currentTypeRef;
                }
            }

            var previousCollection = new TypeReferenceCollection(previousArray);

            CollectionChanged?.Invoke(previousCollection, this);
        }
    }
}
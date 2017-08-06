using System;
using System.Linq;
using System.Threading;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DotAdvancedDictionary
{
    /// <summary>
    /// Enhanced version of the regular dictionary which includes some cool
    /// features including auto load value if not available and so on.
    /// </summary>
    public sealed class AdvancedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Private Data Members
        private int maxWeight;
        private volatile int currentWeight;
        private int cleanupInProgress;
        private Func<TValue, int> GetDataWeight;
        private byte cleanupThresholdPercentage;
        private Func<TKey, TValue> AutoValueRetrieval;
        private Dictionary<TKey, CustomKeyValuePair<DateTime, TValue>> internalDictionary;
        private ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();
        #endregion

        #region Public Data Members
        public int CurrentWeight => currentWeight;
        public int Count => internalDictionary.Count;
        public ICollection<TKey> Keys
        {
            get
            {
                readerWriterLock.EnterReadLock();

                try
                {
                    return internalDictionary.Keys;
                }
                finally
                {
                    readerWriterLock.ExitReadLock();
                }
            }
        }
        public ICollection<TValue> Values
        {
            get
            {
                readerWriterLock.EnterReadLock();

                try
                {
                    return internalDictionary.Values.Where(x => x != null).Select(x => x.Value).ToList();
                }
                finally
                {
                    readerWriterLock.ExitReadLock();
                }
            }
        }

        // Indexer
        public TValue this[TKey key]
        {
            get => Get(key);
            set => AddItemHelper(key, value);
        }
        #endregion

        #region Constructors
        public AdvancedDictionary(int maxWeight, byte cleanupThresholdPercentage = 20, Func<TValue, int> getDataWeight = null, Func<TKey, TValue> autoValueRetrieval = null, IEqualityComparer<TKey> comparer = null)
        {
            #region Initialization
            currentWeight = 0;
            this.cleanupInProgress = 0;
            this.maxWeight = maxWeight;
            this.cleanupThresholdPercentage = cleanupThresholdPercentage;
            this.GetDataWeight = getDataWeight ?? DefaultWeightProvider;
            this.AutoValueRetrieval = autoValueRetrieval;
            internalDictionary = comparer != null ? new Dictionary<TKey, CustomKeyValuePair<DateTime, TValue>>(comparer) : new Dictionary<TKey, CustomKeyValuePair<DateTime, TValue>>();
            #endregion

            #region Validation
            if (cleanupThresholdPercentage > 50)
            {
                throw new ArgumentException("Cleanup threshold should not be more than 50 percent.");
            }
            #endregion
        }
        #endregion

        #region IDictionary Implementation
        public bool IsReadOnly => ((IDictionary)internalDictionary).IsReadOnly;

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            #region Removing Key
            return Remove(item.Key);
            #endregion
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            #region Adding Item
            Add(item.Key, item.Value);
            #endregion
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            #region Searching Key
            return ContainsKey(item.Key);
            #endregion
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            #region Copying Elements
            if (array == null)
                throw new ArgumentNullException(nameof(array));
            if (arrayIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            if (array.Rank > 1)
                throw new ArgumentException("Array is multidimensional.");
            if (array.Length - arrayIndex < Count)
                throw new ArgumentException("Not enough elements after index in the destination array.");

            // Acquiring read lock.
            readerWriterLock.EnterReadLock();

            try
            {
                var index = 0;

                foreach (var item in internalDictionary)
                {
                    array[index + arrayIndex] = new KeyValuePair<TKey, TValue>(item.Key, item.Value.Value);
                    index++;
                }
            }
            finally
            {
                // Releasing lock.
                readerWriterLock.ExitReadLock();
            }
            #endregion
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            #region Return
            return new Enumerator(ref internalDictionary, ref readerWriterLock);
            #endregion
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            #region Return
            return new Enumerator(ref internalDictionary, ref readerWriterLock);
            #endregion
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Check if the given key is present in the dictionary.
        /// </summary>
        /// <param name="key">The key to search</param>
        /// <returns>True if key found, false otherwise</returns>
        public bool ContainsKey(TKey key)
        {
            #region Checking Key Existence
            // Acquiring read lock.
            readerWriterLock.EnterReadLock();

            try
            {
                return internalDictionary.ContainsKey(key);
            }
            finally
            {
                // Releasing lock.
                readerWriterLock.ExitReadLock();
            }
            #endregion
        }

        /// <summary>
        /// Returns the value stored against the given key. 
        /// If key not found, will throw the exception.
        /// </summary>
        /// <param name="key">The key to search</param>
        /// <returns>The value stored against the key</returns>
        public TValue Get(TKey key)
        {
            #region Business Description
            // 1- If key would not be found in the dictionary,
            //    and auto value retrieval delegate is not null
            //    then it will try to load the value.
            #endregion

            #region Finding Key
            // Acquiring read lock.
            readerWriterLock.EnterUpgradeableReadLock();

            try
            {
                var result = default(TValue);
                CustomKeyValuePair<DateTime, TValue> data;

                if (internalDictionary.TryGetValue(key, out data))
                {
                    data.Key = DateTime.UtcNow;
                    result = data.Value;
                }
                else if (AutoValueRetrieval != null)
                {
                    try
                    {
                        result = AutoValueRetrieval(key);

                        AddItemHelper(key, result);
                    }
                    catch (Exception)
                    {
                        throw new KeyNotFoundException("The given key was not present in the dictionary.");
                    }
                }
                else
                {
                    throw new KeyNotFoundException("The given key was not present in the dictionary.");
                }

                return result;
            }
            finally
            {
                // Releasing lock.
                readerWriterLock.ExitUpgradeableReadLock();
            }
            #endregion
        }

        /// <summary>
        /// Returns the value stored against the given key. 
        /// If key not found, will NOT throw the exception.
        /// </summary>
        /// <param name="key">The key to search</param>
        /// <param name="value">The value against the key</param>
        /// <returns>If key found, returns true and set the out parameter; otherwise false</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            #region Finding Key
            try
            {
                value = Get(key);

                return true;
            }
            catch (Exception)
            {
                value = default(TValue);

                return false;
            }
            #endregion
        }

        /// <summary>
        /// Adds the item into dictionary. If key already exists,
        /// will throw the exception.
        /// </summary>
        /// <param name="key">The key to add</param>
        /// <param name="value">The value against the key</param>
        public void Add(TKey key, TValue value)
        {
            #region Adding KeyValue Pair
            // Acquiring read lock.
            readerWriterLock.EnterUpgradeableReadLock();

            try
            {
                if (internalDictionary.ContainsKey(key))
                {
                    throw new ArgumentException("An element with the same key already exists.");
                }

                AddItemHelper(key, value);
            }
            finally
            {
                // Releasing lock.
                readerWriterLock.ExitUpgradeableReadLock();
            }
            #endregion
        }

        /// <summary>
        /// Removes the key from the dictionary if found.
        /// </summary>
        /// <param name="key">The key to remove</param>
        /// <returns>True if key found and removed; otherwise false</returns>
        public bool Remove(TKey key)
        {
            #region Removing Key
            // Acquiring read lock.
            readerWriterLock.EnterUpgradeableReadLock();

            try
            {
                var result = false;

                if (internalDictionary.ContainsKey(key))
                {
                    // Acquiring write lock.
                    readerWriterLock.EnterWriteLock();

                    try
                    {
                        var valueToBeRemoved = internalDictionary[key];
                        result = internalDictionary.Remove(key);

                        // Updating weight.
                        currentWeight -= GetDataWeight(valueToBeRemoved.Value);
                    }
                    finally
                    {
                        // Releasing lock.
                        readerWriterLock.ExitWriteLock();
                    }
                }

                return result;
            }
            finally
            {
                // Releasing lock.
                readerWriterLock.ExitUpgradeableReadLock();
            }
            #endregion
        }

        /// <summary>
        /// Removes all the items from dictionary.
        /// </summary>
        public void Clear()
        {
            #region Clearing
            // Acquiring write lock.
            readerWriterLock.EnterWriteLock();

            try
            {
                internalDictionary.Clear();

                // Updating weight.
                currentWeight = 0;
            }
            finally
            {
                // Releasing lock.
                readerWriterLock.ExitWriteLock();
            }
            #endregion
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Helper method to add the item into dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void AddItemHelper(TKey key, TValue value)
        {
            #region Adding Item
            // Acquiring write lock.
            readerWriterLock.EnterWriteLock();

            try
            {
                internalDictionary[key] = new CustomKeyValuePair<DateTime, TValue> { Key = DateTime.UtcNow, Value = value };

                // Updating weight.
                currentWeight += GetDataWeight(value);
            }
            finally
            {
                // Releasing lock.
                readerWriterLock.ExitWriteLock();
            }
            #endregion

            #region Cleanup Check
            if (currentWeight >= maxWeight && Interlocked.CompareExchange(ref cleanupInProgress, 1, 0) == 0)
            {
                Task.Run(() => CleanupDictionary());
            }
            #endregion
        }

        private int DefaultWeightProvider(TValue value)
        {
            #region Return
            return 1;
            #endregion
        }

        private void CleanupDictionary()
        {
            #region Business Description
            // 1- When the dictionary will reach to the max weight,
            //    old values would be evicted based on the lease used.
            #endregion

            #region Dictionary Cleanup
            try
            {
                double allowedWeight = maxWeight - (((double)cleanupThresholdPercentage / 100) * maxWeight);
                var itemsToCleanup = internalDictionary.Where(x => x.Value != null)
                    .OrderBy(x => x.Value.Key);

                // Acquiring write lock.
                readerWriterLock.EnterWriteLock();

                try
                {
                    foreach (var itemToRemove in itemsToCleanup)
                    {
                        if (internalDictionary.Remove(itemToRemove.Key))
                        {
                            // Updating weight.
                            currentWeight -= GetDataWeight(itemToRemove.Value.Value);
                        }

                        // Check if has been cleaned up to the set percentage.
                        if (currentWeight <= allowedWeight)
                        {
                            break;
                        }
                    }
                }
                finally
                {
                    // Releasing lock.
                    readerWriterLock.ExitWriteLock();
                }
            }
            finally
            {
                Interlocked.Exchange(ref cleanupInProgress, 0);
            }
            #endregion
        }
        #endregion

        #region Enumerator Implementation
        [Serializable]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            #region Private Data Members
            private int index;
            private KeyValuePair<TKey, TValue> current;
            private ReaderWriterLockSlim readerWriterLock;
            private Dictionary<TKey, CustomKeyValuePair<DateTime, TValue>> dictionary;
            #endregion

            #region Constructors
            internal Enumerator(ref Dictionary<TKey, CustomKeyValuePair<DateTime, TValue>> dictionary, ref ReaderWriterLockSlim readerWriterLock)
            {
                #region Initialization
                index = 0;
                current = new KeyValuePair<TKey, TValue>();
                this.dictionary = dictionary;
                this.readerWriterLock = readerWriterLock;
                #endregion
            }
            #endregion

            #region IEnumerator Implementation
            public KeyValuePair<TKey, TValue> Current => current;

            object IEnumerator.Current => current;

            public bool MoveNext()
            {
                #region Moving Next
                // Acquiring read lock.
                readerWriterLock.EnterReadLock();

                try
                {
                    if (dictionary.Count == 0 || dictionary.Count <= index)
                    {
                        current = new KeyValuePair<TKey, TValue>();
                        return false;
                    }

                    current = new KeyValuePair<TKey, TValue>(dictionary.Keys.ElementAt(index), dictionary.ElementAt(index).Value.Value);
                    index++;

                    return true;
                }
                finally
                {
                    // Releasing lock.
                    readerWriterLock.ExitReadLock();
                }
                #endregion
            }

            public void Reset()
            {
                #region Resetting
                index = 0;
                current = new KeyValuePair<TKey, TValue>();
                #endregion
            }

            public void Dispose()
            {
            }
            #endregion
        }
        #endregion
    }
}

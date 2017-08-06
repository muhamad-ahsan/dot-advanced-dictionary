namespace DotAdvancedDictionary
{
    /// <summary>
    /// Helper class for custom dictionary.
    /// </summary>
    /// <typeparam name="TKey">The type of the key</typeparam>
    /// <typeparam name="TValue">The type of the value</typeparam>
    internal class CustomKeyValuePair<TKey, TValue>
    {
        #region Public Data Members
        public TKey Key { get; set; }
        public TValue Value { get; set; }
        #endregion
    }
}

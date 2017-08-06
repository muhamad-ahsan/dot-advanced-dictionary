namespace DotAdvancedDictionary.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initializing advanced dictionary.
            AdvancedDictionary<string, string> myAdvancedDictionary = new AdvancedDictionary<string, string >(250, 30, GetDataWeight, GetValue);

            // Adding items - method way.
            myAdvancedDictionary.Add("key1", "value1");
            myAdvancedDictionary.Add("key2", "value1");

            // Adding items - indexer way.
            myAdvancedDictionary["key3"] =  "value3";
            myAdvancedDictionary["key4"] = "value4";

            // Getting Key - Not exists - Will be retrieved from delegate.
            var myValue = myAdvancedDictionary["key5"];

            // Printing all the keys and values.
            foreach (var currentItem in myAdvancedDictionary)
            {
                System.Console.WriteLine("Key = {0}, Value = {1}", currentItem.Key, currentItem.Value);
            }

            System.Console.ReadKey();
        }

        /// <summary>
        /// Delegate to compute the weight for a given data.
        /// </summary>
        public static int GetDataWeight(string data)
        {
            return 5;
        }

        /// <summary>
        /// Delegate to retrieve the value if key not found in the dictionary.
        /// </summary>
        public static string GetValue(string key)
        {
            return "Value from Delegate for key = " + key;
        }
    }
}

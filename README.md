# About
[![License: GPL v3](https://img.shields.io/badge/License-GPL%20v3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

Advanced dictionary for .Net is regular dictionary with some advanced features. This include maximum size limit to avoid out-of-memory issues, auto value retrieval if key not found, auto cleanup of old keys if more space is required and multi-threading (concurrent access). It is perfect for in-memory cache and for other general purposes.

# Usage
The usage is same as regular dictionary with some extra parameters (only weight is required; others are optional) while creating dictionary object. Please see the below example:

```C#
public void DictionaryUsageExample()
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
```

# Nuget
The package can be downloaded from nuget with the following link:

[![NuGet](https://img.shields.io/badge/nuget-v1.0.0.1-blue.svg)](https://www.nuget.org/packages/DotAdvancedDictionary/1.0.0.1)

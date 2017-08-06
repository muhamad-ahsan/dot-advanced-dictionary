using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotAdvancedDictionary.Test
{
    /// <summary>
    /// Contains tests for Advanced Dictionary class.
    /// </summary>
    [TestClass]
    public class AdvancedDictionaryTest
    {
        [TestMethod]
        public void Initialize_WithdDefaults_ShouldCreateDictionaryWithDefaultParameters()
        {
            // Action
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);
        }

        [TestMethod]
        public void Initialize_WithdParameters_ShouldCreateDictionaryWithProvidedParameters()
        {
            // Action
            var myDictionary = new AdvancedDictionary<string, string>(100, 10, (s => 2), (s => "test value"));

            // Assert
            Assert.IsNotNull(myDictionary);
        }

        [TestMethod]
        public void Initialize_CurrentWeightShouldBeZero()
        {
            // Action
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);
            Assert.AreEqual(0, myDictionary.CurrentWeight);
        }

        [TestMethod]
        public void Add_ShouldAddItem()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Add("MyKey", "MyValue");

            // Assert
            Assert.AreEqual("MyValue", myDictionary["MyKey"]);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_ExistingKey_ShouldThrowException()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Add("MyKey", "MyValue");
            myDictionary.Add("MyKey", "MyValue2");
        }

        [TestMethod]
        public void ContainsKey_ShouldReturnBool()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Add("MyKey", "MyValue");

            // Assert
            Assert.IsTrue(myDictionary.ContainsKey("MyKey"));
            Assert.IsFalse(myDictionary.ContainsKey("InvalidKey"));
        }

        [TestMethod]
        public void Get_ShouldReturnItem()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary["MyKey"] = "MyValue";

            // Assert
            Assert.AreEqual("MyValue", myDictionary["MyKey"]);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void Get_InvalidKey_ShouldThrowException()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Get("MyKey");
        }

        [TestMethod]
        public void Get_NotAddedKey_WithAutoRetrievalDelegate_ShouldReturnItem()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100, 20, null, (s => "Auto Retrieved Value"));

            // Assert
            Assert.IsNotNull(myDictionary);
            Assert.AreEqual("Auto Retrieved Value", myDictionary["MyKey"]);
        }

        [TestMethod]
        public void TryGetValue_ShouldReturnBool_AndValueSetInOutParameter()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Add("MyKey", "MyValue");

            // Assert
            Assert.IsTrue(myDictionary.TryGetValue("MyKey", out string data));
            Assert.IsFalse(myDictionary.TryGetValue("InvalidKey", out string data2));
        }

        [TestMethod]
        public void Remove_ShouldRemoveItem()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Add("MyKey", "MyValue");

            // Assert
            Assert.IsTrue(myDictionary.Remove("MyKey"));
        }

        [TestMethod]
        public void Clear_ShouldRemoveAllItems()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Add("MyKey", "MyValue");
            myDictionary.Add("MyKey1", "MyValue");
            myDictionary.Add("MyKey2", "MyValue");
            myDictionary.Add("MyKey3", "MyValue");

            myDictionary.Clear();

            // Assert
            Assert.AreEqual(0, myDictionary.Count);
        }

        [TestMethod]
        public void Count_ShouldReturnTotalItemsCount()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Add("MyKey", "MyValue");
            myDictionary.Add("MyKey1", "MyValue");
            myDictionary.Add("MyKey2", "MyValue");
            myDictionary.Add("MyKey3", "MyValue");

            // Assert
            Assert.AreEqual(4, myDictionary.Count);
        }

        [TestMethod]
        public void ItemWeight_WithDefault_ShouldUseDefaultWeight()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Add("MyKey", "MyValue");
            myDictionary.Add("MyKey1", "MyValue");
            myDictionary.Add("MyKey2", "MyValue");
            myDictionary.Add("MyKey3", "MyValue");

            // Assert
            Assert.AreEqual(4, myDictionary.CurrentWeight);
        }

        [TestMethod]
        public void ItemWeight_WithCustomDelegate_ShouldUseCustomDelegate()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(100, 20, (s => 5));

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Add("MyKey", "MyValue");
            myDictionary.Add("MyKey1", "MyValue");
            myDictionary.Add("MyKey2", "MyValue");
            myDictionary.Add("MyKey3", "MyValue");

            // Assert
            Assert.AreEqual(20, myDictionary.CurrentWeight);
        }

        [TestMethod]
        public void Add_DictionaryWithMaxWeight_ShouldAddNewItem_RemoveOldest()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<string, string>(5);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Action
            myDictionary.Add("MyKey1", "MyValue1");
            myDictionary.Add("MyKey2", "MyValue2");
            myDictionary.Add("MyKey3", "MyValue3");
            myDictionary.Add("MyKey4", "MyValue4");
            myDictionary.Add("MyKey5", "MyValue5");
            myDictionary.Add("MyKey6", "MyValue6");

            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(5));

            // Assert
            Assert.IsFalse(myDictionary.TryGetValue("MyKey1", out string data1));
            Assert.IsTrue(myDictionary.TryGetValue("MyKey6", out string data2));
        }

        [TestMethod]
        public void ForEachLoop_ShouldIterateThroughDictionary()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<int, string>(14000);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Arrange
            for (int i = 0; i < 10000; i++)
            {
                myDictionary.Add(i, "Current Loop Number " + i);
            }

            // Separate thread to add more values to check the concurrent access.
            var addItemsTask = Task.Delay(200).ContinueWith((task =>
            {
                for (int i = 10000; i < 20000; i++)
                {
                    try
                    {
                        myDictionary.Add(i, "Current Loop Number " + i);
                    }
                    catch (Exception e)
                    {
                        Assert.Fail("Exception in Task. " + e.Message);
                    }
                }
            }));

            // Action
            foreach (var currentItem in myDictionary)
            {
                Console.WriteLine("Key {0}, Value: {1}", currentItem.Key, currentItem.Value);
            }

            addItemsTask.Wait();
        }

        [TestMethod]
        public void LINQMethods_ShouldBeWorking()
        {
            // Arrange
            var myDictionary = new AdvancedDictionary<int, string>(1500);

            // Assert
            Assert.IsNotNull(myDictionary);

            // Arrange
            for (int i = 0; i < 1000; i++)
            {
                myDictionary.Add(i, "Number " + i);
            }

            // Assert - First()
            var firstItem = myDictionary.First();

            Assert.AreEqual("Number 0", firstItem.Value);

            // Assert - Any
            Assert.IsTrue(myDictionary.Any(x => x.Key == 10));

            // Assert - Max
            Assert.AreEqual(999, myDictionary.Max(x => x.Key));

            // Assert - Where
            Assert.IsTrue(myDictionary.Where(x => x.Key == 500).Any());

            // Assert - ToArray
            var myArray = myDictionary.ToArray();

            Assert.AreEqual(1000, myArray.Length);
        }
    }
}

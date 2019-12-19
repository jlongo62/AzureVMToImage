using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DB.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var context = new DB.VMSSManagementEntities();
            var partitionKey = "2a3e4c31-368d-4507-a918-e17383590a76";
            context.ManagementItems.Add(new ManagementItem()
            {
                PartitionKey = partitionKey,
                RowKey = Guid.NewGuid().ToString(),
                sourceResourceId = "sourceResourceId",
                imagesLocation = "imagesLocation",
                imagesResourceGroup = "imagesResourceGroup",
                imagePrefix = "imagePrefix",
                imageVersion = "version"
            });

            context.SaveChanges();

        }
        [TestMethod]
        public void TestMethod2()
        {
            using (var context = new DB.VMSSManagementEntities())
            {
                foreach(var item in context.ManagementItems)
                {
                    Trace.WriteLine(item.PartitionKey.ToString());
                }
            }


        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Script.Services;
using System.Web.Services;
using DB;
//using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.Base;

namespace VMSSManagmentConsole.Controllers
{
    public class TestGrid2Controller : Controller
    {
        public ActionResult TestGrid2()
        {
            ViewBag.Title = "ScaleSet Management Page";
            ViewBag.Message = "Associate Scaleset(s) with Virtual Machine Reference Instances";

            using (var context = new VMSSManagementEntities())
            {
                ViewBag.dataSource = context.ManagementItems.ToArray();
                return View();
            }
        }

        //public ActionResult UrlDatasource([FromBody]DataManagerRequest dm)
        //{
        //    IEnumerable DataSource = db.ManagementItems.ToList();
        //    DataOperations operation = new DataOperations();
        //    int count = DataSource.Cast<ManagementItem>().Count();
        //    if (dm.Skip != 0)
        //    {
        //        DataSource = operation.PerformSkip(DataSource, dm.Skip);   //Paging
        //    }
        //    if (dm.Take != 0)
        //    {
        //        DataSource = operation.PerformTake(DataSource, dm.Take);
        //    }
        //    var result = (ActionResult)(dm.RequiresCounts ? Json(new { result = DataSource, count = count }) : Json(DataSource));
        //    return result;
        //}

        //public ActionResult CrudUpdate([FromBody]ICRUDModel<ManagementItem> value, string action)
        //{
        //    //if (value.action == "update")
        //    //{
        //    //    var ord = value.value;
        //    //    ManagementItem val = db.ManagementItems.Where(or => or.RowKey == ord.RowKey).FirstOrDefault();
        //    //    val.imagePrefix = ord.imagePrefix;
        //    //    val.imagesLocation = ord.imagesLocation;
        //    //    val.imagesResourceGroup = ord.imagesResourceGroup;
        //    //    val.imageVersion = ord.imageVersion;
        //    //    val.sourceResourceId = ord.sourceResourceId;
        //    //}
        //    //else if (value.action == "insert")
        //    //{
        //    //    db.ManagementItems.Add(value.value);
        //    //}
        //    //else if (value.action == "remove")
        //    //{
        //    //    db.ManagementItems.Remove(db.ManagementItems.Where(or => or.RowKey == value.key.ToString()).FirstOrDefault());
        //    //    return Json(value);
        //    //}
        //    //return Json(value.value);
        //    return null;
        //}

        public ActionResult SrcUpdate(CRUDInstance<ManagementItem> newItem)
        {
            using(var context = new VMSSManagementEntities())
            {
                var item = context.ManagementItems.Where(x => x.RowKey == newItem.key).FirstOrDefault();
                context.Entry(item).CurrentValues.SetValues(newItem.Value);
                context.SaveChanges();
            }

            return Json(newItem.Value);

        }
        public ActionResult SrcInsert(CRUDInstance<ManagementItem> newItem)
        {
            using (var context = new VMSSManagementEntities())
            {
                string partitionKey = null;

                var first = context.ManagementItems.FirstOrDefault();
                if (first==null)
                {
                    partitionKey = Guid.NewGuid().ToString();
                }
                else
                {
                    partitionKey = first.PartitionKey;
                }

                newItem.Value.PartitionKey = partitionKey;
                newItem.Value.RowKey = Guid.NewGuid().ToString();

                context.ManagementItems.Add(newItem.Value);
                context.SaveChanges();
            }

            return Json(newItem.Value);
        }
        public ActionResult SrcRemove(string key)
        {
            using (var context = new VMSSManagementEntities())
            {
                var item = context.ManagementItems.Where(x => x.RowKey == key).FirstOrDefault();
                context.ManagementItems.Remove(item);
                context.SaveChanges();

                var items = context.ManagementItems.ToArray();
                return Json(new { result = items, count = items.Length });
            }

        }
        public class CRUDInstance<T> where T : class
        {
            public List<T> Added { get; set; }
            public List<T> Changed { get; set; }
            public List<T> Deleted { get; set; }
            public T Value { get; set; }
            public string key { get; set; }
            public string action { get; set; }
        }
        public class ICRUDModel<T> where T : class
        {
            public string action { get; set; }

            public string table { get; set; }

            public string keyColumn { get; set; }

            public object key { get; set; }

            public T value { get; set; }

            public List<T> added { get; set; }

            public List<T> changed { get; set; }

            public List<T> deleted { get; set; }

            public IDictionary<string, object> @params { get; set; }
        }

    }
}
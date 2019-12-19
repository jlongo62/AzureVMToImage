using System.Linq;
using System.Web.Mvc;
using DB;
using Syncfusion.EJ2.Base;

namespace VMSSManagementWeb.Controllers
{
   public class ManagementController : Controller
   {
        private VMSSManagementEntities db = new VMSSManagementEntities();
        public ActionResult Management()
        {

            ViewBag.Title = "ScaleSet Management Page";
            ViewBag.Message1 = "Associate Virtual Machine Reference Instances with Images";
            ViewBag.Message2 = "Associate Scaleset(s) with Virtual Machine Reference Instances";

            using (var context = new VMSSManagementEntities())
            {
                //foreach (var source in context.Sources)
                //{
                //    var targets = source.Targets.ToArray();
                //}
                var items = (from s in context.Sources select s).ToArray();

                ViewBag.dataSource = items;
                return View();
            }
        }

        public ActionResult Update(CRUDModel<Source> newItem)
        {
            //using (var context = new VMSSManagementEntities())
            //{
            //    var item = context.ManagementItems.Where(x => x.RowKey == newItem.key).FirstOrDefault();
            //    context.Entry(item).CurrentValues.SetValues(newItem.Value);
            //    context.SaveChanges();
            //}

            return Json(newItem.Value);

        }
        public ActionResult Insert(CRUDModel<Source> newItem)
        {
            using (var context = new VMSSManagementEntities())
            {
                newItem.Value.status = "unknown";
                context.Sources.Add(newItem.Value);
                context.SaveChanges();
            }

            return Json(newItem.Value);
        }
        public ActionResult Remove(string key)
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

    }
}
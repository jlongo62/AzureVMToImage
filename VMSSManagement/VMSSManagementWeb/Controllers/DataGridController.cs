using Syncfusion.EJ2.Base;
using System.Collections;
using System.Linq;
using System.Web.Mvc;
using DB;
namespace VMSSManagementWeb.Controllers
{
  public class DataGridController : Controller
  {
	    private VMSSManagementEntities db = new VMSSManagementEntities();
        public ActionResult Index()
        {
	        return View();
        }
        public ActionResult UrlDatasource(DataManagerRequest dm)
        {

            IEnumerable DataSource = db.Sources.ToList();

            DataOperations operation = new DataOperations();   
            int count = DataSource.Cast<Source>().Count();
            if (dm.Skip != 0)//Paging
            {
                DataSource = operation.PerformSkip(DataSource, dm.Skip);         
            }
            if (dm.Take != 0)
            {
                DataSource = operation.PerformTake(DataSource, dm.Take);
            }
            return dm.RequiresCounts ? Json(new { result = DataSource, count = count }, JsonRequestBehavior.AllowGet) : Json(DataSource);
        }
        public ActionResult Insert(Source value)
        {
            //do stuff
            return Json(value);
        }
        public ActionResult Update(Source value)
        {
            //do stuff
            return Json(value);
        }
        public ActionResult Delete(int key)
        {
            //do stuff
            return View();
        }
   }
}
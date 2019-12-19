using System.Linq;
using System.Web.Mvc;
using DB;
namespace VMSSManagementWeb.Controllers
{
   public class TestGridController : Controller
   {
        private VMSSManagementEntities db = new VMSSManagementEntities();
        public ActionResult TestGrid()
        {

            ViewBag.dataSource = db.ManagementItems.ToList();

            return View();
        }

    }
}
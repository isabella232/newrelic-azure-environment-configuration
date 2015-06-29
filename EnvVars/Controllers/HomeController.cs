using System.Text;
using System.Web.Mvc;
using EnvVars.Models;

namespace EnvVars.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(bool getRoleData = false)
        {

            if (getRoleData)
            {
                var roleData = RoleModel.GetRoleDetails();
                var cloudServiceInfo = new StringBuilder();

                if (roleData != null)
                {
                    cloudServiceInfo.AppendFormat("<div>{0}: {1}</div><br/>", "DeploymentSlot", roleData.DeploymentSlot);
                    cloudServiceInfo.AppendFormat("<div>{0}: {1}</div><br/>", "Location", roleData.Location);
                    cloudServiceInfo.AppendFormat("<div>{0}: {1}</div><br/>", "Formatted Name",
                        roleData.FormatName("ext"));
                }
                else
                {
                    cloudServiceInfo.AppendFormat("<div>{0}</div><br/>", "No data");
                }
                ViewBag.CloudServiceInfo = cloudServiceInfo.ToString();
            }
            else
            {
                ViewBag.CloudServiceInfo = "Fetching role data is disabled.  Enable by using the querystring parameter: getRoleData=true ";
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

    }
}
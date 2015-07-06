using System.Linq;
using System.Xml.Linq;
using EnvVars.Helpers;
using JetBrains.Annotations;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace EnvVars.Models
{
    public class RoleModel
    {

        public string RoleName { get; set; }

        public string DeploymentSlot { get; set; }

        public string Location { get; set; }

        public string FormatName(string nameFragment)
        {
            return string.Format("{0}.{1}.{2}", nameFragment, DeploymentSlot, Location.Replace(" ", ""));
        }

        [CanBeNull]
        public static RoleModel GetRoleDetails()
        {
            RoleModel roleInfo = null;

            var hostedServiceNames = ServiceManagementRequest.GetHostedServiceNames();

            if (hostedServiceNames.Count <= 0) return null;

            foreach (var hostedServiceName in hostedServiceNames)
            {
                if (string.IsNullOrEmpty(hostedServiceName)) continue;

                var data = ServiceManagementRequest.GetHostedService(hostedServiceName);

                if (data == null) continue;

                var deploymentLocation =
                    data.Element(XName.Get("HostedServiceProperties", ServiceManagementRequest.ANS))
                        .Element(XName.Get("Location", ServiceManagementRequest.ANS)).Value;

                var deploymentXElements =
                    data.Elements(XName.Get("Deployments", ServiceManagementRequest.ANS))
                        .Elements(XName.Get("Deployment", ServiceManagementRequest.ANS))
                        .ToList();

                if (deploymentXElements.Count <= 0) continue;

                foreach (
                    var deploymentSlotName in from deployment in deploymentXElements
                                              where deployment != null
                                              let currentDeploymentId = deployment.Element(XName.Get("PrivateID", ServiceManagementRequest.ANS)).Value
                                              where currentDeploymentId == RoleEnvironment.DeploymentId
                                              select deployment.Element(XName.Get("DeploymentSlot", ServiceManagementRequest.ANS)).Value
                    )
                {
                    roleInfo = new RoleModel()
                    {
                        RoleName = hostedServiceName,
                        DeploymentSlot = deploymentSlotName,
                        Location = deploymentLocation
                    };
                    break;
                }

                //Exit as soon as the role is found
                if (roleInfo != null) break;
            }
            return roleInfo;

        }

    }
}
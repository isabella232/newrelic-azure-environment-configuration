using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using EnvVars.Models;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace EnvVars
{
    public class MvcApplication : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            SetNewRelicAppNameDynamic();
            //SetNewRelicAppNameStatic();
            //SetNewRelicAppNameFromRoleEnvironment();

        }

        private static void SetNewRelicAppNameDynamic()
        {
            var appName = "AzureEnvCloudService";
            try
            {
                var roleData = RoleModel.GetRoleDetails();
                if (roleData != null)
                {
                    appName = roleData.FormatName("AzureEnv");
                }
            }
            catch (Exception)
            {
                //swallow  
            }

            NewRelic.Api.Agent.NewRelic.SetApplicationName(appName);
            NewRelic.Api.Agent.NewRelic.StartAgent();

        }

        private static void SetNewRelicAppNameStatic()
        {
            NewRelic.Api.Agent.NewRelic.SetApplicationName("AzureEnvCloudService");
            NewRelic.Api.Agent.NewRelic.StartAgent();
        }

        private static void SetNewRelicAppNameFromRoleEnvironment()
        {
            var appName = "AzureEnvCloudService";
            try
            {
                if (RoleEnvironment.IsAvailable)
                {
                    appName = RoleEnvironment.GetConfigurationSettingValue("NewRelic.AppName");
                }
            }
            catch (Exception)
            {
                //swallow  
            }

            NewRelic.Api.Agent.NewRelic.SetApplicationName(appName);
            NewRelic.Api.Agent.NewRelic.StartAgent();

        }

    }
}

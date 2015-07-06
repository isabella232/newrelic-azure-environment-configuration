# newrelic-azure-environment-configuration

A simulation of this project can viewed @ [azureenvcloud.cloudapp.net](http://azureenvcloud.cloudapp.net/?getRoleData=true)
Note the section titled **Cloud Service Info**

This is a demo project that illustrates how to use the [New Relic .NET agent API](https://docs.newrelic.com/docs/agents/net-agent/features/net-agent-api) to set things dynamically, like an application's name, using information provided by the Azure management APIs.  This code illustrates how to use the New Relic [.NET Agent APIs](https://docs.newrelic.com/docs/agents/net-agent/features/net-agent-api) to dynamically assign an Application name that can be reported up to New Relic.  This also illustrates the use of the Azure management APIs (using certs based authentication) to get information for the given WebRole that is not currently available through the [Microsoft.WindowsAzure.ServiceRuntime APIs](https://msdn.microsoft.com/en-us/library/microsoft.windowsazure.serviceruntime.aspx) such as getting the Location of the deployed web role or the deployment slot.

The key here is that as your application moves through your environment your naming and data gathering needs might change and if you are hosting 1000 Cloud Services (for instance) then naming each individual VM something meaningful so that you can track down issues when they come up simply does not scale.

The New Relic .NET Agent team (in an effort to make facing the above challenge a bit easier) has [implemented some new APIs](https://docs.newrelic.com/docs/agents/net-agent/features/net-agent-api) for us to use. This project uses uses two of those new APIs: [StartAgent](https://docs.newrelic.com/docs/agents/net-agent/features/net-agent-api#start_agent) and [SetApplicationName](https://docs.newrelic.com/docs/agents/net-agent/features/net-agent-api#set_application_name) as well as a new feature that has been implemented to delay the [startup](https://docs.newrelic.com/docs/agents/net-agent/installation-configuration/net-agent-configuration#service-autoStart) of the agent until, we the users, specify!



### Setup

This was written assuming that you are a New Relic user and are using Azure cloud services.

One of the approaches in this code requires that you setup a [Management Certificate](https://msdn.microsoft.com/en-us/library/azure/gg551722.aspx).  Please pay close attention to the fact that you must currently upload the management certificate via the [classic portal](manage.windowsazure.com) >> Settings (the last menu item on the left side) >> Management Certificates.  Once you've uploaded the cert make sure to take note of the certificate thumbprint - you need to add a Service Configuration setting using that value. Read [this](https://msdn.microsoft.com/en-us/library/azure/gg551722.aspx) before continuing if this is the option you'll be going with.



1. Begin by creating a cloud service in Azure
2. Add the [New Relic nuget package](https://www.nuget.org/packages/NewRelicWindowsAzure) to the Cloud Service project ``` PM> Install-Package NewRelicWindowsAzure ```
3. (Optional) Add Web Role settings - SubscriptionId - if you are planning on calling the [management APIs](https://msdn.microsoft.com/en-us/library/azure/ee460812.aspx), ManagmentCertificateThumbprint - if you are planning on calling the [management APIs](https://msdn.microsoft.com/en-us/library/azure/ee460812.aspx)
4. (Optional) Add a newrelic.config to the root of your web role so that you can disable the automatic startup of the agent on your role by adding ``` autoStart="false" ``` to the ``` service ``` element.  You'll want to do this so that your application does not report to New Relic before you name it.
5. Follow the implementation below to either statically or dynamically set the Application name.

**NOTE**
If you are using the solution files (not the files in the [Simplified-Implementation](./Simplified-Implementation) folder) then make sure to modify the AzureEnvCloudService project properties >> Certificates and Settings with your own settings and certificate thumbprint

![Alt text](/Simplified-Implementation/web_role_settings.png?raw=true "settings")

### Global.asax.cs

This is where the New Relic Application naming mechanism is implemented.  There are 3 choices in the example:

1. **SetNewRelicAppNameDynamic** - In this example we call the Azure Management APIs to pull back slot and location to dynamically name the application reporting as **AzureEnv-Production-WestUS**.  This is to illustrate the interesting ways we can name applications that are both dynamically obtained from the environment as well as being able to format that data to effectively name things to suit our needs.

2. **SetNewRelicAppNameStatic** - In this example we demonstrate how to simple name the application reporting and then start the agent to begin reporting to New Relic.  The interesting thing here is that if you have some sort of business or routing logic where you'd like a certain name reported then you could do something like what is in the example.  NOTE: this "renaming" the application causes the agent to restart so please take that into consideration if you are planning on doing some sort of route based naming.

3. **SetNewRelicAppNameFromRoleEnvironment** - Simply setting the name from a setting in the RoleEnvironment

```

 protected void Application_Start()
 {
	...
    
    SetNewRelicAppNameDynamic();
    //SetNewRelicAppNameStatic();
    //SetNewRelicAppNameFromRoleEnvironment();

}

private static void SetNewRelicAppNameDynamic()
{
    var appName = "AzureEnvCloudService";
    try
    {
         var roleData = RoleModel.GetRoleDetails(RoleEnvironment.GetConfigurationSettingValue("SubscriptionId"), RoleEnvironment.GetConfigurationSettingValue("ManagementCertificateThumbPrint"));
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

...

```

### /Utilss/ServiceManagementRequest.cs

This is a class that handles all of the requests to both the Management API as well as to the certificate store.  It requires that the webrole settings: SubscriptionId and ManagmentCertificateThumbprint.  

### /Utils/CertificateFactory.cs

This encapsulates the logic to go and get the management certificate out of the cert store using a given thumbprint

### /Models/RoleModel.cs

This encapsulates the structure of the role data that might interest you.  It also provides a static hydration method (just for simplicity and to illustrate how to get the information for the role that your code is currently executing in.

### newrelic.Config

This is where we can set the new attribute to delay the start up of the agent so that we can prevent the reporting multiple application names to New Relic

```
<service licenseKey="REPLACE_WITH_LICENSE_KEY" ssl="true" autoStart="false" />

``` 

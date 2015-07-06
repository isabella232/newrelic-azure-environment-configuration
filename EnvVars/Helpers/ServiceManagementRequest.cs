using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using JetBrains.Annotations;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace EnvVars.Helpers
{
    public class ServiceManagementRequest
    {
        [NotNull] public static string ANS = "http://schemas.microsoft.com/windowsazure";
        [NotNull]
        private const string HostedServicesUriFormatter = "https://management.core.windows.net/{0}/services/hostedservices";
        [NotNull]
        private const string HostedServicePropertiesUriFormatter = "https://management.core.windows.net/{0}/services/hostedservices/{1}?embed-detail=true";
        [CanBeNull]
        private static readonly X509Certificate2 certificate = null;

        private static string SubscriptionId
        {
            get
            {
                return RoleEnvironment.IsAvailable
                    ? RoleEnvironment.GetConfigurationSettingValue("SubscriptionId")
                    : string.Empty;
            }
        }

        private static string CertificateData
        {
            get
            {
                return RoleEnvironment.IsAvailable
                    ? RoleEnvironment.GetConfigurationSettingValue("ManagementCertificateThumbPrint")
                    : string.Empty;
            }
        }

        [CanBeNull]
        private static X509Certificate2 Certificate {
            get
            {
                return certificate ?? GetStoreCertificate(CertificateData);
            }
        }

        [NotNull]
        public static List<string> GetHostedServiceNames()
        {
            var names = new List<string>();

            try
            {
                var data = ExecuteWebRequest(string.Format(HostedServicesUriFormatter, SubscriptionId));

                if (data == null)
                    return names;

                var serviceNameElements = data.Elements().Elements(XName.Get("ServiceName", ANS));

                if (serviceNameElements == null)
                    return names;

                names.AddRange(serviceNameElements.Select(serviceElement => serviceElement.Value));
            }
            catch (Exception)
            {
                //Log Exception here but we do not want to halt execution of the application
            }
           
            return names;
        }

        [CanBeNull]
        public static XElement GetHostedService([NotNull] string hostedServiceName)
        {
            XElement data = null; 

            try
            {
                data = ExecuteWebRequest(string.Format(HostedServicePropertiesUriFormatter, SubscriptionId, hostedServiceName));
            }
            catch (Exception)
            {
                //Log Exception here but we do not want to halt execution of the application
            }

            return data;
        }

        [CanBeNull]
        private static XElement ExecuteWebRequest([NotNull] string uri)
        {
            var request = CreateWebRequest(new Uri(uri));
            XElement body;

            if (request == null) return null;

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                body = XElement.Load(responseStream);
            }
            return body;
        }

        [CanBeNull]
        private static HttpWebRequest CreateWebRequest([NotNull] Uri uri)
        {
            var request = (HttpWebRequest) WebRequest.Create(uri);

            if (Certificate == null) return null;

            request.Method = HttpMethod.Get.Method;
            request.ClientCertificates.Add(Certificate);
            request.Headers.Add("x-ms-version", "2014-06-01");
            request.ContentType = "application/xml";

            return request;
        }

        [CanBeNull]
        private static X509Certificate2 GetStoreCertificate([NotNull] string thumbprint)
        {
            var store = new X509Store("My", StoreLocation.LocalMachine);
            X509Certificate2 cert; 
            try
            {
                store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, true);
                cert = certificates.Count > 0 ? certificates[0] : null;
            }
            catch (Exception ex)
            {
                //Log Exception here but we do not want to halt execution of the application
                cert = null;
            }
            finally
            {
                store.Close();
            }

            return cert;

        }

    }
}
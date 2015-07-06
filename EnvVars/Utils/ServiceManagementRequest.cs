using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace EnvVars.Utils
{
    public class ServiceManagementRequest
    {
        [NotNull]
        public static string ANS = "http://schemas.microsoft.com/windowsazure";
        [NotNull]
        private const string HostedServicesUriFormatter = "https://management.core.windows.net/{0}/services/hostedservices";
        [NotNull]
        private const string HostedServicePropertiesUriFormatter = "https://management.core.windows.net/{0}/services/hostedservices/{1}?embed-detail=true";

        [NotNull]
        public static List<string> GetHostedServiceNames([NotNull] string subscriptionId, [NotNull] X509Certificate2 certificate)
        {
            var names = new List<string>();

            try
            {
                var data = ExecuteWebRequest(string.Format(HostedServicesUriFormatter, subscriptionId), certificate);

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
        public static XElement GetHostedService([NotNull] string hostedServiceName, [NotNull] string subscriptionId, [NotNull] X509Certificate2 certificate)
        {

            XElement data = null;

            try
            {
                data = ExecuteWebRequest(string.Format(HostedServicePropertiesUriFormatter, subscriptionId, hostedServiceName), certificate);
            }
            catch (Exception)
            {
                //Log Exception here but we do not want to halt execution of the application
            }

            return data;
        }

        [CanBeNull]
        private static XElement ExecuteWebRequest([NotNull] string uri, [NotNull] X509Certificate2 certificate)
        {

            var request = CreateWebRequest(new Uri(uri), certificate);
            XElement body;

            if (request == null) return null;

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                var responseStream = response.GetResponseStream();
                body = XElement.Load(responseStream);
            }
            return body;
        }

        [CanBeNull]
        public static HttpWebRequest CreateWebRequest([NotNull] Uri uri, [NotNull] X509Certificate2 certificate)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri);

            request.Method = HttpMethod.Get.Method;
            request.ClientCertificates.Add(certificate);
            request.Headers.Add("x-ms-version", "2014-06-01");
            request.ContentType = "application/xml";

            return request;
        }
    }
}
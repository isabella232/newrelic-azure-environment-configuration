using System;
using System.Security.Cryptography.X509Certificates;
using EnvVars.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EnvVars.Tests.Utils
{
    [TestClass]
    public class ServiceManagementRequestTests
    {
        [TestMethod]
        public void CreateWebRequest_returns_request_with_xmsversion_header()
        {
            var certificate = new X509Certificate2();

            var request = ServiceManagementRequest.CreateWebRequest(new Uri("http://foo.bar.baz"), certificate);
            Assert.IsNotNull(request.Headers.Get("x-ms-version"));
            Assert.AreEqual("2014-06-01", request.Headers.Get("x-ms-version"));
        }

        [TestMethod]
        public void CreateWebRequest_returns_request_with_xml_contenttype()
        {
            var certificate = new X509Certificate2();

            var request = ServiceManagementRequest.CreateWebRequest(new Uri("http://foo.bar.baz"), certificate);
            Assert.AreEqual("application/xml", request.ContentType);
        }
    }
}

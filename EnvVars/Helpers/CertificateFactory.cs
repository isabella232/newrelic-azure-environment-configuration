using System;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;

namespace EnvVars.Helpers
{
    public static class CertificateFactory
    {
        [CanBeNull]
        public static X509Certificate2 GetStoreCertificate([NotNull] string thumbprint)
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
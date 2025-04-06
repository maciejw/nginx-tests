namespace CertTools.Certificates;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public static class CertificateRequestBuilderExtensions
{
    public static X509KeyUsageFlags ToKeyUsage(this CertificateUsageType certificateUsageType)
    {
        X509KeyUsageFlags keyUsage = X509KeyUsageFlags.None;
        foreach (var item in Enum.GetValues<CertificateUsageType>())
        {
            if ((item & certificateUsageType) == CertificateUsageType.ServerAuthentication)
            {
                keyUsage |= X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature;
            }
            if ((item & certificateUsageType) == CertificateUsageType.ClientAuthentication)
            {
                keyUsage |= X509KeyUsageFlags.DigitalSignature;
            }
        }
        return keyUsage;
    }

    public static IEnumerable<Oid> ToEnhancedKeyUsages(this CertificateUsageType certificateUsageType)
    {
        foreach (var item in Enum.GetValues<CertificateUsageType>())
        {
            if ((item & certificateUsageType) == CertificateUsageType.ServerAuthentication)
            {
                yield return Oids.ServerAuthentication;
            }
            if ((item & certificateUsageType) == CertificateUsageType.ClientAuthentication)
            {
                yield return Oids.ClientAuthentication;
            }
        }

    }

}

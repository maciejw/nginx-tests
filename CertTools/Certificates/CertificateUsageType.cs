namespace CertTools.Certificates;

using System;

[Flags]
public enum CertificateUsageType
{
    ServerAuthentication = 1,
    ClientAuthentication = 2,
}

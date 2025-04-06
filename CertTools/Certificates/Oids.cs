using System.Security.Cryptography;

namespace CertTools.Certificates;

internal class Oids
{
    public static readonly Oid ClientAuthentication = new("1.3.6.1.5.5.7.3.2");
    public static readonly Oid ServerAuthentication = new("1.3.6.1.5.5.7.3.1");
}
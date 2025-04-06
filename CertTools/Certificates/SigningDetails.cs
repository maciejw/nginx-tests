using System.Security.Cryptography.X509Certificates;

namespace CertTools.Certificates;

public record struct SigningDetails(X500DistinguishedName IssuerName, X509SignatureGenerator IssuerSigningGenerator)
{
}
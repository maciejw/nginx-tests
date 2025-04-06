namespace CertTools.Certificates;
using System.Security.Cryptography.X509Certificates;

public interface ICertificateRequestBuilderWithWithSubjectKeyIdentifier
{
    ICertificateRequestBuilderWithWithSubjectKeyIdentifier AddAuthorityKeyIdentifierFromSubjectKeyIdentifier();
    CertificateRequest Build();
}

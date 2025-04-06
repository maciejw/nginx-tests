namespace CertTools.Certificates;
using System.Security.Cryptography.X509Certificates;

public interface ICertificateRequestBuilderWithPublicKey
{
    ICertificateRequestBuilderWithWithSubjectKeyIdentifier AddSubjectKeyIdentifier(bool critical = false);
    CertificateRequest Build();
}

using System.CommandLine;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertTools.Certificates;

public static class CertificateExtensions
{
    public static X509SignatureGenerator GetSignatureGenerator(this X509Certificate2 @this)
    {
        if (@this.HasPrivateKey == false)
        {
            throw new InvalidOperationException("Certificate does not have a private key");
        }

        if (@this.GetRSAPrivateKey() is { } rsa)
        {
            return X509SignatureGenerator.CreateForRSA(rsa, RSASignaturePadding.Pkcs1);
        }
        else if (@this.GetECDsaPrivateKey() is { } ecdsa)
        {
            return X509SignatureGenerator.CreateForECDsa(ecdsa);
        }
        else
        {
            throw new NotSupportedException("Unsupported private key type");
        }
    }

    public static SigningDetails GetSigningDetails(this X509Certificate2 @this)
    {
        return new SigningDetails(@this.SubjectName, @this.GetSignatureGenerator());
    }

    public static void SaveCertificateToPkcs12(this X509Certificate2 certificate, FileInfo outputPkcs12, IConsole? console = null)
    {
        using FileStream fileStream = outputPkcs12.OpenWrite();
        console?.WriteLine($"Saving pkcs12 to: {outputPkcs12}");
        fileStream.Write(certificate.Export(X509ContentType.Pkcs12));
    }

    public static X509Certificate2Collection CopyCertificatesToCollection(this X509Certificate2Collection certificateCollection)
    {
        var newCollection = new X509Certificate2Collection();

        certificateCollection
            .Select(item => X509CertificateLoader.LoadCertificate(item.Export(X509ContentType.Cert)))
            .ToList().ForEach(item => newCollection.Add(item));

        return newCollection;
    }

    public static void SaveCertificateToPkcs12(this X509Certificate2Collection certificateCollection, FileInfo outputPkcs12, IConsole? console = null)
    {
        using FileStream fileStream = outputPkcs12.OpenWrite();
        console?.WriteLine($"Saving pkcs12 to: {outputPkcs12}");
        fileStream.Write(certificateCollection.Export(X509ContentType.Pkcs12));
    }

    public static X509Certificate2 CreateCertificate(this CertificateRequest certificateRequest, SigningDetails signingDetails, int monthsValid = 12)
    {
        byte[] serialNumber = new byte[8];
        RandomNumberGenerator.Fill(serialNumber);

        X509Certificate2 certificate = certificateRequest.Create(signingDetails.IssuerName, signingDetails.IssuerSigningGenerator, DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddMonths(monthsValid), serialNumber);
        return certificate;
    }

    public static X509Certificate2 CreateSelfSignedCertificate(this CertificateRequest certificateRequest, X509SignatureGenerator x509SignatureGenerator, int monthsValid = 12)
    {
        return certificateRequest.CreateCertificate(new SigningDetails(certificateRequest.SubjectName, x509SignatureGenerator), monthsValid);

    }
}

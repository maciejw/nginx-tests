using CertTools.Certificates;
using CertTools.CommandBuilders;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertTools.Commands;

internal class NewRootCa : Command
{
    public NewRootCa() : base(NameExtensions.ToKebabCase("", nameof(NewRootCa)), "Creates new Root CA")
    {
        OptionBuilder.For<Handler>()
            .NewOption(h => h.CommonName).Configure(o => o.IsRequired = true).AddTo(this)
            .NewOption(h => h.OutputPkcs12).Configure(o =>
            {
                o.IsRequired = true;
                o.LegalFilePathsOnly();
            }).AddTo(this)
            ;
    }

    new internal class Handler(IConsole console) : ICommandHandler
    {
        public required string CommonName { get; set; }
        public required FileInfo OutputPkcs12 { get; set; }
        public int Invoke(InvocationContext context)
        {
            RSA rsa = RSA.Create(4096);

            var certificateRequest = CertificateRequestBuilder.New(dn => dn.AddCommonName(CommonName))
                .AddBasicConstraints(true, false)
                .AddKeyUsage(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.DigitalSignature)
                .AddCrlDistributionPointFromDistinguishedName("http://test-root-ca/crl/")
                .SetSubjectPublicKeyInfo(rsa.ExportSubjectPublicKeyInfo())
                .AddSubjectKeyIdentifier()
                .AddAuthorityKeyIdentifierFromSubjectKeyIdentifier()
                .Build();

            X509Certificate2 certificate = certificateRequest.CreateSelfSignedCertificate(X509SignatureGenerator.CreateForRSA(rsa, RSASignaturePadding.Pkcs1), 120);

            certificate = certificate.CopyWithPrivateKey(rsa);

            certificate.SaveCertificateToPkcs12(OutputPkcs12, console);

            return 0;
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            return Task.FromResult(Invoke(context));
        }
    }
}

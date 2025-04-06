using CertTools.Certificates;
using CertTools.CommandBuilders;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertTools.Commands;

internal class NewIntermediateCa : Command
{
    public NewIntermediateCa() : base(NameExtensions.ToKebabCase("", nameof(NewIntermediateCa)), "Creates new Intermediate CA")
    {
        OptionBuilder.For<Handler>()
            .NewOption(h => h.RootCertificatePkcs12).Configure(o =>
            {
                o.IsRequired = true;
                o.LegalFilePathsOnly();
                o.ExistingOnly();
            }).AddTo(this)
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
        public required FileInfo RootCertificatePkcs12 { get; set; }
        public required FileInfo OutputPkcs12 { get; set; }
        public int Invoke(InvocationContext context)
        {
            var rootCollection = X509CertificateLoader.LoadPkcs12CollectionFromFile(RootCertificatePkcs12.FullName, null, X509KeyStorageFlags.EphemeralKeySet);

            var rootCa = rootCollection
                 .OfType<X509Certificate2>()
                 .Single(c => c.HasPrivateKey);

            RSA rsa = RSA.Create(2048);

            var certificateRequest = CertificateRequestBuilder.New(dn => dn.AddCommonName(CommonName))
                .AddBasicConstraints(true)
                .AddKeyUsage(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.DigitalSignature)
                .AddCrlDistributionPointFromDistinguishedName("http://test-root-ca/crl/")
                .AddAuthorityKeyIdentifier(rootCa.Extensions.OfType<X509SubjectKeyIdentifierExtension>().First())
                .SetSubjectPublicKeyInfo(rsa.ExportSubjectPublicKeyInfo())
                .AddSubjectKeyIdentifier()
                .Build();

            X509Certificate2 certificate = certificateRequest.CreateCertificate(rootCa.GetSigningDetails(), 36);

            certificate = certificate.CopyWithPrivateKey(rsa);

            var collection = rootCollection.CopyCertificatesToCollection();
            collection.Add(certificate);

            collection.SaveCertificateToPkcs12(OutputPkcs12, console);

            return 0;
        }

        public Task<int> InvokeAsync(InvocationContext context)
        {
            return Task.FromResult(Invoke(context));
        }
    }
}
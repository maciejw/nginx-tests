using CertTools.Certificates;
using CertTools.CommandBuilders;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CertTools.Commands;


internal class NewLeafCertificate : Command
{
    public NewLeafCertificate() : base(NameExtensions.ToKebabCase("", nameof(NewLeafCertificate)), "Creates new certificate")
    {
        OptionBuilder.For<Handler>()
            .NewOption(h => h.RootCertificatePkcs12).Configure(o =>
            {
                o.IsRequired = true;
                o.ExistingOnly();
            }).AddTo(this)
            .NewOption(h => h.CertificateUsage).Configure(o => o.IsRequired = true).AddTo(this)
            .NewOption(h => h.CommonName).Configure(o => o.IsRequired = true).AddTo(this)
            .NewOption(h => h.SubjectAlternativeNameDns).Configure(o => o.Arity = ArgumentArity.OneOrMore).AddTo(this)
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
        public required CertificateUsageType CertificateUsage { get; set; }
        public List<string>? SubjectAlternativeNameDns { get; set; }
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
                .AddBasicConstraints()
                .AddKeyUsage(CertificateUsage.ToKeyUsage())
                .AddEnhancedKeyUsage(CertificateUsage.ToEnhancedKeyUsages())
                .AddSubjectAlternativeName(san => SubjectAlternativeNameDns?.ForEach(san.AddDnsName))
                .SetSubjectPublicKeyInfo(rsa.ExportSubjectPublicKeyInfo())
                .AddSubjectKeyIdentifier()
                .Build();

            X509Certificate2 certificate = certificateRequest.CreateCertificate(rootCa.GetSigningDetails());

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
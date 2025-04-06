
using CertTools.CommandBuilders;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.Security.Cryptography.X509Certificates;

namespace CertTools.Commands;

internal class Pkcs12ToPem : Command
{
    public Pkcs12ToPem() : base(NameExtensions.ToKebabCase("", nameof(Pkcs12ToPem)), "Converts a Pkcs12 to PEM")
    {
        OptionBuilder.For<Handler>()
            .NewOption(h => h.InputPkcs12).Configure(o =>
            {
                o.IsRequired = true;
                o.LegalFilePathsOnly();
                o.ExistingOnly();
            }).AddTo(this)
            .NewOption(h => h.WithPrivateKey).Configure(o =>
            {
                o.IsRequired = false;
            }).AddTo(this)
            .NewOption(h => h.OutputPEM).Configure(o =>
            {
                o.IsRequired = true;
                o.LegalFilePathsOnly();
            }).AddTo(this)
            ;
    }

    new internal class Handler(IConsole console) : ICommandHandler
    {
        public required FileInfo InputPkcs12 { get; set; }
        public required bool WithPrivateKey { get; set; }
        public required FileInfo OutputPEM { get; set; }
        public int Invoke(InvocationContext context)
        {
            X509Certificate2 certificate = X509CertificateLoader.LoadPkcs12FromFile(InputPkcs12.FullName, null, X509KeyStorageFlags.Exportable);

            console.WriteLine($"Saving pem certificate to: {OutputPEM}.crt");
            File.WriteAllText(Path.ChangeExtension(OutputPEM.FullName, "crt"), certificate.ExportCertificatePem());

            if (WithPrivateKey && certificate.HasPrivateKey && certificate.GetRSAPrivateKey() is { } rsa)
            {
                console.WriteLine($"Saving pem certificate key to: {OutputPEM}.key");
                File.WriteAllText(Path.ChangeExtension(OutputPEM.FullName, "key"), rsa.ExportRSAPrivateKeyPem());
            }
            if (WithPrivateKey && certificate.HasPrivateKey && certificate.GetECDsaPrivateKey() is { } ecdsa)
            {
                console.WriteLine($"Saving pem certificate key to: {OutputPEM}.key");
                File.WriteAllText(Path.ChangeExtension(OutputPEM.FullName, "key"), ecdsa.ExportECPrivateKeyPem());
            }

            return 0;
        }
        public Task<int> InvokeAsync(InvocationContext context)
        {
            return Task.FromResult(Invoke(context));
        }

    }

}
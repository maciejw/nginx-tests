
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;

using CertTools.Commands;

CommandLineBuilder commandLineBuilder = new(new RootCommand("Certificate management")
        {
            new NewRootCa(),
            new NewIntermediateCa(),
            new NewLeafCertificate(),
            new Pkcs12ToPem(),
        });

commandLineBuilder.UseDefaults().UseHost(configureHost =>
{
    configureHost.UseCommandHandler<NewRootCa, NewRootCa.Handler>();
    configureHost.UseCommandHandler<NewIntermediateCa, NewIntermediateCa.Handler>();
    configureHost.UseCommandHandler<NewLeafCertificate, NewLeafCertificate.Handler>();
    configureHost.UseCommandHandler<Pkcs12ToPem, Pkcs12ToPem.Handler>();
});

return commandLineBuilder.Build().Invoke(args);

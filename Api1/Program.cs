using System.Net;
using System.Security.Cryptography.X509Certificates;

using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Https;

using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, lc) => lc
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(theme: AnsiConsoleTheme.Code));

builder.WebHost.ConfigureKestrel((options) =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ClientCertificateValidation = (certificate, chain, sslPolicyErrors) =>
        {
            var logger = options.ApplicationServices.GetRequiredService<ILogger<HttpsConnectionAdapterOptions>>();

            logger.LogInformation("RemoteCertificateValidationCallback invoked");
            logger.LogInformation("Certificate Subject: {subject}", certificate?.Subject);
            logger.LogInformation("Certificate Issuer: {issuer}", certificate?.Issuer);
            logger.LogInformation("SSL Policy Errors: {sslPolicyErrors}", sslPolicyErrors);

            if (chain != null)
            {
                logger.LogInformation("Chain Information");
                foreach (var element in chain.ChainElements)
                {
                    logger.LogInformation("Chain Subject: {subject}", element.Certificate.Subject);
                    foreach (var status in element.ChainElementStatus)
                    {
                        logger.LogInformation("Chain element status: {Status} - {StatusInformation}", status.Status, status.StatusInformation);
                    }
                }
            }

            return sslPolicyErrors == System.Net.Security.SslPolicyErrors.None;
        };
    });
});

builder.Services
    .AddOpenApi()
    .AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(o => o.RevocationMode = X509RevocationMode.NoCheck);

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor
    | ForwardedHeaders.XForwardedHost
    | ForwardedHeaders.XForwardedProto;
});

static X509Certificate2 NginxHeaderConverter(string certificate)
{
    return X509Certificate2.CreateFromPem(WebUtility.UrlDecode(certificate));
}

builder.Services.Configure<CertificateForwardingOptions>(o =>
{
    o.CertificateHeader = "X-SSL-Client-Cert";
    o.HeaderConverter = NginxHeaderConverter;
});

var app = builder.Build();

app.UseForwardedHeaders();
app.UseCertificateForwarding();

app.UseAuthentication();

app.MapGet("/", async (context) =>
{
    context.Response.ContentType = "text/plain";
    context.Response.StatusCode = (int)HttpStatusCode.OK;

    await context.Response.WriteAsync($"Certificate: {context.Connection.ClientCertificate?.SubjectName.Name}\n");
    await context.Response.WriteAsync($"Request headers:\n");
    foreach (var item in context.Request.Headers)
    {
        await context.Response.WriteAsync($"{item.Key}: {item.Value}\n");
    }

});
app.Run();

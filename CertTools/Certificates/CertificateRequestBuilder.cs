namespace CertTools.Certificates;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public class CertificateRequestBuilder : ICertificateRequestBuilder, ICertificateRequestBuilderWithPublicKey, ICertificateRequestBuilderWithWithSubjectKeyIdentifier
{
    public static ICertificateRequestBuilder New(Action<X500DistinguishedNameBuilder> action)
    {
        X500DistinguishedNameBuilder distinguishedNameBuilder = new();
        action(distinguishedNameBuilder);
        return new CertificateRequestBuilder(distinguishedNameBuilder);
    }

    private readonly X500DistinguishedName distinguishedName;
    private HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
    private PublicKey? publicKey;
    private readonly List<X509Extension> extensions = new();

    private CertificateRequestBuilder(X500DistinguishedNameBuilder distinguishedNameBuilder)
    {
        distinguishedName = distinguishedNameBuilder.Build();
    }
    ///<inheritdoc/>
    public ICertificateRequestBuilderWithPublicKey SetSubjectPublicKeyInfo(byte[] subjectPublicKeyInfo)
    {
        publicKey = PublicKey.CreateFromSubjectPublicKeyInfo(subjectPublicKeyInfo, out _);
        return this;
    }

    public ICertificateRequestBuilder SetSigningHashAlgorithm(HashAlgorithmName hashAlgorithm)
    {
        this.hashAlgorithm = hashAlgorithm;
        return this;
    }
    // Add a Basic Constraints extension
    public ICertificateRequestBuilder AddBasicConstraints(bool certificateAuthority = false, bool hasPathLengthConstraint = true, int pathLength = 0, bool critical = true)
    {
        var ext = new X509BasicConstraintsExtension(certificateAuthority, hasPathLengthConstraint, pathLength, critical);
        extensions.Add(ext);
        return this;
    }

    // Add a Key Usage extension
    public ICertificateRequestBuilder AddKeyUsage(X509KeyUsageFlags usage, bool critical = true)
    {
        var ext = new X509KeyUsageExtension(usage, critical);
        extensions.Add(ext);
        return this;
    }
    public ICertificateRequestBuilder AddCrlDistributionPoint(IEnumerable<string> crlUrls, bool critical = false)
    {
        extensions.Add(CertificateRevocationListBuilder.BuildCrlDistributionPointExtension(crlUrls, critical));
        return this;
    }
    public ICertificateRequestBuilder AddCrlDistributionPointFromDistinguishedName(string distributionPointPrefix, bool critical = false)
    {
        var distributionPoint = $"{distributionPointPrefix.TrimEnd('/')}/{distinguishedName.Name}.crl";
        extensions.Add(CertificateRevocationListBuilder.BuildCrlDistributionPointExtension([distributionPoint], critical));
        return this;
    }
    public ICertificateRequestBuilder AddAuthorityKeyIdentifier(X509SubjectKeyIdentifierExtension subjectKeyIdentifier)
    {
        extensions.Add(X509AuthorityKeyIdentifierExtension.CreateFromSubjectKeyIdentifier(subjectKeyIdentifier));
        return this;
    }
    public ICertificateRequestBuilderWithWithSubjectKeyIdentifier AddSubjectKeyIdentifier(bool critical = false)
    {
        extensions.Add(new X509SubjectKeyIdentifierExtension(publicKey!, critical));
        return this;
    }
    public ICertificateRequestBuilderWithWithSubjectKeyIdentifier AddAuthorityKeyIdentifierFromSubjectKeyIdentifier()
    {
        var subjectKeyId = extensions.OfType<X509SubjectKeyIdentifierExtension>().First();
        extensions.Add(X509AuthorityKeyIdentifierExtension.CreateFromSubjectKeyIdentifier(subjectKeyId));
        return this;
    }
    public CertificateRequest Build()
    {
        RSASignaturePadding? padding = RSASignaturePadding.Pkcs1;
        if (publicKey!.GetRSAPublicKey() == null)
        {
            padding = null;
        }
        var request = new CertificateRequest(distinguishedName, publicKey!, hashAlgorithm, padding);

        extensions.ForEach(request.CertificateExtensions.Add);

        return request;
    }

    public ICertificateRequestBuilder AddEnhancedKeyUsage(IEnumerable<Oid> oids, bool critical = false)
    {
        extensions.Add(new X509EnhancedKeyUsageExtension([.. oids], critical));
        return this;
    }
    public ICertificateRequestBuilder AddSubjectAlternativeName(Action<SubjectAlternativeNameBuilder> action, bool critical = false)
    {
        var subjectAlternativeNameBuilder = new SubjectAlternativeNameBuilder();
        action(subjectAlternativeNameBuilder);
        extensions.Add(subjectAlternativeNameBuilder.Build(critical));
        return this;
    }
}

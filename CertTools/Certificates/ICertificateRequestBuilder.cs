namespace CertTools.Certificates;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public interface ICertificateRequestBuilder
{
    ICertificateRequestBuilder AddBasicConstraints(bool certificateAuthority = false, bool hasPathLengthConstraint = true, int pathLength = 0, bool critical = true);
    ICertificateRequestBuilder AddKeyUsage(X509KeyUsageFlags usage = X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, bool critical = true);
    ICertificateRequestBuilder SetSigningHashAlgorithm(HashAlgorithmName hashAlgorithm);
    ICertificateRequestBuilder AddCrlDistributionPoint(IEnumerable<string> crlDistributionPoints, bool critical = false);
    ICertificateRequestBuilder AddCrlDistributionPointFromDistinguishedName(string distributionPointPrefix, bool critical = false);
    ICertificateRequestBuilder AddEnhancedKeyUsage(IEnumerable<Oid> oids, bool critical = false);
    ICertificateRequestBuilder AddSubjectAlternativeName(Action<SubjectAlternativeNameBuilder> action, bool critical = false);
    ICertificateRequestBuilder AddAuthorityKeyIdentifier(X509SubjectKeyIdentifierExtension subjectKeyIdentifier);
    
    /// <summary>
    /// Sets the public key for the certificate request.
    /// </summary>
    /// <param name="subjectPublicKeyInfo"><see cref="AsymmetricAlgorithm.ExportSubjectPublicKeyInfo"/></param>
    /// <returns></returns>
    ICertificateRequestBuilderWithPublicKey SetSubjectPublicKeyInfo(byte[] subjectPublicKeyInfo);
}

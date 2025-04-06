[CmdLetBinding()]
param(
    $outDir = ".\certs"
)

dotnet publish .\CertTools -o out\CertTools

mkdir $outDir\ca -ErrorAction SilentlyContinue

.\out\CertTools\CertTools.exe new-root-ca --common-name "Root CA" --output-pkcs-12 $outDir\root-ca.pfx

.\out\CertTools\CertTools.exe new-intermediate-ca --root-certificate-pkcs-12 $outDir\root-ca.pfx --common-name "Dev CA" --output-pkcs-12 $outDir\dev-ca.pfx

.\out\CertTools\CertTools.exe new-leaf-certificate --certificate-usage ServerAuthentication --root-certificate-pkcs-12 $outDir\dev-ca.pfx --common-name localhost --subject-alternative-name-dns localhost --output-pkcs-12 $outDir\localhost.pfx

.\out\CertTools\CertTools.exe new-leaf-certificate --certificate-usage ClientAuthentication --root-certificate-pkcs-12 $outDir\dev-ca.pfx --common-name client --output-pkcs-12 $outDir\client.pfx

.\out\CertTools\CertTools.exe pkcs-12-to-pem --input-pkcs-12 $outDir\root-ca.pfx --output-pem $outDir\ca\root-ca
.\out\CertTools\CertTools.exe pkcs-12-to-pem --input-pkcs-12 $outDir\dev-ca.pfx --output-pem $outDir\ca\dev-ca
.\out\CertTools\CertTools.exe pkcs-12-to-pem --input-pkcs-12 $outDir\localhost.pfx --output-pem $outDir\localhost --with-private-key
.\out\CertTools\CertTools.exe pkcs-12-to-pem --input-pkcs-12 $outDir\client.pfx --output-pem $outDir\client --with-private-key

$rootCa = Get-Content $outDir\ca\root-ca.crt
Add-Content -Path $outDir\ca\dev-ca.crt -Value ([System.Environment]::NewLine)
Add-Content -Path $outDir\ca\dev-ca.crt -Value $rootCa

$devCa = Get-Content $outDir\ca\dev-ca.crt

Add-Content -Path $outDir\localhost.crt -Value ([System.Environment]::NewLine)
Add-Content -Path $outDir\client.crt -Value ([System.Environment]::NewLine)

Add-Content -Path $outDir\localhost.crt -Value $devCa
Add-Content -Path $outDir\client.crt -Value $devCa


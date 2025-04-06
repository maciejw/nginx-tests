Describe "mtls tests" {

    BeforeAll {
        $cert = Get-PfxCertificate $PSScriptRoot\certs\client.pfx
        $PSDefaultParameterValues = @{
            "Invoke-RestMethod:Certificate"        = $cert
            "Invoke-RestMethod:SkipHttpErrorCheck" = $true
        }
    }
    It "keycloak connect token" {
        $params = @{
            "client_id"  = "client"
            "grant_type" = "client_credentials"
        }
        Invoke-RestMethod https://localhost:1443/realms/master/protocol/openid-connect/token -Method Post -Body $params | Out-Host

    }

    Context "openssl" {

        Context "proxy" {

            It "should connect using openssl" {
                Write-Output QUIT | & 'C:\Program Files\Git\mingw64\bin\openssl.exe' s_client -connect localhost:1443 -servername localhost -CAfile .\certs\ca\root-ca.crt -verify_return_error | Out-Host
            }

            It "should connect using openssl with mtls" {
                Write-Output QUIT | & 'C:\Program Files\Git\mingw64\bin\openssl.exe' s_client -connect localhost:1443 -servername localhost -cert .\certs\client.crt -key .\certs\client.key -CAfile .\certs\ca\root-ca.crt -verify_return_error  | Out-Host
            }
        }

        Context "no proxy 2443" {

            It "should connect using openssl" {
                Write-Output QUIT | & 'C:\Program Files\Git\mingw64\bin\openssl.exe' s_client -connect localhost:2443 -servername localhost -CAfile .\certs\ca\root-ca.crt -verify_return_error | Out-Host
            }

            It "should connect using openssl with mtls" {
                Write-Output QUIT | & 'C:\Program Files\Git\mingw64\bin\openssl.exe' s_client -connect localhost:2443 -servername localhost -cert .\certs\client.crt -key .\certs\client.key -CAfile .\certs\ca\root-ca.crt -verify_return_error  | Out-Host
            }
        }
        Context "no proxy 3443" {

            It "should connect using openssl" {
                Write-Output QUIT | & 'C:\Program Files\Git\mingw64\bin\openssl.exe' s_client -connect localhost:3443 -servername localhost -CAfile .\certs\ca\root-ca.crt -verify_return_error | Out-Host
            }

            It "should connect using openssl with mtls" {
                Write-Output QUIT | & 'C:\Program Files\Git\mingw64\bin\openssl.exe' s_client -connect localhost:3443 -servername localhost -cert .\certs\client.crt -key .\certs\client.key -CAfile .\certs\ca\root-ca.crt -verify_return_error  | Out-Host
            }
        }
    }
    Context "proxy" {

        It "should connect to backend" {
            Invoke-RestMethod https://localhost:1443/test/backend | Out-Host
        }
        It "should connect to api1" {
            Invoke-RestMethod https://localhost:1443/test/api1 | Out-Host
        }
    }
    Context "no proxy" {
        It "should connect to backend" {
            Invoke-RestMethod https://localhost:2443/ | Out-Host
        }

        It "should connect to api1" {
            Invoke-RestMethod https://localhost:3443/ | Out-Host
        }
    }
}
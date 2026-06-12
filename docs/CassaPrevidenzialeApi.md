# Amica.Sdk.Api.CassaPrevidenzialeApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiVversionCompanyIdTabelleCassaPrevidenzialeGet**](CassaPrevidenzialeApi.md#apivversioncompanyidtabellecassaprevidenzialeget) | **GET** /api/v{version}/{companyId}/tabelle/CassaPrevidenziale | Restituisce il contenuto della tabella come lista di elementi. |

<a id="apivversioncompanyidtabellecassaprevidenzialeget"></a>
# **ApiVversionCompanyIdTabelleCassaPrevidenzialeGet**
> List&lt;CassaPrevidenziale&gt; ApiVversionCompanyIdTabelleCassaPrevidenzialeGet (string companyId, string version)

Restituisce il contenuto della tabella come lista di elementi.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdTabelleCassaPrevidenzialeGetExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure Bearer token for authorization: bearer
            config.AccessToken = "YOUR_BEARER_TOKEN";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            var apiInstance = new CassaPrevidenzialeApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var version = "version_example";  // string | 

            try
            {
                // Restituisce il contenuto della tabella come lista di elementi.
                List<CassaPrevidenziale> result = apiInstance.ApiVversionCompanyIdTabelleCassaPrevidenzialeGet(companyId, version);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling CassaPrevidenzialeApi.ApiVversionCompanyIdTabelleCassaPrevidenzialeGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdTabelleCassaPrevidenzialeGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce il contenuto della tabella come lista di elementi.
    ApiResponse<List<CassaPrevidenziale>> response = apiInstance.ApiVversionCompanyIdTabelleCassaPrevidenzialeGetWithHttpInfo(companyId, version);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling CassaPrevidenzialeApi.ApiVversionCompanyIdTabelleCassaPrevidenzialeGetWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **version** | **string** |  |  |

### Return type

[**List&lt;CassaPrevidenziale&gt;**](CassaPrevidenziale.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


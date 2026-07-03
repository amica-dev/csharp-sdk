# Amica.Sdk.Api.SituazioneApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiVversionCompanyIdSituazioneCreditodebitomensileGet**](SituazioneApi.md#apivversioncompanyidsituazionecreditodebitomensileget) | **GET** /api/v{version}/{companyId}/Situazione/creditodebitomensile | Restituisce i saldi mensili di credito e debito dell&#39;azienda. |
| [**ApiVversionCompanyIdSituazioneFatturatomensileGet**](SituazioneApi.md#apivversioncompanyidsituazionefatturatomensileget) | **GET** /api/v{version}/{companyId}/Situazione/fatturatomensile | Restituisce il fatturato mensile dell&#39;azienda. |

<a id="apivversioncompanyidsituazionecreditodebitomensileget"></a>
# **ApiVversionCompanyIdSituazioneCreditodebitomensileGet**
> List&lt;CreditoDebitoMensile&gt; ApiVversionCompanyIdSituazioneCreditodebitomensileGet (string companyId, string version)

Restituisce i saldi mensili di credito e debito dell'azienda.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdSituazioneCreditodebitomensileGetExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure Bearer token for authorization: bearer
            config.AccessToken = "YOUR_BEARER_TOKEN";
            // Configure OAuth2 access token for authorization: oauth2ClientCredentials
            config.AccessToken = "YOUR_ACCESS_TOKEN";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            var apiInstance = new SituazioneApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var version = "version_example";  // string | 

            try
            {
                // Restituisce i saldi mensili di credito e debito dell'azienda.
                List<CreditoDebitoMensile> result = apiInstance.ApiVversionCompanyIdSituazioneCreditodebitomensileGet(companyId, version);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SituazioneApi.ApiVversionCompanyIdSituazioneCreditodebitomensileGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdSituazioneCreditodebitomensileGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce i saldi mensili di credito e debito dell'azienda.
    ApiResponse<List<CreditoDebitoMensile>> response = apiInstance.ApiVversionCompanyIdSituazioneCreditodebitomensileGetWithHttpInfo(companyId, version);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SituazioneApi.ApiVversionCompanyIdSituazioneCreditodebitomensileGetWithHttpInfo: " + e.Message);
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

[**List&lt;CreditoDebitoMensile&gt;**](CreditoDebitoMensile.md)

### Authorization

[bearer](../README.md#bearer), [oauth2ClientCredentials](../README.md#oauth2ClientCredentials), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce i dati dei saldi mensili. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidsituazionefatturatomensileget"></a>
# **ApiVversionCompanyIdSituazioneFatturatomensileGet**
> List&lt;FatturatoMensile&gt; ApiVversionCompanyIdSituazioneFatturatomensileGet (string companyId, string version)

Restituisce il fatturato mensile dell'azienda.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdSituazioneFatturatomensileGetExample
    {
        public static void Main()
        {
            Configuration config = new Configuration();
            config.BasePath = "http://localhost";
            // Configure Bearer token for authorization: bearer
            config.AccessToken = "YOUR_BEARER_TOKEN";
            // Configure OAuth2 access token for authorization: oauth2ClientCredentials
            config.AccessToken = "YOUR_ACCESS_TOKEN";
            // Configure OAuth2 access token for authorization: oauth2
            config.AccessToken = "YOUR_ACCESS_TOKEN";

            var apiInstance = new SituazioneApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var version = "version_example";  // string | 

            try
            {
                // Restituisce il fatturato mensile dell'azienda.
                List<FatturatoMensile> result = apiInstance.ApiVversionCompanyIdSituazioneFatturatomensileGet(companyId, version);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling SituazioneApi.ApiVversionCompanyIdSituazioneFatturatomensileGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdSituazioneFatturatomensileGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce il fatturato mensile dell'azienda.
    ApiResponse<List<FatturatoMensile>> response = apiInstance.ApiVversionCompanyIdSituazioneFatturatomensileGetWithHttpInfo(companyId, version);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling SituazioneApi.ApiVversionCompanyIdSituazioneFatturatomensileGetWithHttpInfo: " + e.Message);
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

[**List&lt;FatturatoMensile&gt;**](FatturatoMensile.md)

### Authorization

[bearer](../README.md#bearer), [oauth2ClientCredentials](../README.md#oauth2ClientCredentials), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce i dati del fatturato mensile. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


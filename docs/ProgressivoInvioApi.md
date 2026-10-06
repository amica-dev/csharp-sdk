# Amica.Sdk.Api.ProgressivoInvioApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ProgressivoInvioDelete**](ProgressivoInvioApi.md#progressivoinviodelete) | **DELETE** /api/v3/{companyId}/ProgressivoInvio/{id} | Elimina un elemento. |
| [**ProgressivoInvioGet**](ProgressivoInvioApi.md#progressivoinvioget) | **GET** /api/v3/{companyId}/ProgressivoInvio/{id} | Restituisce un elemento. |
| [**ProgressivoInvioList**](ProgressivoInvioApi.md#progressivoinviolist) | **GET** /api/v3/{companyId}/ProgressivoInvio | Restituisce una lista di elementi. |
| [**ProgressivoInvioNewInstance**](ProgressivoInvioApi.md#progressivoinvionewinstance) | **GET** /api/v3/{companyId}/ProgressivoInvio/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile). |
| [**ProgressivoInvioPost**](ProgressivoInvioApi.md#progressivoinviopost) | **POST** /api/v3/{companyId}/ProgressivoInvio | Salva un nuovo elemento. |
| [**ProgressivoInvioPut**](ProgressivoInvioApi.md#progressivoinvioput) | **PUT** /api/v3/{companyId}/ProgressivoInvio/{id} | Aggiorna un elemento esistente. |

<a id="progressivoinviodelete"></a>
# **ProgressivoInvioDelete**
> ProgressivoInvio ProgressivoInvioDelete (string companyId, int id)

Elimina un elemento.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ProgressivoInvioDeleteExample
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

            var apiInstance = new ProgressivoInvioApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID dell'elemento da eliminare.

            try
            {
                // Elimina un elemento.
                ProgressivoInvio result = apiInstance.ProgressivoInvioDelete(companyId, id);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioDelete: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ProgressivoInvioDeleteWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Elimina un elemento.
    ApiResponse<ProgressivoInvio> response = apiInstance.ProgressivoInvioDeleteWithHttpInfo(companyId, id);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioDeleteWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **id** | **int** | ID dell&#39;elemento da eliminare. |  |

### Return type

[**ProgressivoInvio**](ProgressivoInvio.md)

### Authorization

[bearer](../README.md#bearer), [oauth2ClientCredentials](../README.md#oauth2ClientCredentials), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, application/problem+json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce l&#39;elemento eliminato. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **404** | Non trovato. L&#39;elemento non è stato trovato. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  |
| **401** | Non autenticato: access token mancante, non valido o scaduto. Ottieni un nuovo access token e ripeti la richiesta. La risposta non ha corpo. |  * WWW-Authenticate - Schema di autenticazione richiesto (&#x60;Bearer&#x60;) ed eventuale motivo del rifiuto del token. <br>  |
| **403** | Accesso negato. Le cause possibili:  - il token non possiede lo scope richiesto dall&#39;operazione (risposta senza corpo); - &#x60;code: cloud_not_active&#x60;: l&#39;account lavora con una connessione locale e la Cloud API non espone i suoi dati; - &#x60;code: company_license_not_active&#x60;: la licenza dell&#39;azienda indicata nel percorso è scaduta o non è attiva; le altre aziende con licenza attiva restano accessibili. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="progressivoinvioget"></a>
# **ProgressivoInvioGet**
> ProgressivoInvio ProgressivoInvioGet (string companyId, int id, List<string>? expand = null)

Restituisce un elemento.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ProgressivoInvioGetExample
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

            var apiInstance = new ProgressivoInvioApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID dell'elemento da recuperare.
            var expand = new List<string>?(); // List<string>? | Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un'unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge `expand=Rows` alla chiamata. (optional) 

            try
            {
                // Restituisce un elemento.
                ProgressivoInvio result = apiInstance.ProgressivoInvioGet(companyId, id, expand);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ProgressivoInvioGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce un elemento.
    ApiResponse<ProgressivoInvio> response = apiInstance.ProgressivoInvioGetWithHttpInfo(companyId, id, expand);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioGetWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **id** | **int** | ID dell&#39;elemento da recuperare. |  |
| **expand** | [**List&lt;string&gt;?**](string.md) | Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. | [optional]  |

### Return type

[**ProgressivoInvio**](ProgressivoInvio.md)

### Authorization

[bearer](../README.md#bearer), [oauth2ClientCredentials](../README.md#oauth2ClientCredentials), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, application/problem+json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce l&#39;elemento richiesto. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **404** | Non trovato. L&#39;elemento non è stato trovato. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  |
| **401** | Non autenticato: access token mancante, non valido o scaduto. Ottieni un nuovo access token e ripeti la richiesta. La risposta non ha corpo. |  * WWW-Authenticate - Schema di autenticazione richiesto (&#x60;Bearer&#x60;) ed eventuale motivo del rifiuto del token. <br>  |
| **403** | Accesso negato. Le cause possibili:  - il token non possiede lo scope richiesto dall&#39;operazione (risposta senza corpo); - &#x60;code: cloud_not_active&#x60;: l&#39;account lavora con una connessione locale e la Cloud API non espone i suoi dati; - &#x60;code: company_license_not_active&#x60;: la licenza dell&#39;azienda indicata nel percorso è scaduta o non è attiva; le altre aziende con licenza attiva restano accessibili. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="progressivoinviolist"></a>
# **ProgressivoInvioList**
> List&lt;ProgressivoInvio&gt; ProgressivoInvioList (string companyId, int? skip = null, int? take = null, bool? skipTotalCount = null, string? filter = null, string? order = null, List<string>? expand = null)

Restituisce una lista di elementi.

 La risposta include due header con informazioni sui risultati.  `X-Read-TotalCount` indica il numero di elementi che soddisfano il filtro impostato nella query;  `X-Read-Count` indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri `Take` e `Skip` sono stati usati nelle opzioni di query.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ProgressivoInvioListExample
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

            var apiInstance = new ProgressivoInvioApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var skip = 100;  // int? | Numero di elementi da escludere dall'inizio del risultato. Combinato con `Take`, è utile per la paginazione. (optional) 
            var take = 25;  // int? | Numero di elementi da includere nel risultato. Combinato con `Skip`, è utile per la paginazione. (optional) 
            var skipTotalCount = true;  // bool? | Indica se l'header `X-Read-TotalCount` debba essere escluso dalla risposta. Impostare a `true` per prestazioni migliori a scapito dell'accuratezza. Il valore predefinito è `false`. (optional) 
            var filter = field1 $eq hello $or, field2 $gt 10;  // string? | Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: `$eq` (uguale), `$ne` (diverso), `$gt` (maggiore di), `$lt` (minore di), `$lte` (minore o uguale), `$gte` (maggiore o uguale), `$ct` (contiene), `$nct` (non contiene), `$sw` (inizia con), `$ew` (finisce con). Più espressioni possono essere concatenate con un connettore logico `$and` o `$or` (`$and` può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (`[]`). Nell'esempio vengono inclusi tutti gli elementi con `field1=\"hello\"` oppure `field2>10`. (optional) 
            var order = -field1,field2;  // string? | Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell'esempio i risultati sono ordinati prima per `field1` (discendente), poi per `field2`. (optional) 
            var expand = new List<string>?(); // List<string>? | Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un'unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge `expand=Rows` alla chiamata. (optional) 

            try
            {
                // Restituisce una lista di elementi.
                List<ProgressivoInvio> result = apiInstance.ProgressivoInvioList(companyId, skip, take, skipTotalCount, filter, order, expand);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioList: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ProgressivoInvioListWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce una lista di elementi.
    ApiResponse<List<ProgressivoInvio>> response = apiInstance.ProgressivoInvioListWithHttpInfo(companyId, skip, take, skipTotalCount, filter, order, expand);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioListWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **skip** | **int?** | Numero di elementi da escludere dall&#39;inizio del risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. | [optional]  |
| **take** | **int?** | Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. | [optional]  |
| **skipTotalCount** | **bool?** | Indica se l&#39;header &#x60;X-Read-TotalCount&#x60; debba essere escluso dalla risposta. Impostare a &#x60;true&#x60; per prestazioni migliori a scapito dell&#39;accuratezza. Il valore predefinito è &#x60;false&#x60;. | [optional]  |
| **filter** | **string?** | Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: &#x60;$eq&#x60; (uguale), &#x60;$ne&#x60; (diverso), &#x60;$gt&#x60; (maggiore di), &#x60;$lt&#x60; (minore di), &#x60;$lte&#x60; (minore o uguale), &#x60;$gte&#x60; (maggiore o uguale), &#x60;$ct&#x60; (contiene), &#x60;$nct&#x60; (non contiene), &#x60;$sw&#x60; (inizia con), &#x60;$ew&#x60; (finisce con). Più espressioni possono essere concatenate con un connettore logico &#x60;$and&#x60; o &#x60;$or&#x60; (&#x60;$and&#x60; può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (&#x60;[]&#x60;). Nell&#39;esempio vengono inclusi tutti gli elementi con &#x60;field1&#x3D;\&quot;hello\&quot;&#x60; oppure &#x60;field2&gt;10&#x60;. | [optional]  |
| **order** | **string?** | Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell&#39;esempio i risultati sono ordinati prima per &#x60;field1&#x60; (discendente), poi per &#x60;field2&#x60;. | [optional]  |
| **expand** | [**List&lt;string&gt;?**](string.md) | Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. | [optional]  |

### Return type

[**List&lt;ProgressivoInvio&gt;**](ProgressivoInvio.md)

### Authorization

[bearer](../README.md#bearer), [oauth2ClientCredentials](../README.md#oauth2ClientCredentials), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, application/problem+json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce la lista degli elementi corrispondenti ai criteri di ricerca. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  |
| **401** | Non autenticato: access token mancante, non valido o scaduto. Ottieni un nuovo access token e ripeti la richiesta. La risposta non ha corpo. |  * WWW-Authenticate - Schema di autenticazione richiesto (&#x60;Bearer&#x60;) ed eventuale motivo del rifiuto del token. <br>  |
| **403** | Accesso negato. Le cause possibili:  - il token non possiede lo scope richiesto dall&#39;operazione (risposta senza corpo); - &#x60;code: cloud_not_active&#x60;: l&#39;account lavora con una connessione locale e la Cloud API non espone i suoi dati; - &#x60;code: company_license_not_active&#x60;: la licenza dell&#39;azienda indicata nel percorso è scaduta o non è attiva; le altre aziende con licenza attiva restano accessibili. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="progressivoinvionewinstance"></a>
# **ProgressivoInvioNewInstance**
> ProgressivoInvio ProgressivoInvioNewInstance (string companyId)

Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di `Documento` avrà il `Numero` appropriato, basato sui documenti preesistenti di tipo compatibile).

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ProgressivoInvioNewInstanceExample
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

            var apiInstance = new ProgressivoInvioApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.

            try
            {
                // Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di `Documento` avrà il `Numero` appropriato, basato sui documenti preesistenti di tipo compatibile).
                ProgressivoInvio result = apiInstance.ProgressivoInvioNewInstance(companyId);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioNewInstance: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ProgressivoInvioNewInstanceWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di `Documento` avrà il `Numero` appropriato, basato sui documenti preesistenti di tipo compatibile).
    ApiResponse<ProgressivoInvio> response = apiInstance.ProgressivoInvioNewInstanceWithHttpInfo(companyId);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioNewInstanceWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |

### Return type

[**ProgressivoInvio**](ProgressivoInvio.md)

### Authorization

[bearer](../README.md#bearer), [oauth2ClientCredentials](../README.md#oauth2ClientCredentials), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json, application/problem+json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **400** | Bad Request |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  |
| **401** | Non autenticato: access token mancante, non valido o scaduto. Ottieni un nuovo access token e ripeti la richiesta. La risposta non ha corpo. |  * WWW-Authenticate - Schema di autenticazione richiesto (&#x60;Bearer&#x60;) ed eventuale motivo del rifiuto del token. <br>  |
| **403** | Accesso negato. Le cause possibili:  - il token non possiede lo scope richiesto dall&#39;operazione (risposta senza corpo); - &#x60;code: cloud_not_active&#x60;: l&#39;account lavora con una connessione locale e la Cloud API non espone i suoi dati; - &#x60;code: company_license_not_active&#x60;: la licenza dell&#39;azienda indicata nel percorso è scaduta o non è attiva; le altre aziende con licenza attiva restano accessibili. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="progressivoinviopost"></a>
# **ProgressivoInvioPost**
> ProgressivoInvio ProgressivoInvioPost (string companyId, bool? validationOverride = null, string? idempotencyKey = null, ProgressivoInvio? progressivoInvio = null)

Salva un nuovo elemento.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ProgressivoInvioPostExample
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

            var apiInstance = new ProgressivoInvioApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var validationOverride = true;  // bool? | Se applicare eventualmente l'override della validazione. (optional) 
            var idempotencyKey = "idempotencyKey_example";  // string? | Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l'operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l'header `Idempotent-Replayed: true`. (optional) 
            var progressivoInvio = new ProgressivoInvio?(); // ProgressivoInvio? | Istanza dell'elemento da salvare. (optional) 

            try
            {
                // Salva un nuovo elemento.
                ProgressivoInvio result = apiInstance.ProgressivoInvioPost(companyId, validationOverride, idempotencyKey, progressivoInvio);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioPost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ProgressivoInvioPostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Salva un nuovo elemento.
    ApiResponse<ProgressivoInvio> response = apiInstance.ProgressivoInvioPostWithHttpInfo(companyId, validationOverride, idempotencyKey, progressivoInvio);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioPostWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **validationOverride** | **bool?** | Se applicare eventualmente l&#39;override della validazione. | [optional]  |
| **idempotencyKey** | **string?** | Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. | [optional]  |
| **progressivoInvio** | [**ProgressivoInvio?**](ProgressivoInvio?.md) | Istanza dell&#39;elemento da salvare. | [optional]  |

### Return type

[**ProgressivoInvio**](ProgressivoInvio.md)

### Authorization

[bearer](../README.md#bearer), [oauth2ClientCredentials](../README.md#oauth2ClientCredentials), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/json, application/problem+json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Creato. Restituisce l&#39;elemento salvato con il &#x60;ConcurrencyToken&#x60; aggiornato. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **409** | Conflitto. Restituisce il motivo del conflitto. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore (di solito un problema di validazione). |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **422** | La stessa Idempotency-Key è già stata usata per una richiesta con metodo, URL o corpo diversi. |  -  |
| **401** | Non autenticato: access token mancante, non valido o scaduto. Ottieni un nuovo access token e ripeti la richiesta. La risposta non ha corpo. |  * WWW-Authenticate - Schema di autenticazione richiesto (&#x60;Bearer&#x60;) ed eventuale motivo del rifiuto del token. <br>  |
| **403** | Accesso negato. Le cause possibili:  - il token non possiede lo scope richiesto dall&#39;operazione (risposta senza corpo); - &#x60;code: cloud_not_active&#x60;: l&#39;account lavora con una connessione locale e la Cloud API non espone i suoi dati; - &#x60;code: company_license_not_active&#x60;: la licenza dell&#39;azienda indicata nel percorso è scaduta o non è attiva; le altre aziende con licenza attiva restano accessibili. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="progressivoinvioput"></a>
# **ProgressivoInvioPut**
> ProgressivoInvio ProgressivoInvioPut (string companyId, int id, bool? validationOverride = null, string? idempotencyKey = null, ProgressivoInvio? progressivoInvio = null)

Aggiorna un elemento esistente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ProgressivoInvioPutExample
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

            var apiInstance = new ProgressivoInvioApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID univoco dell'elemento da aggiornare.
            var validationOverride = true;  // bool? | Se applicare eventualmente l'override della validazione. (optional) 
            var idempotencyKey = "idempotencyKey_example";  // string? | Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l'operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l'header `Idempotent-Replayed: true`. (optional) 
            var progressivoInvio = new ProgressivoInvio?(); // ProgressivoInvio? | Istanza dell'elemento da salvare. (optional) 

            try
            {
                // Aggiorna un elemento esistente.
                ProgressivoInvio result = apiInstance.ProgressivoInvioPut(companyId, id, validationOverride, idempotencyKey, progressivoInvio);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioPut: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ProgressivoInvioPutWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Aggiorna un elemento esistente.
    ApiResponse<ProgressivoInvio> response = apiInstance.ProgressivoInvioPutWithHttpInfo(companyId, id, validationOverride, idempotencyKey, progressivoInvio);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling ProgressivoInvioApi.ProgressivoInvioPutWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **id** | **int** | ID univoco dell&#39;elemento da aggiornare. |  |
| **validationOverride** | **bool?** | Se applicare eventualmente l&#39;override della validazione. | [optional]  |
| **idempotencyKey** | **string?** | Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. | [optional]  |
| **progressivoInvio** | [**ProgressivoInvio?**](ProgressivoInvio?.md) | Istanza dell&#39;elemento da salvare. | [optional]  |

### Return type

[**ProgressivoInvio**](ProgressivoInvio.md)

### Authorization

[bearer](../README.md#bearer), [oauth2ClientCredentials](../README.md#oauth2ClientCredentials), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/json, application/problem+json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce l&#39;elemento aggiornato con un nuovo valore di &#x60;ConcurrencyToken&#x60;. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **422** | Conflitto. Restituisce il motivo del conflitto. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **404** | Non trovato. L&#39;elemento non è stato trovato. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore (di solito un problema di validazione). |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **409** | Conflitto: una richiesta con la stessa Idempotency-Key è ancora in elaborazione. |  -  |
| **401** | Non autenticato: access token mancante, non valido o scaduto. Ottieni un nuovo access token e ripeti la richiesta. La risposta non ha corpo. |  * WWW-Authenticate - Schema di autenticazione richiesto (&#x60;Bearer&#x60;) ed eventuale motivo del rifiuto del token. <br>  |
| **403** | Accesso negato. Le cause possibili:  - il token non possiede lo scope richiesto dall&#39;operazione (risposta senza corpo); - &#x60;code: cloud_not_active&#x60;: l&#39;account lavora con una connessione locale e la Cloud API non espone i suoi dati; - &#x60;code: company_license_not_active&#x60;: la licenza dell&#39;azienda indicata nel percorso è scaduta o non è attiva; le altre aziende con licenza attiva restano accessibili. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


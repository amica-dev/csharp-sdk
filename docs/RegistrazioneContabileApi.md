# Amica.Sdk.Api.RegistrazioneContabileApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**RegistrazioneContabileDelete**](RegistrazioneContabileApi.md#registrazionecontabiledelete) | **DELETE** /api/v3/{companyId}/RegistrazioneContabile/{id} | Elimina un elemento. |
| [**RegistrazioneContabileGet**](RegistrazioneContabileApi.md#registrazionecontabileget) | **GET** /api/v3/{companyId}/RegistrazioneContabile/{id} | Restituisce un elemento. |
| [**RegistrazioneContabileList**](RegistrazioneContabileApi.md#registrazionecontabilelist) | **GET** /api/v3/{companyId}/RegistrazioneContabile | Restituisce una lista di elementi. |
| [**RegistrazioneContabileNewInstance**](RegistrazioneContabileApi.md#registrazionecontabilenewinstance) | **GET** /api/v3/{companyId}/RegistrazioneContabile/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile). |
| [**RegistrazioneContabilePost**](RegistrazioneContabileApi.md#registrazionecontabilepost) | **POST** /api/v3/{companyId}/RegistrazioneContabile | Salva un nuovo elemento. |
| [**RegistrazioneContabilePut**](RegistrazioneContabileApi.md#registrazionecontabileput) | **PUT** /api/v3/{companyId}/RegistrazioneContabile/{id} | Aggiorna un elemento esistente. |
| [**RegistrazioneContabileValidate**](RegistrazioneContabileApi.md#registrazionecontabilevalidate) | **POST** /api/v3/{companyId}/RegistrazioneContabile/validate | Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili. |

<a id="registrazionecontabiledelete"></a>
# **RegistrazioneContabileDelete**
> RegistrazioneContabile RegistrazioneContabileDelete (string companyId, int id, bool? validationOverride = null)

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
    public class RegistrazioneContabileDeleteExample
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

            var apiInstance = new RegistrazioneContabileApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID dell'elemento da eliminare.
            var validationOverride = true;  // bool? | Se applicare eventualmente l'override della validazione. (optional) 

            try
            {
                // Elimina un elemento.
                RegistrazioneContabile result = apiInstance.RegistrazioneContabileDelete(companyId, id, validationOverride);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabileDelete: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the RegistrazioneContabileDeleteWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Elimina un elemento.
    ApiResponse<RegistrazioneContabile> response = apiInstance.RegistrazioneContabileDeleteWithHttpInfo(companyId, id, validationOverride);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabileDeleteWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **id** | **int** | ID dell&#39;elemento da eliminare. |  |
| **validationOverride** | **bool?** | Se applicare eventualmente l&#39;override della validazione. | [optional]  |

### Return type

[**RegistrazioneContabile**](RegistrazioneContabile.md)

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
| **400** | Bad Request. Restituisce il motivo dell&#39;errore (per esempio un documento già emesso, che si             cancella solo con &#x60;validationOverride&#x3D;true&#x60;). |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  |
| **401** | Non autenticato: access token mancante, non valido o scaduto. Ottieni un nuovo access token e ripeti la richiesta. La risposta non ha corpo. |  * WWW-Authenticate - Schema di autenticazione richiesto (&#x60;Bearer&#x60;) ed eventuale motivo del rifiuto del token. <br>  |
| **403** | Accesso negato. Le cause possibili:  - il token non possiede lo scope richiesto dall&#39;operazione (risposta senza corpo); - &#x60;code: cloud_not_active&#x60;: l&#39;account lavora con una connessione locale e la Cloud API non espone i suoi dati; - &#x60;code: company_license_not_active&#x60;: la licenza dell&#39;azienda indicata nel percorso è scaduta o non è attiva; le altre aziende con licenza attiva restano accessibili. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="registrazionecontabileget"></a>
# **RegistrazioneContabileGet**
> RegistrazioneContabile RegistrazioneContabileGet (string companyId, int id, List<string>? expand = null)

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
    public class RegistrazioneContabileGetExample
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

            var apiInstance = new RegistrazioneContabileApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID dell'elemento da recuperare.
            var expand = new List<string>?(); // List<string>? | Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un'unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge `expand=Rows` alla chiamata. (optional) 

            try
            {
                // Restituisce un elemento.
                RegistrazioneContabile result = apiInstance.RegistrazioneContabileGet(companyId, id, expand);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabileGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the RegistrazioneContabileGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce un elemento.
    ApiResponse<RegistrazioneContabile> response = apiInstance.RegistrazioneContabileGetWithHttpInfo(companyId, id, expand);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabileGetWithHttpInfo: " + e.Message);
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

[**RegistrazioneContabile**](RegistrazioneContabile.md)

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

<a id="registrazionecontabilelist"></a>
# **RegistrazioneContabileList**
> List&lt;RegistrazioneContabile&gt; RegistrazioneContabileList (string companyId, int? skip = null, int? take = null, bool? skipTotalCount = null, string? filter = null, string? order = null, List<string>? expand = null)

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
    public class RegistrazioneContabileListExample
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

            var apiInstance = new RegistrazioneContabileApi(config);
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
                List<RegistrazioneContabile> result = apiInstance.RegistrazioneContabileList(companyId, skip, take, skipTotalCount, filter, order, expand);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabileList: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the RegistrazioneContabileListWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce una lista di elementi.
    ApiResponse<List<RegistrazioneContabile>> response = apiInstance.RegistrazioneContabileListWithHttpInfo(companyId, skip, take, skipTotalCount, filter, order, expand);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabileListWithHttpInfo: " + e.Message);
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

[**List&lt;RegistrazioneContabile&gt;**](RegistrazioneContabile.md)

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

<a id="registrazionecontabilenewinstance"></a>
# **RegistrazioneContabileNewInstance**
> RegistrazioneContabile RegistrazioneContabileNewInstance (string companyId)

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
    public class RegistrazioneContabileNewInstanceExample
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

            var apiInstance = new RegistrazioneContabileApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.

            try
            {
                // Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di `Documento` avrà il `Numero` appropriato, basato sui documenti preesistenti di tipo compatibile).
                RegistrazioneContabile result = apiInstance.RegistrazioneContabileNewInstance(companyId);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabileNewInstance: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the RegistrazioneContabileNewInstanceWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di `Documento` avrà il `Numero` appropriato, basato sui documenti preesistenti di tipo compatibile).
    ApiResponse<RegistrazioneContabile> response = apiInstance.RegistrazioneContabileNewInstanceWithHttpInfo(companyId);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabileNewInstanceWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |

### Return type

[**RegistrazioneContabile**](RegistrazioneContabile.md)

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

<a id="registrazionecontabilepost"></a>
# **RegistrazioneContabilePost**
> RegistrazioneContabile RegistrazioneContabilePost (string companyId, bool? validationOverride = null, string? idempotencyKey = null, RegistrazioneContabile? registrazioneContabile = null)

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
    public class RegistrazioneContabilePostExample
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

            var apiInstance = new RegistrazioneContabileApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var validationOverride = true;  // bool? | Se applicare eventualmente l'override della validazione. (optional) 
            var idempotencyKey = "idempotencyKey_example";  // string? | Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l'operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l'header `Idempotent-Replayed: true`. (optional) 
            var registrazioneContabile = new RegistrazioneContabile?(); // RegistrazioneContabile? | Istanza dell'elemento da salvare. (optional) 

            try
            {
                // Salva un nuovo elemento.
                RegistrazioneContabile result = apiInstance.RegistrazioneContabilePost(companyId, validationOverride, idempotencyKey, registrazioneContabile);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabilePost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the RegistrazioneContabilePostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Salva un nuovo elemento.
    ApiResponse<RegistrazioneContabile> response = apiInstance.RegistrazioneContabilePostWithHttpInfo(companyId, validationOverride, idempotencyKey, registrazioneContabile);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabilePostWithHttpInfo: " + e.Message);
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
| **registrazioneContabile** | [**RegistrazioneContabile?**](RegistrazioneContabile?.md) | Istanza dell&#39;elemento da salvare. | [optional]  |

### Return type

[**RegistrazioneContabile**](RegistrazioneContabile.md)

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

<a id="registrazionecontabileput"></a>
# **RegistrazioneContabilePut**
> RegistrazioneContabile RegistrazioneContabilePut (string companyId, int id, bool? validationOverride = null, string? idempotencyKey = null, RegistrazioneContabile? registrazioneContabile = null)

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
    public class RegistrazioneContabilePutExample
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

            var apiInstance = new RegistrazioneContabileApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID univoco dell'elemento da aggiornare.
            var validationOverride = true;  // bool? | Se applicare eventualmente l'override della validazione. (optional) 
            var idempotencyKey = "idempotencyKey_example";  // string? | Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l'operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l'header `Idempotent-Replayed: true`. (optional) 
            var registrazioneContabile = new RegistrazioneContabile?(); // RegistrazioneContabile? | Istanza dell'elemento da salvare. (optional) 

            try
            {
                // Aggiorna un elemento esistente.
                RegistrazioneContabile result = apiInstance.RegistrazioneContabilePut(companyId, id, validationOverride, idempotencyKey, registrazioneContabile);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabilePut: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the RegistrazioneContabilePutWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Aggiorna un elemento esistente.
    ApiResponse<RegistrazioneContabile> response = apiInstance.RegistrazioneContabilePutWithHttpInfo(companyId, id, validationOverride, idempotencyKey, registrazioneContabile);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabilePutWithHttpInfo: " + e.Message);
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
| **registrazioneContabile** | [**RegistrazioneContabile?**](RegistrazioneContabile?.md) | Istanza dell&#39;elemento da salvare. | [optional]  |

### Return type

[**RegistrazioneContabile**](RegistrazioneContabile.md)

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

<a id="registrazionecontabilevalidate"></a>
# **RegistrazioneContabileValidate**
> List&lt;RegistrazioneContabileValidationResult&gt; RegistrazioneContabileValidate (string companyId, int? year = null, string? idempotencyKey = null)

Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class RegistrazioneContabileValidateExample
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

            var apiInstance = new RegistrazioneContabileApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var year = 56;  // int? | Anno contabile da validare. (optional) 
            var idempotencyKey = "idempotencyKey_example";  // string? | Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l'operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l'header `Idempotent-Replayed: true`. (optional) 

            try
            {
                // Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili.
                List<RegistrazioneContabileValidationResult> result = apiInstance.RegistrazioneContabileValidate(companyId, year, idempotencyKey);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabileValidate: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the RegistrazioneContabileValidateWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili.
    ApiResponse<List<RegistrazioneContabileValidationResult>> response = apiInstance.RegistrazioneContabileValidateWithHttpInfo(companyId, year, idempotencyKey);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling RegistrazioneContabileApi.RegistrazioneContabileValidateWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **year** | **int?** | Anno contabile da validare. | [optional]  |
| **idempotencyKey** | **string?** | Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. | [optional]  |

### Return type

[**List&lt;RegistrazioneContabileValidationResult&gt;**](RegistrazioneContabileValidationResult.md)

### Authorization

[bearer](../README.md#bearer), [oauth2ClientCredentials](../README.md#oauth2ClientCredentials), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json, application/problem+json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce i risultati della validazione. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **429** | Troppe richieste: superato il limite di rate per il client. |  * RateLimit-Limit - Numero massimo di richieste consentite al client nella finestra corrente. <br>  * RateLimit-Remaining - Richieste ancora disponibili per il client nella finestra corrente. <br>  * RateLimit-Reset - Secondi al rinnovo dei permessi del client. <br>  * RateLimit-Policy - Policy di rate limiting applicata, nel formato &#x60;&lt;limite&gt;;w&#x3D;&lt;finestra in secondi&gt;&#x60;. <br>  * Retry-After - Secondi da attendere prima di inviare una nuova richiesta. <br>  * Idempotent-Replayed - Presente e pari a &#x60;true&#x60; quando la risposta è la replica di una richiesta precedente con la stessa Idempotency-Key. <br>  |
| **409** | Conflitto: una richiesta con la stessa Idempotency-Key è ancora in elaborazione. |  -  |
| **422** | La stessa Idempotency-Key è già stata usata per una richiesta con metodo, URL o corpo diversi. |  -  |
| **401** | Non autenticato: access token mancante, non valido o scaduto. Ottieni un nuovo access token e ripeti la richiesta. La risposta non ha corpo. |  * WWW-Authenticate - Schema di autenticazione richiesto (&#x60;Bearer&#x60;) ed eventuale motivo del rifiuto del token. <br>  |
| **403** | Accesso negato. Le cause possibili:  - il token non possiede lo scope richiesto dall&#39;operazione (risposta senza corpo); - &#x60;code: cloud_not_active&#x60;: l&#39;account lavora con una connessione locale e la Cloud API non espone i suoi dati; - &#x60;code: company_license_not_active&#x60;: la licenza dell&#39;azienda indicata nel percorso è scaduta o non è attiva; le altre aziende con licenza attiva restano accessibili. |  -  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


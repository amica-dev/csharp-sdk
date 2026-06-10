# Amica.Sdk.Api.DocumentoMediaFileApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiVversionCompanyIdDocumentoMediaFileBulkDelete**](DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafilebulkdelete) | **DELETE** /api/v{version}/{companyId}/DocumentoMediaFile/bulk | Elimina in blocco tutti i file multimediali dei documenti. |
| [**ApiVversionCompanyIdDocumentoMediaFileBulkGet**](DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafilebulkget) | **GET** /api/v{version}/{companyId}/DocumentoMediaFile/bulk | Scarica tutti i file multimediali dei documenti come archivio ZIP. |
| [**ApiVversionCompanyIdDocumentoMediaFileGet**](DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafileget) | **GET** /api/v{version}/{companyId}/DocumentoMediaFile | Restituisce una lista di elementi. |
| [**ApiVversionCompanyIdDocumentoMediaFileIdDelete**](DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafileiddelete) | **DELETE** /api/v{version}/{companyId}/DocumentoMediaFile/{id} | Elimina un elemento. |
| [**ApiVversionCompanyIdDocumentoMediaFileIdGet**](DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafileidget) | **GET** /api/v{version}/{companyId}/DocumentoMediaFile/{id} | Restituisce un elemento. |
| [**ApiVversionCompanyIdDocumentoMediaFileIdPut**](DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafileidput) | **PUT** /api/v{version}/{companyId}/DocumentoMediaFile/{id} | Aggiorna un elemento esistente. |
| [**ApiVversionCompanyIdDocumentoMediaFileNewGet**](DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafilenewget) | **GET** /api/v{version}/{companyId}/DocumentoMediaFile/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile). |
| [**ApiVversionCompanyIdDocumentoMediaFilePost**](DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafilepost) | **POST** /api/v{version}/{companyId}/DocumentoMediaFile | Salva un nuovo elemento. |

<a id="apivversioncompanyiddocumentomediafilebulkdelete"></a>
# **ApiVversionCompanyIdDocumentoMediaFileBulkDelete**
> void ApiVversionCompanyIdDocumentoMediaFileBulkDelete (string companyId, string version)

Elimina in blocco tutti i file multimediali dei documenti.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdDocumentoMediaFileBulkDeleteExample
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

            var apiInstance = new DocumentoMediaFileApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var version = "version_example";  // string | 

            try
            {
                // Elimina in blocco tutti i file multimediali dei documenti.
                apiInstance.ApiVversionCompanyIdDocumentoMediaFileBulkDelete(companyId, version);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileBulkDelete: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdDocumentoMediaFileBulkDeleteWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Elimina in blocco tutti i file multimediali dei documenti.
    apiInstance.ApiVversionCompanyIdDocumentoMediaFileBulkDeleteWithHttpInfo(companyId, version);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileBulkDeleteWithHttpInfo: " + e.Message);
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

void (empty response body)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **204** | Successo. Tutti i file sono stati eliminati. |  -  |
| **404** | Non trovato. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyiddocumentomediafilebulkget"></a>
# **ApiVversionCompanyIdDocumentoMediaFileBulkGet**
> void ApiVversionCompanyIdDocumentoMediaFileBulkGet (string companyId, string version)

Scarica tutti i file multimediali dei documenti come archivio ZIP.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdDocumentoMediaFileBulkGetExample
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

            var apiInstance = new DocumentoMediaFileApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var version = "version_example";  // string | 

            try
            {
                // Scarica tutti i file multimediali dei documenti come archivio ZIP.
                apiInstance.ApiVversionCompanyIdDocumentoMediaFileBulkGet(companyId, version);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileBulkGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdDocumentoMediaFileBulkGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Scarica tutti i file multimediali dei documenti come archivio ZIP.
    apiInstance.ApiVversionCompanyIdDocumentoMediaFileBulkGetWithHttpInfo(companyId, version);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileBulkGetWithHttpInfo: " + e.Message);
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

void (empty response body)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/zip


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce l&#39;archivio ZIP. |  -  |
| **404** | Non trovato. Nessun file disponibile. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyiddocumentomediafileget"></a>
# **ApiVversionCompanyIdDocumentoMediaFileGet**
> List&lt;DocumentoMediaFile&gt; ApiVversionCompanyIdDocumentoMediaFileGet (string version, string companyId, int? skip = null, int? take = null, bool? skipTotalCount = null, string? filter = null, string? order = null, List<string>? expand = null)

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
    public class ApiVversionCompanyIdDocumentoMediaFileGetExample
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

            var apiInstance = new DocumentoMediaFileApi(config);
            var version = "version_example";  // string | Versione API.
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
                List<DocumentoMediaFile> result = apiInstance.ApiVversionCompanyIdDocumentoMediaFileGet(version, companyId, skip, take, skipTotalCount, filter, order, expand);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdDocumentoMediaFileGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce una lista di elementi.
    ApiResponse<List<DocumentoMediaFile>> response = apiInstance.ApiVversionCompanyIdDocumentoMediaFileGetWithHttpInfo(version, companyId, skip, take, skipTotalCount, filter, order, expand);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileGetWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **skip** | **int?** | Numero di elementi da escludere dall&#39;inizio del risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. | [optional]  |
| **take** | **int?** | Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. | [optional]  |
| **skipTotalCount** | **bool?** | Indica se l&#39;header &#x60;X-Read-TotalCount&#x60; debba essere escluso dalla risposta. Impostare a &#x60;true&#x60; per prestazioni migliori a scapito dell&#39;accuratezza. Il valore predefinito è &#x60;false&#x60;. | [optional]  |
| **filter** | **string?** | Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: &#x60;$eq&#x60; (uguale), &#x60;$ne&#x60; (diverso), &#x60;$gt&#x60; (maggiore di), &#x60;$lt&#x60; (minore di), &#x60;$lte&#x60; (minore o uguale), &#x60;$gte&#x60; (maggiore o uguale), &#x60;$ct&#x60; (contiene), &#x60;$nct&#x60; (non contiene), &#x60;$sw&#x60; (inizia con), &#x60;$ew&#x60; (finisce con). Più espressioni possono essere concatenate con un connettore logico &#x60;$and&#x60; o &#x60;$or&#x60; (&#x60;$and&#x60; può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (&#x60;[]&#x60;). Nell&#39;esempio vengono inclusi tutti gli elementi con &#x60;field1&#x3D;\&quot;hello\&quot;&#x60; oppure &#x60;field2&gt;10&#x60;. | [optional]  |
| **order** | **string?** | Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell&#39;esempio i risultati sono ordinati prima per &#x60;field1&#x60; (discendente), poi per &#x60;field2&#x60;. | [optional]  |
| **expand** | [**List&lt;string&gt;?**](string.md) | Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. | [optional]  |

### Return type

[**List&lt;DocumentoMediaFile&gt;**](DocumentoMediaFile.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce la lista degli elementi corrispondenti ai criteri di ricerca. |  -  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyiddocumentomediafileiddelete"></a>
# **ApiVversionCompanyIdDocumentoMediaFileIdDelete**
> DocumentoMediaFile ApiVversionCompanyIdDocumentoMediaFileIdDelete (string version, string companyId, int id)

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
    public class ApiVversionCompanyIdDocumentoMediaFileIdDeleteExample
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

            var apiInstance = new DocumentoMediaFileApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID dell'elemento da eliminare.

            try
            {
                // Elimina un elemento.
                DocumentoMediaFile result = apiInstance.ApiVversionCompanyIdDocumentoMediaFileIdDelete(version, companyId, id);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileIdDelete: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdDocumentoMediaFileIdDeleteWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Elimina un elemento.
    ApiResponse<DocumentoMediaFile> response = apiInstance.ApiVversionCompanyIdDocumentoMediaFileIdDeleteWithHttpInfo(version, companyId, id);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileIdDeleteWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **id** | **int** | ID dell&#39;elemento da eliminare. |  |

### Return type

[**DocumentoMediaFile**](DocumentoMediaFile.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce l&#39;elemento eliminato. |  -  |
| **404** | Non trovato. L&#39;elemento non è stato trovato. |  -  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyiddocumentomediafileidget"></a>
# **ApiVversionCompanyIdDocumentoMediaFileIdGet**
> DocumentoMediaFile ApiVversionCompanyIdDocumentoMediaFileIdGet (string version, string companyId, int id, List<string>? expand = null)

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
    public class ApiVversionCompanyIdDocumentoMediaFileIdGetExample
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

            var apiInstance = new DocumentoMediaFileApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID dell'elemento da recuperare.
            var expand = new List<string>?(); // List<string>? | Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un'unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge `expand=Rows` alla chiamata. (optional) 

            try
            {
                // Restituisce un elemento.
                DocumentoMediaFile result = apiInstance.ApiVversionCompanyIdDocumentoMediaFileIdGet(version, companyId, id, expand);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileIdGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdDocumentoMediaFileIdGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce un elemento.
    ApiResponse<DocumentoMediaFile> response = apiInstance.ApiVversionCompanyIdDocumentoMediaFileIdGetWithHttpInfo(version, companyId, id, expand);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileIdGetWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **id** | **int** | ID dell&#39;elemento da recuperare. |  |
| **expand** | [**List&lt;string&gt;?**](string.md) | Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. | [optional]  |

### Return type

[**DocumentoMediaFile**](DocumentoMediaFile.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce l&#39;elemento richiesto. |  -  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore. |  -  |
| **404** | Non trovato. L&#39;elemento non è stato trovato. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyiddocumentomediafileidput"></a>
# **ApiVversionCompanyIdDocumentoMediaFileIdPut**
> DocumentoMediaFile ApiVversionCompanyIdDocumentoMediaFileIdPut (string version, string companyId, int id, bool? validationOverride = null, DocumentoMediaFile? documentoMediaFile = null)

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
    public class ApiVversionCompanyIdDocumentoMediaFileIdPutExample
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

            var apiInstance = new DocumentoMediaFileApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID univoco dell'elemento da aggiornare.
            var validationOverride = true;  // bool? | Se applicare eventualmente l'override della validazione. (optional) 
            var documentoMediaFile = new DocumentoMediaFile?(); // DocumentoMediaFile? | Istanza dell'elemento da salvare. (optional) 

            try
            {
                // Aggiorna un elemento esistente.
                DocumentoMediaFile result = apiInstance.ApiVversionCompanyIdDocumentoMediaFileIdPut(version, companyId, id, validationOverride, documentoMediaFile);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileIdPut: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdDocumentoMediaFileIdPutWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Aggiorna un elemento esistente.
    ApiResponse<DocumentoMediaFile> response = apiInstance.ApiVversionCompanyIdDocumentoMediaFileIdPutWithHttpInfo(version, companyId, id, validationOverride, documentoMediaFile);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileIdPutWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **id** | **int** | ID univoco dell&#39;elemento da aggiornare. |  |
| **validationOverride** | **bool?** | Se applicare eventualmente l&#39;override della validazione. | [optional]  |
| **documentoMediaFile** | [**DocumentoMediaFile?**](DocumentoMediaFile?.md) | Istanza dell&#39;elemento da salvare. | [optional]  |

### Return type

[**DocumentoMediaFile**](DocumentoMediaFile.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | Successo. Restituisce l&#39;elemento aggiornato con un nuovo valore di Amica.BaseEntity.ConcurrencyToken. |  -  |
| **422** | Conflitto. Restituisce il motivo del conflitto. |  -  |
| **404** | Non trovato. L&#39;elemento non è stato trovato. |  -  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore (di solito un problema di validazione). |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyiddocumentomediafilenewget"></a>
# **ApiVversionCompanyIdDocumentoMediaFileNewGet**
> DocumentoMediaFile ApiVversionCompanyIdDocumentoMediaFileNewGet (string version, string companyId)

Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdDocumentoMediaFileNewGetExample
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

            var apiInstance = new DocumentoMediaFileApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.

            try
            {
                // Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
                DocumentoMediaFile result = apiInstance.ApiVversionCompanyIdDocumentoMediaFileNewGet(version, companyId);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileNewGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdDocumentoMediaFileNewGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
    ApiResponse<DocumentoMediaFile> response = apiInstance.ApiVversionCompanyIdDocumentoMediaFileNewGetWithHttpInfo(version, companyId);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFileNewGetWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |

### Return type

[**DocumentoMediaFile**](DocumentoMediaFile.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyiddocumentomediafilepost"></a>
# **ApiVversionCompanyIdDocumentoMediaFilePost**
> DocumentoMediaFile ApiVversionCompanyIdDocumentoMediaFilePost (string version, string companyId, bool? validationOverride = null, DocumentoMediaFile? documentoMediaFile = null)

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
    public class ApiVversionCompanyIdDocumentoMediaFilePostExample
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

            var apiInstance = new DocumentoMediaFileApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var validationOverride = true;  // bool? | Se applicare eventualmente l'override della validazione. (optional) 
            var documentoMediaFile = new DocumentoMediaFile?(); // DocumentoMediaFile? | Istanza dell'elemento da salvare. (optional) 

            try
            {
                // Salva un nuovo elemento.
                DocumentoMediaFile result = apiInstance.ApiVversionCompanyIdDocumentoMediaFilePost(version, companyId, validationOverride, documentoMediaFile);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFilePost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdDocumentoMediaFilePostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Salva un nuovo elemento.
    ApiResponse<DocumentoMediaFile> response = apiInstance.ApiVversionCompanyIdDocumentoMediaFilePostWithHttpInfo(version, companyId, validationOverride, documentoMediaFile);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling DocumentoMediaFileApi.ApiVversionCompanyIdDocumentoMediaFilePostWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **validationOverride** | **bool?** | Se applicare eventualmente l&#39;override della validazione. | [optional]  |
| **documentoMediaFile** | [**DocumentoMediaFile?**](DocumentoMediaFile?.md) | Istanza dell&#39;elemento da salvare. | [optional]  |

### Return type

[**DocumentoMediaFile**](DocumentoMediaFile.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Creato. Restituisce l&#39;elemento salvato con il Amica.BaseEntity.ConcurrencyToken aggiornato. |  -  |
| **409** | Conflitto. Restituisce il motivo del conflitto. |  -  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore (di solito un problema di validazione). |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


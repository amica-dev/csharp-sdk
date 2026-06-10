# Amica.Sdk.Api.FatturaElettronicaXmlApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|--------|--------------|-------------|
| [**ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlarchivebydaterangepost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/archivebydaterange | Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlarchivebyfilenamepost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/archivebyfilename | Scarica un archivio ZIP di fatture XML, filtrate per nome file. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlbulkdelete) | **DELETE** /api/v{version}/{companyId}/FatturaElettronicaXml/bulk | Elimina in blocco tutte le fatture XML dell&#39;azienda. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlBulkGet**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlbulkget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml/bulk | Scarica tutte le fatture XML come archivio ZIP. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlBulkPost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlbulkpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/bulk | Carica in blocco fatture XML da un archivio ZIP. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlExportPost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlexportpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/export | Esporta uno o più documenti in una fattura XML. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlfilenameattachmentsget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml/{fileName}/attachments | Scarica i file allegati di una fattura XML. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlfilenameget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml/{fileName} | Recupera una fattura XML per nome file. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlGet**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml | Restituisce una lista di elementi. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlIdDelete**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmliddelete) | **DELETE** /api/v{version}/{companyId}/FatturaElettronicaXml/{id} | Elimina un elemento. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlIdGet**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlidget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml/{id} | Restituisce un elemento. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlIdPut**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlidput) | **PUT** /api/v{version}/{companyId}/FatturaElettronicaXml/{id} | Aggiorna un elemento esistente. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlImportPost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlimportpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/import | Importa una fattura XML. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlNewGet**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlnewget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile). |
| [**ApiVversionCompanyIdFatturaElettronicaXmlPairPost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlpairpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/pair | Associa una fattura XML a un documento esistente. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlPost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml | Salva un nuovo elemento. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlsdidownloadesitipost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/sdi/download/esiti | Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate. |
| [**ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlsdidownloadfatturepost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/sdi/download/fatture | Scarica le nuove fatture dal Sistema di Interscambio (SDI). |
| [**ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlsdiuploadpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/sdi/upload | Invia una o più fatture al Sistema di Interscambio (SDI). |
| [**ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost**](FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlstylesheetpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/stylesheet | Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato. |

<a id="apivversioncompanyidfatturaelettronicaxmlarchivebydaterangepost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost**
> void ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost (string version, string companyId, FatturaElettronicaXmlGetArchiveByDateRangeOptions? fatturaElettronicaXmlGetArchiveByDateRangeOptions = null)

Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date.

Non è possibile scaricare più di 350 file alla volta.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var fatturaElettronicaXmlGetArchiveByDateRangeOptions = new FatturaElettronicaXmlGetArchiveByDateRangeOptions?(); // FatturaElettronicaXmlGetArchiveByDateRangeOptions? | Opzioni di download. (optional) 

            try
            {
                // Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date.
                apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost(version, companyId, fatturaElettronicaXmlGetArchiveByDateRangeOptions);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date.
    apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostWithHttpInfo(version, companyId, fatturaElettronicaXmlGetArchiveByDateRangeOptions);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **fatturaElettronicaXmlGetArchiveByDateRangeOptions** | [**FatturaElettronicaXmlGetArchiveByDateRangeOptions?**](FatturaElettronicaXmlGetArchiveByDateRangeOptions?.md) | Opzioni di download. | [optional]  |

### Return type

void (empty response body)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/zip


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **204** | No Content |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidfatturaelettronicaxmlarchivebyfilenamepost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost**
> void ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost (string version, string companyId, FatturaElettronicaXmlGetArchiveByFileNameOptions? fatturaElettronicaXmlGetArchiveByFileNameOptions = null)

Scarica un archivio ZIP di fatture XML, filtrate per nome file.

Non è possibile scaricare più di 350 file alla volta.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var fatturaElettronicaXmlGetArchiveByFileNameOptions = new FatturaElettronicaXmlGetArchiveByFileNameOptions?(); // FatturaElettronicaXmlGetArchiveByFileNameOptions? | Opzioni di download. (optional) 

            try
            {
                // Scarica un archivio ZIP di fatture XML, filtrate per nome file.
                apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost(version, companyId, fatturaElettronicaXmlGetArchiveByFileNameOptions);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Scarica un archivio ZIP di fatture XML, filtrate per nome file.
    apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostWithHttpInfo(version, companyId, fatturaElettronicaXmlGetArchiveByFileNameOptions);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **fatturaElettronicaXmlGetArchiveByFileNameOptions** | [**FatturaElettronicaXmlGetArchiveByFileNameOptions?**](FatturaElettronicaXmlGetArchiveByFileNameOptions?.md) | Opzioni di download. | [optional]  |

### Return type

void (empty response body)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/zip


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **204** | No Content |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidfatturaelettronicaxmlbulkdelete"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete**
> void ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete (string companyId, string version)

Elimina in blocco tutte le fatture XML dell'azienda.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var version = "version_example";  // string | 

            try
            {
                // Elimina in blocco tutte le fatture XML dell'azienda.
                apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete(companyId, version);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Elimina in blocco tutte le fatture XML dell'azienda.
    apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteWithHttpInfo(companyId, version);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteWithHttpInfo: " + e.Message);
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
| **204** | Successo. Tutte le fatture XML sono state eliminate. |  -  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidfatturaelettronicaxmlbulkget"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlBulkGet**
> void ApiVversionCompanyIdFatturaElettronicaXmlBulkGet (string companyId, string version)

Scarica tutte le fatture XML come archivio ZIP.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlBulkGetExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var version = "version_example";  // string | 

            try
            {
                // Scarica tutte le fatture XML come archivio ZIP.
                apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlBulkGet(companyId, version);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlBulkGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Scarica tutte le fatture XML come archivio ZIP.
    apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlBulkGetWithHttpInfo(companyId, version);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkGetWithHttpInfo: " + e.Message);
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

<a id="apivversioncompanyidfatturaelettronicaxmlbulkpost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlBulkPost**
> void ApiVversionCompanyIdFatturaElettronicaXmlBulkPost (string companyId, string version, System.IO.Stream? file = null)

Carica in blocco fatture XML da un archivio ZIP.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlBulkPostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var version = "version_example";  // string | 
            var file = new System.IO.MemoryStream(System.IO.File.ReadAllBytes("/path/to/file.txt"));  // System.IO.Stream? | Archivio ZIP contenente i file XML. (optional) 

            try
            {
                // Carica in blocco fatture XML da un archivio ZIP.
                apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlBulkPost(companyId, version, file);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkPost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlBulkPostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Carica in blocco fatture XML da un archivio ZIP.
    apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlBulkPostWithHttpInfo(companyId, version, file);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkPostWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **version** | **string** |  |  |
| **file** | **System.IO.Stream?****System.IO.Stream?** | Archivio ZIP contenente i file XML. | [optional]  |

### Return type

void (empty response body)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: multipart/form-data
 - **Accept**: text/plain, application/json, text/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **201** | Creato. I file sono stati caricati con successo. |  -  |
| **400** | Bad Request. Restituisce il motivo dell&#39;errore. |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidfatturaelettronicaxmlexportpost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlExportPost**
> FatturaElettronicaExportResult ApiVversionCompanyIdFatturaElettronicaXmlExportPost (string version, string companyId, bool? validationOverride = null, FatturaElettronicaExportOptions? fatturaElettronicaExportOptions = null)

Esporta uno o più documenti in una fattura XML.

Tutti i documenti devono riferirsi alla stessa <a href=\"#tag/Anagrafica\">Anagrafica</a>.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlExportPostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var validationOverride = true;  // bool? | Se applicare eventualmente l'override della validazione. (optional) 
            var fatturaElettronicaExportOptions = new FatturaElettronicaExportOptions?(); // FatturaElettronicaExportOptions? | Opzioni di esportazione. (optional) 

            try
            {
                // Esporta uno o più documenti in una fattura XML.
                FatturaElettronicaExportResult result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlExportPost(version, companyId, validationOverride, fatturaElettronicaExportOptions);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlExportPost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlExportPostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Esporta uno o più documenti in una fattura XML.
    ApiResponse<FatturaElettronicaExportResult> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlExportPostWithHttpInfo(version, companyId, validationOverride, fatturaElettronicaExportOptions);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlExportPostWithHttpInfo: " + e.Message);
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
| **fatturaElettronicaExportOptions** | [**FatturaElettronicaExportOptions?**](FatturaElettronicaExportOptions?.md) | Opzioni di esportazione. | [optional]  |

### Return type

[**FatturaElettronicaExportResult**](FatturaElettronicaExportResult.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidfatturaelettronicaxmlfilenameattachmentsget"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet**
> List&lt;AllegatoDocumento&gt; ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet (string version, string companyId, string fileName)

Scarica i file allegati di una fattura XML.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var fileName = "fileName_example";  // string | Nome del file.

            try
            {
                // Scarica i file allegati di una fattura XML.
                List<AllegatoDocumento> result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet(version, companyId, fileName);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Scarica i file allegati di una fattura XML.
    ApiResponse<List<AllegatoDocumento>> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetWithHttpInfo(version, companyId, fileName);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **fileName** | **string** | Nome del file. |  |

### Return type

[**List&lt;AllegatoDocumento&gt;**](AllegatoDocumento.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidfatturaelettronicaxmlfilenameget"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet**
> void ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet (string version, string companyId, string fileName)

Recupera una fattura XML per nome file.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var fileName = "fileName_example";  // string | Nome del file.

            try
            {
                // Recupera una fattura XML per nome file.
                apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet(version, companyId, fileName);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Recupera una fattura XML per nome file.
    apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetWithHttpInfo(version, companyId, fileName);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **fileName** | **string** | Nome del file. |  |

### Return type

void (empty response body)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/xml


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidfatturaelettronicaxmlget"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlGet**
> List&lt;FatturaElettronicaXml&gt; ApiVversionCompanyIdFatturaElettronicaXmlGet (string version, string companyId, int? skip = null, int? take = null, bool? skipTotalCount = null, string? filter = null, string? order = null, List<string>? expand = null)

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
    public class ApiVversionCompanyIdFatturaElettronicaXmlGetExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
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
                List<FatturaElettronicaXml> result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlGet(version, companyId, skip, take, skipTotalCount, filter, order, expand);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce una lista di elementi.
    ApiResponse<List<FatturaElettronicaXml>> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlGetWithHttpInfo(version, companyId, skip, take, skipTotalCount, filter, order, expand);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlGetWithHttpInfo: " + e.Message);
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

[**List&lt;FatturaElettronicaXml&gt;**](FatturaElettronicaXml.md)

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

<a id="apivversioncompanyidfatturaelettronicaxmliddelete"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlIdDelete**
> FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlIdDelete (string version, string companyId, int id)

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
    public class ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID dell'elemento da eliminare.

            try
            {
                // Elimina un elemento.
                FatturaElettronicaXml result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlIdDelete(version, companyId, id);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdDelete: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Elimina un elemento.
    ApiResponse<FatturaElettronicaXml> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteWithHttpInfo(version, companyId, id);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteWithHttpInfo: " + e.Message);
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

[**FatturaElettronicaXml**](FatturaElettronicaXml.md)

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

<a id="apivversioncompanyidfatturaelettronicaxmlidget"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlIdGet**
> FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlIdGet (string version, string companyId, int id, List<string>? expand = null)

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
    public class ApiVversionCompanyIdFatturaElettronicaXmlIdGetExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID dell'elemento da recuperare.
            var expand = new List<string>?(); // List<string>? | Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un'unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge `expand=Rows` alla chiamata. (optional) 

            try
            {
                // Restituisce un elemento.
                FatturaElettronicaXml result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlIdGet(version, companyId, id, expand);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlIdGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce un elemento.
    ApiResponse<FatturaElettronicaXml> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlIdGetWithHttpInfo(version, companyId, id, expand);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdGetWithHttpInfo: " + e.Message);
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

[**FatturaElettronicaXml**](FatturaElettronicaXml.md)

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

<a id="apivversioncompanyidfatturaelettronicaxmlidput"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlIdPut**
> FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlIdPut (string version, string companyId, int id, bool? validationOverride = null, FatturaElettronicaXml? fatturaElettronicaXml = null)

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
    public class ApiVversionCompanyIdFatturaElettronicaXmlIdPutExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var id = 56;  // int | ID univoco dell'elemento da aggiornare.
            var validationOverride = true;  // bool? | Se applicare eventualmente l'override della validazione. (optional) 
            var fatturaElettronicaXml = new FatturaElettronicaXml?(); // FatturaElettronicaXml? | Istanza dell'elemento da salvare. (optional) 

            try
            {
                // Aggiorna un elemento esistente.
                FatturaElettronicaXml result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlIdPut(version, companyId, id, validationOverride, fatturaElettronicaXml);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdPut: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlIdPutWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Aggiorna un elemento esistente.
    ApiResponse<FatturaElettronicaXml> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlIdPutWithHttpInfo(version, companyId, id, validationOverride, fatturaElettronicaXml);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdPutWithHttpInfo: " + e.Message);
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
| **fatturaElettronicaXml** | [**FatturaElettronicaXml?**](FatturaElettronicaXml?.md) | Istanza dell&#39;elemento da salvare. | [optional]  |

### Return type

[**FatturaElettronicaXml**](FatturaElettronicaXml.md)

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

<a id="apivversioncompanyidfatturaelettronicaxmlimportpost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlImportPost**
> List&lt;FatturaElettronicaImportResult&gt; ApiVversionCompanyIdFatturaElettronicaXmlImportPost (string version, string companyId, DateTime? dataConsegnaSdi = null, string? identificativoSdi = null, System.IO.Stream? file = null)

Importa una fattura XML.

Viene creato un nuovo <a href=\"#tag/Documento\">Documento</a> corrispondente al contenuto XML.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlImportPostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var dataConsegnaSdi = DateTime.Parse("2013-10-20T19:20:30+01:00");  // DateTime? | Data di consegna del Sistema di Interscambio (SdI). (optional) 
            var identificativoSdi = "identificativoSdi_example";  // string? | Identificativo assegnato dal Sistema di Interscambio (SdI). (optional) 
            var file = new System.IO.MemoryStream(System.IO.File.ReadAllBytes("/path/to/file.txt"));  // System.IO.Stream? | Contenuto del file. (optional) 

            try
            {
                // Importa una fattura XML.
                List<FatturaElettronicaImportResult> result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlImportPost(version, companyId, dataConsegnaSdi, identificativoSdi, file);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlImportPost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlImportPostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Importa una fattura XML.
    ApiResponse<List<FatturaElettronicaImportResult>> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlImportPostWithHttpInfo(version, companyId, dataConsegnaSdi, identificativoSdi, file);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlImportPostWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **dataConsegnaSdi** | **DateTime?** | Data di consegna del Sistema di Interscambio (SdI). | [optional]  |
| **identificativoSdi** | **string?** | Identificativo assegnato dal Sistema di Interscambio (SdI). | [optional]  |
| **file** | **System.IO.Stream?****System.IO.Stream?** | Contenuto del file. | [optional]  |

### Return type

[**List&lt;FatturaElettronicaImportResult&gt;**](FatturaElettronicaImportResult.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: multipart/form-data
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidfatturaelettronicaxmlnewget"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlNewGet**
> FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlNewGet (string version, string companyId)

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
    public class ApiVversionCompanyIdFatturaElettronicaXmlNewGetExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.

            try
            {
                // Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
                FatturaElettronicaXml result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlNewGet(version, companyId);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlNewGet: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlNewGetWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
    ApiResponse<FatturaElettronicaXml> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlNewGetWithHttpInfo(version, companyId);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlNewGetWithHttpInfo: " + e.Message);
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

[**FatturaElettronicaXml**](FatturaElettronicaXml.md)

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

<a id="apivversioncompanyidfatturaelettronicaxmlpairpost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlPairPost**
> void ApiVversionCompanyIdFatturaElettronicaXmlPairPost (string version, string companyId, string? fileName = null, string? progressivo = null, int? documentoId = null, System.IO.Stream? file = null)

Associa una fattura XML a un documento esistente.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlPairPostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var fileName = "fileName_example";  // string? | Nome del file. (optional) 
            var progressivo = "progressivo_example";  // string? | Progressivo del Sistema di Interscambio (SdI). (optional) 
            var documentoId = 56;  // int? | Identificativo del Amica.Documento da abbinare. (optional) 
            var file = new System.IO.MemoryStream(System.IO.File.ReadAllBytes("/path/to/file.txt"));  // System.IO.Stream? | Contenuto del file. (optional) 

            try
            {
                // Associa una fattura XML a un documento esistente.
                apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlPairPost(version, companyId, fileName, progressivo, documentoId, file);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlPairPost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlPairPostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Associa una fattura XML a un documento esistente.
    apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlPairPostWithHttpInfo(version, companyId, fileName, progressivo, documentoId, file);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlPairPostWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **fileName** | **string?** | Nome del file. | [optional]  |
| **progressivo** | **string?** | Progressivo del Sistema di Interscambio (SdI). | [optional]  |
| **documentoId** | **int?** | Identificativo del Amica.Documento da abbinare. | [optional]  |
| **file** | **System.IO.Stream?****System.IO.Stream?** | Contenuto del file. | [optional]  |

### Return type

void (empty response body)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: multipart/form-data
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidfatturaelettronicaxmlpost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlPost**
> FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlPost (string version, string companyId, bool? validationOverride = null, FatturaElettronicaXml? fatturaElettronicaXml = null)

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
    public class ApiVversionCompanyIdFatturaElettronicaXmlPostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var validationOverride = true;  // bool? | Se applicare eventualmente l'override della validazione. (optional) 
            var fatturaElettronicaXml = new FatturaElettronicaXml?(); // FatturaElettronicaXml? | Istanza dell'elemento da salvare. (optional) 

            try
            {
                // Salva un nuovo elemento.
                FatturaElettronicaXml result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlPost(version, companyId, validationOverride, fatturaElettronicaXml);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlPost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlPostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Salva un nuovo elemento.
    ApiResponse<FatturaElettronicaXml> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlPostWithHttpInfo(version, companyId, validationOverride, fatturaElettronicaXml);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlPostWithHttpInfo: " + e.Message);
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
| **fatturaElettronicaXml** | [**FatturaElettronicaXml?**](FatturaElettronicaXml?.md) | Istanza dell&#39;elemento da salvare. | [optional]  |

### Return type

[**FatturaElettronicaXml**](FatturaElettronicaXml.md)

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

<a id="apivversioncompanyidfatturaelettronicaxmlsdidownloadesitipost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost**
> List&lt;FatturaElettronicaImportResult&gt; ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost (string version, string companyId)

Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.

            try
            {
                // Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate.
                List<FatturaElettronicaImportResult> result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost(version, companyId);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate.
    ApiResponse<List<FatturaElettronicaImportResult>> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostWithHttpInfo(version, companyId);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostWithHttpInfo: " + e.Message);
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

[**List&lt;FatturaElettronicaImportResult&gt;**](FatturaElettronicaImportResult.md)

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

<a id="apivversioncompanyidfatturaelettronicaxmlsdidownloadfatturepost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost**
> List&lt;FatturaElettronicaImportResult&gt; ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost (string version, string companyId)

Scarica le nuove fatture dal Sistema di Interscambio (SDI).

Il <a href=\"https://gestionaleamica.com/Software/ServizioInterscambioFatture\">Servizio di Interscambio Documenti</a> deve essere attivo per l'azienda e la relativa chiave di attivazione deve essere presente in !:ImpostazioniSdi.SdiApiKey.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.

            try
            {
                // Scarica le nuove fatture dal Sistema di Interscambio (SDI).
                List<FatturaElettronicaImportResult> result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost(version, companyId);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Scarica le nuove fatture dal Sistema di Interscambio (SDI).
    ApiResponse<List<FatturaElettronicaImportResult>> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostWithHttpInfo(version, companyId);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostWithHttpInfo: " + e.Message);
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

[**List&lt;FatturaElettronicaImportResult&gt;**](FatturaElettronicaImportResult.md)

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

<a id="apivversioncompanyidfatturaelettronicaxmlsdiuploadpost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost**
> FatturaElettronicaUploadResult ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost (string version, string companyId, FatturaElettronicaUploadOptions? fatturaElettronicaUploadOptions = null)

Invia una o più fatture al Sistema di Interscambio (SDI).

Il <a href=\"https://gestionaleamica.com/Software/ServizioInterscambioFatture\">Servizio di Interscambio Documenti</a> deve essere attivo per l'azienda e la relativa chiave di attivazione deve essere presente in !:ImpostazioniSdi.SdiApiKey.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var fatturaElettronicaUploadOptions = new FatturaElettronicaUploadOptions?(); // FatturaElettronicaUploadOptions? | Opzioni di invio. (optional) 

            try
            {
                // Invia una o più fatture al Sistema di Interscambio (SDI).
                FatturaElettronicaUploadResult result = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost(version, companyId, fatturaElettronicaUploadOptions);
                Debug.WriteLine(result);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Invia una o più fatture al Sistema di Interscambio (SDI).
    ApiResponse<FatturaElettronicaUploadResult> response = apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostWithHttpInfo(version, companyId, fatturaElettronicaUploadOptions);
    Debug.Write("Status Code: " + response.StatusCode);
    Debug.Write("Response Headers: " + response.Headers);
    Debug.Write("Response Body: " + response.Data);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **fatturaElettronicaUploadOptions** | [**FatturaElettronicaUploadOptions?**](FatturaElettronicaUploadOptions?.md) | Opzioni di invio. | [optional]  |

### Return type

[**FatturaElettronicaUploadResult**](FatturaElettronicaUploadResult.md)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/json


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

<a id="apivversioncompanyidfatturaelettronicaxmlstylesheetpost"></a>
# **ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost**
> void ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost (string version, string companyId, FatturaElettronicaXmlStyleSheetData? fatturaElettronicaXmlStyleSheetData = null)

Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato.

### Example
```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostExample
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

            var apiInstance = new FatturaElettronicaXmlApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var fatturaElettronicaXmlStyleSheetData = new FatturaElettronicaXmlStyleSheetData?(); // FatturaElettronicaXmlStyleSheetData? | Opzioni di corrispondenza e stile. (optional) 

            try
            {
                // Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato.
                apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost(version, companyId, fatturaElettronicaXmlStyleSheetData);
            }
            catch (ApiException  e)
            {
                Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost: " + e.Message);
                Debug.Print("Status Code: " + e.ErrorCode);
                Debug.Print(e.StackTrace);
            }
        }
    }
}
```

#### Using the ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostWithHttpInfo variant
This returns an ApiResponse object which contains the response data, status code and headers.

```csharp
try
{
    // Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato.
    apiInstance.ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostWithHttpInfo(version, companyId, fatturaElettronicaXmlStyleSheetData);
}
catch (ApiException e)
{
    Debug.Print("Exception when calling FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostWithHttpInfo: " + e.Message);
    Debug.Print("Status Code: " + e.ErrorCode);
    Debug.Print(e.StackTrace);
}
```

### Parameters

| Name | Type | Description | Notes |
|------|------|-------------|-------|
| **version** | **string** | Versione API. |  |
| **companyId** | **string** | ID univoco dell&#39;azienda. |  |
| **fatturaElettronicaXmlStyleSheetData** | [**FatturaElettronicaXmlStyleSheetData?**](FatturaElettronicaXmlStyleSheetData?.md) | Opzioni di corrispondenza e stile. | [optional]  |

### Return type

void (empty response body)

### Authorization

[bearer](../README.md#bearer), [oauth2](../README.md#oauth2), [oauth2](../README.md#oauth2)

### HTTP request headers

 - **Content-Type**: application/json, text/json, application/*+json
 - **Accept**: application/xml


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)


# Amica.Sdk - the C# library for the Customers API

La Customers API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).

## Autenticazione

Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:

```
Authorization: Bearer <token>
```

### Registra la tua app

Per integrare la Customers API registra la tua applicazione sull'**App Store di Amica**. Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.

### Scegli il flusso

- **Authorization Code con PKCE** (consigliato per app che operano per conto di un utente Amica): l'utente autorizza la tua app tramite la pagina di consenso dell'identity provider; la tua app riceve un `code` da scambiare con un access token.

  ```
  GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256
  POST {identity-provider}/connect/token
       grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri>
  ```

- **Client Credentials** (per integrazioni server-to-server, senza utente interattivo):

  ```
  POST {identity-provider}/connect/token
  Content-Type: application/x-www-form-urlencoded

  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=read write documenti contabilita
  ```

  > **Nota:** il flusso Client Credentials è disponibile solo per le **app private**, non per quelle pubblicate sul marketplace.

In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.

## Scope di autorizzazione

L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:

| Scope | Descrizione |
|- -- -- --|- -- -- -- -- -- --|
| `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. |
| `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. |
| `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. |
| `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |

Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.

## Company ID

La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:

```
/api/v3/{companyId}/risorsa
```

Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.

## Paginazione

Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:

| Parametro | Descrizione |
|- -- -- -- -- --|- -- -- -- -- -- --|
| `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. |
| `Take` | Numero di elementi da includere nella risposta. |

La risposta include due header:

| Header | Descrizione |
|- -- -- -- -|- -- -- -- -- -- --|
| `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. |
| `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |

## Ordinamento e filtri

Gli endpoint lista accettano parametri aggiuntivi:

| Parametro | Descrizione |
|- -- -- -- -- --|- -- -- -- -- -- --|
| `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. |
| `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. |
| `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |

## Espansione oggetti correlati

Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.

## Gestione errori

L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:

| Codice | Significato |
|- -- -- -- -|- -- -- -- -- -- --|
| `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. |
| `401` | Unauthorized — token mancante o scaduto. |
| `403` | Forbidden — il token non possiede lo scope richiesto. |
| `404` | Not Found — la risorsa richiesta non esiste. |
| `409` | Conflict — conflitto di concorrenza. |
| `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |

Alcune risposte `400` includono un campo `overridable` che indica se l'errore di validazione può essere ignorato impostando il parametro `validationOverride` a `true`.

## Controllo della concorrenza

Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`.


This C# SDK is automatically generated by the [OpenAPI Generator](https://openapi-generator.tech) project:

- API version: v3
- SDK version: 1.0.0
- Generator version: 7.11.0
- Build package: org.openapitools.codegen.languages.CSharpClientCodegen
    For more information, please visit [https://amica.dev](https://amica.dev)

<a id="frameworks-supported"></a>
## Frameworks supported

<a id="dependencies"></a>
## Dependencies

- [RestSharp](https://www.nuget.org/packages/RestSharp) - 112.0.0 or later
- [Json.NET](https://www.nuget.org/packages/Newtonsoft.Json/) - 13.0.2 or later
- [JsonSubTypes](https://www.nuget.org/packages/JsonSubTypes/) - 1.8.0 or later
- [System.ComponentModel.Annotations](https://www.nuget.org/packages/System.ComponentModel.Annotations) - 5.0.0 or later

The DLLs included in the package may not be the latest version. We recommend using [NuGet](https://docs.nuget.org/consume/installing-nuget) to obtain the latest version of the packages:
```
Install-Package RestSharp
Install-Package Newtonsoft.Json
Install-Package JsonSubTypes
Install-Package System.ComponentModel.Annotations
```

NOTE: RestSharp versions greater than 105.1.0 have a bug which causes file uploads to fail. See [RestSharp#742](https://github.com/restsharp/RestSharp/issues/742).
NOTE: RestSharp for .Net Core creates a new socket for each api call, which can lead to a socket exhaustion problem. See [RestSharp#1406](https://github.com/restsharp/RestSharp/issues/1406).

<a id="installation"></a>
## Installation
Run the following command to generate the DLL
- [Mac/Linux] `/bin/sh build.sh`
- [Windows] `build.bat`

Then include the DLL (under the `bin` folder) in the C# project, and use the namespaces:
```csharp
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;
```
<a id="packaging"></a>
## Packaging

A `.nuspec` is included with the project. You can follow the Nuget quickstart to [create](https://docs.microsoft.com/en-us/nuget/quickstart/create-and-publish-a-package#create-the-package) and [publish](https://docs.microsoft.com/en-us/nuget/quickstart/create-and-publish-a-package#publish-the-package) packages.

This `.nuspec` uses placeholders from the `.csproj`, so build the `.csproj` directly:

```
nuget pack -Build -OutputDirectory out Amica.Sdk.csproj
```

Then, publish to a [local feed](https://docs.microsoft.com/en-us/nuget/hosting-packages/local-feeds) or [other host](https://docs.microsoft.com/en-us/nuget/hosting-packages/overview) and consume the new package via Nuget as usual.

<a id="usage"></a>
## Usage

To use the API client with a HTTP proxy, setup a `System.Net.WebProxy`
```csharp
Configuration c = new Configuration();
System.Net.WebProxy webProxy = new System.Net.WebProxy("http://myProxyUrl:80/");
webProxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
c.Proxy = webProxy;
```

<a id="getting-started"></a>
## Getting Started

```csharp
using System.Collections.Generic;
using System.Diagnostics;
using Amica.Sdk.Api;
using Amica.Sdk.Client;
using Amica.Sdk.Model;

namespace Example
{
    public class Example
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

            var apiInstance = new AnagraficaApi(config);
            var version = "version_example";  // string | Versione API.
            var companyId = "companyId_example";  // string | ID univoco dell'azienda.
            var selector = RagioneSociale1;  // string | Il campo usato per distinguere gli elementi.
            var skip = 100;  // int? | Numero di elementi, a partire dal primo, da escludere dal risultato. Combinato con `Take`, è utile per la paginazione. (optional) 
            var take = 25;  // int? | Numero di elementi da includere nel risultato. Combinato con `Skip`, è utile per la paginazione. (optional) 

            try
            {
                // Restituisce una lista di elementi distinti.
                List<DistinctPropertyGroup> result = apiInstance.ApiVversionCompanyIdAnagraficaDistinctGet(version, companyId, selector, skip, take);
                Debug.WriteLine(result);
            }
            catch (ApiException e)
            {
                Debug.Print("Exception when calling AnagraficaApi.ApiVversionCompanyIdAnagraficaDistinctGet: " + e.Message );
                Debug.Print("Status Code: "+ e.ErrorCode);
                Debug.Print(e.StackTrace);
            }

        }
    }
}
```

<a id="documentation-for-api-endpoints"></a>
## Documentation for API Endpoints

All URIs are relative to *http://localhost*

Class | Method | HTTP request | Description
------------ | ------------- | ------------- | -------------
*AnagraficaApi* | [**ApiVversionCompanyIdAnagraficaDistinctGet**](docs/AnagraficaApi.md#apivversioncompanyidanagraficadistinctget) | **GET** /api/v{version}/{companyId}/Anagrafica/distinct | Restituisce una lista di elementi distinti.
*AnagraficaApi* | [**ApiVversionCompanyIdAnagraficaGet**](docs/AnagraficaApi.md#apivversioncompanyidanagraficaget) | **GET** /api/v{version}/{companyId}/Anagrafica | Restituisce una lista di elementi.
*AnagraficaApi* | [**ApiVversionCompanyIdAnagraficaIdDelete**](docs/AnagraficaApi.md#apivversioncompanyidanagraficaiddelete) | **DELETE** /api/v{version}/{companyId}/Anagrafica/{id} | Elimina un elemento.
*AnagraficaApi* | [**ApiVversionCompanyIdAnagraficaIdGet**](docs/AnagraficaApi.md#apivversioncompanyidanagraficaidget) | **GET** /api/v{version}/{companyId}/Anagrafica/{id} | Restituisce un elemento.
*AnagraficaApi* | [**ApiVversionCompanyIdAnagraficaIdPut**](docs/AnagraficaApi.md#apivversioncompanyidanagraficaidput) | **PUT** /api/v{version}/{companyId}/Anagrafica/{id} | Aggiorna un elemento esistente.
*AnagraficaApi* | [**ApiVversionCompanyIdAnagraficaNewGet**](docs/AnagraficaApi.md#apivversioncompanyidanagraficanewget) | **GET** /api/v{version}/{companyId}/Anagrafica/new | Restituisce una nuova istanza di un contatto del tipo specificato.
*AnagraficaApi* | [**ApiVversionCompanyIdAnagraficaPost**](docs/AnagraficaApi.md#apivversioncompanyidanagraficapost) | **POST** /api/v{version}/{companyId}/Anagrafica | Salva un nuovo elemento.
*AreaGeograficaApi* | [**ApiVversionCompanyIdAreaGeograficaDistinctGet**](docs/AreaGeograficaApi.md#apivversioncompanyidareageograficadistinctget) | **GET** /api/v{version}/{companyId}/AreaGeografica/distinct | Restituisce una lista di elementi distinti.
*AreaGeograficaApi* | [**ApiVversionCompanyIdAreaGeograficaGet**](docs/AreaGeograficaApi.md#apivversioncompanyidareageograficaget) | **GET** /api/v{version}/{companyId}/AreaGeografica | Restituisce una lista di elementi.
*AreaGeograficaApi* | [**ApiVversionCompanyIdAreaGeograficaIdDelete**](docs/AreaGeograficaApi.md#apivversioncompanyidareageograficaiddelete) | **DELETE** /api/v{version}/{companyId}/AreaGeografica/{id} | Elimina un elemento.
*AreaGeograficaApi* | [**ApiVversionCompanyIdAreaGeograficaIdGet**](docs/AreaGeograficaApi.md#apivversioncompanyidareageograficaidget) | **GET** /api/v{version}/{companyId}/AreaGeografica/{id} | Restituisce un elemento.
*AreaGeograficaApi* | [**ApiVversionCompanyIdAreaGeograficaIdPut**](docs/AreaGeograficaApi.md#apivversioncompanyidareageograficaidput) | **PUT** /api/v{version}/{companyId}/AreaGeografica/{id} | Aggiorna un elemento esistente.
*AreaGeograficaApi* | [**ApiVversionCompanyIdAreaGeograficaNewGet**](docs/AreaGeograficaApi.md#apivversioncompanyidareageograficanewget) | **GET** /api/v{version}/{companyId}/AreaGeografica/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*AreaGeograficaApi* | [**ApiVversionCompanyIdAreaGeograficaPost**](docs/AreaGeograficaApi.md#apivversioncompanyidareageograficapost) | **POST** /api/v{version}/{companyId}/AreaGeografica | Salva un nuovo elemento.
*ArticoloApi* | [**ApiVversionCompanyIdArticoloGet**](docs/ArticoloApi.md#apivversioncompanyidarticologet) | **GET** /api/v{version}/{companyId}/Articolo | Restituisce una lista di elementi.
*ArticoloApi* | [**ApiVversionCompanyIdArticoloIdDelete**](docs/ArticoloApi.md#apivversioncompanyidarticoloiddelete) | **DELETE** /api/v{version}/{companyId}/Articolo/{id} | Elimina un elemento.
*ArticoloApi* | [**ApiVversionCompanyIdArticoloIdGet**](docs/ArticoloApi.md#apivversioncompanyidarticoloidget) | **GET** /api/v{version}/{companyId}/Articolo/{id} | Restituisce un elemento.
*ArticoloApi* | [**ApiVversionCompanyIdArticoloIdPut**](docs/ArticoloApi.md#apivversioncompanyidarticoloidput) | **PUT** /api/v{version}/{companyId}/Articolo/{id} | Aggiorna un elemento esistente.
*ArticoloApi* | [**ApiVversionCompanyIdArticoloNewGet**](docs/ArticoloApi.md#apivversioncompanyidarticolonewget) | **GET** /api/v{version}/{companyId}/Articolo/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*ArticoloApi* | [**ApiVversionCompanyIdArticoloPost**](docs/ArticoloApi.md#apivversioncompanyidarticolopost) | **POST** /api/v{version}/{companyId}/Articolo | Salva un nuovo elemento.
*ArticoloMediaFileApi* | [**ApiVversionCompanyIdArticoloMediaFileBulkDelete**](docs/ArticoloMediaFileApi.md#apivversioncompanyidarticolomediafilebulkdelete) | **DELETE** /api/v{version}/{companyId}/ArticoloMediaFile/bulk | Elimina in blocco tutti i file multimediali degli articoli.
*ArticoloMediaFileApi* | [**ApiVversionCompanyIdArticoloMediaFileBulkGet**](docs/ArticoloMediaFileApi.md#apivversioncompanyidarticolomediafilebulkget) | **GET** /api/v{version}/{companyId}/ArticoloMediaFile/bulk | Scarica tutti i file multimediali degli articoli come archivio ZIP.
*ArticoloMediaFileApi* | [**ApiVversionCompanyIdArticoloMediaFileGet**](docs/ArticoloMediaFileApi.md#apivversioncompanyidarticolomediafileget) | **GET** /api/v{version}/{companyId}/ArticoloMediaFile | Restituisce una lista di elementi.
*ArticoloMediaFileApi* | [**ApiVversionCompanyIdArticoloMediaFileIdDelete**](docs/ArticoloMediaFileApi.md#apivversioncompanyidarticolomediafileiddelete) | **DELETE** /api/v{version}/{companyId}/ArticoloMediaFile/{id} | Elimina un elemento.
*ArticoloMediaFileApi* | [**ApiVversionCompanyIdArticoloMediaFileIdGet**](docs/ArticoloMediaFileApi.md#apivversioncompanyidarticolomediafileidget) | **GET** /api/v{version}/{companyId}/ArticoloMediaFile/{id} | Restituisce un elemento.
*ArticoloMediaFileApi* | [**ApiVversionCompanyIdArticoloMediaFileIdPut**](docs/ArticoloMediaFileApi.md#apivversioncompanyidarticolomediafileidput) | **PUT** /api/v{version}/{companyId}/ArticoloMediaFile/{id} | Aggiorna un elemento esistente.
*ArticoloMediaFileApi* | [**ApiVversionCompanyIdArticoloMediaFileNewGet**](docs/ArticoloMediaFileApi.md#apivversioncompanyidarticolomediafilenewget) | **GET** /api/v{version}/{companyId}/ArticoloMediaFile/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*ArticoloMediaFileApi* | [**ApiVversionCompanyIdArticoloMediaFilePost**](docs/ArticoloMediaFileApi.md#apivversioncompanyidarticolomediafilepost) | **POST** /api/v{version}/{companyId}/ArticoloMediaFile | Salva un nuovo elemento.
*AziendaApi* | [**ApiVversionConfigAziendaGet**](docs/AziendaApi.md#apivversionconfigaziendaget) | **GET** /api/v{version}/config/Azienda | Restituisce una lista di elementi.
*AziendaApi* | [**ApiVversionConfigAziendaIdGet**](docs/AziendaApi.md#apivversionconfigaziendaidget) | **GET** /api/v{version}/config/Azienda/{id} | Restituisce un elemento.
*BeneServizioApi* | [**ApiVversionCompanyIdBeneServizioCollegadocumentoPost**](docs/BeneServizioApi.md#apivversioncompanyidbeneserviziocollegadocumentopost) | **POST** /api/v{version}/{companyId}/BeneServizio/collegadocumento | Collega un bene/servizio a un documento.
*BeneServizioApi* | [**ApiVversionCompanyIdBeneServizioComputePost**](docs/BeneServizioApi.md#apivversioncompanyidbeneserviziocomputepost) | **POST** /api/v{version}/{companyId}/BeneServizio/compute | Calcola gli importi di un Amica.BeneServizio (vedi Amica.BeneServizioComputeResult), considerando parametri come quantità movimentata, IVA applicata, eventuali sconti.
*BeneServizioApi* | [**ApiVversionCompanyIdBeneServizioGet**](docs/BeneServizioApi.md#apivversioncompanyidbeneservizioget) | **GET** /api/v{version}/{companyId}/BeneServizio | Restituisce una lista di elementi.
*BeneServizioApi* | [**ApiVversionCompanyIdBeneServizioIdDelete**](docs/BeneServizioApi.md#apivversioncompanyidbeneservizioiddelete) | **DELETE** /api/v{version}/{companyId}/BeneServizio/{id} | Elimina un elemento.
*BeneServizioApi* | [**ApiVversionCompanyIdBeneServizioIdGet**](docs/BeneServizioApi.md#apivversioncompanyidbeneservizioidget) | **GET** /api/v{version}/{companyId}/BeneServizio/{id} | Restituisce un elemento.
*BeneServizioApi* | [**ApiVversionCompanyIdBeneServizioIdPut**](docs/BeneServizioApi.md#apivversioncompanyidbeneservizioidput) | **PUT** /api/v{version}/{companyId}/BeneServizio/{id} | Aggiorna un elemento esistente.
*BeneServizioApi* | [**ApiVversionCompanyIdBeneServizioNewGet**](docs/BeneServizioApi.md#apivversioncompanyidbeneservizionewget) | **GET** /api/v{version}/{companyId}/BeneServizio/new | Restituisce una nuova istanza di un bene/servizio.
*BeneServizioApi* | [**ApiVversionCompanyIdBeneServizioPost**](docs/BeneServizioApi.md#apivversioncompanyidbeneserviziopost) | **POST** /api/v{version}/{companyId}/BeneServizio | Salva un nuovo elemento.
*CassaPrevidenzialeApi* | [**ApiVversionCompanyIdTabelleCassaPrevidenzialeGet**](docs/CassaPrevidenzialeApi.md#apivversioncompanyidtabellecassaprevidenzialeget) | **GET** /api/v{version}/{companyId}/tabelle/CassaPrevidenziale | Restituisce il contenuto della tabella come lista di elementi.
*CastellettoIvaApi* | [**ApiVversionCompanyIdCastellettoIvaGet**](docs/CastellettoIvaApi.md#apivversioncompanyidcastellettoivaget) | **GET** /api/v{version}/{companyId}/CastellettoIva | Restituisce una lista di elementi.
*CastellettoIvaApi* | [**ApiVversionCompanyIdCastellettoIvaIdDelete**](docs/CastellettoIvaApi.md#apivversioncompanyidcastellettoivaiddelete) | **DELETE** /api/v{version}/{companyId}/CastellettoIva/{id} | Elimina un elemento.
*CastellettoIvaApi* | [**ApiVversionCompanyIdCastellettoIvaIdGet**](docs/CastellettoIvaApi.md#apivversioncompanyidcastellettoivaidget) | **GET** /api/v{version}/{companyId}/CastellettoIva/{id} | Restituisce un elemento.
*CastellettoIvaApi* | [**ApiVversionCompanyIdCastellettoIvaIdPut**](docs/CastellettoIvaApi.md#apivversioncompanyidcastellettoivaidput) | **PUT** /api/v{version}/{companyId}/CastellettoIva/{id} | Aggiorna un elemento esistente.
*CastellettoIvaApi* | [**ApiVversionCompanyIdCastellettoIvaNewGet**](docs/CastellettoIvaApi.md#apivversioncompanyidcastellettoivanewget) | **GET** /api/v{version}/{companyId}/CastellettoIva/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*CastellettoIvaApi* | [**ApiVversionCompanyIdCastellettoIvaPost**](docs/CastellettoIvaApi.md#apivversioncompanyidcastellettoivapost) | **POST** /api/v{version}/{companyId}/CastellettoIva | Salva un nuovo elemento.
*CategoriaAnagraficaApi* | [**ApiVversionCompanyIdCategoriaAnagraficaDistinctGet**](docs/CategoriaAnagraficaApi.md#apivversioncompanyidcategoriaanagraficadistinctget) | **GET** /api/v{version}/{companyId}/CategoriaAnagrafica/distinct | Restituisce una lista di elementi distinti.
*CategoriaAnagraficaApi* | [**ApiVversionCompanyIdCategoriaAnagraficaGet**](docs/CategoriaAnagraficaApi.md#apivversioncompanyidcategoriaanagraficaget) | **GET** /api/v{version}/{companyId}/CategoriaAnagrafica | Restituisce una lista di elementi.
*CategoriaAnagraficaApi* | [**ApiVversionCompanyIdCategoriaAnagraficaIdDelete**](docs/CategoriaAnagraficaApi.md#apivversioncompanyidcategoriaanagraficaiddelete) | **DELETE** /api/v{version}/{companyId}/CategoriaAnagrafica/{id} | Elimina un elemento.
*CategoriaAnagraficaApi* | [**ApiVversionCompanyIdCategoriaAnagraficaIdGet**](docs/CategoriaAnagraficaApi.md#apivversioncompanyidcategoriaanagraficaidget) | **GET** /api/v{version}/{companyId}/CategoriaAnagrafica/{id} | Restituisce un elemento.
*CategoriaAnagraficaApi* | [**ApiVversionCompanyIdCategoriaAnagraficaIdPut**](docs/CategoriaAnagraficaApi.md#apivversioncompanyidcategoriaanagraficaidput) | **PUT** /api/v{version}/{companyId}/CategoriaAnagrafica/{id} | Aggiorna un elemento esistente.
*CategoriaAnagraficaApi* | [**ApiVversionCompanyIdCategoriaAnagraficaNewGet**](docs/CategoriaAnagraficaApi.md#apivversioncompanyidcategoriaanagraficanewget) | **GET** /api/v{version}/{companyId}/CategoriaAnagrafica/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*CategoriaAnagraficaApi* | [**ApiVversionCompanyIdCategoriaAnagraficaPost**](docs/CategoriaAnagraficaApi.md#apivversioncompanyidcategoriaanagraficapost) | **POST** /api/v{version}/{companyId}/CategoriaAnagrafica | Salva un nuovo elemento.
*CategoriaMerceologicaApi* | [**ApiVversionCompanyIdCategoriaMerceologicaGet**](docs/CategoriaMerceologicaApi.md#apivversioncompanyidcategoriamerceologicaget) | **GET** /api/v{version}/{companyId}/CategoriaMerceologica | Restituisce una lista di elementi.
*CategoriaMerceologicaApi* | [**ApiVversionCompanyIdCategoriaMerceologicaIdDelete**](docs/CategoriaMerceologicaApi.md#apivversioncompanyidcategoriamerceologicaiddelete) | **DELETE** /api/v{version}/{companyId}/CategoriaMerceologica/{id} | Elimina un elemento.
*CategoriaMerceologicaApi* | [**ApiVversionCompanyIdCategoriaMerceologicaIdGet**](docs/CategoriaMerceologicaApi.md#apivversioncompanyidcategoriamerceologicaidget) | **GET** /api/v{version}/{companyId}/CategoriaMerceologica/{id} | Restituisce un elemento.
*CategoriaMerceologicaApi* | [**ApiVversionCompanyIdCategoriaMerceologicaIdPut**](docs/CategoriaMerceologicaApi.md#apivversioncompanyidcategoriamerceologicaidput) | **PUT** /api/v{version}/{companyId}/CategoriaMerceologica/{id} | Aggiorna un elemento esistente.
*CategoriaMerceologicaApi* | [**ApiVversionCompanyIdCategoriaMerceologicaNewGet**](docs/CategoriaMerceologicaApi.md#apivversioncompanyidcategoriamerceologicanewget) | **GET** /api/v{version}/{companyId}/CategoriaMerceologica/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*CategoriaMerceologicaApi* | [**ApiVversionCompanyIdCategoriaMerceologicaPost**](docs/CategoriaMerceologicaApi.md#apivversioncompanyidcategoriamerceologicapost) | **POST** /api/v{version}/{companyId}/CategoriaMerceologica | Salva un nuovo elemento.
*CausaleDocumentoApi* | [**ApiVversionCompanyIdCausaleDocumentoGet**](docs/CausaleDocumentoApi.md#apivversioncompanyidcausaledocumentoget) | **GET** /api/v{version}/{companyId}/CausaleDocumento | Restituisce una lista di elementi.
*CausaleDocumentoApi* | [**ApiVversionCompanyIdCausaleDocumentoIdDelete**](docs/CausaleDocumentoApi.md#apivversioncompanyidcausaledocumentoiddelete) | **DELETE** /api/v{version}/{companyId}/CausaleDocumento/{id} | Elimina un elemento.
*CausaleDocumentoApi* | [**ApiVversionCompanyIdCausaleDocumentoIdGet**](docs/CausaleDocumentoApi.md#apivversioncompanyidcausaledocumentoidget) | **GET** /api/v{version}/{companyId}/CausaleDocumento/{id} | Restituisce un elemento.
*CausaleDocumentoApi* | [**ApiVversionCompanyIdCausaleDocumentoIdPut**](docs/CausaleDocumentoApi.md#apivversioncompanyidcausaledocumentoidput) | **PUT** /api/v{version}/{companyId}/CausaleDocumento/{id} | Aggiorna un elemento esistente.
*CausaleDocumentoApi* | [**ApiVversionCompanyIdCausaleDocumentoNewGet**](docs/CausaleDocumentoApi.md#apivversioncompanyidcausaledocumentonewget) | **GET** /api/v{version}/{companyId}/CausaleDocumento/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*CausaleDocumentoApi* | [**ApiVversionCompanyIdCausaleDocumentoPost**](docs/CausaleDocumentoApi.md#apivversioncompanyidcausaledocumentopost) | **POST** /api/v{version}/{companyId}/CausaleDocumento | Salva un nuovo elemento.
*CausaleIvaApi* | [**ApiVversionCompanyIdCausaleIvaGet**](docs/CausaleIvaApi.md#apivversioncompanyidcausaleivaget) | **GET** /api/v{version}/{companyId}/CausaleIva | Restituisce una lista di elementi.
*CausaleIvaApi* | [**ApiVversionCompanyIdCausaleIvaIdDelete**](docs/CausaleIvaApi.md#apivversioncompanyidcausaleivaiddelete) | **DELETE** /api/v{version}/{companyId}/CausaleIva/{id} | Elimina un elemento.
*CausaleIvaApi* | [**ApiVversionCompanyIdCausaleIvaIdGet**](docs/CausaleIvaApi.md#apivversioncompanyidcausaleivaidget) | **GET** /api/v{version}/{companyId}/CausaleIva/{id} | Restituisce un elemento.
*CausaleIvaApi* | [**ApiVversionCompanyIdCausaleIvaIdPut**](docs/CausaleIvaApi.md#apivversioncompanyidcausaleivaidput) | **PUT** /api/v{version}/{companyId}/CausaleIva/{id} | Aggiorna un elemento esistente.
*CausaleIvaApi* | [**ApiVversionCompanyIdCausaleIvaNewGet**](docs/CausaleIvaApi.md#apivversioncompanyidcausaleivanewget) | **GET** /api/v{version}/{companyId}/CausaleIva/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*CausaleIvaApi* | [**ApiVversionCompanyIdCausaleIvaPost**](docs/CausaleIvaApi.md#apivversioncompanyidcausaleivapost) | **POST** /api/v{version}/{companyId}/CausaleIva | Salva un nuovo elemento.
*CausaleMagazzinoApi* | [**ApiVversionCompanyIdCausaleMagazzinoGet**](docs/CausaleMagazzinoApi.md#apivversioncompanyidcausalemagazzinoget) | **GET** /api/v{version}/{companyId}/CausaleMagazzino | Restituisce una lista di elementi.
*CausaleMagazzinoApi* | [**ApiVversionCompanyIdCausaleMagazzinoIdDelete**](docs/CausaleMagazzinoApi.md#apivversioncompanyidcausalemagazzinoiddelete) | **DELETE** /api/v{version}/{companyId}/CausaleMagazzino/{id} | Elimina un elemento.
*CausaleMagazzinoApi* | [**ApiVversionCompanyIdCausaleMagazzinoIdGet**](docs/CausaleMagazzinoApi.md#apivversioncompanyidcausalemagazzinoidget) | **GET** /api/v{version}/{companyId}/CausaleMagazzino/{id} | Restituisce un elemento.
*CausaleMagazzinoApi* | [**ApiVversionCompanyIdCausaleMagazzinoIdPut**](docs/CausaleMagazzinoApi.md#apivversioncompanyidcausalemagazzinoidput) | **PUT** /api/v{version}/{companyId}/CausaleMagazzino/{id} | Aggiorna un elemento esistente.
*CausaleMagazzinoApi* | [**ApiVversionCompanyIdCausaleMagazzinoNewGet**](docs/CausaleMagazzinoApi.md#apivversioncompanyidcausalemagazzinonewget) | **GET** /api/v{version}/{companyId}/CausaleMagazzino/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*CausaleMagazzinoApi* | [**ApiVversionCompanyIdCausaleMagazzinoPost**](docs/CausaleMagazzinoApi.md#apivversioncompanyidcausalemagazzinopost) | **POST** /api/v{version}/{companyId}/CausaleMagazzino | Salva un nuovo elemento.
*CodicePagamentoApi* | [**ApiVversionCompanyIdTabelleCodicePagamentoGet**](docs/CodicePagamentoApi.md#apivversioncompanyidtabellecodicepagamentoget) | **GET** /api/v{version}/{companyId}/tabelle/CodicePagamento | Restituisce il contenuto della tabella come lista di elementi.
*ContoContabileApi* | [**ApiVversionCompanyIdContoContabileGet**](docs/ContoContabileApi.md#apivversioncompanyidcontocontabileget) | **GET** /api/v{version}/{companyId}/ContoContabile | Restituisce una lista di elementi.
*ContoContabileApi* | [**ApiVversionCompanyIdContoContabileIdDelete**](docs/ContoContabileApi.md#apivversioncompanyidcontocontabileiddelete) | **DELETE** /api/v{version}/{companyId}/ContoContabile/{id} | Elimina un elemento.
*ContoContabileApi* | [**ApiVversionCompanyIdContoContabileIdGet**](docs/ContoContabileApi.md#apivversioncompanyidcontocontabileidget) | **GET** /api/v{version}/{companyId}/ContoContabile/{id} | Restituisce un elemento.
*ContoContabileApi* | [**ApiVversionCompanyIdContoContabileIdPut**](docs/ContoContabileApi.md#apivversioncompanyidcontocontabileidput) | **PUT** /api/v{version}/{companyId}/ContoContabile/{id} | Aggiorna un elemento esistente.
*ContoContabileApi* | [**ApiVversionCompanyIdContoContabileNewGet**](docs/ContoContabileApi.md#apivversioncompanyidcontocontabilenewget) | **GET** /api/v{version}/{companyId}/ContoContabile/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*ContoContabileApi* | [**ApiVversionCompanyIdContoContabilePost**](docs/ContoContabileApi.md#apivversioncompanyidcontocontabilepost) | **POST** /api/v{version}/{companyId}/ContoContabile | Salva un nuovo elemento.
*ContoContabileApi* | [**ApiVversionCompanyIdContoContabileRepairPost**](docs/ContoContabileApi.md#apivversioncompanyidcontocontabilerepairpost) | **POST** /api/v{version}/{companyId}/ContoContabile/repair | Valida le registrazioni contabili, cercando di correggere i problemi più comuni.
*DistintaBaseApi* | [**ApiVversionCompanyIdDistintaBaseGet**](docs/DistintaBaseApi.md#apivversioncompanyiddistintabaseget) | **GET** /api/v{version}/{companyId}/DistintaBase | Restituisce una lista di elementi.
*DistintaBaseApi* | [**ApiVversionCompanyIdDistintaBaseIdDelete**](docs/DistintaBaseApi.md#apivversioncompanyiddistintabaseiddelete) | **DELETE** /api/v{version}/{companyId}/DistintaBase/{id} | Elimina un elemento.
*DistintaBaseApi* | [**ApiVversionCompanyIdDistintaBaseIdGet**](docs/DistintaBaseApi.md#apivversioncompanyiddistintabaseidget) | **GET** /api/v{version}/{companyId}/DistintaBase/{id} | Restituisce un elemento.
*DistintaBaseApi* | [**ApiVversionCompanyIdDistintaBaseIdPut**](docs/DistintaBaseApi.md#apivversioncompanyiddistintabaseidput) | **PUT** /api/v{version}/{companyId}/DistintaBase/{id} | Aggiorna un elemento esistente.
*DistintaBaseApi* | [**ApiVversionCompanyIdDistintaBaseNewGet**](docs/DistintaBaseApi.md#apivversioncompanyiddistintabasenewget) | **GET** /api/v{version}/{companyId}/DistintaBase/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*DistintaBaseApi* | [**ApiVversionCompanyIdDistintaBasePost**](docs/DistintaBaseApi.md#apivversioncompanyiddistintabasepost) | **POST** /api/v{version}/{companyId}/DistintaBase | Salva un nuovo elemento.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoComputePost**](docs/DocumentoApi.md#apivversioncompanyiddocumentocomputepost) | **POST** /api/v{version}/{companyId}/Documento/compute | Calcola i totali di un documento.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoDistinctGet**](docs/DocumentoApi.md#apivversioncompanyiddocumentodistinctget) | **GET** /api/v{version}/{companyId}/Documento/distinct | Restituisce una lista di elementi distinti.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoDocumentoIdEmitPost**](docs/DocumentoApi.md#apivversioncompanyiddocumentodocumentoidemitpost) | **POST** /api/v{version}/{companyId}/Documento/{documentoId}/emit | Emette un documento.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoDocumentoIdNewinstancefromtemplatePost**](docs/DocumentoApi.md#apivversioncompanyiddocumentodocumentoidnewinstancefromtemplatepost) | **POST** /api/v{version}/{companyId}/Documento/{documentoId}/newinstancefromtemplate | Replica un documento. Crea una copia con la data odierna e, se è un documento di vendita, un nuovo numero. Alcuni campi vengono reimpostati ai valori predefiniti (stato del documento, stato SDI, stato del pagamento, ecc.).
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoGet**](docs/DocumentoApi.md#apivversioncompanyiddocumentoget) | **GET** /api/v{version}/{companyId}/Documento | Restituisce una lista di elementi.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoIdDelete**](docs/DocumentoApi.md#apivversioncompanyiddocumentoiddelete) | **DELETE** /api/v{version}/{companyId}/Documento/{id} | Elimina un elemento.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoIdGet**](docs/DocumentoApi.md#apivversioncompanyiddocumentoidget) | **GET** /api/v{version}/{companyId}/Documento/{id} | Restituisce un elemento.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoIdImportAccontoPost**](docs/DocumentoApi.md#apivversioncompanyiddocumentoidimportaccontopost) | **POST** /api/v{version}/{companyId}/Documento/{id}/import/acconto | Importa righe da uno o più acconti in un documento esistente.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoIdImportDdtPost**](docs/DocumentoApi.md#apivversioncompanyiddocumentoidimportddtpost) | **POST** /api/v{version}/{companyId}/Documento/{id}/import/ddt | Importa righe da uno o più DDT in un documento esistente.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoIdImportOrdinePost**](docs/DocumentoApi.md#apivversioncompanyiddocumentoidimportordinepost) | **POST** /api/v{version}/{companyId}/Documento/{id}/import/ordine | Importa righe da uno o più ordini in un documento esistente.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoIdImportPreventivoPost**](docs/DocumentoApi.md#apivversioncompanyiddocumentoidimportpreventivopost) | **POST** /api/v{version}/{companyId}/Documento/{id}/import/preventivo | Importa righe da uno o più preventivi in un documento esistente.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoIdImportProformaPost**](docs/DocumentoApi.md#apivversioncompanyiddocumentoidimportproformapost) | **POST** /api/v{version}/{companyId}/Documento/{id}/import/proforma | Importa righe da una o più fatture proforma in un documento esistente.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoIdImportResoPost**](docs/DocumentoApi.md#apivversioncompanyiddocumentoidimportresopost) | **POST** /api/v{version}/{companyId}/Documento/{id}/import/reso | Importa righe da uno o più resi in un documento esistente.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoIdPut**](docs/DocumentoApi.md#apivversioncompanyiddocumentoidput) | **PUT** /api/v{version}/{companyId}/Documento/{id} | Aggiorna un elemento esistente.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoNewGet**](docs/DocumentoApi.md#apivversioncompanyiddocumentonewget) | **GET** /api/v{version}/{companyId}/Documento/new | Restituisce una nuova istanza di un documento del tipo specificato.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoPeriodicinvoicingPost**](docs/DocumentoApi.md#apivversioncompanyiddocumentoperiodicinvoicingpost) | **POST** /api/v{version}/{companyId}/Documento/periodicinvoicing | Genera fatture per tutti i contatti che corrispondono ai criteri di fatturazione periodica.
*DocumentoApi* | [**ApiVversionCompanyIdDocumentoPost**](docs/DocumentoApi.md#apivversioncompanyiddocumentopost) | **POST** /api/v{version}/{companyId}/Documento | Salva un nuovo elemento.
*DocumentoMediaFileApi* | [**ApiVversionCompanyIdDocumentoMediaFileBulkDelete**](docs/DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafilebulkdelete) | **DELETE** /api/v{version}/{companyId}/DocumentoMediaFile/bulk | Elimina in blocco tutti i file multimediali dei documenti.
*DocumentoMediaFileApi* | [**ApiVversionCompanyIdDocumentoMediaFileBulkGet**](docs/DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafilebulkget) | **GET** /api/v{version}/{companyId}/DocumentoMediaFile/bulk | Scarica tutti i file multimediali dei documenti come archivio ZIP.
*DocumentoMediaFileApi* | [**ApiVversionCompanyIdDocumentoMediaFileGet**](docs/DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafileget) | **GET** /api/v{version}/{companyId}/DocumentoMediaFile | Restituisce una lista di elementi.
*DocumentoMediaFileApi* | [**ApiVversionCompanyIdDocumentoMediaFileIdDelete**](docs/DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafileiddelete) | **DELETE** /api/v{version}/{companyId}/DocumentoMediaFile/{id} | Elimina un elemento.
*DocumentoMediaFileApi* | [**ApiVversionCompanyIdDocumentoMediaFileIdGet**](docs/DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafileidget) | **GET** /api/v{version}/{companyId}/DocumentoMediaFile/{id} | Restituisce un elemento.
*DocumentoMediaFileApi* | [**ApiVversionCompanyIdDocumentoMediaFileIdPut**](docs/DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafileidput) | **PUT** /api/v{version}/{companyId}/DocumentoMediaFile/{id} | Aggiorna un elemento esistente.
*DocumentoMediaFileApi* | [**ApiVversionCompanyIdDocumentoMediaFileNewGet**](docs/DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafilenewget) | **GET** /api/v{version}/{companyId}/DocumentoMediaFile/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*DocumentoMediaFileApi* | [**ApiVversionCompanyIdDocumentoMediaFilePost**](docs/DocumentoMediaFileApi.md#apivversioncompanyiddocumentomediafilepost) | **POST** /api/v{version}/{companyId}/DocumentoMediaFile | Salva un nuovo elemento.
*EventoApi* | [**ApiVversionEventoGet**](docs/EventoApi.md#apivversioneventoget) | **GET** /api/v{version}/Evento | 
*EventoApi* | [**ApiVversionEventoIdDelete**](docs/EventoApi.md#apivversioneventoiddelete) | **DELETE** /api/v{version}/Evento/{id} | Elimina un elemento.
*EventoApi* | [**ApiVversionEventoIdGet**](docs/EventoApi.md#apivversioneventoidget) | **GET** /api/v{version}/Evento/{id} | 
*EventoApi* | [**ApiVversionEventoIdPut**](docs/EventoApi.md#apivversioneventoidput) | **PUT** /api/v{version}/Evento/{id} | Aggiorna un elemento esistente.
*EventoApi* | [**ApiVversionEventoNewGet**](docs/EventoApi.md#apivversioneventonewget) | **GET** /api/v{version}/Evento/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*EventoApi* | [**ApiVversionEventoPost**](docs/EventoApi.md#apivversioneventopost) | **POST** /api/v{version}/Evento | Salva un nuovo elemento.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlarchivebydaterangepost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/archivebydaterange | Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlarchivebyfilenamepost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/archivebyfilename | Scarica un archivio ZIP di fatture XML, filtrate per nome file.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlbulkdelete) | **DELETE** /api/v{version}/{companyId}/FatturaElettronicaXml/bulk | Elimina in blocco tutte le fatture XML dell'azienda.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlBulkGet**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlbulkget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml/bulk | Scarica tutte le fatture XML come archivio ZIP.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlBulkPost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlbulkpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/bulk | Carica in blocco fatture XML da un archivio ZIP.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlExportPost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlexportpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/export | Esporta uno o più documenti in una fattura XML.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlfilenameattachmentsget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml/{fileName}/attachments | Scarica i file allegati di una fattura XML.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlfilenameget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml/{fileName} | Recupera una fattura XML per nome file.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlGet**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml | Restituisce una lista di elementi.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlIdDelete**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmliddelete) | **DELETE** /api/v{version}/{companyId}/FatturaElettronicaXml/{id} | Elimina un elemento.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlIdGet**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlidget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml/{id} | Restituisce un elemento.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlIdPut**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlidput) | **PUT** /api/v{version}/{companyId}/FatturaElettronicaXml/{id} | Aggiorna un elemento esistente.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlImportPost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlimportpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/import | Importa una fattura XML.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlNewGet**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlnewget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXml/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlPairPost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlpairpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/pair | Associa una fattura XML a un documento esistente.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlPost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml | Salva un nuovo elemento.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlsdidownloadesitipost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/sdi/download/esiti | Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate.
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlsdidownloadfatturepost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/sdi/download/fatture | Scarica le nuove fatture dal Sistema di Interscambio (SDI).
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlsdiuploadpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/sdi/upload | Invia una o più fatture al Sistema di Interscambio (SDI).
*FatturaElettronicaXmlApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost**](docs/FatturaElettronicaXmlApi.md#apivversioncompanyidfatturaelettronicaxmlstylesheetpost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXml/stylesheet | Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato.
*FatturaElettronicaXmlTemplateApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlTemplateGet**](docs/FatturaElettronicaXmlTemplateApi.md#apivversioncompanyidfatturaelettronicaxmltemplateget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXmlTemplate | Restituisce una lista di elementi.
*FatturaElettronicaXmlTemplateApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlTemplateIdDelete**](docs/FatturaElettronicaXmlTemplateApi.md#apivversioncompanyidfatturaelettronicaxmltemplateiddelete) | **DELETE** /api/v{version}/{companyId}/FatturaElettronicaXmlTemplate/{id} | Elimina un elemento.
*FatturaElettronicaXmlTemplateApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlTemplateIdGet**](docs/FatturaElettronicaXmlTemplateApi.md#apivversioncompanyidfatturaelettronicaxmltemplateidget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXmlTemplate/{id} | Restituisce un elemento.
*FatturaElettronicaXmlTemplateApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlTemplateIdPut**](docs/FatturaElettronicaXmlTemplateApi.md#apivversioncompanyidfatturaelettronicaxmltemplateidput) | **PUT** /api/v{version}/{companyId}/FatturaElettronicaXmlTemplate/{id} | Aggiorna un elemento esistente.
*FatturaElettronicaXmlTemplateApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlTemplateNewGet**](docs/FatturaElettronicaXmlTemplateApi.md#apivversioncompanyidfatturaelettronicaxmltemplatenewget) | **GET** /api/v{version}/{companyId}/FatturaElettronicaXmlTemplate/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*FatturaElettronicaXmlTemplateApi* | [**ApiVversionCompanyIdFatturaElettronicaXmlTemplatePost**](docs/FatturaElettronicaXmlTemplateApi.md#apivversioncompanyidfatturaelettronicaxmltemplatepost) | **POST** /api/v{version}/{companyId}/FatturaElettronicaXmlTemplate | Salva un nuovo elemento.
*GiacenzaApi* | [**ApiVversionCompanyIdGiacenzaDistinctGet**](docs/GiacenzaApi.md#apivversioncompanyidgiacenzadistinctget) | **GET** /api/v{version}/{companyId}/Giacenza/distinct | Restituisce una lista di elementi distinti.
*GiacenzaApi* | [**ApiVversionCompanyIdGiacenzaGet**](docs/GiacenzaApi.md#apivversioncompanyidgiacenzaget) | **GET** /api/v{version}/{companyId}/Giacenza | Restituisce una lista di elementi.
*GiacenzaApi* | [**ApiVversionCompanyIdGiacenzaIdDelete**](docs/GiacenzaApi.md#apivversioncompanyidgiacenzaiddelete) | **DELETE** /api/v{version}/{companyId}/Giacenza/{id} | Elimina un elemento.
*GiacenzaApi* | [**ApiVversionCompanyIdGiacenzaIdGet**](docs/GiacenzaApi.md#apivversioncompanyidgiacenzaidget) | **GET** /api/v{version}/{companyId}/Giacenza/{id} | Restituisce un elemento.
*GiacenzaApi* | [**ApiVversionCompanyIdGiacenzaIdPut**](docs/GiacenzaApi.md#apivversioncompanyidgiacenzaidput) | **PUT** /api/v{version}/{companyId}/Giacenza/{id} | Aggiorna un elemento esistente.
*GiacenzaApi* | [**ApiVversionCompanyIdGiacenzaNewGet**](docs/GiacenzaApi.md#apivversioncompanyidgiacenzanewget) | **GET** /api/v{version}/{companyId}/Giacenza/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*GiacenzaApi* | [**ApiVversionCompanyIdGiacenzaPost**](docs/GiacenzaApi.md#apivversioncompanyidgiacenzapost) | **POST** /api/v{version}/{companyId}/Giacenza | Salva un nuovo elemento.
*GiornaleMagazzinoApi* | [**ApiVversionCompanyIdGiornaleMagazzinoGet**](docs/GiornaleMagazzinoApi.md#apivversioncompanyidgiornalemagazzinoget) | **GET** /api/v{version}/{companyId}/GiornaleMagazzino | Restituisce una lista di elementi.
*GiornaleMagazzinoApi* | [**ApiVversionCompanyIdGiornaleMagazzinoIdDelete**](docs/GiornaleMagazzinoApi.md#apivversioncompanyidgiornalemagazzinoiddelete) | **DELETE** /api/v{version}/{companyId}/GiornaleMagazzino/{id} | Elimina un elemento.
*GiornaleMagazzinoApi* | [**ApiVversionCompanyIdGiornaleMagazzinoIdGet**](docs/GiornaleMagazzinoApi.md#apivversioncompanyidgiornalemagazzinoidget) | **GET** /api/v{version}/{companyId}/GiornaleMagazzino/{id} | Restituisce un elemento.
*GiornaleMagazzinoApi* | [**ApiVversionCompanyIdGiornaleMagazzinoIdPut**](docs/GiornaleMagazzinoApi.md#apivversioncompanyidgiornalemagazzinoidput) | **PUT** /api/v{version}/{companyId}/GiornaleMagazzino/{id} | Aggiorna un elemento esistente.
*GiornaleMagazzinoApi* | [**ApiVversionCompanyIdGiornaleMagazzinoNewGet**](docs/GiornaleMagazzinoApi.md#apivversioncompanyidgiornalemagazzinonewget) | **GET** /api/v{version}/{companyId}/GiornaleMagazzino/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*GiornaleMagazzinoApi* | [**ApiVversionCompanyIdGiornaleMagazzinoPost**](docs/GiornaleMagazzinoApi.md#apivversioncompanyidgiornalemagazzinopost) | **POST** /api/v{version}/{companyId}/GiornaleMagazzino | Salva un nuovo elemento.
*GruppoMerceologicoApi* | [**ApiVversionCompanyIdGruppoMerceologicoGet**](docs/GruppoMerceologicoApi.md#apivversioncompanyidgruppomerceologicoget) | **GET** /api/v{version}/{companyId}/GruppoMerceologico | Restituisce una lista di elementi.
*GruppoMerceologicoApi* | [**ApiVversionCompanyIdGruppoMerceologicoIdDelete**](docs/GruppoMerceologicoApi.md#apivversioncompanyidgruppomerceologicoiddelete) | **DELETE** /api/v{version}/{companyId}/GruppoMerceologico/{id} | Elimina un elemento.
*GruppoMerceologicoApi* | [**ApiVversionCompanyIdGruppoMerceologicoIdGet**](docs/GruppoMerceologicoApi.md#apivversioncompanyidgruppomerceologicoidget) | **GET** /api/v{version}/{companyId}/GruppoMerceologico/{id} | Restituisce un elemento.
*GruppoMerceologicoApi* | [**ApiVversionCompanyIdGruppoMerceologicoIdPut**](docs/GruppoMerceologicoApi.md#apivversioncompanyidgruppomerceologicoidput) | **PUT** /api/v{version}/{companyId}/GruppoMerceologico/{id} | Aggiorna un elemento esistente.
*GruppoMerceologicoApi* | [**ApiVversionCompanyIdGruppoMerceologicoNewGet**](docs/GruppoMerceologicoApi.md#apivversioncompanyidgruppomerceologiconewget) | **GET** /api/v{version}/{companyId}/GruppoMerceologico/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*GruppoMerceologicoApi* | [**ApiVversionCompanyIdGruppoMerceologicoPost**](docs/GruppoMerceologicoApi.md#apivversioncompanyidgruppomerceologicopost) | **POST** /api/v{version}/{companyId}/GruppoMerceologico | Salva un nuovo elemento.
*IndirizzoApi* | [**ApiVversionCompanyIdIndirizzoGet**](docs/IndirizzoApi.md#apivversioncompanyidindirizzoget) | **GET** /api/v{version}/{companyId}/Indirizzo | Restituisce una lista di elementi.
*IndirizzoApi* | [**ApiVversionCompanyIdIndirizzoIdDelete**](docs/IndirizzoApi.md#apivversioncompanyidindirizzoiddelete) | **DELETE** /api/v{version}/{companyId}/Indirizzo/{id} | Elimina un elemento.
*IndirizzoApi* | [**ApiVversionCompanyIdIndirizzoIdGet**](docs/IndirizzoApi.md#apivversioncompanyidindirizzoidget) | **GET** /api/v{version}/{companyId}/Indirizzo/{id} | Restituisce un elemento.
*IndirizzoApi* | [**ApiVversionCompanyIdIndirizzoIdPut**](docs/IndirizzoApi.md#apivversioncompanyidindirizzoidput) | **PUT** /api/v{version}/{companyId}/Indirizzo/{id} | Aggiorna un elemento esistente.
*IndirizzoApi* | [**ApiVversionCompanyIdIndirizzoNewGet**](docs/IndirizzoApi.md#apivversioncompanyidindirizzonewget) | **GET** /api/v{version}/{companyId}/Indirizzo/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*IndirizzoApi* | [**ApiVversionCompanyIdIndirizzoPost**](docs/IndirizzoApi.md#apivversioncompanyidindirizzopost) | **POST** /api/v{version}/{companyId}/Indirizzo | Salva un nuovo elemento.
*ListinoApi* | [**ApiVversionCompanyIdListinoGet**](docs/ListinoApi.md#apivversioncompanyidlistinoget) | **GET** /api/v{version}/{companyId}/Listino | Restituisce una lista di elementi.
*ListinoApi* | [**ApiVversionCompanyIdListinoIdDelete**](docs/ListinoApi.md#apivversioncompanyidlistinoiddelete) | **DELETE** /api/v{version}/{companyId}/Listino/{id} | Elimina un elemento.
*ListinoApi* | [**ApiVversionCompanyIdListinoIdGet**](docs/ListinoApi.md#apivversioncompanyidlistinoidget) | **GET** /api/v{version}/{companyId}/Listino/{id} | Restituisce un elemento.
*ListinoApi* | [**ApiVversionCompanyIdListinoIdPut**](docs/ListinoApi.md#apivversioncompanyidlistinoidput) | **PUT** /api/v{version}/{companyId}/Listino/{id} | Aggiorna un elemento esistente.
*ListinoApi* | [**ApiVversionCompanyIdListinoNewGet**](docs/ListinoApi.md#apivversioncompanyidlistinonewget) | **GET** /api/v{version}/{companyId}/Listino/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*ListinoApi* | [**ApiVversionCompanyIdListinoPost**](docs/ListinoApi.md#apivversioncompanyidlistinopost) | **POST** /api/v{version}/{companyId}/Listino | Salva un nuovo elemento.
*MagazzinoApi* | [**ApiVversionCompanyIdMagazzinoGet**](docs/MagazzinoApi.md#apivversioncompanyidmagazzinoget) | **GET** /api/v{version}/{companyId}/Magazzino | Restituisce una lista di elementi.
*MagazzinoApi* | [**ApiVversionCompanyIdMagazzinoIdDelete**](docs/MagazzinoApi.md#apivversioncompanyidmagazzinoiddelete) | **DELETE** /api/v{version}/{companyId}/Magazzino/{id} | Elimina un elemento.
*MagazzinoApi* | [**ApiVversionCompanyIdMagazzinoIdGet**](docs/MagazzinoApi.md#apivversioncompanyidmagazzinoidget) | **GET** /api/v{version}/{companyId}/Magazzino/{id} | Restituisce un elemento.
*MagazzinoApi* | [**ApiVversionCompanyIdMagazzinoIdPut**](docs/MagazzinoApi.md#apivversioncompanyidmagazzinoidput) | **PUT** /api/v{version}/{companyId}/Magazzino/{id} | Aggiorna un elemento esistente.
*MagazzinoApi* | [**ApiVversionCompanyIdMagazzinoNewGet**](docs/MagazzinoApi.md#apivversioncompanyidmagazzinonewget) | **GET** /api/v{version}/{companyId}/Magazzino/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*MagazzinoApi* | [**ApiVversionCompanyIdMagazzinoPost**](docs/MagazzinoApi.md#apivversioncompanyidmagazzinopost) | **POST** /api/v{version}/{companyId}/Magazzino | Salva un nuovo elemento.
*MarcaApi* | [**ApiVversionCompanyIdMarcaGet**](docs/MarcaApi.md#apivversioncompanyidmarcaget) | **GET** /api/v{version}/{companyId}/Marca | Restituisce una lista di elementi.
*MarcaApi* | [**ApiVversionCompanyIdMarcaIdDelete**](docs/MarcaApi.md#apivversioncompanyidmarcaiddelete) | **DELETE** /api/v{version}/{companyId}/Marca/{id} | Elimina un elemento.
*MarcaApi* | [**ApiVversionCompanyIdMarcaIdGet**](docs/MarcaApi.md#apivversioncompanyidmarcaidget) | **GET** /api/v{version}/{companyId}/Marca/{id} | Restituisce un elemento.
*MarcaApi* | [**ApiVversionCompanyIdMarcaIdPut**](docs/MarcaApi.md#apivversioncompanyidmarcaidput) | **PUT** /api/v{version}/{companyId}/Marca/{id} | Aggiorna un elemento esistente.
*MarcaApi* | [**ApiVversionCompanyIdMarcaNewGet**](docs/MarcaApi.md#apivversioncompanyidmarcanewget) | **GET** /api/v{version}/{companyId}/Marca/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*MarcaApi* | [**ApiVversionCompanyIdMarcaPost**](docs/MarcaApi.md#apivversioncompanyidmarcapost) | **POST** /api/v{version}/{companyId}/Marca | Salva un nuovo elemento.
*ModalitaPagamentoApi* | [**ApiVversionCompanyIdModalitaPagamentoDistinctGet**](docs/ModalitaPagamentoApi.md#apivversioncompanyidmodalitapagamentodistinctget) | **GET** /api/v{version}/{companyId}/ModalitaPagamento/distinct | Restituisce una lista di elementi distinti.
*ModalitaPagamentoApi* | [**ApiVversionCompanyIdModalitaPagamentoGet**](docs/ModalitaPagamentoApi.md#apivversioncompanyidmodalitapagamentoget) | **GET** /api/v{version}/{companyId}/ModalitaPagamento | Restituisce una lista di elementi.
*ModalitaPagamentoApi* | [**ApiVversionCompanyIdModalitaPagamentoIdDelete**](docs/ModalitaPagamentoApi.md#apivversioncompanyidmodalitapagamentoiddelete) | **DELETE** /api/v{version}/{companyId}/ModalitaPagamento/{id} | Elimina un elemento.
*ModalitaPagamentoApi* | [**ApiVversionCompanyIdModalitaPagamentoIdGet**](docs/ModalitaPagamentoApi.md#apivversioncompanyidmodalitapagamentoidget) | **GET** /api/v{version}/{companyId}/ModalitaPagamento/{id} | Restituisce un elemento.
*ModalitaPagamentoApi* | [**ApiVversionCompanyIdModalitaPagamentoIdPut**](docs/ModalitaPagamentoApi.md#apivversioncompanyidmodalitapagamentoidput) | **PUT** /api/v{version}/{companyId}/ModalitaPagamento/{id} | Aggiorna un elemento esistente.
*ModalitaPagamentoApi* | [**ApiVversionCompanyIdModalitaPagamentoNewGet**](docs/ModalitaPagamentoApi.md#apivversioncompanyidmodalitapagamentonewget) | **GET** /api/v{version}/{companyId}/ModalitaPagamento/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*ModalitaPagamentoApi* | [**ApiVversionCompanyIdModalitaPagamentoPost**](docs/ModalitaPagamentoApi.md#apivversioncompanyidmodalitapagamentopost) | **POST** /api/v{version}/{companyId}/ModalitaPagamento | Salva un nuovo elemento.
*ModelloContabileApi* | [**ApiVversionCompanyIdModelloContabileGet**](docs/ModelloContabileApi.md#apivversioncompanyidmodellocontabileget) | **GET** /api/v{version}/{companyId}/ModelloContabile | Restituisce una lista di elementi.
*ModelloContabileApi* | [**ApiVversionCompanyIdModelloContabileIdDelete**](docs/ModelloContabileApi.md#apivversioncompanyidmodellocontabileiddelete) | **DELETE** /api/v{version}/{companyId}/ModelloContabile/{id} | Elimina un elemento.
*ModelloContabileApi* | [**ApiVversionCompanyIdModelloContabileIdGet**](docs/ModelloContabileApi.md#apivversioncompanyidmodellocontabileidget) | **GET** /api/v{version}/{companyId}/ModelloContabile/{id} | Restituisce un elemento.
*ModelloContabileApi* | [**ApiVversionCompanyIdModelloContabileIdPut**](docs/ModelloContabileApi.md#apivversioncompanyidmodellocontabileidput) | **PUT** /api/v{version}/{companyId}/ModelloContabile/{id} | Aggiorna un elemento esistente.
*ModelloContabileApi* | [**ApiVversionCompanyIdModelloContabileNewGet**](docs/ModelloContabileApi.md#apivversioncompanyidmodellocontabilenewget) | **GET** /api/v{version}/{companyId}/ModelloContabile/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*ModelloContabileApi* | [**ApiVversionCompanyIdModelloContabilePost**](docs/ModelloContabileApi.md#apivversioncompanyidmodellocontabilepost) | **POST** /api/v{version}/{companyId}/ModelloContabile | Salva un nuovo elemento.
*MovimentoMerceApi* | [**ApiVversionCompanyIdMovimentoMerceGet**](docs/MovimentoMerceApi.md#apivversioncompanyidmovimentomerceget) | **GET** /api/v{version}/{companyId}/MovimentoMerce | Restituisce una lista di elementi.
*MovimentoMerceApi* | [**ApiVversionCompanyIdMovimentoMerceIdDelete**](docs/MovimentoMerceApi.md#apivversioncompanyidmovimentomerceiddelete) | **DELETE** /api/v{version}/{companyId}/MovimentoMerce/{id} | Elimina un elemento.
*MovimentoMerceApi* | [**ApiVversionCompanyIdMovimentoMerceIdGet**](docs/MovimentoMerceApi.md#apivversioncompanyidmovimentomerceidget) | **GET** /api/v{version}/{companyId}/MovimentoMerce/{id} | Restituisce un elemento.
*MovimentoMerceApi* | [**ApiVversionCompanyIdMovimentoMerceIdPut**](docs/MovimentoMerceApi.md#apivversioncompanyidmovimentomerceidput) | **PUT** /api/v{version}/{companyId}/MovimentoMerce/{id} | Aggiorna un elemento esistente.
*MovimentoMerceApi* | [**ApiVversionCompanyIdMovimentoMerceNewGet**](docs/MovimentoMerceApi.md#apivversioncompanyidmovimentomercenewget) | **GET** /api/v{version}/{companyId}/MovimentoMerce/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*MovimentoMerceApi* | [**ApiVversionCompanyIdMovimentoMercePost**](docs/MovimentoMerceApi.md#apivversioncompanyidmovimentomercepost) | **POST** /api/v{version}/{companyId}/MovimentoMerce | Salva un nuovo elemento.
*NaturaIvaApi* | [**ApiVversionCompanyIdTabelleNaturaIvaGet**](docs/NaturaIvaApi.md#apivversioncompanyidtabellenaturaivaget) | **GET** /api/v{version}/{companyId}/tabelle/NaturaIva | Restituisce il contenuto della tabella come lista di elementi.
*NazioneApi* | [**ApiVversionCompanyIdNazioneDistinctGet**](docs/NazioneApi.md#apivversioncompanyidnazionedistinctget) | **GET** /api/v{version}/{companyId}/Nazione/distinct | Restituisce una lista di elementi distinti.
*NazioneApi* | [**ApiVversionCompanyIdNazioneGet**](docs/NazioneApi.md#apivversioncompanyidnazioneget) | **GET** /api/v{version}/{companyId}/Nazione | Restituisce una lista di elementi.
*NazioneApi* | [**ApiVversionCompanyIdNazioneIdDelete**](docs/NazioneApi.md#apivversioncompanyidnazioneiddelete) | **DELETE** /api/v{version}/{companyId}/Nazione/{id} | Elimina un elemento.
*NazioneApi* | [**ApiVversionCompanyIdNazioneIdGet**](docs/NazioneApi.md#apivversioncompanyidnazioneidget) | **GET** /api/v{version}/{companyId}/Nazione/{id} | Restituisce un elemento.
*NazioneApi* | [**ApiVversionCompanyIdNazioneIdPut**](docs/NazioneApi.md#apivversioncompanyidnazioneidput) | **PUT** /api/v{version}/{companyId}/Nazione/{id} | Aggiorna un elemento esistente.
*NazioneApi* | [**ApiVversionCompanyIdNazioneNewGet**](docs/NazioneApi.md#apivversioncompanyidnazionenewget) | **GET** /api/v{version}/{companyId}/Nazione/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*NazioneApi* | [**ApiVversionCompanyIdNazionePost**](docs/NazioneApi.md#apivversioncompanyidnazionepost) | **POST** /api/v{version}/{companyId}/Nazione | Salva un nuovo elemento.
*PagamentoApi* | [**ApiVversionCompanyIdPagamentoDistinctGet**](docs/PagamentoApi.md#apivversioncompanyidpagamentodistinctget) | **GET** /api/v{version}/{companyId}/Pagamento/distinct | Restituisce una lista di elementi distinti.
*PagamentoApi* | [**ApiVversionCompanyIdPagamentoGet**](docs/PagamentoApi.md#apivversioncompanyidpagamentoget) | **GET** /api/v{version}/{companyId}/Pagamento | Restituisce una lista di elementi.
*PagamentoApi* | [**ApiVversionCompanyIdPagamentoIdDelete**](docs/PagamentoApi.md#apivversioncompanyidpagamentoiddelete) | **DELETE** /api/v{version}/{companyId}/Pagamento/{id} | Elimina un elemento.
*PagamentoApi* | [**ApiVversionCompanyIdPagamentoIdGet**](docs/PagamentoApi.md#apivversioncompanyidpagamentoidget) | **GET** /api/v{version}/{companyId}/Pagamento/{id} | Restituisce un elemento.
*PagamentoApi* | [**ApiVversionCompanyIdPagamentoIdPut**](docs/PagamentoApi.md#apivversioncompanyidpagamentoidput) | **PUT** /api/v{version}/{companyId}/Pagamento/{id} | Aggiorna un elemento esistente.
*PagamentoApi* | [**ApiVversionCompanyIdPagamentoNewGet**](docs/PagamentoApi.md#apivversioncompanyidpagamentonewget) | **GET** /api/v{version}/{companyId}/Pagamento/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*PagamentoApi* | [**ApiVversionCompanyIdPagamentoPost**](docs/PagamentoApi.md#apivversioncompanyidpagamentopost) | **POST** /api/v{version}/{companyId}/Pagamento | Salva un nuovo elemento.
*PreferenzeApi* | [**ApiVversionCompanyIdPreferenzeGet**](docs/PreferenzeApi.md#apivversioncompanyidpreferenzeget) | **GET** /api/v{version}/{companyId}/Preferenze | Restituisce una lista di elementi.
*PreferenzeApi* | [**ApiVversionCompanyIdPreferenzeIdDelete**](docs/PreferenzeApi.md#apivversioncompanyidpreferenzeiddelete) | **DELETE** /api/v{version}/{companyId}/Preferenze/{id} | Elimina un elemento.
*PreferenzeApi* | [**ApiVversionCompanyIdPreferenzeIdGet**](docs/PreferenzeApi.md#apivversioncompanyidpreferenzeidget) | **GET** /api/v{version}/{companyId}/Preferenze/{id} | Restituisce un elemento.
*PreferenzeApi* | [**ApiVversionCompanyIdPreferenzeIdPut**](docs/PreferenzeApi.md#apivversioncompanyidpreferenzeidput) | **PUT** /api/v{version}/{companyId}/Preferenze/{id} | Aggiorna un elemento esistente.
*PreferenzeApi* | [**ApiVversionCompanyIdPreferenzeNewGet**](docs/PreferenzeApi.md#apivversioncompanyidpreferenzenewget) | **GET** /api/v{version}/{companyId}/Preferenze/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*PreferenzeApi* | [**ApiVversionCompanyIdPreferenzePost**](docs/PreferenzeApi.md#apivversioncompanyidpreferenzepost) | **POST** /api/v{version}/{companyId}/Preferenze | Salva un nuovo elemento.
*PrestazioneApi* | [**ApiVversionCompanyIdPrestazioneGet**](docs/PrestazioneApi.md#apivversioncompanyidprestazioneget) | **GET** /api/v{version}/{companyId}/Prestazione | Restituisce una lista di elementi.
*PrestazioneApi* | [**ApiVversionCompanyIdPrestazioneIdDelete**](docs/PrestazioneApi.md#apivversioncompanyidprestazioneiddelete) | **DELETE** /api/v{version}/{companyId}/Prestazione/{id} | Elimina un elemento.
*PrestazioneApi* | [**ApiVversionCompanyIdPrestazioneIdGet**](docs/PrestazioneApi.md#apivversioncompanyidprestazioneidget) | **GET** /api/v{version}/{companyId}/Prestazione/{id} | Restituisce un elemento.
*PrestazioneApi* | [**ApiVversionCompanyIdPrestazioneIdPut**](docs/PrestazioneApi.md#apivversioncompanyidprestazioneidput) | **PUT** /api/v{version}/{companyId}/Prestazione/{id} | Aggiorna un elemento esistente.
*PrestazioneApi* | [**ApiVversionCompanyIdPrestazioneNewGet**](docs/PrestazioneApi.md#apivversioncompanyidprestazionenewget) | **GET** /api/v{version}/{companyId}/Prestazione/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*PrestazioneApi* | [**ApiVversionCompanyIdPrestazionePost**](docs/PrestazioneApi.md#apivversioncompanyidprestazionepost) | **POST** /api/v{version}/{companyId}/Prestazione | Salva un nuovo elemento.
*PrezzoApi* | [**ApiVversionCompanyIdPrezzoGet**](docs/PrezzoApi.md#apivversioncompanyidprezzoget) | **GET** /api/v{version}/{companyId}/Prezzo | Restituisce una lista di elementi.
*PrezzoApi* | [**ApiVversionCompanyIdPrezzoIdDelete**](docs/PrezzoApi.md#apivversioncompanyidprezzoiddelete) | **DELETE** /api/v{version}/{companyId}/Prezzo/{id} | Elimina un elemento.
*PrezzoApi* | [**ApiVversionCompanyIdPrezzoIdGet**](docs/PrezzoApi.md#apivversioncompanyidprezzoidget) | **GET** /api/v{version}/{companyId}/Prezzo/{id} | Restituisce un elemento.
*PrezzoApi* | [**ApiVversionCompanyIdPrezzoIdPut**](docs/PrezzoApi.md#apivversioncompanyidprezzoidput) | **PUT** /api/v{version}/{companyId}/Prezzo/{id} | Aggiorna un elemento esistente.
*PrezzoApi* | [**ApiVversionCompanyIdPrezzoNewGet**](docs/PrezzoApi.md#apivversioncompanyidprezzonewget) | **GET** /api/v{version}/{companyId}/Prezzo/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*PrezzoApi* | [**ApiVversionCompanyIdPrezzoPost**](docs/PrezzoApi.md#apivversioncompanyidprezzopost) | **POST** /api/v{version}/{companyId}/Prezzo | Salva un nuovo elemento.
*ProduttoreApi* | [**ApiVversionCompanyIdProduttoreGet**](docs/ProduttoreApi.md#apivversioncompanyidproduttoreget) | **GET** /api/v{version}/{companyId}/Produttore | Restituisce una lista di elementi.
*ProduttoreApi* | [**ApiVversionCompanyIdProduttoreIdDelete**](docs/ProduttoreApi.md#apivversioncompanyidproduttoreiddelete) | **DELETE** /api/v{version}/{companyId}/Produttore/{id} | Elimina un elemento.
*ProduttoreApi* | [**ApiVversionCompanyIdProduttoreIdGet**](docs/ProduttoreApi.md#apivversioncompanyidproduttoreidget) | **GET** /api/v{version}/{companyId}/Produttore/{id} | Restituisce un elemento.
*ProduttoreApi* | [**ApiVversionCompanyIdProduttoreIdPut**](docs/ProduttoreApi.md#apivversioncompanyidproduttoreidput) | **PUT** /api/v{version}/{companyId}/Produttore/{id} | Aggiorna un elemento esistente.
*ProduttoreApi* | [**ApiVversionCompanyIdProduttoreNewGet**](docs/ProduttoreApi.md#apivversioncompanyidproduttorenewget) | **GET** /api/v{version}/{companyId}/Produttore/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*ProduttoreApi* | [**ApiVversionCompanyIdProduttorePost**](docs/ProduttoreApi.md#apivversioncompanyidproduttorepost) | **POST** /api/v{version}/{companyId}/Produttore | Salva un nuovo elemento.
*ProgressivoInvioApi* | [**ApiVversionCompanyIdProgressivoInvioGet**](docs/ProgressivoInvioApi.md#apivversioncompanyidprogressivoinvioget) | **GET** /api/v{version}/{companyId}/ProgressivoInvio | Restituisce una lista di elementi.
*ProgressivoInvioApi* | [**ApiVversionCompanyIdProgressivoInvioIdDelete**](docs/ProgressivoInvioApi.md#apivversioncompanyidprogressivoinvioiddelete) | **DELETE** /api/v{version}/{companyId}/ProgressivoInvio/{id} | Elimina un elemento.
*ProgressivoInvioApi* | [**ApiVversionCompanyIdProgressivoInvioIdGet**](docs/ProgressivoInvioApi.md#apivversioncompanyidprogressivoinvioidget) | **GET** /api/v{version}/{companyId}/ProgressivoInvio/{id} | Restituisce un elemento.
*ProgressivoInvioApi* | [**ApiVversionCompanyIdProgressivoInvioIdPut**](docs/ProgressivoInvioApi.md#apivversioncompanyidprogressivoinvioidput) | **PUT** /api/v{version}/{companyId}/ProgressivoInvio/{id} | Aggiorna un elemento esistente.
*ProgressivoInvioApi* | [**ApiVversionCompanyIdProgressivoInvioNewGet**](docs/ProgressivoInvioApi.md#apivversioncompanyidprogressivoinvionewget) | **GET** /api/v{version}/{companyId}/ProgressivoInvio/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*ProgressivoInvioApi* | [**ApiVversionCompanyIdProgressivoInvioPost**](docs/ProgressivoInvioApi.md#apivversioncompanyidprogressivoinviopost) | **POST** /api/v{version}/{companyId}/ProgressivoInvio | Salva un nuovo elemento.
*RegistrazioneContabileApi* | [**ApiVversionCompanyIdRegistrazioneContabileGet**](docs/RegistrazioneContabileApi.md#apivversioncompanyidregistrazionecontabileget) | **GET** /api/v{version}/{companyId}/RegistrazioneContabile | Restituisce una lista di elementi.
*RegistrazioneContabileApi* | [**ApiVversionCompanyIdRegistrazioneContabileIdDelete**](docs/RegistrazioneContabileApi.md#apivversioncompanyidregistrazionecontabileiddelete) | **DELETE** /api/v{version}/{companyId}/RegistrazioneContabile/{id} | Elimina un elemento.
*RegistrazioneContabileApi* | [**ApiVversionCompanyIdRegistrazioneContabileIdGet**](docs/RegistrazioneContabileApi.md#apivversioncompanyidregistrazionecontabileidget) | **GET** /api/v{version}/{companyId}/RegistrazioneContabile/{id} | Restituisce un elemento.
*RegistrazioneContabileApi* | [**ApiVversionCompanyIdRegistrazioneContabileIdPut**](docs/RegistrazioneContabileApi.md#apivversioncompanyidregistrazionecontabileidput) | **PUT** /api/v{version}/{companyId}/RegistrazioneContabile/{id} | Aggiorna un elemento esistente.
*RegistrazioneContabileApi* | [**ApiVversionCompanyIdRegistrazioneContabileNewGet**](docs/RegistrazioneContabileApi.md#apivversioncompanyidregistrazionecontabilenewget) | **GET** /api/v{version}/{companyId}/RegistrazioneContabile/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*RegistrazioneContabileApi* | [**ApiVversionCompanyIdRegistrazioneContabilePost**](docs/RegistrazioneContabileApi.md#apivversioncompanyidregistrazionecontabilepost) | **POST** /api/v{version}/{companyId}/RegistrazioneContabile | Salva un nuovo elemento.
*RegistrazioneContabileApi* | [**ApiVversionCompanyIdRegistrazioneContabileValidatePost**](docs/RegistrazioneContabileApi.md#apivversioncompanyidregistrazionecontabilevalidatepost) | **POST** /api/v{version}/{companyId}/RegistrazioneContabile/validate | Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili.
*RepartoApi* | [**ApiVversionCompanyIdRepartoGet**](docs/RepartoApi.md#apivversioncompanyidrepartoget) | **GET** /api/v{version}/{companyId}/Reparto | Restituisce una lista di elementi.
*RepartoApi* | [**ApiVversionCompanyIdRepartoIdDelete**](docs/RepartoApi.md#apivversioncompanyidrepartoiddelete) | **DELETE** /api/v{version}/{companyId}/Reparto/{id} | Elimina un elemento.
*RepartoApi* | [**ApiVversionCompanyIdRepartoIdGet**](docs/RepartoApi.md#apivversioncompanyidrepartoidget) | **GET** /api/v{version}/{companyId}/Reparto/{id} | Restituisce un elemento.
*RepartoApi* | [**ApiVversionCompanyIdRepartoIdPut**](docs/RepartoApi.md#apivversioncompanyidrepartoidput) | **PUT** /api/v{version}/{companyId}/Reparto/{id} | Aggiorna un elemento esistente.
*RepartoApi* | [**ApiVversionCompanyIdRepartoNewGet**](docs/RepartoApi.md#apivversioncompanyidrepartonewget) | **GET** /api/v{version}/{companyId}/Reparto/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*RepartoApi* | [**ApiVversionCompanyIdRepartoPost**](docs/RepartoApi.md#apivversioncompanyidrepartopost) | **POST** /api/v{version}/{companyId}/Reparto | Salva un nuovo elemento.
*RisorsaFinanziariaApi* | [**ApiVversionCompanyIdRisorsaFinanziariaGet**](docs/RisorsaFinanziariaApi.md#apivversioncompanyidrisorsafinanziariaget) | **GET** /api/v{version}/{companyId}/RisorsaFinanziaria | Restituisce una lista di elementi.
*RisorsaFinanziariaApi* | [**ApiVversionCompanyIdRisorsaFinanziariaIdDelete**](docs/RisorsaFinanziariaApi.md#apivversioncompanyidrisorsafinanziariaiddelete) | **DELETE** /api/v{version}/{companyId}/RisorsaFinanziaria/{id} | Elimina un elemento.
*RisorsaFinanziariaApi* | [**ApiVversionCompanyIdRisorsaFinanziariaIdGet**](docs/RisorsaFinanziariaApi.md#apivversioncompanyidrisorsafinanziariaidget) | **GET** /api/v{version}/{companyId}/RisorsaFinanziaria/{id} | Restituisce un elemento.
*RisorsaFinanziariaApi* | [**ApiVversionCompanyIdRisorsaFinanziariaIdPut**](docs/RisorsaFinanziariaApi.md#apivversioncompanyidrisorsafinanziariaidput) | **PUT** /api/v{version}/{companyId}/RisorsaFinanziaria/{id} | Aggiorna un elemento esistente.
*RisorsaFinanziariaApi* | [**ApiVversionCompanyIdRisorsaFinanziariaNewGet**](docs/RisorsaFinanziariaApi.md#apivversioncompanyidrisorsafinanziarianewget) | **GET** /api/v{version}/{companyId}/RisorsaFinanziaria/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*RisorsaFinanziariaApi* | [**ApiVversionCompanyIdRisorsaFinanziariaPost**](docs/RisorsaFinanziariaApi.md#apivversioncompanyidrisorsafinanziariapost) | **POST** /api/v{version}/{companyId}/RisorsaFinanziaria | Salva un nuovo elemento.
*ScadenzaApi* | [**ApiVversionCompanyIdScadenzaDistinctGet**](docs/ScadenzaApi.md#apivversioncompanyidscadenzadistinctget) | **GET** /api/v{version}/{companyId}/Scadenza/distinct | Restituisce una lista di elementi distinti.
*ScadenzaApi* | [**ApiVversionCompanyIdScadenzaGet**](docs/ScadenzaApi.md#apivversioncompanyidscadenzaget) | **GET** /api/v{version}/{companyId}/Scadenza | Restituisce una lista di elementi.
*ScadenzaApi* | [**ApiVversionCompanyIdScadenzaIdDelete**](docs/ScadenzaApi.md#apivversioncompanyidscadenzaiddelete) | **DELETE** /api/v{version}/{companyId}/Scadenza/{id} | Elimina un elemento.
*ScadenzaApi* | [**ApiVversionCompanyIdScadenzaIdGet**](docs/ScadenzaApi.md#apivversioncompanyidscadenzaidget) | **GET** /api/v{version}/{companyId}/Scadenza/{id} | Restituisce un elemento.
*ScadenzaApi* | [**ApiVversionCompanyIdScadenzaIdPut**](docs/ScadenzaApi.md#apivversioncompanyidscadenzaidput) | **PUT** /api/v{version}/{companyId}/Scadenza/{id} | Aggiorna un elemento esistente.
*ScadenzaApi* | [**ApiVversionCompanyIdScadenzaNewGet**](docs/ScadenzaApi.md#apivversioncompanyidscadenzanewget) | **GET** /api/v{version}/{companyId}/Scadenza/new | Restituisce una nuova istanza di una scadenza del tipo specificato.
*ScadenzaApi* | [**ApiVversionCompanyIdScadenzaPost**](docs/ScadenzaApi.md#apivversioncompanyidscadenzapost) | **POST** /api/v{version}/{companyId}/Scadenza | Salva un nuovo elemento.
*SituazioneApi* | [**ApiVversionCompanyIdSituazioneCreditodebitomensileGet**](docs/SituazioneApi.md#apivversioncompanyidsituazionecreditodebitomensileget) | **GET** /api/v{version}/{companyId}/Situazione/creditodebitomensile | Restituisce i saldi mensili di credito e debito dell'azienda.
*SituazioneApi* | [**ApiVversionCompanyIdSituazioneFatturatomensileGet**](docs/SituazioneApi.md#apivversioncompanyidsituazionefatturatomensileget) | **GET** /api/v{version}/{companyId}/Situazione/fatturatomensile | Restituisce il fatturato mensile dell'azienda.
*SpesaApi* | [**ApiVversionCompanyIdSpesaDistinctGet**](docs/SpesaApi.md#apivversioncompanyidspesadistinctget) | **GET** /api/v{version}/{companyId}/Spesa/distinct | Restituisce una lista di elementi distinti.
*SpesaApi* | [**ApiVversionCompanyIdSpesaGet**](docs/SpesaApi.md#apivversioncompanyidspesaget) | **GET** /api/v{version}/{companyId}/Spesa | Restituisce una lista di elementi.
*SpesaApi* | [**ApiVversionCompanyIdSpesaIdDelete**](docs/SpesaApi.md#apivversioncompanyidspesaiddelete) | **DELETE** /api/v{version}/{companyId}/Spesa/{id} | Elimina un elemento.
*SpesaApi* | [**ApiVversionCompanyIdSpesaIdGet**](docs/SpesaApi.md#apivversioncompanyidspesaidget) | **GET** /api/v{version}/{companyId}/Spesa/{id} | Restituisce un elemento.
*SpesaApi* | [**ApiVversionCompanyIdSpesaIdPut**](docs/SpesaApi.md#apivversioncompanyidspesaidput) | **PUT** /api/v{version}/{companyId}/Spesa/{id} | Aggiorna un elemento esistente.
*SpesaApi* | [**ApiVversionCompanyIdSpesaNewGet**](docs/SpesaApi.md#apivversioncompanyidspesanewget) | **GET** /api/v{version}/{companyId}/Spesa/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*SpesaApi* | [**ApiVversionCompanyIdSpesaPost**](docs/SpesaApi.md#apivversioncompanyidspesapost) | **POST** /api/v{version}/{companyId}/Spesa | Salva un nuovo elemento.
*SpesaDocumentoApi* | [**ApiVversionCompanyIdSpesaDocumentoGet**](docs/SpesaDocumentoApi.md#apivversioncompanyidspesadocumentoget) | **GET** /api/v{version}/{companyId}/SpesaDocumento | Restituisce una lista di elementi.
*SpesaDocumentoApi* | [**ApiVversionCompanyIdSpesaDocumentoIdDelete**](docs/SpesaDocumentoApi.md#apivversioncompanyidspesadocumentoiddelete) | **DELETE** /api/v{version}/{companyId}/SpesaDocumento/{id} | Elimina un elemento.
*SpesaDocumentoApi* | [**ApiVversionCompanyIdSpesaDocumentoIdGet**](docs/SpesaDocumentoApi.md#apivversioncompanyidspesadocumentoidget) | **GET** /api/v{version}/{companyId}/SpesaDocumento/{id} | Restituisce un elemento.
*SpesaDocumentoApi* | [**ApiVversionCompanyIdSpesaDocumentoIdPut**](docs/SpesaDocumentoApi.md#apivversioncompanyidspesadocumentoidput) | **PUT** /api/v{version}/{companyId}/SpesaDocumento/{id} | Aggiorna un elemento esistente.
*SpesaDocumentoApi* | [**ApiVversionCompanyIdSpesaDocumentoNewGet**](docs/SpesaDocumentoApi.md#apivversioncompanyidspesadocumentonewget) | **GET** /api/v{version}/{companyId}/SpesaDocumento/new | Restituisce una nuova istanza di una spesa documento.
*SpesaDocumentoApi* | [**ApiVversionCompanyIdSpesaDocumentoPost**](docs/SpesaDocumentoApi.md#apivversioncompanyidspesadocumentopost) | **POST** /api/v{version}/{companyId}/SpesaDocumento | Salva un nuovo elemento.
*TagliaApi* | [**ApiVversionCompanyIdTagliaGet**](docs/TagliaApi.md#apivversioncompanyidtagliaget) | **GET** /api/v{version}/{companyId}/Taglia | Restituisce una lista di elementi.
*TagliaApi* | [**ApiVversionCompanyIdTagliaIdDelete**](docs/TagliaApi.md#apivversioncompanyidtagliaiddelete) | **DELETE** /api/v{version}/{companyId}/Taglia/{id} | Elimina un elemento.
*TagliaApi* | [**ApiVversionCompanyIdTagliaIdGet**](docs/TagliaApi.md#apivversioncompanyidtagliaidget) | **GET** /api/v{version}/{companyId}/Taglia/{id} | Restituisce un elemento.
*TagliaApi* | [**ApiVversionCompanyIdTagliaIdPut**](docs/TagliaApi.md#apivversioncompanyidtagliaidput) | **PUT** /api/v{version}/{companyId}/Taglia/{id} | Aggiorna un elemento esistente.
*TagliaApi* | [**ApiVversionCompanyIdTagliaNewGet**](docs/TagliaApi.md#apivversioncompanyidtaglianewget) | **GET** /api/v{version}/{companyId}/Taglia/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*TagliaApi* | [**ApiVversionCompanyIdTagliaPost**](docs/TagliaApi.md#apivversioncompanyidtagliapost) | **POST** /api/v{version}/{companyId}/Taglia | Salva un nuovo elemento.
*ValutaApi* | [**ApiVversionCompanyIdValutaGet**](docs/ValutaApi.md#apivversioncompanyidvalutaget) | **GET** /api/v{version}/{companyId}/Valuta | Restituisce una lista di elementi.
*ValutaApi* | [**ApiVversionCompanyIdValutaIdDelete**](docs/ValutaApi.md#apivversioncompanyidvalutaiddelete) | **DELETE** /api/v{version}/{companyId}/Valuta/{id} | Elimina un elemento.
*ValutaApi* | [**ApiVversionCompanyIdValutaIdGet**](docs/ValutaApi.md#apivversioncompanyidvalutaidget) | **GET** /api/v{version}/{companyId}/Valuta/{id} | Restituisce un elemento.
*ValutaApi* | [**ApiVversionCompanyIdValutaIdPut**](docs/ValutaApi.md#apivversioncompanyidvalutaidput) | **PUT** /api/v{version}/{companyId}/Valuta/{id} | Aggiorna un elemento esistente.
*ValutaApi* | [**ApiVversionCompanyIdValutaNewGet**](docs/ValutaApi.md#apivversioncompanyidvalutanewget) | **GET** /api/v{version}/{companyId}/Valuta/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*ValutaApi* | [**ApiVversionCompanyIdValutaPost**](docs/ValutaApi.md#apivversioncompanyidvalutapost) | **POST** /api/v{version}/{companyId}/Valuta | Salva un nuovo elemento.
*WebHookApi* | [**ApiVversionCompanyIdWebHookGet**](docs/WebHookApi.md#apivversioncompanyidwebhookget) | **GET** /api/v{version}/{companyId}/WebHook | Restituisce una lista di Amica.WebHook.
*WebHookApi* | [**ApiVversionCompanyIdWebHookIdDelete**](docs/WebHookApi.md#apivversioncompanyidwebhookiddelete) | **DELETE** /api/v{version}/{companyId}/WebHook/{id} | 
*WebHookApi* | [**ApiVversionCompanyIdWebHookIdGet**](docs/WebHookApi.md#apivversioncompanyidwebhookidget) | **GET** /api/v{version}/{companyId}/WebHook/{id} | Restituisce un Amica.WebHook.
*WebHookApi* | [**ApiVversionCompanyIdWebHookIdPut**](docs/WebHookApi.md#apivversioncompanyidwebhookidput) | **PUT** /api/v{version}/{companyId}/WebHook/{id} | Aggiorna un Amica.WebHook.
*WebHookApi* | [**ApiVversionCompanyIdWebHookNewGet**](docs/WebHookApi.md#apivversioncompanyidwebhooknewget) | **GET** /api/v{version}/{companyId}/WebHook/new | Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell'applicazione (ad es., una nuova istanza di Amica.Documento avrà il Amica.Documento.Numero appropriato, basato sui documenti preesistenti di tipo compatibile).
*WebHookApi* | [**ApiVversionCompanyIdWebHookPost**](docs/WebHookApi.md#apivversioncompanyidwebhookpost) | **POST** /api/v{version}/{companyId}/WebHook | 


<a id="documentation-for-models"></a>
## Documentation for Models

 - [Model.AllegatoDocumento](docs/AllegatoDocumento.md)
 - [Model.Anagrafica](docs/Anagrafica.md)
 - [Model.AnagraficaNominativo1](docs/AnagraficaNominativo1.md)
 - [Model.AnagraficaNominativo2](docs/AnagraficaNominativo2.md)
 - [Model.AnagraficaSedeLegale](docs/AnagraficaSedeLegale.md)
 - [Model.AreaGeografica](docs/AreaGeografica.md)
 - [Model.Articolo](docs/Articolo.md)
 - [Model.ArticoloMediaFile](docs/ArticoloMediaFile.md)
 - [Model.Azienda](docs/Azienda.md)
 - [Model.Banca](docs/Banca.md)
 - [Model.BancaDocumento](docs/BancaDocumento.md)
 - [Model.BeneServizio](docs/BeneServizio.md)
 - [Model.BeneServizioComputeData](docs/BeneServizioComputeData.md)
 - [Model.BeneServizioComputeResult](docs/BeneServizioComputeResult.md)
 - [Model.CassaPrevidenziale](docs/CassaPrevidenziale.md)
 - [Model.CastellettoIva](docs/CastellettoIva.md)
 - [Model.CategoriaAnagrafica](docs/CategoriaAnagrafica.md)
 - [Model.CategoriaMerceologica](docs/CategoriaMerceologica.md)
 - [Model.CausaleDocumento](docs/CausaleDocumento.md)
 - [Model.CausaleIva](docs/CausaleIva.md)
 - [Model.CausaleMagazzino](docs/CausaleMagazzino.md)
 - [Model.CodicePagamento](docs/CodicePagamento.md)
 - [Model.CollegaDocumentoData](docs/CollegaDocumentoData.md)
 - [Model.Componente](docs/Componente.md)
 - [Model.ContoContabile](docs/ContoContabile.md)
 - [Model.ContoContabileValidationResult](docs/ContoContabileValidationResult.md)
 - [Model.CreditoDebitoMensile](docs/CreditoDebitoMensile.md)
 - [Model.DatiAggiuntivi](docs/DatiAggiuntivi.md)
 - [Model.DatiPA](docs/DatiPA.md)
 - [Model.DatiSdi](docs/DatiSdi.md)
 - [Model.DatiTrasporto](docs/DatiTrasporto.md)
 - [Model.DdtImportData](docs/DdtImportData.md)
 - [Model.DistinctPropertyGroup](docs/DistinctPropertyGroup.md)
 - [Model.Documento](docs/Documento.md)
 - [Model.DocumentoCollegato](docs/DocumentoCollegato.md)
 - [Model.DocumentoComputeData](docs/DocumentoComputeData.md)
 - [Model.DocumentoComputeDataBeneServizio](docs/DocumentoComputeDataBeneServizio.md)
 - [Model.DocumentoComputeDataSpesa](docs/DocumentoComputeDataSpesa.md)
 - [Model.DocumentoComputeResult](docs/DocumentoComputeResult.md)
 - [Model.DocumentoComputeResultRiepilogoIva](docs/DocumentoComputeResultRiepilogoIva.md)
 - [Model.DocumentoImportData](docs/DocumentoImportData.md)
 - [Model.DocumentoMediaFile](docs/DocumentoMediaFile.md)
 - [Model.Evento](docs/Evento.md)
 - [Model.FatturaElettronicaExportOptions](docs/FatturaElettronicaExportOptions.md)
 - [Model.FatturaElettronicaExportResult](docs/FatturaElettronicaExportResult.md)
 - [Model.FatturaElettronicaImportResult](docs/FatturaElettronicaImportResult.md)
 - [Model.FatturaElettronicaUploadOptions](docs/FatturaElettronicaUploadOptions.md)
 - [Model.FatturaElettronicaUploadResult](docs/FatturaElettronicaUploadResult.md)
 - [Model.FatturaElettronicaXml](docs/FatturaElettronicaXml.md)
 - [Model.FatturaElettronicaXmlGetArchiveByDateRangeOptions](docs/FatturaElettronicaXmlGetArchiveByDateRangeOptions.md)
 - [Model.FatturaElettronicaXmlGetArchiveByFileNameOptions](docs/FatturaElettronicaXmlGetArchiveByFileNameOptions.md)
 - [Model.FatturaElettronicaXmlStyleSheetData](docs/FatturaElettronicaXmlStyleSheetData.md)
 - [Model.FatturaElettronicaXmlTemplate](docs/FatturaElettronicaXmlTemplate.md)
 - [Model.FatturatoMensile](docs/FatturatoMensile.md)
 - [Model.Giacenza](docs/Giacenza.md)
 - [Model.GruppoMerceologico](docs/GruppoMerceologico.md)
 - [Model.ImpostazioniSdi](docs/ImpostazioniSdi.md)
 - [Model.Indirizzo](docs/Indirizzo.md)
 - [Model.Listino](docs/Listino.md)
 - [Model.Magazzino](docs/Magazzino.md)
 - [Model.Marca](docs/Marca.md)
 - [Model.ModalitaPagamento](docs/ModalitaPagamento.md)
 - [Model.ModelloContabile](docs/ModelloContabile.md)
 - [Model.ModelloContabileRiga](docs/ModelloContabileRiga.md)
 - [Model.MovimentoMerce](docs/MovimentoMerce.md)
 - [Model.MovimentoMerceRiga](docs/MovimentoMerceRiga.md)
 - [Model.NaturaIva](docs/NaturaIva.md)
 - [Model.Nazione](docs/Nazione.md)
 - [Model.OrdiniImportData](docs/OrdiniImportData.md)
 - [Model.Pagamento](docs/Pagamento.md)
 - [Model.PeriodicInvoicingData](docs/PeriodicInvoicingData.md)
 - [Model.Preferenze](docs/Preferenze.md)
 - [Model.Prestazione](docs/Prestazione.md)
 - [Model.Prezzo](docs/Prezzo.md)
 - [Model.ProblemDetails](docs/ProblemDetails.md)
 - [Model.Produttore](docs/Produttore.md)
 - [Model.ProgressivoInvio](docs/ProgressivoInvio.md)
 - [Model.RegistrazioneContabile](docs/RegistrazioneContabile.md)
 - [Model.RegistrazioneContabileRiga](docs/RegistrazioneContabileRiga.md)
 - [Model.RegistrazioneContabileValidationResult](docs/RegistrazioneContabileValidationResult.md)
 - [Model.Reparto](docs/Reparto.md)
 - [Model.ResoImportData](docs/ResoImportData.md)
 - [Model.RicercaArticolo](docs/RicercaArticolo.md)
 - [Model.RisorsaFinanziaria](docs/RisorsaFinanziaria.md)
 - [Model.Scadenza](docs/Scadenza.md)
 - [Model.SchedaCliente](docs/SchedaCliente.md)
 - [Model.SchedaFornitore](docs/SchedaFornitore.md)
 - [Model.SedeAltra](docs/SedeAltra.md)
 - [Model.SedeLegale](docs/SedeLegale.md)
 - [Model.SedeOperativa](docs/SedeOperativa.md)
 - [Model.Spesa](docs/Spesa.md)
 - [Model.SpesaDocumento](docs/SpesaDocumento.md)
 - [Model.Taglia](docs/Taglia.md)
 - [Model.Valuta](docs/Valuta.md)
 - [Model.WebHook](docs/WebHook.md)


<a id="documentation-for-authorization"></a>
## Documentation for Authorization


Authentication schemes defined for the API:
<a id="oauth2"></a>
### oauth2

- **Type**: OAuth
- **Flow**: application
- **Authorization URL**: 
- **Scopes**: 
  - read: Accesso in lettura alle risorse
  - write: Accesso in scrittura alle risorse
  - comune: Anagrafiche e tabelle di base comuni
  - magazzino: Articoli, listini, giacenze e movimenti di magazzino
  - documenti: Documenti commerciali (ordini, DDT, preventivi, ...)
  - contabilita: Contabilità, fatture elettroniche, pagamenti e scadenze

<a id="oauth2"></a>
### oauth2

- **Type**: OAuth
- **Flow**: accessCode
- **Authorization URL**: https://auth.amica20.it/connect/authorize
- **Scopes**: 
  - read: Accesso in lettura alle risorse
  - write: Accesso in scrittura alle risorse
  - comune: Anagrafiche e tabelle di base comuni
  - magazzino: Articoli, listini, giacenze e movimenti di magazzino
  - documenti: Documenti commerciali (ordini, DDT, preventivi, ...)
  - contabilita: Contabilità, fatture elettroniche, pagamenti e scadenze

<a id="bearer"></a>
### bearer

- **Type**: Bearer Authentication


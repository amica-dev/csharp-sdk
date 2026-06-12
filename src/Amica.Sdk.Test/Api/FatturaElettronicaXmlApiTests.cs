/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'**App Store di Amica**. Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  - **Authorization Code con PKCE** (consigliato per app che operano per conto di un utente Amica): l'utente autorizza la tua app tramite la pagina di consenso dell'identity provider; la tua app riceve un `code` da scambiare con un access token.    ```   GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256   POST {identity-provider}/connect/token        grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri>   ```  - **Client Credentials** (per integrazioni server-to-server, senza utente interattivo):    ```   POST {identity-provider}/connect/token   Content-Type: application/x-www-form-urlencoded    grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=read write documenti contabilita   ```    > **Nota:** il flusso Client Credentials è disponibile solo per le **app private**, non per quelle pubblicate sul marketplace.  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — il token non possiede lo scope richiesto. | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Alcune risposte `400` includono un campo `overridable` che indica se l'errore di validazione può essere ignorato impostando il parametro `validationOverride` a `true`.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: v3
 * Contact: assistenza@gestionaleamica.com
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using RestSharp;
using Xunit;

using Amica.Sdk.Client;
using Amica.Sdk.Api;
// uncomment below to import models
//using Amica.Sdk.Model;

namespace Amica.Sdk.Test.Api
{
    /// <summary>
    ///  Class for testing FatturaElettronicaXmlApi
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by OpenAPI Generator (https://openapi-generator.tech).
    /// Please update the test case below to test the API endpoint.
    /// </remarks>
    public class FatturaElettronicaXmlApiTests : IDisposable
    {
        private FatturaElettronicaXmlApi instance;

        public FatturaElettronicaXmlApiTests()
        {
            instance = new FatturaElettronicaXmlApi();
        }

        public void Dispose()
        {
            // Cleanup when everything is done.
        }

        /// <summary>
        /// Test an instance of FatturaElettronicaXmlApi
        /// </summary>
        [Fact]
        public void InstanceTest()
        {
            // TODO uncomment below to test 'IsType' FatturaElettronicaXmlApi
            //Assert.IsType<FatturaElettronicaXmlApi>(instance);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //FatturaElettronicaXmlGetArchiveByDateRangeOptions? fatturaElettronicaXmlGetArchiveByDateRangeOptions = null;
            //instance.ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost(version, companyId, fatturaElettronicaXmlGetArchiveByDateRangeOptions);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //FatturaElettronicaXmlGetArchiveByFileNameOptions? fatturaElettronicaXmlGetArchiveByFileNameOptions = null;
            //instance.ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost(version, companyId, fatturaElettronicaXmlGetArchiveByFileNameOptions);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string companyId = null;
            //string version = null;
            //instance.ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete(companyId, version);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlBulkGet
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlBulkGetTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string companyId = null;
            //string version = null;
            //instance.ApiVversionCompanyIdFatturaElettronicaXmlBulkGet(companyId, version);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlBulkPost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlBulkPostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string companyId = null;
            //string version = null;
            //System.IO.Stream? file = null;
            //instance.ApiVversionCompanyIdFatturaElettronicaXmlBulkPost(companyId, version, file);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlExportPost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlExportPostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //bool? validationOverride = null;
            //FatturaElettronicaExportOptions? fatturaElettronicaExportOptions = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlExportPost(version, companyId, validationOverride, fatturaElettronicaExportOptions);
            //Assert.IsType<FatturaElettronicaExportResult>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //string fileName = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet(version, companyId, fileName);
            //Assert.IsType<List<AllegatoDocumento>>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //string fileName = null;
            //instance.ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet(version, companyId, fileName);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlGet
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlGetTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //int? skip = null;
            //int? take = null;
            //bool? skipTotalCount = null;
            //string? filter = null;
            //string? order = null;
            //List<string>? expand = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlGet(version, companyId, skip, take, skipTotalCount, filter, order, expand);
            //Assert.IsType<List<FatturaElettronicaXml>>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlIdDelete
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //int id = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlIdDelete(version, companyId, id);
            //Assert.IsType<FatturaElettronicaXml>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlIdGet
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlIdGetTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //int id = null;
            //List<string>? expand = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlIdGet(version, companyId, id, expand);
            //Assert.IsType<FatturaElettronicaXml>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlIdPut
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlIdPutTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //int id = null;
            //bool? validationOverride = null;
            //FatturaElettronicaXml? fatturaElettronicaXml = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlIdPut(version, companyId, id, validationOverride, fatturaElettronicaXml);
            //Assert.IsType<FatturaElettronicaXml>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlImportPost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlImportPostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //DateTime? dataConsegnaSdi = null;
            //string? identificativoSdi = null;
            //System.IO.Stream? file = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlImportPost(version, companyId, dataConsegnaSdi, identificativoSdi, file);
            //Assert.IsType<List<FatturaElettronicaImportResult>>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlNewGet
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlNewGetTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlNewGet(version, companyId);
            //Assert.IsType<FatturaElettronicaXml>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlPairPost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlPairPostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //string? fileName = null;
            //string? progressivo = null;
            //int? documentoId = null;
            //System.IO.Stream? file = null;
            //instance.ApiVversionCompanyIdFatturaElettronicaXmlPairPost(version, companyId, fileName, progressivo, documentoId, file);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlPost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlPostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //bool? validationOverride = null;
            //FatturaElettronicaXml? fatturaElettronicaXml = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlPost(version, companyId, validationOverride, fatturaElettronicaXml);
            //Assert.IsType<FatturaElettronicaXml>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost(version, companyId);
            //Assert.IsType<List<FatturaElettronicaImportResult>>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost(version, companyId);
            //Assert.IsType<List<FatturaElettronicaImportResult>>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //FatturaElettronicaUploadOptions? fatturaElettronicaUploadOptions = null;
            //var response = instance.ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost(version, companyId, fatturaElettronicaUploadOptions);
            //Assert.IsType<FatturaElettronicaUploadResult>(response);
        }

        /// <summary>
        /// Test ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost
        /// </summary>
        [Fact]
        public void ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostTest()
        {
            // TODO uncomment below to test the method and replace null with proper value
            //string version = null;
            //string companyId = null;
            //FatturaElettronicaXmlStyleSheetData? fatturaElettronicaXmlStyleSheetData = null;
            //instance.ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost(version, companyId, fatturaElettronicaXmlStyleSheetData);
        }
    }
}

/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.1.3
 * Contact: assistenza@gestionaleamica.com
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Mime;
using Amica.Sdk.Client;
using Amica.Sdk.Client.Auth;
using Amica.Sdk.Model;

namespace Amica.Sdk.Api
{

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IFatturaElettronicaXmlApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date.
        /// </summary>
        /// <remarks>
        /// Non è possibile scaricare più di 350 file alla volta.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByDateRangeOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        void ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByDateRangeOptions? fatturaElettronicaXmlGetArchiveByDateRangeOptions = default(FatturaElettronicaXmlGetArchiveByDateRangeOptions?), int operationIndex = 0);

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date.
        /// </summary>
        /// <remarks>
        /// Non è possibile scaricare più di 350 file alla volta.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByDateRangeOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByDateRangeOptions? fatturaElettronicaXmlGetArchiveByDateRangeOptions = default(FatturaElettronicaXmlGetArchiveByDateRangeOptions?), int operationIndex = 0);
        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per nome file.
        /// </summary>
        /// <remarks>
        /// Non è possibile scaricare più di 350 file alla volta.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByFileNameOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        void ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByFileNameOptions? fatturaElettronicaXmlGetArchiveByFileNameOptions = default(FatturaElettronicaXmlGetArchiveByFileNameOptions?), int operationIndex = 0);

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per nome file.
        /// </summary>
        /// <remarks>
        /// Non è possibile scaricare più di 350 file alla volta.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByFileNameOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByFileNameOptions? fatturaElettronicaXmlGetArchiveByFileNameOptions = default(FatturaElettronicaXmlGetArchiveByFileNameOptions?), int operationIndex = 0);
        /// <summary>
        /// Elimina in blocco tutte le fatture XML dell&#39;azienda.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        void ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete(string companyId, string version, int operationIndex = 0);

        /// <summary>
        /// Elimina in blocco tutte le fatture XML dell&#39;azienda.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteWithHttpInfo(string companyId, string version, int operationIndex = 0);
        /// <summary>
        /// Scarica tutte le fatture XML come archivio ZIP.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        void ApiVversionCompanyIdFatturaElettronicaXmlBulkGet(string companyId, string version, int operationIndex = 0);

        /// <summary>
        /// Scarica tutte le fatture XML come archivio ZIP.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlBulkGetWithHttpInfo(string companyId, string version, int operationIndex = 0);
        /// <summary>
        /// Carica in blocco fatture XML da un archivio ZIP.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="file">Archivio ZIP contenente i file XML. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        void ApiVversionCompanyIdFatturaElettronicaXmlBulkPost(string companyId, string version, string? idempotencyKey = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0);

        /// <summary>
        /// Carica in blocco fatture XML da un archivio ZIP.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="file">Archivio ZIP contenente i file XML. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlBulkPostWithHttpInfo(string companyId, string version, string? idempotencyKey = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0);
        /// <summary>
        /// Esporta uno o più documenti in una fattura XML.
        /// </summary>
        /// <remarks>
        /// Tutti i documenti devono riferirsi alla stessa &lt;a href&#x3D;\&quot;#tag/Anagrafica\&quot;&gt;Anagrafica&lt;/a&gt;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaExportOptions">Opzioni di esportazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaExportResult</returns>
        FatturaElettronicaExportResult ApiVversionCompanyIdFatturaElettronicaXmlExportPost(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaExportOptions? fatturaElettronicaExportOptions = default(FatturaElettronicaExportOptions?), int operationIndex = 0);

        /// <summary>
        /// Esporta uno o più documenti in una fattura XML.
        /// </summary>
        /// <remarks>
        /// Tutti i documenti devono riferirsi alla stessa &lt;a href&#x3D;\&quot;#tag/Anagrafica\&quot;&gt;Anagrafica&lt;/a&gt;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaExportOptions">Opzioni di esportazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaExportResult</returns>
        ApiResponse<FatturaElettronicaExportResult> ApiVversionCompanyIdFatturaElettronicaXmlExportPostWithHttpInfo(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaExportOptions? fatturaElettronicaExportOptions = default(FatturaElettronicaExportOptions?), int operationIndex = 0);
        /// <summary>
        /// Scarica i file allegati di una fattura XML.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;AllegatoDocumento&gt;</returns>
        List<AllegatoDocumento> ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet(string version, string companyId, string fileName, int operationIndex = 0);

        /// <summary>
        /// Scarica i file allegati di una fattura XML.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;AllegatoDocumento&gt;</returns>
        ApiResponse<List<AllegatoDocumento>> ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetWithHttpInfo(string version, string companyId, string fileName, int operationIndex = 0);
        /// <summary>
        /// Recupera una fattura XML per nome file.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        void ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet(string version, string companyId, string fileName, int operationIndex = 0);

        /// <summary>
        /// Recupera una fattura XML per nome file.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetWithHttpInfo(string version, string companyId, string fileName, int operationIndex = 0);
        /// <summary>
        /// Restituisce una lista di elementi.
        /// </summary>
        /// <remarks>
        ///  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip"> (optional)</param>
        /// <param name="take"> (optional)</param>
        /// <param name="skipTotalCount"> (optional)</param>
        /// <param name="filter"> (optional)</param>
        /// <param name="order"> (optional)</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;FatturaElettronicaXml&gt;</returns>
        List<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlGet(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0);

        /// <summary>
        /// Restituisce una lista di elementi.
        /// </summary>
        /// <remarks>
        ///  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip"> (optional)</param>
        /// <param name="take"> (optional)</param>
        /// <param name="skipTotalCount"> (optional)</param>
        /// <param name="filter"> (optional)</param>
        /// <param name="order"> (optional)</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;FatturaElettronicaXml&gt;</returns>
        ApiResponse<List<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlGetWithHttpInfo(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0);
        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaXml</returns>
        FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlIdDelete(string version, string companyId, int id, int operationIndex = 0);

        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaXml</returns>
        ApiResponse<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteWithHttpInfo(string version, string companyId, int id, int operationIndex = 0);
        /// <summary>
        /// Restituisce un elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaXml</returns>
        FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlIdGet(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0);

        /// <summary>
        /// Restituisce un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaXml</returns>
        ApiResponse<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdGetWithHttpInfo(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0);
        /// <summary>
        /// Aggiorna un elemento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaXml</returns>
        FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlIdPut(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0);

        /// <summary>
        /// Aggiorna un elemento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaXml</returns>
        ApiResponse<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdPutWithHttpInfo(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0);
        /// <summary>
        /// Importa una fattura XML.
        /// </summary>
        /// <remarks>
        /// Viene creato un nuovo &lt;a href&#x3D;\&quot;#tag/Documento\&quot;&gt;Documento&lt;/a&gt; corrispondente al contenuto XML.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="dataConsegnaSdi"> (optional)</param>
        /// <param name="identificativoSdi"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;FatturaElettronicaImportResult&gt;</returns>
        List<FatturaElettronicaImportResult> ApiVversionCompanyIdFatturaElettronicaXmlImportPost(string version, string companyId, string? idempotencyKey = default(string?), DateTime? dataConsegnaSdi = default(DateTime?), string? identificativoSdi = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0);

        /// <summary>
        /// Importa una fattura XML.
        /// </summary>
        /// <remarks>
        /// Viene creato un nuovo &lt;a href&#x3D;\&quot;#tag/Documento\&quot;&gt;Documento&lt;/a&gt; corrispondente al contenuto XML.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="dataConsegnaSdi"> (optional)</param>
        /// <param name="identificativoSdi"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;FatturaElettronicaImportResult&gt;</returns>
        ApiResponse<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlImportPostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), DateTime? dataConsegnaSdi = default(DateTime?), string? identificativoSdi = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0);
        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile).
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaXml</returns>
        FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlNewGet(string version, string companyId, int operationIndex = 0);

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile).
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaXml</returns>
        ApiResponse<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlNewGetWithHttpInfo(string version, string companyId, int operationIndex = 0);
        /// <summary>
        /// Associa una fattura XML a un documento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fileName"> (optional)</param>
        /// <param name="progressivo"> (optional)</param>
        /// <param name="documentoId"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        void ApiVversionCompanyIdFatturaElettronicaXmlPairPost(string version, string companyId, string? idempotencyKey = default(string?), string? fileName = default(string?), string? progressivo = default(string?), int? documentoId = default(int?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0);

        /// <summary>
        /// Associa una fattura XML a un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fileName"> (optional)</param>
        /// <param name="progressivo"> (optional)</param>
        /// <param name="documentoId"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlPairPostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), string? fileName = default(string?), string? progressivo = default(string?), int? documentoId = default(int?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0);
        /// <summary>
        /// Salva un nuovo elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaXml</returns>
        FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlPost(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0);

        /// <summary>
        /// Salva un nuovo elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaXml</returns>
        ApiResponse<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlPostWithHttpInfo(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0);
        /// <summary>
        /// Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;FatturaElettronicaImportResult&gt;</returns>
        List<FatturaElettronicaImportResult> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0);

        /// <summary>
        /// Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;FatturaElettronicaImportResult&gt;</returns>
        ApiResponse<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0);
        /// <summary>
        /// Scarica le nuove fatture dal Sistema di Interscambio (SDI).
        /// </summary>
        /// <remarks>
        /// Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;FatturaElettronicaImportResult&gt;</returns>
        List<FatturaElettronicaImportResult> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0);

        /// <summary>
        /// Scarica le nuove fatture dal Sistema di Interscambio (SDI).
        /// </summary>
        /// <remarks>
        /// Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;FatturaElettronicaImportResult&gt;</returns>
        ApiResponse<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0);
        /// <summary>
        /// Invia una o più fatture al Sistema di Interscambio (SDI).
        /// </summary>
        /// <remarks>
        /// Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaUploadOptions">Opzioni di invio. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaUploadResult</returns>
        FatturaElettronicaUploadResult ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaUploadOptions? fatturaElettronicaUploadOptions = default(FatturaElettronicaUploadOptions?), int operationIndex = 0);

        /// <summary>
        /// Invia una o più fatture al Sistema di Interscambio (SDI).
        /// </summary>
        /// <remarks>
        /// Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaUploadOptions">Opzioni di invio. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaUploadResult</returns>
        ApiResponse<FatturaElettronicaUploadResult> ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaUploadOptions? fatturaElettronicaUploadOptions = default(FatturaElettronicaUploadOptions?), int operationIndex = 0);
        /// <summary>
        /// Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlStyleSheetData">Opzioni di corrispondenza e stile. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        void ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlStyleSheetData? fatturaElettronicaXmlStyleSheetData = default(FatturaElettronicaXmlStyleSheetData?), int operationIndex = 0);

        /// <summary>
        /// Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlStyleSheetData">Opzioni di corrispondenza e stile. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlStyleSheetData? fatturaElettronicaXmlStyleSheetData = default(FatturaElettronicaXmlStyleSheetData?), int operationIndex = 0);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IFatturaElettronicaXmlApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date.
        /// </summary>
        /// <remarks>
        /// Non è possibile scaricare più di 350 file alla volta.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByDateRangeOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByDateRangeOptions? fatturaElettronicaXmlGetArchiveByDateRangeOptions = default(FatturaElettronicaXmlGetArchiveByDateRangeOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date.
        /// </summary>
        /// <remarks>
        /// Non è possibile scaricare più di 350 file alla volta.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByDateRangeOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByDateRangeOptions? fatturaElettronicaXmlGetArchiveByDateRangeOptions = default(FatturaElettronicaXmlGetArchiveByDateRangeOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per nome file.
        /// </summary>
        /// <remarks>
        /// Non è possibile scaricare più di 350 file alla volta.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByFileNameOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByFileNameOptions? fatturaElettronicaXmlGetArchiveByFileNameOptions = default(FatturaElettronicaXmlGetArchiveByFileNameOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per nome file.
        /// </summary>
        /// <remarks>
        /// Non è possibile scaricare più di 350 file alla volta.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByFileNameOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByFileNameOptions? fatturaElettronicaXmlGetArchiveByFileNameOptions = default(FatturaElettronicaXmlGetArchiveByFileNameOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Elimina in blocco tutte le fatture XML dell&#39;azienda.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteAsync(string companyId, string version, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Elimina in blocco tutte le fatture XML dell&#39;azienda.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteWithHttpInfoAsync(string companyId, string version, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Scarica tutte le fatture XML come archivio ZIP.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlBulkGetAsync(string companyId, string version, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Scarica tutte le fatture XML come archivio ZIP.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlBulkGetWithHttpInfoAsync(string companyId, string version, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Carica in blocco fatture XML da un archivio ZIP.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="file">Archivio ZIP contenente i file XML. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlBulkPostAsync(string companyId, string version, string? idempotencyKey = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Carica in blocco fatture XML da un archivio ZIP.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="file">Archivio ZIP contenente i file XML. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlBulkPostWithHttpInfoAsync(string companyId, string version, string? idempotencyKey = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Esporta uno o più documenti in una fattura XML.
        /// </summary>
        /// <remarks>
        /// Tutti i documenti devono riferirsi alla stessa &lt;a href&#x3D;\&quot;#tag/Anagrafica\&quot;&gt;Anagrafica&lt;/a&gt;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaExportOptions">Opzioni di esportazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaExportResult</returns>
        System.Threading.Tasks.Task<FatturaElettronicaExportResult> ApiVversionCompanyIdFatturaElettronicaXmlExportPostAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaExportOptions? fatturaElettronicaExportOptions = default(FatturaElettronicaExportOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Esporta uno o più documenti in una fattura XML.
        /// </summary>
        /// <remarks>
        /// Tutti i documenti devono riferirsi alla stessa &lt;a href&#x3D;\&quot;#tag/Anagrafica\&quot;&gt;Anagrafica&lt;/a&gt;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaExportOptions">Opzioni di esportazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaExportResult)</returns>
        System.Threading.Tasks.Task<ApiResponse<FatturaElettronicaExportResult>> ApiVversionCompanyIdFatturaElettronicaXmlExportPostWithHttpInfoAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaExportOptions? fatturaElettronicaExportOptions = default(FatturaElettronicaExportOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Scarica i file allegati di una fattura XML.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;AllegatoDocumento&gt;</returns>
        System.Threading.Tasks.Task<List<AllegatoDocumento>> ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetAsync(string version, string companyId, string fileName, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Scarica i file allegati di una fattura XML.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;AllegatoDocumento&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<AllegatoDocumento>>> ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetWithHttpInfoAsync(string version, string companyId, string fileName, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Recupera una fattura XML per nome file.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetAsync(string version, string companyId, string fileName, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Recupera una fattura XML per nome file.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetWithHttpInfoAsync(string version, string companyId, string fileName, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Restituisce una lista di elementi.
        /// </summary>
        /// <remarks>
        ///  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip"> (optional)</param>
        /// <param name="take"> (optional)</param>
        /// <param name="skipTotalCount"> (optional)</param>
        /// <param name="filter"> (optional)</param>
        /// <param name="order"> (optional)</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;FatturaElettronicaXml&gt;</returns>
        System.Threading.Tasks.Task<List<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlGetAsync(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Restituisce una lista di elementi.
        /// </summary>
        /// <remarks>
        ///  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip"> (optional)</param>
        /// <param name="take"> (optional)</param>
        /// <param name="skipTotalCount"> (optional)</param>
        /// <param name="filter"> (optional)</param>
        /// <param name="order"> (optional)</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;FatturaElettronicaXml&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<FatturaElettronicaXml>>> ApiVversionCompanyIdFatturaElettronicaXmlGetWithHttpInfoAsync(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaXml</returns>
        System.Threading.Tasks.Task<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteAsync(string version, string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaXml)</returns>
        System.Threading.Tasks.Task<ApiResponse<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteWithHttpInfoAsync(string version, string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Restituisce un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaXml</returns>
        System.Threading.Tasks.Task<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdGetAsync(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Restituisce un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaXml)</returns>
        System.Threading.Tasks.Task<ApiResponse<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlIdGetWithHttpInfoAsync(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Aggiorna un elemento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaXml</returns>
        System.Threading.Tasks.Task<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdPutAsync(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Aggiorna un elemento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaXml)</returns>
        System.Threading.Tasks.Task<ApiResponse<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlIdPutWithHttpInfoAsync(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Importa una fattura XML.
        /// </summary>
        /// <remarks>
        /// Viene creato un nuovo &lt;a href&#x3D;\&quot;#tag/Documento\&quot;&gt;Documento&lt;/a&gt; corrispondente al contenuto XML.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="dataConsegnaSdi"> (optional)</param>
        /// <param name="identificativoSdi"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;FatturaElettronicaImportResult&gt;</returns>
        System.Threading.Tasks.Task<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlImportPostAsync(string version, string companyId, string? idempotencyKey = default(string?), DateTime? dataConsegnaSdi = default(DateTime?), string? identificativoSdi = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Importa una fattura XML.
        /// </summary>
        /// <remarks>
        /// Viene creato un nuovo &lt;a href&#x3D;\&quot;#tag/Documento\&quot;&gt;Documento&lt;/a&gt; corrispondente al contenuto XML.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="dataConsegnaSdi"> (optional)</param>
        /// <param name="identificativoSdi"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;FatturaElettronicaImportResult&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<FatturaElettronicaImportResult>>> ApiVversionCompanyIdFatturaElettronicaXmlImportPostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), DateTime? dataConsegnaSdi = default(DateTime?), string? identificativoSdi = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile).
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaXml</returns>
        System.Threading.Tasks.Task<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlNewGetAsync(string version, string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile).
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaXml)</returns>
        System.Threading.Tasks.Task<ApiResponse<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlNewGetWithHttpInfoAsync(string version, string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Associa una fattura XML a un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fileName"> (optional)</param>
        /// <param name="progressivo"> (optional)</param>
        /// <param name="documentoId"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlPairPostAsync(string version, string companyId, string? idempotencyKey = default(string?), string? fileName = default(string?), string? progressivo = default(string?), int? documentoId = default(int?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Associa una fattura XML a un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fileName"> (optional)</param>
        /// <param name="progressivo"> (optional)</param>
        /// <param name="documentoId"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlPairPostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), string? fileName = default(string?), string? progressivo = default(string?), int? documentoId = default(int?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Salva un nuovo elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaXml</returns>
        System.Threading.Tasks.Task<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlPostAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Salva un nuovo elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaXml)</returns>
        System.Threading.Tasks.Task<ApiResponse<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlPostWithHttpInfoAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;FatturaElettronicaImportResult&gt;</returns>
        System.Threading.Tasks.Task<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostAsync(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;FatturaElettronicaImportResult&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<FatturaElettronicaImportResult>>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Scarica le nuove fatture dal Sistema di Interscambio (SDI).
        /// </summary>
        /// <remarks>
        /// Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;FatturaElettronicaImportResult&gt;</returns>
        System.Threading.Tasks.Task<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostAsync(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Scarica le nuove fatture dal Sistema di Interscambio (SDI).
        /// </summary>
        /// <remarks>
        /// Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;FatturaElettronicaImportResult&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<FatturaElettronicaImportResult>>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Invia una o più fatture al Sistema di Interscambio (SDI).
        /// </summary>
        /// <remarks>
        /// Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaUploadOptions">Opzioni di invio. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaUploadResult</returns>
        System.Threading.Tasks.Task<FatturaElettronicaUploadResult> ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaUploadOptions? fatturaElettronicaUploadOptions = default(FatturaElettronicaUploadOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Invia una o più fatture al Sistema di Interscambio (SDI).
        /// </summary>
        /// <remarks>
        /// Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaUploadOptions">Opzioni di invio. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaUploadResult)</returns>
        System.Threading.Tasks.Task<ApiResponse<FatturaElettronicaUploadResult>> ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaUploadOptions? fatturaElettronicaUploadOptions = default(FatturaElettronicaUploadOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlStyleSheetData">Opzioni di corrispondenza e stile. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlStyleSheetData? fatturaElettronicaXmlStyleSheetData = default(FatturaElettronicaXmlStyleSheetData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlStyleSheetData">Opzioni di corrispondenza e stile. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlStyleSheetData? fatturaElettronicaXmlStyleSheetData = default(FatturaElettronicaXmlStyleSheetData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IFatturaElettronicaXmlApi : IFatturaElettronicaXmlApiSync, IFatturaElettronicaXmlApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class FatturaElettronicaXmlApi : IFatturaElettronicaXmlApi
    {
        private Amica.Sdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="FatturaElettronicaXmlApi"/> class.
        /// </summary>
        /// <returns></returns>
        public FatturaElettronicaXmlApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FatturaElettronicaXmlApi"/> class.
        /// </summary>
        /// <returns></returns>
        public FatturaElettronicaXmlApi(string basePath)
        {
            this.Configuration = Amica.Sdk.Client.Configuration.MergeConfigurations(
                Amica.Sdk.Client.GlobalConfiguration.Instance,
                new Amica.Sdk.Client.Configuration { BasePath = basePath }
            );
            this.Client = new Amica.Sdk.Client.ApiClient(this.Configuration.BasePath);
            this.AsynchronousClient = new Amica.Sdk.Client.ApiClient(this.Configuration.BasePath);
            this.ExceptionFactory = Amica.Sdk.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FatturaElettronicaXmlApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public FatturaElettronicaXmlApi(Amica.Sdk.Client.Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Configuration = Amica.Sdk.Client.Configuration.MergeConfigurations(
                Amica.Sdk.Client.GlobalConfiguration.Instance,
                configuration
            );
            this.Client = new Amica.Sdk.Client.ApiClient(this.Configuration.BasePath);
            this.AsynchronousClient = new Amica.Sdk.Client.ApiClient(this.Configuration.BasePath);
            ExceptionFactory = Amica.Sdk.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FatturaElettronicaXmlApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        public FatturaElettronicaXmlApi(Amica.Sdk.Client.ISynchronousClient client, Amica.Sdk.Client.IAsynchronousClient asyncClient, Amica.Sdk.Client.IReadableConfiguration configuration)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (asyncClient == null) throw new ArgumentNullException("asyncClient");
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Client = client;
            this.AsynchronousClient = asyncClient;
            this.Configuration = configuration;
            this.ExceptionFactory = Amica.Sdk.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// The client for accessing this underlying API asynchronously.
        /// </summary>
        public Amica.Sdk.Client.IAsynchronousClient AsynchronousClient { get; set; }

        /// <summary>
        /// The client for accessing this underlying API synchronously.
        /// </summary>
        public Amica.Sdk.Client.ISynchronousClient Client { get; set; }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return this.Configuration.BasePath;
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public Amica.Sdk.Client.IReadableConfiguration Configuration { get; set; }

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public Amica.Sdk.Client.ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date. Non è possibile scaricare più di 350 file alla volta.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByDateRangeOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        public void ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByDateRangeOptions? fatturaElettronicaXmlGetArchiveByDateRangeOptions = default(FatturaElettronicaXmlGetArchiveByDateRangeOptions?), int operationIndex = 0)
        {
            ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostWithHttpInfo(version, companyId, idempotencyKey, fatturaElettronicaXmlGetArchiveByDateRangeOptions);
        }

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date. Non è possibile scaricare più di 350 file alla volta.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByDateRangeOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public Amica.Sdk.Client.ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByDateRangeOptions? fatturaElettronicaXmlGetArchiveByDateRangeOptions = default(FatturaElettronicaXmlGetArchiveByDateRangeOptions?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json",
                "text/json",
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/zip",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaXmlGetArchiveByDateRangeOptions;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/archivebydaterange", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date. Non è possibile scaricare più di 350 file alla volta.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByDateRangeOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByDateRangeOptions? fatturaElettronicaXmlGetArchiveByDateRangeOptions = default(FatturaElettronicaXmlGetArchiveByDateRangeOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            await ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostWithHttpInfoAsync(version, companyId, idempotencyKey, fatturaElettronicaXmlGetArchiveByDateRangeOptions, operationIndex, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per intervallo di date. Non è possibile scaricare più di 350 file alla volta.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByDateRangeOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByDateRangeOptions? fatturaElettronicaXmlGetArchiveByDateRangeOptions = default(FatturaElettronicaXmlGetArchiveByDateRangeOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/zip",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaXmlGetArchiveByDateRangeOptions;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/archivebydaterange", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlArchivebydaterangePost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per nome file. Non è possibile scaricare più di 350 file alla volta.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByFileNameOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        public void ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByFileNameOptions? fatturaElettronicaXmlGetArchiveByFileNameOptions = default(FatturaElettronicaXmlGetArchiveByFileNameOptions?), int operationIndex = 0)
        {
            ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostWithHttpInfo(version, companyId, idempotencyKey, fatturaElettronicaXmlGetArchiveByFileNameOptions);
        }

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per nome file. Non è possibile scaricare più di 350 file alla volta.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByFileNameOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public Amica.Sdk.Client.ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByFileNameOptions? fatturaElettronicaXmlGetArchiveByFileNameOptions = default(FatturaElettronicaXmlGetArchiveByFileNameOptions?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json",
                "text/json",
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/zip",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaXmlGetArchiveByFileNameOptions;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/archivebyfilename", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per nome file. Non è possibile scaricare più di 350 file alla volta.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByFileNameOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByFileNameOptions? fatturaElettronicaXmlGetArchiveByFileNameOptions = default(FatturaElettronicaXmlGetArchiveByFileNameOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            await ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostWithHttpInfoAsync(version, companyId, idempotencyKey, fatturaElettronicaXmlGetArchiveByFileNameOptions, operationIndex, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Scarica un archivio ZIP di fatture XML, filtrate per nome file. Non è possibile scaricare più di 350 file alla volta.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlGetArchiveByFileNameOptions">Opzioni di download. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlGetArchiveByFileNameOptions? fatturaElettronicaXmlGetArchiveByFileNameOptions = default(FatturaElettronicaXmlGetArchiveByFileNameOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/zip",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaXmlGetArchiveByFileNameOptions;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/archivebyfilename", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlArchivebyfilenamePost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Elimina in blocco tutte le fatture XML dell&#39;azienda. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        public void ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete(string companyId, string version, int operationIndex = 0)
        {
            ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteWithHttpInfo(companyId, version);
        }

        /// <summary>
        /// Elimina in blocco tutte le fatture XML dell&#39;azienda. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public Amica.Sdk.Client.ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteWithHttpInfo(string companyId, string version, int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete");
            }

            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Delete<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/bulk", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Elimina in blocco tutte le fatture XML dell&#39;azienda. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteAsync(string companyId, string version, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            await ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteWithHttpInfoAsync(companyId, version, operationIndex, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Elimina in blocco tutte le fatture XML dell&#39;azienda. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlBulkDeleteWithHttpInfoAsync(string companyId, string version, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete");
            }

            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.DeleteAsync<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/bulk", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlBulkDelete", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Scarica tutte le fatture XML come archivio ZIP. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        public void ApiVversionCompanyIdFatturaElettronicaXmlBulkGet(string companyId, string version, int operationIndex = 0)
        {
            ApiVversionCompanyIdFatturaElettronicaXmlBulkGetWithHttpInfo(companyId, version);
        }

        /// <summary>
        /// Scarica tutte le fatture XML come archivio ZIP. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public Amica.Sdk.Client.ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlBulkGetWithHttpInfo(string companyId, string version, int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkGet");
            }

            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkGet");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/zip",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Get<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/bulk", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlBulkGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Scarica tutte le fatture XML come archivio ZIP. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlBulkGetAsync(string companyId, string version, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            await ApiVversionCompanyIdFatturaElettronicaXmlBulkGetWithHttpInfoAsync(companyId, version, operationIndex, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Scarica tutte le fatture XML come archivio ZIP. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlBulkGetWithHttpInfoAsync(string companyId, string version, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkGet");
            }

            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkGet");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/zip",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.GetAsync<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/bulk", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlBulkGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Carica in blocco fatture XML da un archivio ZIP. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="file">Archivio ZIP contenente i file XML. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        public void ApiVversionCompanyIdFatturaElettronicaXmlBulkPost(string companyId, string version, string? idempotencyKey = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0)
        {
            ApiVversionCompanyIdFatturaElettronicaXmlBulkPostWithHttpInfo(companyId, version, idempotencyKey, file);
        }

        /// <summary>
        /// Carica in blocco fatture XML da un archivio ZIP. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="file">Archivio ZIP contenente i file XML. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public Amica.Sdk.Client.ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlBulkPostWithHttpInfo(string companyId, string version, string? idempotencyKey = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkPost");
            }

            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkPost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "multipart/form-data"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            if (file != null)
            {
                localVarRequestOptions.FileParameters.Add("file", file);
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/bulk", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlBulkPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Carica in blocco fatture XML da un archivio ZIP. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="file">Archivio ZIP contenente i file XML. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlBulkPostAsync(string companyId, string version, string? idempotencyKey = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            await ApiVversionCompanyIdFatturaElettronicaXmlBulkPostWithHttpInfoAsync(companyId, version, idempotencyKey, file, operationIndex, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Carica in blocco fatture XML da un archivio ZIP. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="version"></param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="file">Archivio ZIP contenente i file XML. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlBulkPostWithHttpInfoAsync(string companyId, string version, string? idempotencyKey = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkPost");
            }

            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlBulkPost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "multipart/form-data"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "text/plain",
                "application/json",
                "text/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            if (file != null)
            {
                localVarRequestOptions.FileParameters.Add("file", file);
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlBulkPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/bulk", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlBulkPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Esporta uno o più documenti in una fattura XML. Tutti i documenti devono riferirsi alla stessa &lt;a href&#x3D;\&quot;#tag/Anagrafica\&quot;&gt;Anagrafica&lt;/a&gt;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaExportOptions">Opzioni di esportazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaExportResult</returns>
        public FatturaElettronicaExportResult ApiVversionCompanyIdFatturaElettronicaXmlExportPost(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaExportOptions? fatturaElettronicaExportOptions = default(FatturaElettronicaExportOptions?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaExportResult> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlExportPostWithHttpInfo(version, companyId, validationOverride, idempotencyKey, fatturaElettronicaExportOptions);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Esporta uno o più documenti in una fattura XML. Tutti i documenti devono riferirsi alla stessa &lt;a href&#x3D;\&quot;#tag/Anagrafica\&quot;&gt;Anagrafica&lt;/a&gt;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaExportOptions">Opzioni di esportazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaExportResult</returns>
        public Amica.Sdk.Client.ApiResponse<FatturaElettronicaExportResult> ApiVversionCompanyIdFatturaElettronicaXmlExportPostWithHttpInfo(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaExportOptions? fatturaElettronicaExportOptions = default(FatturaElettronicaExportOptions?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlExportPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlExportPost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json",
                "text/json",
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaExportOptions;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlExportPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<FatturaElettronicaExportResult>("/api/v{version}/{companyId}/FatturaElettronicaXml/export", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlExportPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Esporta uno o più documenti in una fattura XML. Tutti i documenti devono riferirsi alla stessa &lt;a href&#x3D;\&quot;#tag/Anagrafica\&quot;&gt;Anagrafica&lt;/a&gt;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaExportOptions">Opzioni di esportazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaExportResult</returns>
        public async System.Threading.Tasks.Task<FatturaElettronicaExportResult> ApiVversionCompanyIdFatturaElettronicaXmlExportPostAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaExportOptions? fatturaElettronicaExportOptions = default(FatturaElettronicaExportOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaExportResult> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlExportPostWithHttpInfoAsync(version, companyId, validationOverride, idempotencyKey, fatturaElettronicaExportOptions, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Esporta uno o più documenti in una fattura XML. Tutti i documenti devono riferirsi alla stessa &lt;a href&#x3D;\&quot;#tag/Anagrafica\&quot;&gt;Anagrafica&lt;/a&gt;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaExportOptions">Opzioni di esportazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaExportResult)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<FatturaElettronicaExportResult>> ApiVversionCompanyIdFatturaElettronicaXmlExportPostWithHttpInfoAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaExportOptions? fatturaElettronicaExportOptions = default(FatturaElettronicaExportOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlExportPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlExportPost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaExportOptions;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlExportPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<FatturaElettronicaExportResult>("/api/v{version}/{companyId}/FatturaElettronicaXml/export", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlExportPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Scarica i file allegati di una fattura XML. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;AllegatoDocumento&gt;</returns>
        public List<AllegatoDocumento> ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet(string version, string companyId, string fileName, int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<AllegatoDocumento>> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetWithHttpInfo(version, companyId, fileName);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Scarica i file allegati di una fattura XML. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;AllegatoDocumento&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<AllegatoDocumento>> ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetWithHttpInfo(string version, string companyId, string fileName, int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet");
            }

            // verify the required parameter 'fileName' is set
            if (fileName == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'fileName' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("fileName", Amica.Sdk.Client.ClientUtils.ParameterToString(fileName)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Get<List<AllegatoDocumento>>("/api/v{version}/{companyId}/FatturaElettronicaXml/{fileName}/attachments", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Scarica i file allegati di una fattura XML. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;AllegatoDocumento&gt;</returns>
        public async System.Threading.Tasks.Task<List<AllegatoDocumento>> ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetAsync(string version, string companyId, string fileName, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<AllegatoDocumento>> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetWithHttpInfoAsync(version, companyId, fileName, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Scarica i file allegati di una fattura XML. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;AllegatoDocumento&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<AllegatoDocumento>>> ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGetWithHttpInfoAsync(string version, string companyId, string fileName, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet");
            }

            // verify the required parameter 'fileName' is set
            if (fileName == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'fileName' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("fileName", Amica.Sdk.Client.ClientUtils.ParameterToString(fileName)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.GetAsync<List<AllegatoDocumento>>("/api/v{version}/{companyId}/FatturaElettronicaXml/{fileName}/attachments", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlFileNameAttachmentsGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Recupera una fattura XML per nome file. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        public void ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet(string version, string companyId, string fileName, int operationIndex = 0)
        {
            ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetWithHttpInfo(version, companyId, fileName);
        }

        /// <summary>
        /// Recupera una fattura XML per nome file. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public Amica.Sdk.Client.ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetWithHttpInfo(string version, string companyId, string fileName, int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet");
            }

            // verify the required parameter 'fileName' is set
            if (fileName == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'fileName' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/xml",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("fileName", Amica.Sdk.Client.ClientUtils.ParameterToString(fileName)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Get<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/{fileName}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Recupera una fattura XML per nome file. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetAsync(string version, string companyId, string fileName, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            await ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetWithHttpInfoAsync(version, companyId, fileName, operationIndex, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Recupera una fattura XML per nome file. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="fileName">Nome del file.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlFileNameGetWithHttpInfoAsync(string version, string companyId, string fileName, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet");
            }

            // verify the required parameter 'fileName' is set
            if (fileName == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'fileName' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/xml",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("fileName", Amica.Sdk.Client.ClientUtils.ParameterToString(fileName)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.GetAsync<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/{fileName}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlFileNameGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce una lista di elementi.  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip"> (optional)</param>
        /// <param name="take"> (optional)</param>
        /// <param name="skipTotalCount"> (optional)</param>
        /// <param name="filter"> (optional)</param>
        /// <param name="order"> (optional)</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;FatturaElettronicaXml&gt;</returns>
        public List<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlGet(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaXml>> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlGetWithHttpInfo(version, companyId, skip, take, skipTotalCount, filter, order, expand);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una lista di elementi.  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip"> (optional)</param>
        /// <param name="take"> (optional)</param>
        /// <param name="skipTotalCount"> (optional)</param>
        /// <param name="filter"> (optional)</param>
        /// <param name="order"> (optional)</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;FatturaElettronicaXml&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlGetWithHttpInfo(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlGet");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (skip != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Skip", skip));
            }
            if (take != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Take", take));
            }
            if (skipTotalCount != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "SkipTotalCount", skipTotalCount));
            }
            if (filter != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Filter", filter));
            }
            if (order != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Order", order));
            }
            if (expand != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("multi", "Expand", expand));
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Get<List<FatturaElettronicaXml>>("/api/v{version}/{companyId}/FatturaElettronicaXml", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce una lista di elementi.  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip"> (optional)</param>
        /// <param name="take"> (optional)</param>
        /// <param name="skipTotalCount"> (optional)</param>
        /// <param name="filter"> (optional)</param>
        /// <param name="order"> (optional)</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;FatturaElettronicaXml&gt;</returns>
        public async System.Threading.Tasks.Task<List<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlGetAsync(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaXml>> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlGetWithHttpInfoAsync(version, companyId, skip, take, skipTotalCount, filter, order, expand, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una lista di elementi.  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip"> (optional)</param>
        /// <param name="take"> (optional)</param>
        /// <param name="skipTotalCount"> (optional)</param>
        /// <param name="filter"> (optional)</param>
        /// <param name="order"> (optional)</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;FatturaElettronicaXml&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaXml>>> ApiVversionCompanyIdFatturaElettronicaXmlGetWithHttpInfoAsync(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlGet");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (skip != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Skip", skip));
            }
            if (take != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Take", take));
            }
            if (skipTotalCount != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "SkipTotalCount", skipTotalCount));
            }
            if (filter != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Filter", filter));
            }
            if (order != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Order", order));
            }
            if (expand != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("multi", "Expand", expand));
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.GetAsync<List<FatturaElettronicaXml>>("/api/v{version}/{companyId}/FatturaElettronicaXml", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Elimina un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaXml</returns>
        public FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlIdDelete(string version, string companyId, int id, int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteWithHttpInfo(version, companyId, id);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Elimina un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaXml</returns>
        public Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteWithHttpInfo(string version, string companyId, int id, int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdDelete");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdDelete");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("id", Amica.Sdk.Client.ClientUtils.ParameterToString(id)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdDelete";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Delete<FatturaElettronicaXml>("/api/v{version}/{companyId}/FatturaElettronicaXml/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlIdDelete", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Elimina un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaXml</returns>
        public async System.Threading.Tasks.Task<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteAsync(string version, string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteWithHttpInfoAsync(version, companyId, id, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Elimina un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaXml)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlIdDeleteWithHttpInfoAsync(string version, string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdDelete");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdDelete");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("id", Amica.Sdk.Client.ClientUtils.ParameterToString(id)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdDelete";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.DeleteAsync<FatturaElettronicaXml>("/api/v{version}/{companyId}/FatturaElettronicaXml/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlIdDelete", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaXml</returns>
        public FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlIdGet(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlIdGetWithHttpInfo(version, companyId, id, expand);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaXml</returns>
        public Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdGetWithHttpInfo(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdGet");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("id", Amica.Sdk.Client.ClientUtils.ParameterToString(id)); // path parameter
            if (expand != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("multi", "Expand", expand));
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Get<FatturaElettronicaXml>("/api/v{version}/{companyId}/FatturaElettronicaXml/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlIdGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaXml</returns>
        public async System.Threading.Tasks.Task<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdGetAsync(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlIdGetWithHttpInfoAsync(version, companyId, id, expand, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaXml)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlIdGetWithHttpInfoAsync(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdGet");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("id", Amica.Sdk.Client.ClientUtils.ParameterToString(id)); // path parameter
            if (expand != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("multi", "Expand", expand));
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.GetAsync<FatturaElettronicaXml>("/api/v{version}/{companyId}/FatturaElettronicaXml/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlIdGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Aggiorna un elemento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaXml</returns>
        public FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlIdPut(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlIdPutWithHttpInfo(version, companyId, id, validationOverride, idempotencyKey, fatturaElettronicaXml);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Aggiorna un elemento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaXml</returns>
        public Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdPutWithHttpInfo(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdPut");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdPut");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json",
                "text/json",
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("id", Amica.Sdk.Client.ClientUtils.ParameterToString(id)); // path parameter
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaXml;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdPut";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Put<FatturaElettronicaXml>("/api/v{version}/{companyId}/FatturaElettronicaXml/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlIdPut", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Aggiorna un elemento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaXml</returns>
        public async System.Threading.Tasks.Task<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlIdPutAsync(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlIdPutWithHttpInfoAsync(version, companyId, id, validationOverride, idempotencyKey, fatturaElettronicaXml, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Aggiorna un elemento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaXml)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlIdPutWithHttpInfoAsync(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdPut");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlIdPut");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("id", Amica.Sdk.Client.ClientUtils.ParameterToString(id)); // path parameter
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaXml;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlIdPut";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PutAsync<FatturaElettronicaXml>("/api/v{version}/{companyId}/FatturaElettronicaXml/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlIdPut", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa una fattura XML. Viene creato un nuovo &lt;a href&#x3D;\&quot;#tag/Documento\&quot;&gt;Documento&lt;/a&gt; corrispondente al contenuto XML.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="dataConsegnaSdi"> (optional)</param>
        /// <param name="identificativoSdi"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;FatturaElettronicaImportResult&gt;</returns>
        public List<FatturaElettronicaImportResult> ApiVversionCompanyIdFatturaElettronicaXmlImportPost(string version, string companyId, string? idempotencyKey = default(string?), DateTime? dataConsegnaSdi = default(DateTime?), string? identificativoSdi = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlImportPostWithHttpInfo(version, companyId, idempotencyKey, dataConsegnaSdi, identificativoSdi, file);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa una fattura XML. Viene creato un nuovo &lt;a href&#x3D;\&quot;#tag/Documento\&quot;&gt;Documento&lt;/a&gt; corrispondente al contenuto XML.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="dataConsegnaSdi"> (optional)</param>
        /// <param name="identificativoSdi"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;FatturaElettronicaImportResult&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlImportPostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), DateTime? dataConsegnaSdi = default(DateTime?), string? identificativoSdi = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlImportPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlImportPost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "multipart/form-data"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            if (dataConsegnaSdi != null)
            {
                localVarRequestOptions.FormParameters.Add("DataConsegnaSdi", Amica.Sdk.Client.ClientUtils.ParameterToString(dataConsegnaSdi)); // form parameter
            }
            if (identificativoSdi != null)
            {
                localVarRequestOptions.FormParameters.Add("IdentificativoSdi", Amica.Sdk.Client.ClientUtils.ParameterToString(identificativoSdi)); // form parameter
            }
            if (file != null)
            {
                localVarRequestOptions.FileParameters.Add("file", file);
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlImportPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<List<FatturaElettronicaImportResult>>("/api/v{version}/{companyId}/FatturaElettronicaXml/import", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlImportPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa una fattura XML. Viene creato un nuovo &lt;a href&#x3D;\&quot;#tag/Documento\&quot;&gt;Documento&lt;/a&gt; corrispondente al contenuto XML.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="dataConsegnaSdi"> (optional)</param>
        /// <param name="identificativoSdi"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;FatturaElettronicaImportResult&gt;</returns>
        public async System.Threading.Tasks.Task<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlImportPostAsync(string version, string companyId, string? idempotencyKey = default(string?), DateTime? dataConsegnaSdi = default(DateTime?), string? identificativoSdi = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlImportPostWithHttpInfoAsync(version, companyId, idempotencyKey, dataConsegnaSdi, identificativoSdi, file, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa una fattura XML. Viene creato un nuovo &lt;a href&#x3D;\&quot;#tag/Documento\&quot;&gt;Documento&lt;/a&gt; corrispondente al contenuto XML.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="dataConsegnaSdi"> (optional)</param>
        /// <param name="identificativoSdi"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;FatturaElettronicaImportResult&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>>> ApiVversionCompanyIdFatturaElettronicaXmlImportPostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), DateTime? dataConsegnaSdi = default(DateTime?), string? identificativoSdi = default(string?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlImportPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlImportPost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "multipart/form-data"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            if (dataConsegnaSdi != null)
            {
                localVarRequestOptions.FormParameters.Add("DataConsegnaSdi", Amica.Sdk.Client.ClientUtils.ParameterToString(dataConsegnaSdi)); // form parameter
            }
            if (identificativoSdi != null)
            {
                localVarRequestOptions.FormParameters.Add("IdentificativoSdi", Amica.Sdk.Client.ClientUtils.ParameterToString(identificativoSdi)); // form parameter
            }
            if (file != null)
            {
                localVarRequestOptions.FileParameters.Add("file", file);
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlImportPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<List<FatturaElettronicaImportResult>>("/api/v{version}/{companyId}/FatturaElettronicaXml/import", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlImportPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaXml</returns>
        public FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlNewGet(string version, string companyId, int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlNewGetWithHttpInfo(version, companyId);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaXml</returns>
        public Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlNewGetWithHttpInfo(string version, string companyId, int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlNewGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlNewGet");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlNewGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Get<FatturaElettronicaXml>("/api/v{version}/{companyId}/FatturaElettronicaXml/new", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlNewGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaXml</returns>
        public async System.Threading.Tasks.Task<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlNewGetAsync(string version, string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlNewGetWithHttpInfoAsync(version, companyId, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaXml)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlNewGetWithHttpInfoAsync(string version, string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlNewGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlNewGet");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlNewGet";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.GetAsync<FatturaElettronicaXml>("/api/v{version}/{companyId}/FatturaElettronicaXml/new", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlNewGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Associa una fattura XML a un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fileName"> (optional)</param>
        /// <param name="progressivo"> (optional)</param>
        /// <param name="documentoId"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        public void ApiVversionCompanyIdFatturaElettronicaXmlPairPost(string version, string companyId, string? idempotencyKey = default(string?), string? fileName = default(string?), string? progressivo = default(string?), int? documentoId = default(int?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0)
        {
            ApiVversionCompanyIdFatturaElettronicaXmlPairPostWithHttpInfo(version, companyId, idempotencyKey, fileName, progressivo, documentoId, file);
        }

        /// <summary>
        /// Associa una fattura XML a un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fileName"> (optional)</param>
        /// <param name="progressivo"> (optional)</param>
        /// <param name="documentoId"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public Amica.Sdk.Client.ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlPairPostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), string? fileName = default(string?), string? progressivo = default(string?), int? documentoId = default(int?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlPairPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlPairPost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "multipart/form-data"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            if (fileName != null)
            {
                localVarRequestOptions.FormParameters.Add("FileName", Amica.Sdk.Client.ClientUtils.ParameterToString(fileName)); // form parameter
            }
            if (progressivo != null)
            {
                localVarRequestOptions.FormParameters.Add("Progressivo", Amica.Sdk.Client.ClientUtils.ParameterToString(progressivo)); // form parameter
            }
            if (documentoId != null)
            {
                localVarRequestOptions.FormParameters.Add("DocumentoId", Amica.Sdk.Client.ClientUtils.ParameterToString(documentoId)); // form parameter
            }
            if (file != null)
            {
                localVarRequestOptions.FileParameters.Add("file", file);
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlPairPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/pair", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlPairPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Associa una fattura XML a un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fileName"> (optional)</param>
        /// <param name="progressivo"> (optional)</param>
        /// <param name="documentoId"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlPairPostAsync(string version, string companyId, string? idempotencyKey = default(string?), string? fileName = default(string?), string? progressivo = default(string?), int? documentoId = default(int?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            await ApiVversionCompanyIdFatturaElettronicaXmlPairPostWithHttpInfoAsync(version, companyId, idempotencyKey, fileName, progressivo, documentoId, file, operationIndex, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Associa una fattura XML a un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fileName"> (optional)</param>
        /// <param name="progressivo"> (optional)</param>
        /// <param name="documentoId"> (optional)</param>
        /// <param name="file">Contenuto del file. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlPairPostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), string? fileName = default(string?), string? progressivo = default(string?), int? documentoId = default(int?), System.IO.Stream? file = default(System.IO.Stream?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlPairPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlPairPost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "multipart/form-data"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            if (fileName != null)
            {
                localVarRequestOptions.FormParameters.Add("FileName", Amica.Sdk.Client.ClientUtils.ParameterToString(fileName)); // form parameter
            }
            if (progressivo != null)
            {
                localVarRequestOptions.FormParameters.Add("Progressivo", Amica.Sdk.Client.ClientUtils.ParameterToString(progressivo)); // form parameter
            }
            if (documentoId != null)
            {
                localVarRequestOptions.FormParameters.Add("DocumentoId", Amica.Sdk.Client.ClientUtils.ParameterToString(documentoId)); // form parameter
            }
            if (file != null)
            {
                localVarRequestOptions.FileParameters.Add("file", file);
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlPairPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/pair", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlPairPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Salva un nuovo elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaXml</returns>
        public FatturaElettronicaXml ApiVversionCompanyIdFatturaElettronicaXmlPost(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlPostWithHttpInfo(version, companyId, validationOverride, idempotencyKey, fatturaElettronicaXml);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Salva un nuovo elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaXml</returns>
        public Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlPostWithHttpInfo(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlPost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json",
                "text/json",
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaXml;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<FatturaElettronicaXml>("/api/v{version}/{companyId}/FatturaElettronicaXml", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Salva un nuovo elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaXml</returns>
        public async System.Threading.Tasks.Task<FatturaElettronicaXml> ApiVversionCompanyIdFatturaElettronicaXmlPostAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlPostWithHttpInfoAsync(version, companyId, validationOverride, idempotencyKey, fatturaElettronicaXml, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Salva un nuovo elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXml">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaXml)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<FatturaElettronicaXml>> ApiVversionCompanyIdFatturaElettronicaXmlPostWithHttpInfoAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), FatturaElettronicaXml? fatturaElettronicaXml = default(FatturaElettronicaXml?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlPost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaXml;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<FatturaElettronicaXml>("/api/v{version}/{companyId}/FatturaElettronicaXml", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;FatturaElettronicaImportResult&gt;</returns>
        public List<FatturaElettronicaImportResult> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostWithHttpInfo(version, companyId, idempotencyKey);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;FatturaElettronicaImportResult&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<List<FatturaElettronicaImportResult>>("/api/v{version}/{companyId}/FatturaElettronicaXml/sdi/download/esiti", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;FatturaElettronicaImportResult&gt;</returns>
        public async System.Threading.Tasks.Task<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostAsync(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostWithHttpInfoAsync(version, companyId, idempotencyKey, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Interroga il Sistema di Interscambio (SDI) e scarica gli esiti di invio delle fatture precedentemente caricate. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;FatturaElettronicaImportResult&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<List<FatturaElettronicaImportResult>>("/api/v{version}/{companyId}/FatturaElettronicaXml/sdi/download/esiti", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadEsitiPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Scarica le nuove fatture dal Sistema di Interscambio (SDI). Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;FatturaElettronicaImportResult&gt;</returns>
        public List<FatturaElettronicaImportResult> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostWithHttpInfo(version, companyId, idempotencyKey);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Scarica le nuove fatture dal Sistema di Interscambio (SDI). Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;FatturaElettronicaImportResult&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<List<FatturaElettronicaImportResult>>("/api/v{version}/{companyId}/FatturaElettronicaXml/sdi/download/fatture", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Scarica le nuove fatture dal Sistema di Interscambio (SDI). Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;FatturaElettronicaImportResult&gt;</returns>
        public async System.Threading.Tasks.Task<List<FatturaElettronicaImportResult>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostAsync(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostWithHttpInfoAsync(version, companyId, idempotencyKey, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Scarica le nuove fatture dal Sistema di Interscambio (SDI). Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;FatturaElettronicaImportResult&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<FatturaElettronicaImportResult>>> ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<List<FatturaElettronicaImportResult>>("/api/v{version}/{companyId}/FatturaElettronicaXml/sdi/download/fatture", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlSdiDownloadFatturePost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Invia una o più fatture al Sistema di Interscambio (SDI). Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaUploadOptions">Opzioni di invio. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>FatturaElettronicaUploadResult</returns>
        public FatturaElettronicaUploadResult ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaUploadOptions? fatturaElettronicaUploadOptions = default(FatturaElettronicaUploadOptions?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaUploadResult> localVarResponse = ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostWithHttpInfo(version, companyId, idempotencyKey, fatturaElettronicaUploadOptions);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Invia una o più fatture al Sistema di Interscambio (SDI). Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaUploadOptions">Opzioni di invio. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of FatturaElettronicaUploadResult</returns>
        public Amica.Sdk.Client.ApiResponse<FatturaElettronicaUploadResult> ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaUploadOptions? fatturaElettronicaUploadOptions = default(FatturaElettronicaUploadOptions?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json",
                "text/json",
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaUploadOptions;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<FatturaElettronicaUploadResult>("/api/v{version}/{companyId}/FatturaElettronicaXml/sdi/upload", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Invia una o più fatture al Sistema di Interscambio (SDI). Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaUploadOptions">Opzioni di invio. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of FatturaElettronicaUploadResult</returns>
        public async System.Threading.Tasks.Task<FatturaElettronicaUploadResult> ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaUploadOptions? fatturaElettronicaUploadOptions = default(FatturaElettronicaUploadOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<FatturaElettronicaUploadResult> localVarResponse = await ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostWithHttpInfoAsync(version, companyId, idempotencyKey, fatturaElettronicaUploadOptions, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Invia una o più fatture al Sistema di Interscambio (SDI). Il &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/ServizioInterscambioFatture\&quot;&gt;Servizio di Interscambio Documenti&lt;/a&gt; deve essere attivo per l&#39;azienda e la relativa chiave di attivazione deve essere presente in &#x60;SdiApiKey&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaUploadOptions">Opzioni di invio. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (FatturaElettronicaUploadResult)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<FatturaElettronicaUploadResult>> ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaUploadOptions? fatturaElettronicaUploadOptions = default(FatturaElettronicaUploadOptions?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaUploadOptions;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<FatturaElettronicaUploadResult>("/api/v{version}/{companyId}/FatturaElettronicaXml/sdi/upload", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlSdiUploadPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlStyleSheetData">Opzioni di corrispondenza e stile. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns></returns>
        public void ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlStyleSheetData? fatturaElettronicaXmlStyleSheetData = default(FatturaElettronicaXmlStyleSheetData?), int operationIndex = 0)
        {
            ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostWithHttpInfo(version, companyId, idempotencyKey, fatturaElettronicaXmlStyleSheetData);
        }

        /// <summary>
        /// Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlStyleSheetData">Opzioni di corrispondenza e stile. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public Amica.Sdk.Client.ApiResponse<Object> ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostWithHttpInfo(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlStyleSheetData? fatturaElettronicaXmlStyleSheetData = default(FatturaElettronicaXmlStyleSheetData?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json",
                "text/json",
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/xml",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            var localVarMultipartFormData = localVarContentType == "multipart/form-data";
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaXmlStyleSheetData;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = this.Client.Post<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/stylesheet", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlStyleSheetData">Opzioni di corrispondenza e stile. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of void</returns>
        public async System.Threading.Tasks.Task ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlStyleSheetData? fatturaElettronicaXmlStyleSheetData = default(FatturaElettronicaXmlStyleSheetData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            await ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostWithHttpInfoAsync(version, companyId, idempotencyKey, fatturaElettronicaXmlStyleSheetData, operationIndex, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Applica un foglio di stile CSS a una fattura XML esistente e restituisce il risultato. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="fatturaElettronicaXmlStyleSheetData">Opzioni di corrispondenza e stile. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Object>> ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPostWithHttpInfoAsync(string version, string companyId, string? idempotencyKey = default(string?), FatturaElettronicaXmlStyleSheetData? fatturaElettronicaXmlStyleSheetData = default(FatturaElettronicaXmlStyleSheetData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling FatturaElettronicaXmlApi->ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/*+json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/xml",
                "application/problem+json"
            };

            var localVarContentType = Amica.Sdk.Client.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);
            }

            var localVarAccept = Amica.Sdk.Client.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);
            }

            localVarRequestOptions.PathParameters.Add("version", Amica.Sdk.Client.ClientUtils.ParameterToString(version)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = fatturaElettronicaXmlStyleSheetData;

            localVarRequestOptions.Operation = "FatturaElettronicaXmlApi.ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost";
            localVarRequestOptions.OperationIndex = operationIndex;

            // authentication (bearer) required
            // bearer authentication required
            if (!string.IsNullOrEmpty(this.Configuration.AccessToken) && !localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
            }
            // authentication (oauth2ClientCredentials) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }
            // authentication (oauth2) required
            // oauth required
            if (!localVarRequestOptions.HeaderParameters.ContainsKey("Authorization"))
            {
                if (!string.IsNullOrEmpty(this.Configuration.AccessToken))
                {
                    localVarRequestOptions.HeaderParameters.Add("Authorization", "Bearer " + this.Configuration.AccessToken);
                }
                else if (!string.IsNullOrEmpty(this.Configuration.OAuthTokenUrl) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientId) &&
                         !string.IsNullOrEmpty(this.Configuration.OAuthClientSecret) &&
                         this.Configuration.OAuthFlow != null)
                {
                    localVarRequestOptions.OAuth = true;
                }
            }

            // make the HTTP request
            var localVarResponse = await this.AsynchronousClient.PostAsync<Object>("/api/v{version}/{companyId}/FatturaElettronicaXml/stylesheet", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdFatturaElettronicaXmlStylesheetPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

    }
}

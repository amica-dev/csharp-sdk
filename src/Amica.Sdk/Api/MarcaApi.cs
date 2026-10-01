/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.1.4
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
    public interface IMarcaApiSync : IApiAccessor
    {
        #region Synchronous Operations
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
        /// <returns>List&lt;Marca&gt;</returns>
        List<Marca> ApiVversionCompanyIdMarcaGet(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0);

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
        /// <returns>ApiResponse of List&lt;Marca&gt;</returns>
        ApiResponse<List<Marca>> ApiVversionCompanyIdMarcaGetWithHttpInfo(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0);
        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Marca</returns>
        Marca ApiVversionCompanyIdMarcaIdDelete(string version, string companyId, int id, int operationIndex = 0);

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
        /// <returns>ApiResponse of Marca</returns>
        ApiResponse<Marca> ApiVversionCompanyIdMarcaIdDeleteWithHttpInfo(string version, string companyId, int id, int operationIndex = 0);
        /// <summary>
        /// Restituisce un elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand"> (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Marca</returns>
        Marca ApiVversionCompanyIdMarcaIdGet(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0);

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
        /// <returns>ApiResponse of Marca</returns>
        ApiResponse<Marca> ApiVversionCompanyIdMarcaIdGetWithHttpInfo(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0);
        /// <summary>
        /// Aggiorna un elemento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Marca</returns>
        Marca ApiVversionCompanyIdMarcaIdPut(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0);

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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Marca</returns>
        ApiResponse<Marca> ApiVversionCompanyIdMarcaIdPutWithHttpInfo(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0);
        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile).
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Marca</returns>
        Marca ApiVversionCompanyIdMarcaNewGet(string version, string companyId, int operationIndex = 0);

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
        /// <returns>ApiResponse of Marca</returns>
        ApiResponse<Marca> ApiVversionCompanyIdMarcaNewGetWithHttpInfo(string version, string companyId, int operationIndex = 0);
        /// <summary>
        /// Salva un nuovo elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Marca</returns>
        Marca ApiVversionCompanyIdMarcaPost(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0);

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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Marca</returns>
        ApiResponse<Marca> ApiVversionCompanyIdMarcaPostWithHttpInfo(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IMarcaApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
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
        /// <returns>Task of List&lt;Marca&gt;</returns>
        System.Threading.Tasks.Task<List<Marca>> ApiVversionCompanyIdMarcaGetAsync(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

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
        /// <returns>Task of ApiResponse (List&lt;Marca&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Marca>>> ApiVversionCompanyIdMarcaGetWithHttpInfoAsync(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
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
        /// <returns>Task of Marca</returns>
        System.Threading.Tasks.Task<Marca> ApiVversionCompanyIdMarcaIdDeleteAsync(string version, string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

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
        /// <returns>Task of ApiResponse (Marca)</returns>
        System.Threading.Tasks.Task<ApiResponse<Marca>> ApiVversionCompanyIdMarcaIdDeleteWithHttpInfoAsync(string version, string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
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
        /// <returns>Task of Marca</returns>
        System.Threading.Tasks.Task<Marca> ApiVversionCompanyIdMarcaIdGetAsync(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

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
        /// <returns>Task of ApiResponse (Marca)</returns>
        System.Threading.Tasks.Task<ApiResponse<Marca>> ApiVversionCompanyIdMarcaIdGetWithHttpInfoAsync(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Marca</returns>
        System.Threading.Tasks.Task<Marca> ApiVversionCompanyIdMarcaIdPutAsync(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Marca)</returns>
        System.Threading.Tasks.Task<ApiResponse<Marca>> ApiVversionCompanyIdMarcaIdPutWithHttpInfoAsync(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
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
        /// <returns>Task of Marca</returns>
        System.Threading.Tasks.Task<Marca> ApiVversionCompanyIdMarcaNewGetAsync(string version, string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

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
        /// <returns>Task of ApiResponse (Marca)</returns>
        System.Threading.Tasks.Task<ApiResponse<Marca>> ApiVversionCompanyIdMarcaNewGetWithHttpInfoAsync(string version, string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Marca</returns>
        System.Threading.Tasks.Task<Marca> ApiVversionCompanyIdMarcaPostAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Marca)</returns>
        System.Threading.Tasks.Task<ApiResponse<Marca>> ApiVversionCompanyIdMarcaPostWithHttpInfoAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IMarcaApi : IMarcaApiSync, IMarcaApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class MarcaApi : IMarcaApi
    {
        private Amica.Sdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarcaApi"/> class.
        /// </summary>
        /// <returns></returns>
        public MarcaApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarcaApi"/> class.
        /// </summary>
        /// <returns></returns>
        public MarcaApi(string basePath)
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
        /// Initializes a new instance of the <see cref="MarcaApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public MarcaApi(Amica.Sdk.Client.Configuration configuration)
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
        /// Initializes a new instance of the <see cref="MarcaApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        public MarcaApi(Amica.Sdk.Client.ISynchronousClient client, Amica.Sdk.Client.IAsynchronousClient asyncClient, Amica.Sdk.Client.IReadableConfiguration configuration)
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
        /// <returns>List&lt;Marca&gt;</returns>
        public List<Marca> ApiVversionCompanyIdMarcaGet(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<Marca>> localVarResponse = ApiVversionCompanyIdMarcaGetWithHttpInfo(version, companyId, skip, take, skipTotalCount, filter, order, expand);
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
        /// <returns>ApiResponse of List&lt;Marca&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<Marca>> ApiVversionCompanyIdMarcaGetWithHttpInfo(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaGet");
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

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaGet";
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
            var localVarResponse = this.Client.Get<List<Marca>>("/api/v{version}/{companyId}/Marca", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaGet", localVarResponse);
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
        /// <returns>Task of List&lt;Marca&gt;</returns>
        public async System.Threading.Tasks.Task<List<Marca>> ApiVversionCompanyIdMarcaGetAsync(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<Marca>> localVarResponse = await ApiVversionCompanyIdMarcaGetWithHttpInfoAsync(version, companyId, skip, take, skipTotalCount, filter, order, expand, operationIndex, cancellationToken).ConfigureAwait(false);
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
        /// <returns>Task of ApiResponse (List&lt;Marca&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<Marca>>> ApiVversionCompanyIdMarcaGetWithHttpInfoAsync(string version, string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaGet");
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

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaGet";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<List<Marca>>("/api/v{version}/{companyId}/Marca", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaGet", localVarResponse);
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
        /// <returns>Marca</returns>
        public Marca ApiVversionCompanyIdMarcaIdDelete(string version, string companyId, int id, int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Marca> localVarResponse = ApiVversionCompanyIdMarcaIdDeleteWithHttpInfo(version, companyId, id);
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
        /// <returns>ApiResponse of Marca</returns>
        public Amica.Sdk.Client.ApiResponse<Marca> ApiVversionCompanyIdMarcaIdDeleteWithHttpInfo(string version, string companyId, int id, int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaIdDelete");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaIdDelete");
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

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaIdDelete";
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
            var localVarResponse = this.Client.Delete<Marca>("/api/v{version}/{companyId}/Marca/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaIdDelete", localVarResponse);
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
        /// <returns>Task of Marca</returns>
        public async System.Threading.Tasks.Task<Marca> ApiVversionCompanyIdMarcaIdDeleteAsync(string version, string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Marca> localVarResponse = await ApiVversionCompanyIdMarcaIdDeleteWithHttpInfoAsync(version, companyId, id, operationIndex, cancellationToken).ConfigureAwait(false);
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
        /// <returns>Task of ApiResponse (Marca)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Marca>> ApiVversionCompanyIdMarcaIdDeleteWithHttpInfoAsync(string version, string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaIdDelete");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaIdDelete");
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

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaIdDelete";
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
            var localVarResponse = await this.AsynchronousClient.DeleteAsync<Marca>("/api/v{version}/{companyId}/Marca/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaIdDelete", localVarResponse);
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
        /// <returns>Marca</returns>
        public Marca ApiVversionCompanyIdMarcaIdGet(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Marca> localVarResponse = ApiVversionCompanyIdMarcaIdGetWithHttpInfo(version, companyId, id, expand);
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
        /// <returns>ApiResponse of Marca</returns>
        public Amica.Sdk.Client.ApiResponse<Marca> ApiVversionCompanyIdMarcaIdGetWithHttpInfo(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaIdGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaIdGet");
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

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaIdGet";
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
            var localVarResponse = this.Client.Get<Marca>("/api/v{version}/{companyId}/Marca/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaIdGet", localVarResponse);
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
        /// <returns>Task of Marca</returns>
        public async System.Threading.Tasks.Task<Marca> ApiVversionCompanyIdMarcaIdGetAsync(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Marca> localVarResponse = await ApiVversionCompanyIdMarcaIdGetWithHttpInfoAsync(version, companyId, id, expand, operationIndex, cancellationToken).ConfigureAwait(false);
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
        /// <returns>Task of ApiResponse (Marca)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Marca>> ApiVversionCompanyIdMarcaIdGetWithHttpInfoAsync(string version, string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaIdGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaIdGet");
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

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaIdGet";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<Marca>("/api/v{version}/{companyId}/Marca/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaIdGet", localVarResponse);
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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Marca</returns>
        public Marca ApiVversionCompanyIdMarcaIdPut(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Marca> localVarResponse = ApiVversionCompanyIdMarcaIdPutWithHttpInfo(version, companyId, id, validationOverride, idempotencyKey, marca);
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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Marca</returns>
        public Amica.Sdk.Client.ApiResponse<Marca> ApiVversionCompanyIdMarcaIdPutWithHttpInfo(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaIdPut");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaIdPut");
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
            localVarRequestOptions.Data = marca;

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaIdPut";
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
            var localVarResponse = this.Client.Put<Marca>("/api/v{version}/{companyId}/Marca/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaIdPut", localVarResponse);
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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Marca</returns>
        public async System.Threading.Tasks.Task<Marca> ApiVversionCompanyIdMarcaIdPutAsync(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Marca> localVarResponse = await ApiVversionCompanyIdMarcaIdPutWithHttpInfoAsync(version, companyId, id, validationOverride, idempotencyKey, marca, operationIndex, cancellationToken).ConfigureAwait(false);
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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Marca)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Marca>> ApiVversionCompanyIdMarcaIdPutWithHttpInfoAsync(string version, string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaIdPut");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaIdPut");
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
            localVarRequestOptions.Data = marca;

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaIdPut";
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
            var localVarResponse = await this.AsynchronousClient.PutAsync<Marca>("/api/v{version}/{companyId}/Marca/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaIdPut", localVarResponse);
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
        /// <returns>Marca</returns>
        public Marca ApiVversionCompanyIdMarcaNewGet(string version, string companyId, int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Marca> localVarResponse = ApiVversionCompanyIdMarcaNewGetWithHttpInfo(version, companyId);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="version">Versione API.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Marca</returns>
        public Amica.Sdk.Client.ApiResponse<Marca> ApiVversionCompanyIdMarcaNewGetWithHttpInfo(string version, string companyId, int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaNewGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaNewGet");
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

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaNewGet";
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
            var localVarResponse = this.Client.Get<Marca>("/api/v{version}/{companyId}/Marca/new", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaNewGet", localVarResponse);
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
        /// <returns>Task of Marca</returns>
        public async System.Threading.Tasks.Task<Marca> ApiVversionCompanyIdMarcaNewGetAsync(string version, string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Marca> localVarResponse = await ApiVversionCompanyIdMarcaNewGetWithHttpInfoAsync(version, companyId, operationIndex, cancellationToken).ConfigureAwait(false);
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
        /// <returns>Task of ApiResponse (Marca)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Marca>> ApiVversionCompanyIdMarcaNewGetWithHttpInfoAsync(string version, string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaNewGet");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaNewGet");
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

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaNewGet";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<Marca>("/api/v{version}/{companyId}/Marca/new", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaNewGet", localVarResponse);
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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Marca</returns>
        public Marca ApiVversionCompanyIdMarcaPost(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Marca> localVarResponse = ApiVversionCompanyIdMarcaPostWithHttpInfo(version, companyId, validationOverride, idempotencyKey, marca);
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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Marca</returns>
        public Amica.Sdk.Client.ApiResponse<Marca> ApiVversionCompanyIdMarcaPostWithHttpInfo(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0)
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaPost");
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
            localVarRequestOptions.Data = marca;

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaPost";
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
            var localVarResponse = this.Client.Post<Marca>("/api/v{version}/{companyId}/Marca", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaPost", localVarResponse);
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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Marca</returns>
        public async System.Threading.Tasks.Task<Marca> ApiVversionCompanyIdMarcaPostAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Marca> localVarResponse = await ApiVversionCompanyIdMarcaPostWithHttpInfoAsync(version, companyId, validationOverride, idempotencyKey, marca, operationIndex, cancellationToken).ConfigureAwait(false);
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
        /// <param name="marca">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Marca)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Marca>> ApiVversionCompanyIdMarcaPostWithHttpInfoAsync(string version, string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Marca? marca = default(Marca?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'version' is set
            if (version == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'version' when calling MarcaApi->ApiVversionCompanyIdMarcaPost");
            }

            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling MarcaApi->ApiVversionCompanyIdMarcaPost");
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
            localVarRequestOptions.Data = marca;

            localVarRequestOptions.Operation = "MarcaApi.ApiVversionCompanyIdMarcaPost";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<Marca>("/api/v{version}/{companyId}/Marca", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("ApiVversionCompanyIdMarcaPost", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

    }
}

/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.2.4
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
    public interface IDocumentoApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Calcola i totali di un documento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoComputeData">Dati di input per l&#39;operazione di calcolo. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>DocumentoComputeResult</returns>
        DocumentoComputeResult DocumentoCompute(string companyId, string? idempotencyKey = default(string?), DocumentoComputeData? documentoComputeData = default(DocumentoComputeData?), int operationIndex = 0);

        /// <summary>
        /// Calcola i totali di un documento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoComputeData">Dati di input per l&#39;operazione di calcolo. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of DocumentoComputeResult</returns>
        ApiResponse<DocumentoComputeResult> DocumentoComputeWithHttpInfo(string companyId, string? idempotencyKey = default(string?), DocumentoComputeData? documentoComputeData = default(DocumentoComputeData?), int operationIndex = 0);
        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoDelete(string companyId, int id, bool? validationOverride = default(bool?), int operationIndex = 0);

        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoDeleteWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), int operationIndex = 0);
        /// <summary>
        /// Restituisce una lista di elementi distinti.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="selector">Il campo usato per distinguere gli elementi.</param>
        /// <param name="skip">Numero di elementi, a partire dal primo, da escludere dal risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;DistinctPropertyGroup&gt;</returns>
        List<DistinctPropertyGroup> DocumentoDistinct(string companyId, string selector, int? skip = default(int?), int? take = default(int?), int operationIndex = 0);

        /// <summary>
        /// Restituisce una lista di elementi distinti.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="selector">Il campo usato per distinguere gli elementi.</param>
        /// <param name="skip">Numero di elementi, a partire dal primo, da escludere dal risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;DistinctPropertyGroup&gt;</returns>
        ApiResponse<List<DistinctPropertyGroup>> DocumentoDistinctWithHttpInfo(string companyId, string selector, int? skip = default(int?), int? take = default(int?), int operationIndex = 0);
        /// <summary>
        /// Emette un documento.
        /// </summary>
        /// <remarks>
        /// A seconda del tipo di documento, l&#39;emissione comporta una serie di operazioni amministrative e contabili:               1) Aggiornamento dell&#39;&#x60;Anagrafica&#x60; coinvolta (fatturato, numero fatture, ecc.);              2) Creazione della registrazione contabile di emissione;              3) Creazione delle scadenze attive o passive, in base al tipo di pagamento;              4) Eventuale autoproduzione dei beni fatturati tramite distinta base, se l&#39;opzione è attiva;              5) Creazione del movimento di magazzino;              6) Aggiornamento delle giacenze dei beni coinvolti, nei rispettivi magazzini;              7) Aggiornamento di eventuali ordini collegati.               Un documento già emesso non può essere riemesso. Alcuni tipi di documento non supportano l&#39;emissione (ordini e preventivi, ad esempio).
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="documentoId">ID univoco del documento da emettere.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoEmit(string companyId, int documentoId, string? idempotencyKey = default(string?), int operationIndex = 0);

        /// <summary>
        /// Emette un documento.
        /// </summary>
        /// <remarks>
        /// A seconda del tipo di documento, l&#39;emissione comporta una serie di operazioni amministrative e contabili:               1) Aggiornamento dell&#39;&#x60;Anagrafica&#x60; coinvolta (fatturato, numero fatture, ecc.);              2) Creazione della registrazione contabile di emissione;              3) Creazione delle scadenze attive o passive, in base al tipo di pagamento;              4) Eventuale autoproduzione dei beni fatturati tramite distinta base, se l&#39;opzione è attiva;              5) Creazione del movimento di magazzino;              6) Aggiornamento delle giacenze dei beni coinvolti, nei rispettivi magazzini;              7) Aggiornamento di eventuali ordini collegati.               Un documento già emesso non può essere riemesso. Alcuni tipi di documento non supportano l&#39;emissione (ordini e preventivi, ad esempio).
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="documentoId">ID univoco del documento da emettere.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoEmitWithHttpInfo(string companyId, int documentoId, string? idempotencyKey = default(string?), int operationIndex = 0);
        /// <summary>
        /// Restituisce un elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoGet(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0);

        /// <summary>
        /// Restituisce un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoGetWithHttpInfo(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0);
        /// <summary>
        /// Importa righe da uno o più acconti in un documento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID degli acconti da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoImportAcconti(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0);

        /// <summary>
        /// Importa righe da uno o più acconti in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID degli acconti da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoImportAccontiWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0);
        /// <summary>
        /// Importa righe da uno o più DDT in un documento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ddtImportData">Opzioni di importazione, inclusi gli ID dei DDT da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoImportDdt(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DdtImportData? ddtImportData = default(DdtImportData?), int operationIndex = 0);

        /// <summary>
        /// Importa righe da uno o più DDT in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ddtImportData">Opzioni di importazione, inclusi gli ID dei DDT da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoImportDdtWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DdtImportData? ddtImportData = default(DdtImportData?), int operationIndex = 0);
        /// <summary>
        /// Importa righe da uno o più ordini in un documento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ordiniImportData">Opzioni di importazione, inclusi gli ID degli ordini da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoImportOrdini(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), OrdiniImportData? ordiniImportData = default(OrdiniImportData?), int operationIndex = 0);

        /// <summary>
        /// Importa righe da uno o più ordini in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ordiniImportData">Opzioni di importazione, inclusi gli ID degli ordini da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoImportOrdiniWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), OrdiniImportData? ordiniImportData = default(OrdiniImportData?), int operationIndex = 0);
        /// <summary>
        /// Importa righe da uno o più preventivi in un documento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID dei preventivi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoImportPreventivi(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0);

        /// <summary>
        /// Importa righe da uno o più preventivi in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID dei preventivi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoImportPreventiviWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0);
        /// <summary>
        /// Importa righe da una o più fatture proforma in un documento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID delle proforma da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoImportProForma(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0);

        /// <summary>
        /// Importa righe da una o più fatture proforma in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID delle proforma da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoImportProFormaWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0);
        /// <summary>
        /// Importa righe da uno o più resi in un documento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="resoImportData">Opzioni di importazione, inclusi gli ID dei resi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoImportResi(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), ResoImportData? resoImportData = default(ResoImportData?), int operationIndex = 0);

        /// <summary>
        /// Importa righe da uno o più resi in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="resoImportData">Opzioni di importazione, inclusi gli ID dei resi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoImportResiWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), ResoImportData? resoImportData = default(ResoImportData?), int operationIndex = 0);
        /// <summary>
        /// Restituisce una lista di elementi.
        /// </summary>
        /// <remarks>
        ///  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip">Numero di elementi da escludere dall&#39;inizio del risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="skipTotalCount">Indica se l&#39;header &#x60;X-Read-TotalCount&#x60; debba essere escluso dalla risposta. Impostare a &#x60;true&#x60; per prestazioni migliori a scapito dell&#39;accuratezza. Il valore predefinito è &#x60;false&#x60;. (optional)</param>
        /// <param name="filter">Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: &#x60;$eq&#x60; (uguale), &#x60;$ne&#x60; (diverso), &#x60;$gt&#x60; (maggiore di), &#x60;$lt&#x60; (minore di), &#x60;$lte&#x60; (minore o uguale), &#x60;$gte&#x60; (maggiore o uguale), &#x60;$ct&#x60; (contiene), &#x60;$nct&#x60; (non contiene), &#x60;$sw&#x60; (inizia con), &#x60;$ew&#x60; (finisce con). Più espressioni possono essere concatenate con un connettore logico &#x60;$and&#x60; o &#x60;$or&#x60; (&#x60;$and&#x60; può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (&#x60;[]&#x60;). Nell&#39;esempio vengono inclusi tutti gli elementi con &#x60;field1&#x3D;\&quot;hello\&quot;&#x60; oppure &#x60;field2&gt;10&#x60;. (optional)</param>
        /// <param name="order">Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell&#39;esempio i risultati sono ordinati prima per &#x60;field1&#x60; (discendente), poi per &#x60;field2&#x60;. (optional)</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;Documento&gt;</returns>
        List<Documento> DocumentoList(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0);

        /// <summary>
        /// Restituisce una lista di elementi.
        /// </summary>
        /// <remarks>
        ///  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip">Numero di elementi da escludere dall&#39;inizio del risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="skipTotalCount">Indica se l&#39;header &#x60;X-Read-TotalCount&#x60; debba essere escluso dalla risposta. Impostare a &#x60;true&#x60; per prestazioni migliori a scapito dell&#39;accuratezza. Il valore predefinito è &#x60;false&#x60;. (optional)</param>
        /// <param name="filter">Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: &#x60;$eq&#x60; (uguale), &#x60;$ne&#x60; (diverso), &#x60;$gt&#x60; (maggiore di), &#x60;$lt&#x60; (minore di), &#x60;$lte&#x60; (minore o uguale), &#x60;$gte&#x60; (maggiore o uguale), &#x60;$ct&#x60; (contiene), &#x60;$nct&#x60; (non contiene), &#x60;$sw&#x60; (inizia con), &#x60;$ew&#x60; (finisce con). Più espressioni possono essere concatenate con un connettore logico &#x60;$and&#x60; o &#x60;$or&#x60; (&#x60;$and&#x60; può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (&#x60;[]&#x60;). Nell&#39;esempio vengono inclusi tutti gli elementi con &#x60;field1&#x3D;\&quot;hello\&quot;&#x60; oppure &#x60;field2&gt;10&#x60;. (optional)</param>
        /// <param name="order">Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell&#39;esempio i risultati sono ordinati prima per &#x60;field1&#x60; (discendente), poi per &#x60;field2&#x60;. (optional)</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;Documento&gt;</returns>
        ApiResponse<List<Documento>> DocumentoListWithHttpInfo(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0);
        /// <summary>
        /// Restituisce una nuova istanza di un documento del tipo specificato.
        /// </summary>
        /// <remarks>
        /// Richiede il parametro query &#x60;TipoDocumento&#x60; (int) che indica il tipo di documento da creare.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoNewInstance(string companyId, int operationIndex = 0);

        /// <summary>
        /// Restituisce una nuova istanza di un documento del tipo specificato.
        /// </summary>
        /// <remarks>
        /// Richiede il parametro query &#x60;TipoDocumento&#x60; (int) che indica il tipo di documento da creare.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoNewInstanceWithHttpInfo(string companyId, int operationIndex = 0);
        /// <summary>
        /// Replica un documento. Crea una copia con la data odierna e, se è un documento di vendita, un nuovo numero. Alcuni campi vengono reimpostati ai valori predefiniti (stato del documento, stato SDI, stato del pagamento, ecc.).
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="documentoId">ID univoco del documento.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoNewInstanceFromTemplate(int documentoId, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0);

        /// <summary>
        /// Replica un documento. Crea una copia con la data odierna e, se è un documento di vendita, un nuovo numero. Alcuni campi vengono reimpostati ai valori predefiniti (stato del documento, stato SDI, stato del pagamento, ecc.).
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="documentoId">ID univoco del documento.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoNewInstanceFromTemplateWithHttpInfo(int documentoId, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0);
        /// <summary>
        /// Genera fatture per tutti i contatti che corrispondono ai criteri di fatturazione periodica.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">Non utilizzato. (optional)</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="periodicInvoicingData">Criteri e opzioni di fatturazione periodica. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;Documento&gt;</returns>
        List<Documento> DocumentoPeriodicInvoicing(string companyId, int? id = default(int?), bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), PeriodicInvoicingData? periodicInvoicingData = default(PeriodicInvoicingData?), int operationIndex = 0);

        /// <summary>
        /// Genera fatture per tutti i contatti che corrispondono ai criteri di fatturazione periodica.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">Non utilizzato. (optional)</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="periodicInvoicingData">Criteri e opzioni di fatturazione periodica. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;Documento&gt;</returns>
        ApiResponse<List<Documento>> DocumentoPeriodicInvoicingWithHttpInfo(string companyId, int? id = default(int?), bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), PeriodicInvoicingData? periodicInvoicingData = default(PeriodicInvoicingData?), int operationIndex = 0);
        /// <summary>
        /// Salva un nuovo elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoPost(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0);

        /// <summary>
        /// Salva un nuovo elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoPostWithHttpInfo(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0);
        /// <summary>
        /// Aggiorna un elemento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        Documento DocumentoPut(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0);

        /// <summary>
        /// Aggiorna un elemento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        ApiResponse<Documento> DocumentoPutWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0);
        /// <summary>
        /// Restituisce i totali dei documenti che soddisfano il filtro, eventualmente raggruppati per mese, anno o anagrafica. I totali sono calcolati dal server: per conoscere il fatturato di un periodo non serve scaricare i documenti.
        /// </summary>
        /// <remarks>
        ///  Le note di credito, clienti e fornitori, entrano nei totali con segno negativo.  Senza raggruppamento la risposta contiene un solo gruppo, anche se nessun documento soddisfa il filtro.             Raggruppando per mese o per anno i gruppi sono in ordine cronologico e &#x60;Periodo&#x60; ne indica il primo giorno;             raggruppando per anagrafica sono in ordine di totale documento decrescente, con &#x60;AnagraficaId&#x60; e             &#x60;RagioneSociale&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="filter">Condizioni di ricerca che selezionano i documenti da sommare, con la stessa sintassi del parametro &#x60;Filter&#x60; dell&#39;elenco dei documenti. Senza filtro si sommano tutti i documenti dell&#39;azienda, compresi ordini e preventivi: per il fatturato, filtrare per classe e tipo di documento. (optional)</param>
        /// <param name="raggruppa">Come raggruppare i totali: &#x60;Nessuno&#x60; (un solo gruppo, predefinito), &#x60;Mese&#x60;, &#x60;Anno&#x60; o &#x60;Anagrafica&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;DocumentoTotali&gt;</returns>
        List<DocumentoTotali> DocumentoTotali(string companyId, string? filter = default(string?), string? raggruppa = default(string?), int operationIndex = 0);

        /// <summary>
        /// Restituisce i totali dei documenti che soddisfano il filtro, eventualmente raggruppati per mese, anno o anagrafica. I totali sono calcolati dal server: per conoscere il fatturato di un periodo non serve scaricare i documenti.
        /// </summary>
        /// <remarks>
        ///  Le note di credito, clienti e fornitori, entrano nei totali con segno negativo.  Senza raggruppamento la risposta contiene un solo gruppo, anche se nessun documento soddisfa il filtro.             Raggruppando per mese o per anno i gruppi sono in ordine cronologico e &#x60;Periodo&#x60; ne indica il primo giorno;             raggruppando per anagrafica sono in ordine di totale documento decrescente, con &#x60;AnagraficaId&#x60; e             &#x60;RagioneSociale&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="filter">Condizioni di ricerca che selezionano i documenti da sommare, con la stessa sintassi del parametro &#x60;Filter&#x60; dell&#39;elenco dei documenti. Senza filtro si sommano tutti i documenti dell&#39;azienda, compresi ordini e preventivi: per il fatturato, filtrare per classe e tipo di documento. (optional)</param>
        /// <param name="raggruppa">Come raggruppare i totali: &#x60;Nessuno&#x60; (un solo gruppo, predefinito), &#x60;Mese&#x60;, &#x60;Anno&#x60; o &#x60;Anagrafica&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;DocumentoTotali&gt;</returns>
        ApiResponse<List<DocumentoTotali>> DocumentoTotaliWithHttpInfo(string companyId, string? filter = default(string?), string? raggruppa = default(string?), int operationIndex = 0);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IDocumentoApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Calcola i totali di un documento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoComputeData">Dati di input per l&#39;operazione di calcolo. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of DocumentoComputeResult</returns>
        System.Threading.Tasks.Task<DocumentoComputeResult> DocumentoComputeAsync(string companyId, string? idempotencyKey = default(string?), DocumentoComputeData? documentoComputeData = default(DocumentoComputeData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Calcola i totali di un documento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoComputeData">Dati di input per l&#39;operazione di calcolo. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (DocumentoComputeResult)</returns>
        System.Threading.Tasks.Task<ApiResponse<DocumentoComputeResult>> DocumentoComputeWithHttpInfoAsync(string companyId, string? idempotencyKey = default(string?), DocumentoComputeData? documentoComputeData = default(DocumentoComputeData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoDeleteAsync(string companyId, int id, bool? validationOverride = default(bool?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoDeleteWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Restituisce una lista di elementi distinti.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="selector">Il campo usato per distinguere gli elementi.</param>
        /// <param name="skip">Numero di elementi, a partire dal primo, da escludere dal risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;DistinctPropertyGroup&gt;</returns>
        System.Threading.Tasks.Task<List<DistinctPropertyGroup>> DocumentoDistinctAsync(string companyId, string selector, int? skip = default(int?), int? take = default(int?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Restituisce una lista di elementi distinti.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="selector">Il campo usato per distinguere gli elementi.</param>
        /// <param name="skip">Numero di elementi, a partire dal primo, da escludere dal risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;DistinctPropertyGroup&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<DistinctPropertyGroup>>> DocumentoDistinctWithHttpInfoAsync(string companyId, string selector, int? skip = default(int?), int? take = default(int?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Emette un documento.
        /// </summary>
        /// <remarks>
        /// A seconda del tipo di documento, l&#39;emissione comporta una serie di operazioni amministrative e contabili:               1) Aggiornamento dell&#39;&#x60;Anagrafica&#x60; coinvolta (fatturato, numero fatture, ecc.);              2) Creazione della registrazione contabile di emissione;              3) Creazione delle scadenze attive o passive, in base al tipo di pagamento;              4) Eventuale autoproduzione dei beni fatturati tramite distinta base, se l&#39;opzione è attiva;              5) Creazione del movimento di magazzino;              6) Aggiornamento delle giacenze dei beni coinvolti, nei rispettivi magazzini;              7) Aggiornamento di eventuali ordini collegati.               Un documento già emesso non può essere riemesso. Alcuni tipi di documento non supportano l&#39;emissione (ordini e preventivi, ad esempio).
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="documentoId">ID univoco del documento da emettere.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoEmitAsync(string companyId, int documentoId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Emette un documento.
        /// </summary>
        /// <remarks>
        /// A seconda del tipo di documento, l&#39;emissione comporta una serie di operazioni amministrative e contabili:               1) Aggiornamento dell&#39;&#x60;Anagrafica&#x60; coinvolta (fatturato, numero fatture, ecc.);              2) Creazione della registrazione contabile di emissione;              3) Creazione delle scadenze attive o passive, in base al tipo di pagamento;              4) Eventuale autoproduzione dei beni fatturati tramite distinta base, se l&#39;opzione è attiva;              5) Creazione del movimento di magazzino;              6) Aggiornamento delle giacenze dei beni coinvolti, nei rispettivi magazzini;              7) Aggiornamento di eventuali ordini collegati.               Un documento già emesso non può essere riemesso. Alcuni tipi di documento non supportano l&#39;emissione (ordini e preventivi, ad esempio).
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="documentoId">ID univoco del documento da emettere.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoEmitWithHttpInfoAsync(string companyId, int documentoId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Restituisce un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoGetAsync(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Restituisce un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoGetWithHttpInfoAsync(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Importa righe da uno o più acconti in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID degli acconti da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoImportAccontiAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Importa righe da uno o più acconti in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID degli acconti da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoImportAccontiWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Importa righe da uno o più DDT in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ddtImportData">Opzioni di importazione, inclusi gli ID dei DDT da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoImportDdtAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DdtImportData? ddtImportData = default(DdtImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Importa righe da uno o più DDT in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ddtImportData">Opzioni di importazione, inclusi gli ID dei DDT da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoImportDdtWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DdtImportData? ddtImportData = default(DdtImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Importa righe da uno o più ordini in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ordiniImportData">Opzioni di importazione, inclusi gli ID degli ordini da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoImportOrdiniAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), OrdiniImportData? ordiniImportData = default(OrdiniImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Importa righe da uno o più ordini in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ordiniImportData">Opzioni di importazione, inclusi gli ID degli ordini da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoImportOrdiniWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), OrdiniImportData? ordiniImportData = default(OrdiniImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Importa righe da uno o più preventivi in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID dei preventivi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoImportPreventiviAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Importa righe da uno o più preventivi in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID dei preventivi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoImportPreventiviWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Importa righe da una o più fatture proforma in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID delle proforma da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoImportProFormaAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Importa righe da una o più fatture proforma in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID delle proforma da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoImportProFormaWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Importa righe da uno o più resi in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="resoImportData">Opzioni di importazione, inclusi gli ID dei resi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoImportResiAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), ResoImportData? resoImportData = default(ResoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Importa righe da uno o più resi in un documento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="resoImportData">Opzioni di importazione, inclusi gli ID dei resi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoImportResiWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), ResoImportData? resoImportData = default(ResoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Restituisce una lista di elementi.
        /// </summary>
        /// <remarks>
        ///  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip">Numero di elementi da escludere dall&#39;inizio del risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="skipTotalCount">Indica se l&#39;header &#x60;X-Read-TotalCount&#x60; debba essere escluso dalla risposta. Impostare a &#x60;true&#x60; per prestazioni migliori a scapito dell&#39;accuratezza. Il valore predefinito è &#x60;false&#x60;. (optional)</param>
        /// <param name="filter">Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: &#x60;$eq&#x60; (uguale), &#x60;$ne&#x60; (diverso), &#x60;$gt&#x60; (maggiore di), &#x60;$lt&#x60; (minore di), &#x60;$lte&#x60; (minore o uguale), &#x60;$gte&#x60; (maggiore o uguale), &#x60;$ct&#x60; (contiene), &#x60;$nct&#x60; (non contiene), &#x60;$sw&#x60; (inizia con), &#x60;$ew&#x60; (finisce con). Più espressioni possono essere concatenate con un connettore logico &#x60;$and&#x60; o &#x60;$or&#x60; (&#x60;$and&#x60; può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (&#x60;[]&#x60;). Nell&#39;esempio vengono inclusi tutti gli elementi con &#x60;field1&#x3D;\&quot;hello\&quot;&#x60; oppure &#x60;field2&gt;10&#x60;. (optional)</param>
        /// <param name="order">Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell&#39;esempio i risultati sono ordinati prima per &#x60;field1&#x60; (discendente), poi per &#x60;field2&#x60;. (optional)</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;Documento&gt;</returns>
        System.Threading.Tasks.Task<List<Documento>> DocumentoListAsync(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Restituisce una lista di elementi.
        /// </summary>
        /// <remarks>
        ///  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip">Numero di elementi da escludere dall&#39;inizio del risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="skipTotalCount">Indica se l&#39;header &#x60;X-Read-TotalCount&#x60; debba essere escluso dalla risposta. Impostare a &#x60;true&#x60; per prestazioni migliori a scapito dell&#39;accuratezza. Il valore predefinito è &#x60;false&#x60;. (optional)</param>
        /// <param name="filter">Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: &#x60;$eq&#x60; (uguale), &#x60;$ne&#x60; (diverso), &#x60;$gt&#x60; (maggiore di), &#x60;$lt&#x60; (minore di), &#x60;$lte&#x60; (minore o uguale), &#x60;$gte&#x60; (maggiore o uguale), &#x60;$ct&#x60; (contiene), &#x60;$nct&#x60; (non contiene), &#x60;$sw&#x60; (inizia con), &#x60;$ew&#x60; (finisce con). Più espressioni possono essere concatenate con un connettore logico &#x60;$and&#x60; o &#x60;$or&#x60; (&#x60;$and&#x60; può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (&#x60;[]&#x60;). Nell&#39;esempio vengono inclusi tutti gli elementi con &#x60;field1&#x3D;\&quot;hello\&quot;&#x60; oppure &#x60;field2&gt;10&#x60;. (optional)</param>
        /// <param name="order">Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell&#39;esempio i risultati sono ordinati prima per &#x60;field1&#x60; (discendente), poi per &#x60;field2&#x60;. (optional)</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;Documento&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Documento>>> DocumentoListWithHttpInfoAsync(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Restituisce una nuova istanza di un documento del tipo specificato.
        /// </summary>
        /// <remarks>
        /// Richiede il parametro query &#x60;TipoDocumento&#x60; (int) che indica il tipo di documento da creare.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoNewInstanceAsync(string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Restituisce una nuova istanza di un documento del tipo specificato.
        /// </summary>
        /// <remarks>
        /// Richiede il parametro query &#x60;TipoDocumento&#x60; (int) che indica il tipo di documento da creare.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoNewInstanceWithHttpInfoAsync(string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Replica un documento. Crea una copia con la data odierna e, se è un documento di vendita, un nuovo numero. Alcuni campi vengono reimpostati ai valori predefiniti (stato del documento, stato SDI, stato del pagamento, ecc.).
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="documentoId">ID univoco del documento.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoNewInstanceFromTemplateAsync(int documentoId, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Replica un documento. Crea una copia con la data odierna e, se è un documento di vendita, un nuovo numero. Alcuni campi vengono reimpostati ai valori predefiniti (stato del documento, stato SDI, stato del pagamento, ecc.).
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="documentoId">ID univoco del documento.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoNewInstanceFromTemplateWithHttpInfoAsync(int documentoId, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Genera fatture per tutti i contatti che corrispondono ai criteri di fatturazione periodica.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">Non utilizzato. (optional)</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="periodicInvoicingData">Criteri e opzioni di fatturazione periodica. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;Documento&gt;</returns>
        System.Threading.Tasks.Task<List<Documento>> DocumentoPeriodicInvoicingAsync(string companyId, int? id = default(int?), bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), PeriodicInvoicingData? periodicInvoicingData = default(PeriodicInvoicingData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Genera fatture per tutti i contatti che corrispondono ai criteri di fatturazione periodica.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">Non utilizzato. (optional)</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="periodicInvoicingData">Criteri e opzioni di fatturazione periodica. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;Documento&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<Documento>>> DocumentoPeriodicInvoicingWithHttpInfoAsync(string companyId, int? id = default(int?), bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), PeriodicInvoicingData? periodicInvoicingData = default(PeriodicInvoicingData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Salva un nuovo elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoPostAsync(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Salva un nuovo elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoPostWithHttpInfoAsync(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Aggiorna un elemento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        System.Threading.Tasks.Task<Documento> DocumentoPutAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Aggiorna un elemento esistente.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        System.Threading.Tasks.Task<ApiResponse<Documento>> DocumentoPutWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Restituisce i totali dei documenti che soddisfano il filtro, eventualmente raggruppati per mese, anno o anagrafica. I totali sono calcolati dal server: per conoscere il fatturato di un periodo non serve scaricare i documenti.
        /// </summary>
        /// <remarks>
        ///  Le note di credito, clienti e fornitori, entrano nei totali con segno negativo.  Senza raggruppamento la risposta contiene un solo gruppo, anche se nessun documento soddisfa il filtro.             Raggruppando per mese o per anno i gruppi sono in ordine cronologico e &#x60;Periodo&#x60; ne indica il primo giorno;             raggruppando per anagrafica sono in ordine di totale documento decrescente, con &#x60;AnagraficaId&#x60; e             &#x60;RagioneSociale&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="filter">Condizioni di ricerca che selezionano i documenti da sommare, con la stessa sintassi del parametro &#x60;Filter&#x60; dell&#39;elenco dei documenti. Senza filtro si sommano tutti i documenti dell&#39;azienda, compresi ordini e preventivi: per il fatturato, filtrare per classe e tipo di documento. (optional)</param>
        /// <param name="raggruppa">Come raggruppare i totali: &#x60;Nessuno&#x60; (un solo gruppo, predefinito), &#x60;Mese&#x60;, &#x60;Anno&#x60; o &#x60;Anagrafica&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;DocumentoTotali&gt;</returns>
        System.Threading.Tasks.Task<List<DocumentoTotali>> DocumentoTotaliAsync(string companyId, string? filter = default(string?), string? raggruppa = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Restituisce i totali dei documenti che soddisfano il filtro, eventualmente raggruppati per mese, anno o anagrafica. I totali sono calcolati dal server: per conoscere il fatturato di un periodo non serve scaricare i documenti.
        /// </summary>
        /// <remarks>
        ///  Le note di credito, clienti e fornitori, entrano nei totali con segno negativo.  Senza raggruppamento la risposta contiene un solo gruppo, anche se nessun documento soddisfa il filtro.             Raggruppando per mese o per anno i gruppi sono in ordine cronologico e &#x60;Periodo&#x60; ne indica il primo giorno;             raggruppando per anagrafica sono in ordine di totale documento decrescente, con &#x60;AnagraficaId&#x60; e             &#x60;RagioneSociale&#x60;.
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="filter">Condizioni di ricerca che selezionano i documenti da sommare, con la stessa sintassi del parametro &#x60;Filter&#x60; dell&#39;elenco dei documenti. Senza filtro si sommano tutti i documenti dell&#39;azienda, compresi ordini e preventivi: per il fatturato, filtrare per classe e tipo di documento. (optional)</param>
        /// <param name="raggruppa">Come raggruppare i totali: &#x60;Nessuno&#x60; (un solo gruppo, predefinito), &#x60;Mese&#x60;, &#x60;Anno&#x60; o &#x60;Anagrafica&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;DocumentoTotali&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<DocumentoTotali>>> DocumentoTotaliWithHttpInfoAsync(string companyId, string? filter = default(string?), string? raggruppa = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IDocumentoApi : IDocumentoApiSync, IDocumentoApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class DocumentoApi : IDocumentoApi
    {
        private Amica.Sdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentoApi"/> class.
        /// </summary>
        /// <returns></returns>
        public DocumentoApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentoApi"/> class.
        /// </summary>
        /// <returns></returns>
        public DocumentoApi(string basePath)
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
        /// Initializes a new instance of the <see cref="DocumentoApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public DocumentoApi(Amica.Sdk.Client.Configuration configuration)
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
        /// Initializes a new instance of the <see cref="DocumentoApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        public DocumentoApi(Amica.Sdk.Client.ISynchronousClient client, Amica.Sdk.Client.IAsynchronousClient asyncClient, Amica.Sdk.Client.IReadableConfiguration configuration)
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
        /// Calcola i totali di un documento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoComputeData">Dati di input per l&#39;operazione di calcolo. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>DocumentoComputeResult</returns>
        public DocumentoComputeResult DocumentoCompute(string companyId, string? idempotencyKey = default(string?), DocumentoComputeData? documentoComputeData = default(DocumentoComputeData?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<DocumentoComputeResult> localVarResponse = DocumentoComputeWithHttpInfo(companyId, idempotencyKey, documentoComputeData);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Calcola i totali di un documento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoComputeData">Dati di input per l&#39;operazione di calcolo. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of DocumentoComputeResult</returns>
        public Amica.Sdk.Client.ApiResponse<DocumentoComputeResult> DocumentoComputeWithHttpInfo(string companyId, string? idempotencyKey = default(string?), DocumentoComputeData? documentoComputeData = default(DocumentoComputeData?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoCompute");
            }

            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json",
                "text/json",
                "application/*+json"
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
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = documentoComputeData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoCompute";
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
            var localVarResponse = this.Client.Post<DocumentoComputeResult>("/api/v3/{companyId}/Documento/compute", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoCompute", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Calcola i totali di un documento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoComputeData">Dati di input per l&#39;operazione di calcolo. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of DocumentoComputeResult</returns>
        public async System.Threading.Tasks.Task<DocumentoComputeResult> DocumentoComputeAsync(string companyId, string? idempotencyKey = default(string?), DocumentoComputeData? documentoComputeData = default(DocumentoComputeData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<DocumentoComputeResult> localVarResponse = await DocumentoComputeWithHttpInfoAsync(companyId, idempotencyKey, documentoComputeData, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Calcola i totali di un documento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoComputeData">Dati di input per l&#39;operazione di calcolo. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (DocumentoComputeResult)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<DocumentoComputeResult>> DocumentoComputeWithHttpInfoAsync(string companyId, string? idempotencyKey = default(string?), DocumentoComputeData? documentoComputeData = default(DocumentoComputeData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoCompute");
            }


            Amica.Sdk.Client.RequestOptions localVarRequestOptions = new Amica.Sdk.Client.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json", 
                "text/json", 
                "application/*+json"
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
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = documentoComputeData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoCompute";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<DocumentoComputeResult>("/api/v3/{companyId}/Documento/compute", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoCompute", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoDelete(string companyId, int id, bool? validationOverride = default(bool?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoDeleteWithHttpInfo(companyId, id, validationOverride);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Elimina un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoDeleteWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoDelete");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("id", Amica.Sdk.Client.ClientUtils.ParameterToString(id)); // path parameter
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoDelete";
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
            var localVarResponse = this.Client.Delete<Documento>("/api/v3/{companyId}/Documento/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoDelete", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoDeleteAsync(string companyId, int id, bool? validationOverride = default(bool?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoDeleteWithHttpInfoAsync(companyId, id, validationOverride, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Elimina un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoDeleteWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoDelete");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("id", Amica.Sdk.Client.ClientUtils.ParameterToString(id)); // path parameter
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoDelete";
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
            var localVarResponse = await this.AsynchronousClient.DeleteAsync<Documento>("/api/v3/{companyId}/Documento/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoDelete", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce una lista di elementi distinti. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="selector">Il campo usato per distinguere gli elementi.</param>
        /// <param name="skip">Numero di elementi, a partire dal primo, da escludere dal risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;DistinctPropertyGroup&gt;</returns>
        public List<DistinctPropertyGroup> DocumentoDistinct(string companyId, string selector, int? skip = default(int?), int? take = default(int?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<DistinctPropertyGroup>> localVarResponse = DocumentoDistinctWithHttpInfo(companyId, selector, skip, take);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una lista di elementi distinti. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="selector">Il campo usato per distinguere gli elementi.</param>
        /// <param name="skip">Numero di elementi, a partire dal primo, da escludere dal risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;DistinctPropertyGroup&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<DistinctPropertyGroup>> DocumentoDistinctWithHttpInfo(string companyId, string selector, int? skip = default(int?), int? take = default(int?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoDistinct");
            }

            // verify the required parameter 'selector' is set
            if (selector == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'selector' when calling DocumentoApi->DocumentoDistinct");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Selector", selector));
            if (skip != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Skip", skip));
            }
            if (take != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Take", take));
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoDistinct";
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
            var localVarResponse = this.Client.Get<List<DistinctPropertyGroup>>("/api/v3/{companyId}/Documento/distinct", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoDistinct", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce una lista di elementi distinti. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="selector">Il campo usato per distinguere gli elementi.</param>
        /// <param name="skip">Numero di elementi, a partire dal primo, da escludere dal risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;DistinctPropertyGroup&gt;</returns>
        public async System.Threading.Tasks.Task<List<DistinctPropertyGroup>> DocumentoDistinctAsync(string companyId, string selector, int? skip = default(int?), int? take = default(int?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<DistinctPropertyGroup>> localVarResponse = await DocumentoDistinctWithHttpInfoAsync(companyId, selector, skip, take, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una lista di elementi distinti. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="selector">Il campo usato per distinguere gli elementi.</param>
        /// <param name="skip">Numero di elementi, a partire dal primo, da escludere dal risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;DistinctPropertyGroup&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<DistinctPropertyGroup>>> DocumentoDistinctWithHttpInfoAsync(string companyId, string selector, int? skip = default(int?), int? take = default(int?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoDistinct");
            }

            // verify the required parameter 'selector' is set
            if (selector == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'selector' when calling DocumentoApi->DocumentoDistinct");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Selector", selector));
            if (skip != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Skip", skip));
            }
            if (take != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Take", take));
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoDistinct";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<List<DistinctPropertyGroup>>("/api/v3/{companyId}/Documento/distinct", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoDistinct", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Emette un documento. A seconda del tipo di documento, l&#39;emissione comporta una serie di operazioni amministrative e contabili:               1) Aggiornamento dell&#39;&#x60;Anagrafica&#x60; coinvolta (fatturato, numero fatture, ecc.);              2) Creazione della registrazione contabile di emissione;              3) Creazione delle scadenze attive o passive, in base al tipo di pagamento;              4) Eventuale autoproduzione dei beni fatturati tramite distinta base, se l&#39;opzione è attiva;              5) Creazione del movimento di magazzino;              6) Aggiornamento delle giacenze dei beni coinvolti, nei rispettivi magazzini;              7) Aggiornamento di eventuali ordini collegati.               Un documento già emesso non può essere riemesso. Alcuni tipi di documento non supportano l&#39;emissione (ordini e preventivi, ad esempio).
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="documentoId">ID univoco del documento da emettere.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoEmit(string companyId, int documentoId, string? idempotencyKey = default(string?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoEmitWithHttpInfo(companyId, documentoId, idempotencyKey);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Emette un documento. A seconda del tipo di documento, l&#39;emissione comporta una serie di operazioni amministrative e contabili:               1) Aggiornamento dell&#39;&#x60;Anagrafica&#x60; coinvolta (fatturato, numero fatture, ecc.);              2) Creazione della registrazione contabile di emissione;              3) Creazione delle scadenze attive o passive, in base al tipo di pagamento;              4) Eventuale autoproduzione dei beni fatturati tramite distinta base, se l&#39;opzione è attiva;              5) Creazione del movimento di magazzino;              6) Aggiornamento delle giacenze dei beni coinvolti, nei rispettivi magazzini;              7) Aggiornamento di eventuali ordini collegati.               Un documento già emesso non può essere riemesso. Alcuni tipi di documento non supportano l&#39;emissione (ordini e preventivi, ad esempio).
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="documentoId">ID univoco del documento da emettere.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoEmitWithHttpInfo(string companyId, int documentoId, string? idempotencyKey = default(string?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoEmit");
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
            localVarRequestOptions.PathParameters.Add("documentoId", Amica.Sdk.Client.ClientUtils.ParameterToString(documentoId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoEmit";
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
            var localVarResponse = this.Client.Post<Documento>("/api/v3/{companyId}/Documento/{documentoId}/emit", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoEmit", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Emette un documento. A seconda del tipo di documento, l&#39;emissione comporta una serie di operazioni amministrative e contabili:               1) Aggiornamento dell&#39;&#x60;Anagrafica&#x60; coinvolta (fatturato, numero fatture, ecc.);              2) Creazione della registrazione contabile di emissione;              3) Creazione delle scadenze attive o passive, in base al tipo di pagamento;              4) Eventuale autoproduzione dei beni fatturati tramite distinta base, se l&#39;opzione è attiva;              5) Creazione del movimento di magazzino;              6) Aggiornamento delle giacenze dei beni coinvolti, nei rispettivi magazzini;              7) Aggiornamento di eventuali ordini collegati.               Un documento già emesso non può essere riemesso. Alcuni tipi di documento non supportano l&#39;emissione (ordini e preventivi, ad esempio).
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="documentoId">ID univoco del documento da emettere.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoEmitAsync(string companyId, int documentoId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoEmitWithHttpInfoAsync(companyId, documentoId, idempotencyKey, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Emette un documento. A seconda del tipo di documento, l&#39;emissione comporta una serie di operazioni amministrative e contabili:               1) Aggiornamento dell&#39;&#x60;Anagrafica&#x60; coinvolta (fatturato, numero fatture, ecc.);              2) Creazione della registrazione contabile di emissione;              3) Creazione delle scadenze attive o passive, in base al tipo di pagamento;              4) Eventuale autoproduzione dei beni fatturati tramite distinta base, se l&#39;opzione è attiva;              5) Creazione del movimento di magazzino;              6) Aggiornamento delle giacenze dei beni coinvolti, nei rispettivi magazzini;              7) Aggiornamento di eventuali ordini collegati.               Un documento già emesso non può essere riemesso. Alcuni tipi di documento non supportano l&#39;emissione (ordini e preventivi, ad esempio).
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="documentoId">ID univoco del documento da emettere.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoEmitWithHttpInfoAsync(string companyId, int documentoId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoEmit");
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
            localVarRequestOptions.PathParameters.Add("documentoId", Amica.Sdk.Client.ClientUtils.ParameterToString(documentoId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoEmit";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<Documento>("/api/v3/{companyId}/Documento/{documentoId}/emit", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoEmit", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoGet(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoGetWithHttpInfo(companyId, id, expand);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoGetWithHttpInfo(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoGet");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("id", Amica.Sdk.Client.ClientUtils.ParameterToString(id)); // path parameter
            if (expand != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("multi", "Expand", expand));
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoGet";
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
            var localVarResponse = this.Client.Get<Documento>("/api/v3/{companyId}/Documento/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoGet", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoGetAsync(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoGetWithHttpInfoAsync(companyId, id, expand, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoGetWithHttpInfoAsync(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoGet");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            localVarRequestOptions.PathParameters.Add("id", Amica.Sdk.Client.ClientUtils.ParameterToString(id)); // path parameter
            if (expand != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("multi", "Expand", expand));
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoGet";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<Documento>("/api/v3/{companyId}/Documento/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoGet", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da uno o più acconti in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID degli acconti da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoImportAcconti(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoImportAccontiWithHttpInfo(companyId, id, validationOverride, idempotencyKey, documentoImportData);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da uno o più acconti in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID degli acconti da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoImportAccontiWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportAcconti");
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
            localVarRequestOptions.Data = documentoImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportAcconti";
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
            var localVarResponse = this.Client.Post<Documento>("/api/v3/{companyId}/Documento/{id}/import/acconto", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportAcconti", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da uno o più acconti in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID degli acconti da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoImportAccontiAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoImportAccontiWithHttpInfoAsync(companyId, id, validationOverride, idempotencyKey, documentoImportData, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da uno o più acconti in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID degli acconti da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoImportAccontiWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportAcconti");
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
            localVarRequestOptions.Data = documentoImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportAcconti";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<Documento>("/api/v3/{companyId}/Documento/{id}/import/acconto", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportAcconti", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da uno o più DDT in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ddtImportData">Opzioni di importazione, inclusi gli ID dei DDT da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoImportDdt(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DdtImportData? ddtImportData = default(DdtImportData?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoImportDdtWithHttpInfo(companyId, id, validationOverride, idempotencyKey, ddtImportData);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da uno o più DDT in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ddtImportData">Opzioni di importazione, inclusi gli ID dei DDT da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoImportDdtWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DdtImportData? ddtImportData = default(DdtImportData?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportDdt");
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
            localVarRequestOptions.Data = ddtImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportDdt";
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
            var localVarResponse = this.Client.Post<Documento>("/api/v3/{companyId}/Documento/{id}/import/ddt", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportDdt", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da uno o più DDT in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ddtImportData">Opzioni di importazione, inclusi gli ID dei DDT da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoImportDdtAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DdtImportData? ddtImportData = default(DdtImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoImportDdtWithHttpInfoAsync(companyId, id, validationOverride, idempotencyKey, ddtImportData, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da uno o più DDT in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ddtImportData">Opzioni di importazione, inclusi gli ID dei DDT da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoImportDdtWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DdtImportData? ddtImportData = default(DdtImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportDdt");
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
            localVarRequestOptions.Data = ddtImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportDdt";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<Documento>("/api/v3/{companyId}/Documento/{id}/import/ddt", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportDdt", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da uno o più ordini in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ordiniImportData">Opzioni di importazione, inclusi gli ID degli ordini da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoImportOrdini(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), OrdiniImportData? ordiniImportData = default(OrdiniImportData?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoImportOrdiniWithHttpInfo(companyId, id, validationOverride, idempotencyKey, ordiniImportData);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da uno o più ordini in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ordiniImportData">Opzioni di importazione, inclusi gli ID degli ordini da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoImportOrdiniWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), OrdiniImportData? ordiniImportData = default(OrdiniImportData?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportOrdini");
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
            localVarRequestOptions.Data = ordiniImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportOrdini";
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
            var localVarResponse = this.Client.Post<Documento>("/api/v3/{companyId}/Documento/{id}/import/ordine", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportOrdini", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da uno o più ordini in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ordiniImportData">Opzioni di importazione, inclusi gli ID degli ordini da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoImportOrdiniAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), OrdiniImportData? ordiniImportData = default(OrdiniImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoImportOrdiniWithHttpInfoAsync(companyId, id, validationOverride, idempotencyKey, ordiniImportData, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da uno o più ordini in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="ordiniImportData">Opzioni di importazione, inclusi gli ID degli ordini da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoImportOrdiniWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), OrdiniImportData? ordiniImportData = default(OrdiniImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportOrdini");
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
            localVarRequestOptions.Data = ordiniImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportOrdini";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<Documento>("/api/v3/{companyId}/Documento/{id}/import/ordine", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportOrdini", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da uno o più preventivi in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID dei preventivi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoImportPreventivi(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoImportPreventiviWithHttpInfo(companyId, id, validationOverride, idempotencyKey, documentoImportData);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da uno o più preventivi in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID dei preventivi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoImportPreventiviWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportPreventivi");
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
            localVarRequestOptions.Data = documentoImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportPreventivi";
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
            var localVarResponse = this.Client.Post<Documento>("/api/v3/{companyId}/Documento/{id}/import/preventivo", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportPreventivi", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da uno o più preventivi in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID dei preventivi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoImportPreventiviAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoImportPreventiviWithHttpInfoAsync(companyId, id, validationOverride, idempotencyKey, documentoImportData, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da uno o più preventivi in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID dei preventivi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoImportPreventiviWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportPreventivi");
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
            localVarRequestOptions.Data = documentoImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportPreventivi";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<Documento>("/api/v3/{companyId}/Documento/{id}/import/preventivo", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportPreventivi", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da una o più fatture proforma in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID delle proforma da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoImportProForma(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoImportProFormaWithHttpInfo(companyId, id, validationOverride, idempotencyKey, documentoImportData);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da una o più fatture proforma in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID delle proforma da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoImportProFormaWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportProForma");
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
            localVarRequestOptions.Data = documentoImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportProForma";
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
            var localVarResponse = this.Client.Post<Documento>("/api/v3/{companyId}/Documento/{id}/import/proforma", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportProForma", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da una o più fatture proforma in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID delle proforma da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoImportProFormaAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoImportProFormaWithHttpInfoAsync(companyId, id, validationOverride, idempotencyKey, documentoImportData, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da una o più fatture proforma in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documentoImportData">Opzioni di importazione, inclusi gli ID delle proforma da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoImportProFormaWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), DocumentoImportData? documentoImportData = default(DocumentoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportProForma");
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
            localVarRequestOptions.Data = documentoImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportProForma";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<Documento>("/api/v3/{companyId}/Documento/{id}/import/proforma", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportProForma", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da uno o più resi in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="resoImportData">Opzioni di importazione, inclusi gli ID dei resi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoImportResi(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), ResoImportData? resoImportData = default(ResoImportData?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoImportResiWithHttpInfo(companyId, id, validationOverride, idempotencyKey, resoImportData);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da uno o più resi in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="resoImportData">Opzioni di importazione, inclusi gli ID dei resi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoImportResiWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), ResoImportData? resoImportData = default(ResoImportData?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportResi");
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
            localVarRequestOptions.Data = resoImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportResi";
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
            var localVarResponse = this.Client.Post<Documento>("/api/v3/{companyId}/Documento/{id}/import/reso", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportResi", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Importa righe da uno o più resi in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="resoImportData">Opzioni di importazione, inclusi gli ID dei resi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoImportResiAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), ResoImportData? resoImportData = default(ResoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoImportResiWithHttpInfoAsync(companyId, id, validationOverride, idempotencyKey, resoImportData, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Importa righe da uno o più resi in un documento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID del documento di destinazione.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="resoImportData">Opzioni di importazione, inclusi gli ID dei resi da importare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoImportResiWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), ResoImportData? resoImportData = default(ResoImportData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoImportResi");
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
            localVarRequestOptions.Data = resoImportData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoImportResi";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<Documento>("/api/v3/{companyId}/Documento/{id}/import/reso", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoImportResi", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip">Numero di elementi da escludere dall&#39;inizio del risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="skipTotalCount">Indica se l&#39;header &#x60;X-Read-TotalCount&#x60; debba essere escluso dalla risposta. Impostare a &#x60;true&#x60; per prestazioni migliori a scapito dell&#39;accuratezza. Il valore predefinito è &#x60;false&#x60;. (optional)</param>
        /// <param name="filter">Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: &#x60;$eq&#x60; (uguale), &#x60;$ne&#x60; (diverso), &#x60;$gt&#x60; (maggiore di), &#x60;$lt&#x60; (minore di), &#x60;$lte&#x60; (minore o uguale), &#x60;$gte&#x60; (maggiore o uguale), &#x60;$ct&#x60; (contiene), &#x60;$nct&#x60; (non contiene), &#x60;$sw&#x60; (inizia con), &#x60;$ew&#x60; (finisce con). Più espressioni possono essere concatenate con un connettore logico &#x60;$and&#x60; o &#x60;$or&#x60; (&#x60;$and&#x60; può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (&#x60;[]&#x60;). Nell&#39;esempio vengono inclusi tutti gli elementi con &#x60;field1&#x3D;\&quot;hello\&quot;&#x60; oppure &#x60;field2&gt;10&#x60;. (optional)</param>
        /// <param name="order">Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell&#39;esempio i risultati sono ordinati prima per &#x60;field1&#x60; (discendente), poi per &#x60;field2&#x60;. (optional)</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;Documento&gt;</returns>
        public List<Documento> DocumentoList(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<Documento>> localVarResponse = DocumentoListWithHttpInfo(companyId, skip, take, skipTotalCount, filter, order, expand);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una lista di elementi.  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip">Numero di elementi da escludere dall&#39;inizio del risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="skipTotalCount">Indica se l&#39;header &#x60;X-Read-TotalCount&#x60; debba essere escluso dalla risposta. Impostare a &#x60;true&#x60; per prestazioni migliori a scapito dell&#39;accuratezza. Il valore predefinito è &#x60;false&#x60;. (optional)</param>
        /// <param name="filter">Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: &#x60;$eq&#x60; (uguale), &#x60;$ne&#x60; (diverso), &#x60;$gt&#x60; (maggiore di), &#x60;$lt&#x60; (minore di), &#x60;$lte&#x60; (minore o uguale), &#x60;$gte&#x60; (maggiore o uguale), &#x60;$ct&#x60; (contiene), &#x60;$nct&#x60; (non contiene), &#x60;$sw&#x60; (inizia con), &#x60;$ew&#x60; (finisce con). Più espressioni possono essere concatenate con un connettore logico &#x60;$and&#x60; o &#x60;$or&#x60; (&#x60;$and&#x60; può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (&#x60;[]&#x60;). Nell&#39;esempio vengono inclusi tutti gli elementi con &#x60;field1&#x3D;\&quot;hello\&quot;&#x60; oppure &#x60;field2&gt;10&#x60;. (optional)</param>
        /// <param name="order">Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell&#39;esempio i risultati sono ordinati prima per &#x60;field1&#x60; (discendente), poi per &#x60;field2&#x60;. (optional)</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;Documento&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<Documento>> DocumentoListWithHttpInfo(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoList");
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

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoList";
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
            var localVarResponse = this.Client.Get<List<Documento>>("/api/v3/{companyId}/Documento", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoList", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip">Numero di elementi da escludere dall&#39;inizio del risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="skipTotalCount">Indica se l&#39;header &#x60;X-Read-TotalCount&#x60; debba essere escluso dalla risposta. Impostare a &#x60;true&#x60; per prestazioni migliori a scapito dell&#39;accuratezza. Il valore predefinito è &#x60;false&#x60;. (optional)</param>
        /// <param name="filter">Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: &#x60;$eq&#x60; (uguale), &#x60;$ne&#x60; (diverso), &#x60;$gt&#x60; (maggiore di), &#x60;$lt&#x60; (minore di), &#x60;$lte&#x60; (minore o uguale), &#x60;$gte&#x60; (maggiore o uguale), &#x60;$ct&#x60; (contiene), &#x60;$nct&#x60; (non contiene), &#x60;$sw&#x60; (inizia con), &#x60;$ew&#x60; (finisce con). Più espressioni possono essere concatenate con un connettore logico &#x60;$and&#x60; o &#x60;$or&#x60; (&#x60;$and&#x60; può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (&#x60;[]&#x60;). Nell&#39;esempio vengono inclusi tutti gli elementi con &#x60;field1&#x3D;\&quot;hello\&quot;&#x60; oppure &#x60;field2&gt;10&#x60;. (optional)</param>
        /// <param name="order">Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell&#39;esempio i risultati sono ordinati prima per &#x60;field1&#x60; (discendente), poi per &#x60;field2&#x60;. (optional)</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;Documento&gt;</returns>
        public async System.Threading.Tasks.Task<List<Documento>> DocumentoListAsync(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<Documento>> localVarResponse = await DocumentoListWithHttpInfoAsync(companyId, skip, take, skipTotalCount, filter, order, expand, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una lista di elementi.  La risposta include due header con informazioni sui risultati.  &#x60;X-Read-TotalCount&#x60; indica il numero di elementi che soddisfano il filtro impostato nella query;  &#x60;X-Read-Count&#x60; indica il numero di elementi effettivamente contenuti nella risposta.  I due header possono riportare valori diversi quando i parametri &#x60;Take&#x60; e &#x60;Skip&#x60; sono stati usati nelle opzioni di query.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="skip">Numero di elementi da escludere dall&#39;inizio del risultato. Combinato con &#x60;Take&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="take">Numero di elementi da includere nel risultato. Combinato con &#x60;Skip&#x60;, è utile per la paginazione. (optional)</param>
        /// <param name="skipTotalCount">Indica se l&#39;header &#x60;X-Read-TotalCount&#x60; debba essere escluso dalla risposta. Impostare a &#x60;true&#x60; per prestazioni migliori a scapito dell&#39;accuratezza. Il valore predefinito è &#x60;false&#x60;. (optional)</param>
        /// <param name="filter">Condizioni di ricerca. Elenco di espressioni separate da virgola. Ogni espressione è composta da nome del campo, operatore di confronto, valore ed eventualmente un connettore logico. Operatori validi: &#x60;$eq&#x60; (uguale), &#x60;$ne&#x60; (diverso), &#x60;$gt&#x60; (maggiore di), &#x60;$lt&#x60; (minore di), &#x60;$lte&#x60; (minore o uguale), &#x60;$gte&#x60; (maggiore o uguale), &#x60;$ct&#x60; (contiene), &#x60;$nct&#x60; (non contiene), &#x60;$sw&#x60; (inizia con), &#x60;$ew&#x60; (finisce con). Più espressioni possono essere concatenate con un connettore logico &#x60;$and&#x60; o &#x60;$or&#x60; (&#x60;$and&#x60; può essere omesso). Le query annidate si esprimono racchiudendo le sotto-query tra parentesi quadre (&#x60;[]&#x60;). Nell&#39;esempio vengono inclusi tutti gli elementi con &#x60;field1&#x3D;\&quot;hello\&quot;&#x60; oppure &#x60;field2&gt;10&#x60;. (optional)</param>
        /// <param name="order">Opzioni di ordinamento. Un segno meno (trattino) prima del nome indica ordinamento discendente. Nell&#39;esempio i risultati sono ordinati prima per &#x60;field1&#x60; (discendente), poi per &#x60;field2&#x60;. (optional)</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;Documento&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<Documento>>> DocumentoListWithHttpInfoAsync(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoList");
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

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoList";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<List<Documento>>("/api/v3/{companyId}/Documento", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoList", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un documento del tipo specificato. Richiede il parametro query &#x60;TipoDocumento&#x60; (int) che indica il tipo di documento da creare.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoNewInstance(string companyId, int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoNewInstanceWithHttpInfo(companyId);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un documento del tipo specificato. Richiede il parametro query &#x60;TipoDocumento&#x60; (int) che indica il tipo di documento da creare.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoNewInstanceWithHttpInfo(string companyId, int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoNewInstance");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoNewInstance";
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
            var localVarResponse = this.Client.Get<Documento>("/api/v3/{companyId}/Documento/new", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoNewInstance", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un documento del tipo specificato. Richiede il parametro query &#x60;TipoDocumento&#x60; (int) che indica il tipo di documento da creare.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoNewInstanceAsync(string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoNewInstanceWithHttpInfoAsync(companyId, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un documento del tipo specificato. Richiede il parametro query &#x60;TipoDocumento&#x60; (int) che indica il tipo di documento da creare.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoNewInstanceWithHttpInfoAsync(string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoNewInstance");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoNewInstance";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<Documento>("/api/v3/{companyId}/Documento/new", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoNewInstance", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Replica un documento. Crea una copia con la data odierna e, se è un documento di vendita, un nuovo numero. Alcuni campi vengono reimpostati ai valori predefiniti (stato del documento, stato SDI, stato del pagamento, ecc.). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="documentoId">ID univoco del documento.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoNewInstanceFromTemplate(int documentoId, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoNewInstanceFromTemplateWithHttpInfo(documentoId, companyId, idempotencyKey);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Replica un documento. Crea una copia con la data odierna e, se è un documento di vendita, un nuovo numero. Alcuni campi vengono reimpostati ai valori predefiniti (stato del documento, stato SDI, stato del pagamento, ecc.). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="documentoId">ID univoco del documento.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoNewInstanceFromTemplateWithHttpInfo(int documentoId, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoNewInstanceFromTemplate");
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

            localVarRequestOptions.PathParameters.Add("documentoId", Amica.Sdk.Client.ClientUtils.ParameterToString(documentoId)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoNewInstanceFromTemplate";
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
            var localVarResponse = this.Client.Post<Documento>("/api/v3/{companyId}/Documento/{documentoId}/newinstancefromtemplate", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoNewInstanceFromTemplate", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Replica un documento. Crea una copia con la data odierna e, se è un documento di vendita, un nuovo numero. Alcuni campi vengono reimpostati ai valori predefiniti (stato del documento, stato SDI, stato del pagamento, ecc.). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="documentoId">ID univoco del documento.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoNewInstanceFromTemplateAsync(int documentoId, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoNewInstanceFromTemplateWithHttpInfoAsync(documentoId, companyId, idempotencyKey, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Replica un documento. Crea una copia con la data odierna e, se è un documento di vendita, un nuovo numero. Alcuni campi vengono reimpostati ai valori predefiniti (stato del documento, stato SDI, stato del pagamento, ecc.). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="documentoId">ID univoco del documento.</param>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoNewInstanceFromTemplateWithHttpInfoAsync(int documentoId, string companyId, string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoNewInstanceFromTemplate");
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

            localVarRequestOptions.PathParameters.Add("documentoId", Amica.Sdk.Client.ClientUtils.ParameterToString(documentoId)); // path parameter
            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoNewInstanceFromTemplate";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<Documento>("/api/v3/{companyId}/Documento/{documentoId}/newinstancefromtemplate", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoNewInstanceFromTemplate", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Genera fatture per tutti i contatti che corrispondono ai criteri di fatturazione periodica. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">Non utilizzato. (optional)</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="periodicInvoicingData">Criteri e opzioni di fatturazione periodica. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;Documento&gt;</returns>
        public List<Documento> DocumentoPeriodicInvoicing(string companyId, int? id = default(int?), bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), PeriodicInvoicingData? periodicInvoicingData = default(PeriodicInvoicingData?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<Documento>> localVarResponse = DocumentoPeriodicInvoicingWithHttpInfo(companyId, id, validationOverride, idempotencyKey, periodicInvoicingData);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Genera fatture per tutti i contatti che corrispondono ai criteri di fatturazione periodica. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">Non utilizzato. (optional)</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="periodicInvoicingData">Criteri e opzioni di fatturazione periodica. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;Documento&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<Documento>> DocumentoPeriodicInvoicingWithHttpInfo(string companyId, int? id = default(int?), bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), PeriodicInvoicingData? periodicInvoicingData = default(PeriodicInvoicingData?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoPeriodicInvoicing");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (id != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "id", id));
            }
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = periodicInvoicingData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoPeriodicInvoicing";
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
            var localVarResponse = this.Client.Post<List<Documento>>("/api/v3/{companyId}/Documento/periodicinvoicing", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoPeriodicInvoicing", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Genera fatture per tutti i contatti che corrispondono ai criteri di fatturazione periodica. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">Non utilizzato. (optional)</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="periodicInvoicingData">Criteri e opzioni di fatturazione periodica. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;Documento&gt;</returns>
        public async System.Threading.Tasks.Task<List<Documento>> DocumentoPeriodicInvoicingAsync(string companyId, int? id = default(int?), bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), PeriodicInvoicingData? periodicInvoicingData = default(PeriodicInvoicingData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<Documento>> localVarResponse = await DocumentoPeriodicInvoicingWithHttpInfoAsync(companyId, id, validationOverride, idempotencyKey, periodicInvoicingData, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Genera fatture per tutti i contatti che corrispondono ai criteri di fatturazione periodica. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">Non utilizzato. (optional)</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="periodicInvoicingData">Criteri e opzioni di fatturazione periodica. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;Documento&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<Documento>>> DocumentoPeriodicInvoicingWithHttpInfoAsync(string companyId, int? id = default(int?), bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), PeriodicInvoicingData? periodicInvoicingData = default(PeriodicInvoicingData?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoPeriodicInvoicing");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (id != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "id", id));
            }
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = periodicInvoicingData;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoPeriodicInvoicing";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<List<Documento>>("/api/v3/{companyId}/Documento/periodicinvoicing", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoPeriodicInvoicing", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoPost(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoPostWithHttpInfo(companyId, validationOverride, idempotencyKey, documento);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Salva un nuovo elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoPostWithHttpInfo(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoPost");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = documento;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoPost";
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
            var localVarResponse = this.Client.Post<Documento>("/api/v3/{companyId}/Documento", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoPost", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoPostAsync(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoPostWithHttpInfoAsync(companyId, validationOverride, idempotencyKey, documento, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Salva un nuovo elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoPostWithHttpInfoAsync(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoPost");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (validationOverride != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "validationOverride", validationOverride));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }
            localVarRequestOptions.Data = documento;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoPost";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<Documento>("/api/v3/{companyId}/Documento", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoPost", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>Documento</returns>
        public Documento DocumentoPut(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = DocumentoPutWithHttpInfo(companyId, id, validationOverride, idempotencyKey, documento);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Aggiorna un elemento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of Documento</returns>
        public Amica.Sdk.Client.ApiResponse<Documento> DocumentoPutWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoPut");
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
            localVarRequestOptions.Data = documento;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoPut";
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
            var localVarResponse = this.Client.Put<Documento>("/api/v3/{companyId}/Documento/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoPut", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of Documento</returns>
        public async System.Threading.Tasks.Task<Documento> DocumentoPutAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<Documento> localVarResponse = await DocumentoPutWithHttpInfoAsync(companyId, id, validationOverride, idempotencyKey, documento, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Aggiorna un elemento esistente. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="documento">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (Documento)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<Documento>> DocumentoPutWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), Documento? documento = default(Documento?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoPut");
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
            localVarRequestOptions.Data = documento;

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoPut";
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
            var localVarResponse = await this.AsynchronousClient.PutAsync<Documento>("/api/v3/{companyId}/Documento/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoPut", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce i totali dei documenti che soddisfano il filtro, eventualmente raggruppati per mese, anno o anagrafica. I totali sono calcolati dal server: per conoscere il fatturato di un periodo non serve scaricare i documenti.  Le note di credito, clienti e fornitori, entrano nei totali con segno negativo.  Senza raggruppamento la risposta contiene un solo gruppo, anche se nessun documento soddisfa il filtro.             Raggruppando per mese o per anno i gruppi sono in ordine cronologico e &#x60;Periodo&#x60; ne indica il primo giorno;             raggruppando per anagrafica sono in ordine di totale documento decrescente, con &#x60;AnagraficaId&#x60; e             &#x60;RagioneSociale&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="filter">Condizioni di ricerca che selezionano i documenti da sommare, con la stessa sintassi del parametro &#x60;Filter&#x60; dell&#39;elenco dei documenti. Senza filtro si sommano tutti i documenti dell&#39;azienda, compresi ordini e preventivi: per il fatturato, filtrare per classe e tipo di documento. (optional)</param>
        /// <param name="raggruppa">Come raggruppare i totali: &#x60;Nessuno&#x60; (un solo gruppo, predefinito), &#x60;Mese&#x60;, &#x60;Anno&#x60; o &#x60;Anagrafica&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;DocumentoTotali&gt;</returns>
        public List<DocumentoTotali> DocumentoTotali(string companyId, string? filter = default(string?), string? raggruppa = default(string?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<DocumentoTotali>> localVarResponse = DocumentoTotaliWithHttpInfo(companyId, filter, raggruppa);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce i totali dei documenti che soddisfano il filtro, eventualmente raggruppati per mese, anno o anagrafica. I totali sono calcolati dal server: per conoscere il fatturato di un periodo non serve scaricare i documenti.  Le note di credito, clienti e fornitori, entrano nei totali con segno negativo.  Senza raggruppamento la risposta contiene un solo gruppo, anche se nessun documento soddisfa il filtro.             Raggruppando per mese o per anno i gruppi sono in ordine cronologico e &#x60;Periodo&#x60; ne indica il primo giorno;             raggruppando per anagrafica sono in ordine di totale documento decrescente, con &#x60;AnagraficaId&#x60; e             &#x60;RagioneSociale&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="filter">Condizioni di ricerca che selezionano i documenti da sommare, con la stessa sintassi del parametro &#x60;Filter&#x60; dell&#39;elenco dei documenti. Senza filtro si sommano tutti i documenti dell&#39;azienda, compresi ordini e preventivi: per il fatturato, filtrare per classe e tipo di documento. (optional)</param>
        /// <param name="raggruppa">Come raggruppare i totali: &#x60;Nessuno&#x60; (un solo gruppo, predefinito), &#x60;Mese&#x60;, &#x60;Anno&#x60; o &#x60;Anagrafica&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;DocumentoTotali&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<DocumentoTotali>> DocumentoTotaliWithHttpInfo(string companyId, string? filter = default(string?), string? raggruppa = default(string?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoTotali");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (filter != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Filter", filter));
            }
            if (raggruppa != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Raggruppa", raggruppa));
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoTotali";
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
            var localVarResponse = this.Client.Get<List<DocumentoTotali>>("/api/v3/{companyId}/Documento/totali", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoTotali", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Restituisce i totali dei documenti che soddisfano il filtro, eventualmente raggruppati per mese, anno o anagrafica. I totali sono calcolati dal server: per conoscere il fatturato di un periodo non serve scaricare i documenti.  Le note di credito, clienti e fornitori, entrano nei totali con segno negativo.  Senza raggruppamento la risposta contiene un solo gruppo, anche se nessun documento soddisfa il filtro.             Raggruppando per mese o per anno i gruppi sono in ordine cronologico e &#x60;Periodo&#x60; ne indica il primo giorno;             raggruppando per anagrafica sono in ordine di totale documento decrescente, con &#x60;AnagraficaId&#x60; e             &#x60;RagioneSociale&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="filter">Condizioni di ricerca che selezionano i documenti da sommare, con la stessa sintassi del parametro &#x60;Filter&#x60; dell&#39;elenco dei documenti. Senza filtro si sommano tutti i documenti dell&#39;azienda, compresi ordini e preventivi: per il fatturato, filtrare per classe e tipo di documento. (optional)</param>
        /// <param name="raggruppa">Come raggruppare i totali: &#x60;Nessuno&#x60; (un solo gruppo, predefinito), &#x60;Mese&#x60;, &#x60;Anno&#x60; o &#x60;Anagrafica&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;DocumentoTotali&gt;</returns>
        public async System.Threading.Tasks.Task<List<DocumentoTotali>> DocumentoTotaliAsync(string companyId, string? filter = default(string?), string? raggruppa = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<DocumentoTotali>> localVarResponse = await DocumentoTotaliWithHttpInfoAsync(companyId, filter, raggruppa, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce i totali dei documenti che soddisfano il filtro, eventualmente raggruppati per mese, anno o anagrafica. I totali sono calcolati dal server: per conoscere il fatturato di un periodo non serve scaricare i documenti.  Le note di credito, clienti e fornitori, entrano nei totali con segno negativo.  Senza raggruppamento la risposta contiene un solo gruppo, anche se nessun documento soddisfa il filtro.             Raggruppando per mese o per anno i gruppi sono in ordine cronologico e &#x60;Periodo&#x60; ne indica il primo giorno;             raggruppando per anagrafica sono in ordine di totale documento decrescente, con &#x60;AnagraficaId&#x60; e             &#x60;RagioneSociale&#x60;.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="filter">Condizioni di ricerca che selezionano i documenti da sommare, con la stessa sintassi del parametro &#x60;Filter&#x60; dell&#39;elenco dei documenti. Senza filtro si sommano tutti i documenti dell&#39;azienda, compresi ordini e preventivi: per il fatturato, filtrare per classe e tipo di documento. (optional)</param>
        /// <param name="raggruppa">Come raggruppare i totali: &#x60;Nessuno&#x60; (un solo gruppo, predefinito), &#x60;Mese&#x60;, &#x60;Anno&#x60; o &#x60;Anagrafica&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;DocumentoTotali&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<DocumentoTotali>>> DocumentoTotaliWithHttpInfoAsync(string companyId, string? filter = default(string?), string? raggruppa = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling DocumentoApi->DocumentoTotali");
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

            localVarRequestOptions.PathParameters.Add("companyId", Amica.Sdk.Client.ClientUtils.ParameterToString(companyId)); // path parameter
            if (filter != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Filter", filter));
            }
            if (raggruppa != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "Raggruppa", raggruppa));
            }

            localVarRequestOptions.Operation = "DocumentoApi.DocumentoTotali";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<List<DocumentoTotali>>("/api/v3/{companyId}/Documento/totali", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("DocumentoTotali", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

    }
}

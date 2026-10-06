/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.1.6
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
    public interface IRegistrazioneContabileApiSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>RegistrazioneContabile</returns>
        RegistrazioneContabile RegistrazioneContabileDelete(string companyId, int id, int operationIndex = 0);

        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of RegistrazioneContabile</returns>
        ApiResponse<RegistrazioneContabile> RegistrazioneContabileDeleteWithHttpInfo(string companyId, int id, int operationIndex = 0);
        /// <summary>
        /// Restituisce un elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da recuperare.</param>
        /// <param name="expand">Elenco separato da virgole di oggetti da espandere. Permette di recuperare gli oggetti correlati in un&#39;unica chiamata. Ad esempio, per includere i beni e servizi associati a ciascun documento si aggiunge &#x60;expand&#x3D;Rows&#x60; alla chiamata. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>RegistrazioneContabile</returns>
        RegistrazioneContabile RegistrazioneContabileGet(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0);

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
        /// <returns>ApiResponse of RegistrazioneContabile</returns>
        ApiResponse<RegistrazioneContabile> RegistrazioneContabileGetWithHttpInfo(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0);
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
        /// <returns>List&lt;RegistrazioneContabile&gt;</returns>
        List<RegistrazioneContabile> RegistrazioneContabileList(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0);

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
        /// <returns>ApiResponse of List&lt;RegistrazioneContabile&gt;</returns>
        ApiResponse<List<RegistrazioneContabile>> RegistrazioneContabileListWithHttpInfo(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0);
        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile).
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>RegistrazioneContabile</returns>
        RegistrazioneContabile RegistrazioneContabileNewInstance(string companyId, int operationIndex = 0);

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile).
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of RegistrazioneContabile</returns>
        ApiResponse<RegistrazioneContabile> RegistrazioneContabileNewInstanceWithHttpInfo(string companyId, int operationIndex = 0);
        /// <summary>
        /// Salva un nuovo elemento.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>RegistrazioneContabile</returns>
        RegistrazioneContabile RegistrazioneContabilePost(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0);

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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of RegistrazioneContabile</returns>
        ApiResponse<RegistrazioneContabile> RegistrazioneContabilePostWithHttpInfo(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0);
        /// <summary>
        /// Aggiorna un elemento esistente.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID univoco dell&#39;elemento da aggiornare.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>RegistrazioneContabile</returns>
        RegistrazioneContabile RegistrazioneContabilePut(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0);

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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of RegistrazioneContabile</returns>
        ApiResponse<RegistrazioneContabile> RegistrazioneContabilePutWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0);
        /// <summary>
        /// Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili.
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="year">Anno contabile da validare. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;RegistrazioneContabileValidationResult&gt;</returns>
        List<RegistrazioneContabileValidationResult> RegistrazioneContabileValidate(string companyId, int? year = default(int?), string? idempotencyKey = default(string?), int operationIndex = 0);

        /// <summary>
        /// Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="year">Anno contabile da validare. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;RegistrazioneContabileValidationResult&gt;</returns>
        ApiResponse<List<RegistrazioneContabileValidationResult>> RegistrazioneContabileValidateWithHttpInfo(string companyId, int? year = default(int?), string? idempotencyKey = default(string?), int operationIndex = 0);
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IRegistrazioneContabileApiAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of RegistrazioneContabile</returns>
        System.Threading.Tasks.Task<RegistrazioneContabile> RegistrazioneContabileDeleteAsync(string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Elimina un elemento.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (RegistrazioneContabile)</returns>
        System.Threading.Tasks.Task<ApiResponse<RegistrazioneContabile>> RegistrazioneContabileDeleteWithHttpInfoAsync(string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
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
        /// <returns>Task of RegistrazioneContabile</returns>
        System.Threading.Tasks.Task<RegistrazioneContabile> RegistrazioneContabileGetAsync(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

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
        /// <returns>Task of ApiResponse (RegistrazioneContabile)</returns>
        System.Threading.Tasks.Task<ApiResponse<RegistrazioneContabile>> RegistrazioneContabileGetWithHttpInfoAsync(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
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
        /// <returns>Task of List&lt;RegistrazioneContabile&gt;</returns>
        System.Threading.Tasks.Task<List<RegistrazioneContabile>> RegistrazioneContabileListAsync(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

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
        /// <returns>Task of ApiResponse (List&lt;RegistrazioneContabile&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<RegistrazioneContabile>>> RegistrazioneContabileListWithHttpInfoAsync(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile).
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of RegistrazioneContabile</returns>
        System.Threading.Tasks.Task<RegistrazioneContabile> RegistrazioneContabileNewInstanceAsync(string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile).
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (RegistrazioneContabile)</returns>
        System.Threading.Tasks.Task<ApiResponse<RegistrazioneContabile>> RegistrazioneContabileNewInstanceWithHttpInfoAsync(string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of RegistrazioneContabile</returns>
        System.Threading.Tasks.Task<RegistrazioneContabile> RegistrazioneContabilePostAsync(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (RegistrazioneContabile)</returns>
        System.Threading.Tasks.Task<ApiResponse<RegistrazioneContabile>> RegistrazioneContabilePostWithHttpInfoAsync(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of RegistrazioneContabile</returns>
        System.Threading.Tasks.Task<RegistrazioneContabile> RegistrazioneContabilePutAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (RegistrazioneContabile)</returns>
        System.Threading.Tasks.Task<ApiResponse<RegistrazioneContabile>> RegistrazioneContabilePutWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        /// <summary>
        /// Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="year">Anno contabile da validare. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;RegistrazioneContabileValidationResult&gt;</returns>
        System.Threading.Tasks.Task<List<RegistrazioneContabileValidationResult>> RegistrazioneContabileValidateAsync(string companyId, int? year = default(int?), string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));

        /// <summary>
        /// Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="year">Anno contabile da validare. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;RegistrazioneContabileValidationResult&gt;)</returns>
        System.Threading.Tasks.Task<ApiResponse<List<RegistrazioneContabileValidationResult>>> RegistrazioneContabileValidateWithHttpInfoAsync(string companyId, int? year = default(int?), string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface IRegistrazioneContabileApi : IRegistrazioneContabileApiSync, IRegistrazioneContabileApiAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class RegistrazioneContabileApi : IRegistrazioneContabileApi
    {
        private Amica.Sdk.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrazioneContabileApi"/> class.
        /// </summary>
        /// <returns></returns>
        public RegistrazioneContabileApi() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrazioneContabileApi"/> class.
        /// </summary>
        /// <returns></returns>
        public RegistrazioneContabileApi(string basePath)
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
        /// Initializes a new instance of the <see cref="RegistrazioneContabileApi"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public RegistrazioneContabileApi(Amica.Sdk.Client.Configuration configuration)
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
        /// Initializes a new instance of the <see cref="RegistrazioneContabileApi"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        public RegistrazioneContabileApi(Amica.Sdk.Client.ISynchronousClient client, Amica.Sdk.Client.IAsynchronousClient asyncClient, Amica.Sdk.Client.IReadableConfiguration configuration)
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
        /// Elimina un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>RegistrazioneContabile</returns>
        public RegistrazioneContabile RegistrazioneContabileDelete(string companyId, int id, int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> localVarResponse = RegistrazioneContabileDeleteWithHttpInfo(companyId, id);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Elimina un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of RegistrazioneContabile</returns>
        public Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> RegistrazioneContabileDeleteWithHttpInfo(string companyId, int id, int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabileDelete");
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

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabileDelete";
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
            var localVarResponse = this.Client.Delete<RegistrazioneContabile>("/api/v3/{companyId}/RegistrazioneContabile/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabileDelete", localVarResponse);
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
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of RegistrazioneContabile</returns>
        public async System.Threading.Tasks.Task<RegistrazioneContabile> RegistrazioneContabileDeleteAsync(string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> localVarResponse = await RegistrazioneContabileDeleteWithHttpInfoAsync(companyId, id, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Elimina un elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="id">ID dell&#39;elemento da eliminare.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (RegistrazioneContabile)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<RegistrazioneContabile>> RegistrazioneContabileDeleteWithHttpInfoAsync(string companyId, int id, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabileDelete");
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

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabileDelete";
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
            var localVarResponse = await this.AsynchronousClient.DeleteAsync<RegistrazioneContabile>("/api/v3/{companyId}/RegistrazioneContabile/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabileDelete", localVarResponse);
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
        /// <returns>RegistrazioneContabile</returns>
        public RegistrazioneContabile RegistrazioneContabileGet(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> localVarResponse = RegistrazioneContabileGetWithHttpInfo(companyId, id, expand);
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
        /// <returns>ApiResponse of RegistrazioneContabile</returns>
        public Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> RegistrazioneContabileGetWithHttpInfo(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabileGet");
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

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabileGet";
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
            var localVarResponse = this.Client.Get<RegistrazioneContabile>("/api/v3/{companyId}/RegistrazioneContabile/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabileGet", localVarResponse);
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
        /// <returns>Task of RegistrazioneContabile</returns>
        public async System.Threading.Tasks.Task<RegistrazioneContabile> RegistrazioneContabileGetAsync(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> localVarResponse = await RegistrazioneContabileGetWithHttpInfoAsync(companyId, id, expand, operationIndex, cancellationToken).ConfigureAwait(false);
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
        /// <returns>Task of ApiResponse (RegistrazioneContabile)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<RegistrazioneContabile>> RegistrazioneContabileGetWithHttpInfoAsync(string companyId, int id, List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabileGet");
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

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabileGet";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<RegistrazioneContabile>("/api/v3/{companyId}/RegistrazioneContabile/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabileGet", localVarResponse);
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
        /// <returns>List&lt;RegistrazioneContabile&gt;</returns>
        public List<RegistrazioneContabile> RegistrazioneContabileList(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<RegistrazioneContabile>> localVarResponse = RegistrazioneContabileListWithHttpInfo(companyId, skip, take, skipTotalCount, filter, order, expand);
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
        /// <returns>ApiResponse of List&lt;RegistrazioneContabile&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<RegistrazioneContabile>> RegistrazioneContabileListWithHttpInfo(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabileList");
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

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabileList";
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
            var localVarResponse = this.Client.Get<List<RegistrazioneContabile>>("/api/v3/{companyId}/RegistrazioneContabile", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabileList", localVarResponse);
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
        /// <returns>Task of List&lt;RegistrazioneContabile&gt;</returns>
        public async System.Threading.Tasks.Task<List<RegistrazioneContabile>> RegistrazioneContabileListAsync(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<RegistrazioneContabile>> localVarResponse = await RegistrazioneContabileListWithHttpInfoAsync(companyId, skip, take, skipTotalCount, filter, order, expand, operationIndex, cancellationToken).ConfigureAwait(false);
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
        /// <returns>Task of ApiResponse (List&lt;RegistrazioneContabile&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<RegistrazioneContabile>>> RegistrazioneContabileListWithHttpInfoAsync(string companyId, int? skip = default(int?), int? take = default(int?), bool? skipTotalCount = default(bool?), string? filter = default(string?), string? order = default(string?), List<string>? expand = default(List<string>?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabileList");
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

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabileList";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<List<RegistrazioneContabile>>("/api/v3/{companyId}/RegistrazioneContabile", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabileList", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>RegistrazioneContabile</returns>
        public RegistrazioneContabile RegistrazioneContabileNewInstance(string companyId, int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> localVarResponse = RegistrazioneContabileNewInstanceWithHttpInfo(companyId);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of RegistrazioneContabile</returns>
        public Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> RegistrazioneContabileNewInstanceWithHttpInfo(string companyId, int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabileNewInstance");
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

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabileNewInstance";
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
            var localVarResponse = this.Client.Get<RegistrazioneContabile>("/api/v3/{companyId}/RegistrazioneContabile/new", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabileNewInstance", localVarResponse);
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
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of RegistrazioneContabile</returns>
        public async System.Threading.Tasks.Task<RegistrazioneContabile> RegistrazioneContabileNewInstanceAsync(string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> localVarResponse = await RegistrazioneContabileNewInstanceWithHttpInfoAsync(companyId, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Restituisce una nuova istanza di un oggetto. Dove necessario, i valori delle proprietà saranno coerenti con lo stato e la logica dell&#39;applicazione (ad es., una nuova istanza di &#x60;Documento&#x60; avrà il &#x60;Numero&#x60; appropriato, basato sui documenti preesistenti di tipo compatibile). 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (RegistrazioneContabile)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<RegistrazioneContabile>> RegistrazioneContabileNewInstanceWithHttpInfoAsync(string companyId, int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabileNewInstance");
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

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabileNewInstance";
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
            var localVarResponse = await this.AsynchronousClient.GetAsync<RegistrazioneContabile>("/api/v3/{companyId}/RegistrazioneContabile/new", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabileNewInstance", localVarResponse);
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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>RegistrazioneContabile</returns>
        public RegistrazioneContabile RegistrazioneContabilePost(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> localVarResponse = RegistrazioneContabilePostWithHttpInfo(companyId, validationOverride, idempotencyKey, registrazioneContabile);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Salva un nuovo elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of RegistrazioneContabile</returns>
        public Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> RegistrazioneContabilePostWithHttpInfo(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabilePost");
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
            localVarRequestOptions.Data = registrazioneContabile;

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabilePost";
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
            var localVarResponse = this.Client.Post<RegistrazioneContabile>("/api/v3/{companyId}/RegistrazioneContabile", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabilePost", localVarResponse);
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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of RegistrazioneContabile</returns>
        public async System.Threading.Tasks.Task<RegistrazioneContabile> RegistrazioneContabilePostAsync(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> localVarResponse = await RegistrazioneContabilePostWithHttpInfoAsync(companyId, validationOverride, idempotencyKey, registrazioneContabile, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Salva un nuovo elemento. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="validationOverride">Se applicare eventualmente l&#39;override della validazione. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (RegistrazioneContabile)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<RegistrazioneContabile>> RegistrazioneContabilePostWithHttpInfoAsync(string companyId, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabilePost");
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
            localVarRequestOptions.Data = registrazioneContabile;

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabilePost";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<RegistrazioneContabile>("/api/v3/{companyId}/RegistrazioneContabile", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabilePost", localVarResponse);
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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>RegistrazioneContabile</returns>
        public RegistrazioneContabile RegistrazioneContabilePut(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> localVarResponse = RegistrazioneContabilePutWithHttpInfo(companyId, id, validationOverride, idempotencyKey, registrazioneContabile);
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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of RegistrazioneContabile</returns>
        public Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> RegistrazioneContabilePutWithHttpInfo(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabilePut");
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
            localVarRequestOptions.Data = registrazioneContabile;

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabilePut";
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
            var localVarResponse = this.Client.Put<RegistrazioneContabile>("/api/v3/{companyId}/RegistrazioneContabile/{id}", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabilePut", localVarResponse);
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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of RegistrazioneContabile</returns>
        public async System.Threading.Tasks.Task<RegistrazioneContabile> RegistrazioneContabilePutAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<RegistrazioneContabile> localVarResponse = await RegistrazioneContabilePutWithHttpInfoAsync(companyId, id, validationOverride, idempotencyKey, registrazioneContabile, operationIndex, cancellationToken).ConfigureAwait(false);
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
        /// <param name="registrazioneContabile">Istanza dell&#39;elemento da salvare. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (RegistrazioneContabile)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<RegistrazioneContabile>> RegistrazioneContabilePutWithHttpInfoAsync(string companyId, int id, bool? validationOverride = default(bool?), string? idempotencyKey = default(string?), RegistrazioneContabile? registrazioneContabile = default(RegistrazioneContabile?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabilePut");
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
            localVarRequestOptions.Data = registrazioneContabile;

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabilePut";
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
            var localVarResponse = await this.AsynchronousClient.PutAsync<RegistrazioneContabile>("/api/v3/{companyId}/RegistrazioneContabile/{id}", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabilePut", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="year">Anno contabile da validare. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>List&lt;RegistrazioneContabileValidationResult&gt;</returns>
        public List<RegistrazioneContabileValidationResult> RegistrazioneContabileValidate(string companyId, int? year = default(int?), string? idempotencyKey = default(string?), int operationIndex = 0)
        {
            Amica.Sdk.Client.ApiResponse<List<RegistrazioneContabileValidationResult>> localVarResponse = RegistrazioneContabileValidateWithHttpInfo(companyId, year, idempotencyKey);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="year">Anno contabile da validare. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <returns>ApiResponse of List&lt;RegistrazioneContabileValidationResult&gt;</returns>
        public Amica.Sdk.Client.ApiResponse<List<RegistrazioneContabileValidationResult>> RegistrazioneContabileValidateWithHttpInfo(string companyId, int? year = default(int?), string? idempotencyKey = default(string?), int operationIndex = 0)
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabileValidate");
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
            if (year != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "year", year));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabileValidate";
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
            var localVarResponse = this.Client.Post<List<RegistrazioneContabileValidationResult>>("/api/v3/{companyId}/RegistrazioneContabile/validate", localVarRequestOptions, this.Configuration);
            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabileValidate", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="year">Anno contabile da validare. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of List&lt;RegistrazioneContabileValidationResult&gt;</returns>
        public async System.Threading.Tasks.Task<List<RegistrazioneContabileValidationResult>> RegistrazioneContabileValidateAsync(string companyId, int? year = default(int?), string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            Amica.Sdk.Client.ApiResponse<List<RegistrazioneContabileValidationResult>> localVarResponse = await RegistrazioneContabileValidateWithHttpInfoAsync(companyId, year, idempotencyKey, operationIndex, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Valida le registrazioni contabili di un anno, cercando principalmente squilibri contabili. 
        /// </summary>
        /// <exception cref="Amica.Sdk.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="companyId">ID univoco dell&#39;azienda.</param>
        /// <param name="year">Anno contabile da validare. (optional)</param>
        /// <param name="idempotencyKey">Chiave di idempotenza (opzionale). Inviando la stessa chiave su un retry si riottiene la risposta della prima richiesta senza rieseguire l&#39;operazione: rende sicuri i retry automatici. Usa un valore univoco per operazione logica (es. un UUID). La risposta replicata riporta l&#39;header &#x60;Idempotent-Replayed: true&#x60;. (optional)</param>
        /// <param name="operationIndex">Index associated with the operation.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (List&lt;RegistrazioneContabileValidationResult&gt;)</returns>
        public async System.Threading.Tasks.Task<Amica.Sdk.Client.ApiResponse<List<RegistrazioneContabileValidationResult>>> RegistrazioneContabileValidateWithHttpInfoAsync(string companyId, int? year = default(int?), string? idempotencyKey = default(string?), int operationIndex = 0, System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
        {
            // verify the required parameter 'companyId' is set
            if (companyId == null)
            {
                throw new Amica.Sdk.Client.ApiException(400, "Missing required parameter 'companyId' when calling RegistrazioneContabileApi->RegistrazioneContabileValidate");
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
            if (year != null)
            {
                localVarRequestOptions.QueryParameters.Add(Amica.Sdk.Client.ClientUtils.ParameterToMultiMap("", "year", year));
            }
            if (idempotencyKey != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Idempotency-Key", Amica.Sdk.Client.ClientUtils.ParameterToString(idempotencyKey)); // header parameter
            }

            localVarRequestOptions.Operation = "RegistrazioneContabileApi.RegistrazioneContabileValidate";
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
            var localVarResponse = await this.AsynchronousClient.PostAsync<List<RegistrazioneContabileValidationResult>>("/api/v3/{companyId}/RegistrazioneContabile/validate", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("RegistrazioneContabileValidate", localVarResponse);
                if (_exception != null)
                {
                    throw _exception;
                }
            }

            return localVarResponse;
        }

    }
}

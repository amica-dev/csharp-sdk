/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.0.2
 * Contact: assistenza@gestionaleamica.com
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using OpenAPIDateConverter = Amica.Sdk.Client.OpenAPIDateConverter;

namespace Amica.Sdk.Model
{
    /// <summary>
    /// Impostazioni e informazioni di acquisto di un&#39;&#x60;Anagrafica&#x60;.
    /// </summary>
    [DataContract(Name = "SchedaFornitore")]
    public partial class SchedaFornitore : IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedaFornitore" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="anagraficaId">Identificativo univoco di &#x60;Anagrafica&#x60;..</param>
        /// <param name="pagamentoId">Identificativo univoco di &#x60;Pagamento&#x60;..</param>
        /// <param name="pagamento">pagamento.</param>
        /// <param name="listinoId">Identificativo univoco di &#x60;Listino&#x60;..</param>
        /// <param name="listino">listino.</param>
        /// <param name="capoAreaId">Identificativo univoco di &#x60;CapoArea&#x60;..</param>
        /// <param name="capoArea">capoArea.</param>
        /// <param name="categoriaAnagraficaId">Identificativo univoco di &#x60;CategoriaAnagrafica&#x60;..</param>
        /// <param name="categoriaAnagrafica">categoriaAnagrafica.</param>
        /// <param name="causaleIvaId">Identificativo univoco di &#x60;CausaleIva&#x60;..</param>
        /// <param name="causaleIva">causaleIva.</param>
        /// <param name="fatturatoAnnoCorrente">Fatturato realizzato nell&#39;anno corrente..</param>
        /// <param name="numeroFattureAnnoCorrente">Numero di fatture ricevute nell&#39;anno corrente..</param>
        /// <param name="insolutiAnnoCorrente">Importo insoluto nell&#39;anno corrente..</param>
        /// <param name="numeroInsolutiAnnoCorrente">Numero di insoluti nell&#39;anno corrente..</param>
        /// <param name="fatturatoAnnoPrecedente">Fatturato realizzato nell&#39;anno precedente..</param>
        /// <param name="numeroFattureAnnoPrecedente">Numero di fatture emesse nell&#39;anno precedente..</param>
        /// <param name="insolutiAnnoPrecedente">Importo insoluto nell&#39;anno precedente..</param>
        /// <param name="numeroInsolutiAnnoPrecedente">Numero di insoluti verificatisi nell&#39;anno precedente..</param>
        /// <param name="dataUltimaFattura">Data dell&#39;ultima fattura..</param>
        /// <param name="dataUltimoPagamento">Data dell&#39;ultimo pagamento..</param>
        /// <param name="dataUltimoInsoluto">Data dell&#39;ultimo insoluto..</param>
        /// <param name="contoRaw">Numero di conto, in formato grezzo (es. \&quot;  100   20\&quot;). Sono ammessi fino a cinque livelli..</param>
        /// <param name="iscrizioneRuolo">Numero di iscrizione a ruolo..</param>
        /// <param name="enasarco">Numero di registrazione Enasarco..</param>
        public SchedaFornitore(int id = default(int), int concurrencyToken = default(int), int? anagraficaId = default(int?), int? pagamentoId = default(int?), Pagamento pagamento = default(Pagamento), int? listinoId = default(int?), Listino listino = default(Listino), int? capoAreaId = default(int?), Anagrafica capoArea = default(Anagrafica), int? categoriaAnagraficaId = default(int?), CategoriaAnagrafica categoriaAnagrafica = default(CategoriaAnagrafica), int? causaleIvaId = default(int?), CausaleIva causaleIva = default(CausaleIva), double fatturatoAnnoCorrente = default(double), int numeroFattureAnnoCorrente = default(int), double insolutiAnnoCorrente = default(double), int numeroInsolutiAnnoCorrente = default(int), double fatturatoAnnoPrecedente = default(double), int numeroFattureAnnoPrecedente = default(int), double insolutiAnnoPrecedente = default(double), int numeroInsolutiAnnoPrecedente = default(int), DateTime? dataUltimaFattura = default(DateTime?), DateTime? dataUltimoPagamento = default(DateTime?), DateTime? dataUltimoInsoluto = default(DateTime?), string contoRaw = default(string), string iscrizioneRuolo = default(string), string enasarco = default(string))
        {
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.AnagraficaId = anagraficaId;
            this.PagamentoId = pagamentoId;
            this.Pagamento = pagamento;
            this.ListinoId = listinoId;
            this.Listino = listino;
            this.CapoAreaId = capoAreaId;
            this.CapoArea = capoArea;
            this.CategoriaAnagraficaId = categoriaAnagraficaId;
            this.CategoriaAnagrafica = categoriaAnagrafica;
            this.CausaleIvaId = causaleIvaId;
            this.CausaleIva = causaleIva;
            this.FatturatoAnnoCorrente = fatturatoAnnoCorrente;
            this.NumeroFattureAnnoCorrente = numeroFattureAnnoCorrente;
            this.InsolutiAnnoCorrente = insolutiAnnoCorrente;
            this.NumeroInsolutiAnnoCorrente = numeroInsolutiAnnoCorrente;
            this.FatturatoAnnoPrecedente = fatturatoAnnoPrecedente;
            this.NumeroFattureAnnoPrecedente = numeroFattureAnnoPrecedente;
            this.InsolutiAnnoPrecedente = insolutiAnnoPrecedente;
            this.NumeroInsolutiAnnoPrecedente = numeroInsolutiAnnoPrecedente;
            this.DataUltimaFattura = dataUltimaFattura;
            this.DataUltimoPagamento = dataUltimoPagamento;
            this.DataUltimoInsoluto = dataUltimoInsoluto;
            this.ContoRaw = contoRaw;
            this.IscrizioneRuolo = iscrizioneRuolo;
            this.Enasarco = enasarco;
        }

        /// <summary>
        /// Identificativo univoco.
        /// </summary>
        /// <value>Identificativo univoco.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public int Id { get; set; }

        /// <summary>
        /// Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato.
        /// </summary>
        /// <value>Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato.</value>
        /*
        <example>12345</example>
        */
        [DataMember(Name = "concurrencyToken", EmitDefaultValue = false)]
        public int ConcurrencyToken { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;Anagrafica&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;Anagrafica&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "anagraficaId", EmitDefaultValue = true)]
        public int? AnagraficaId { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;Pagamento&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;Pagamento&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "pagamentoId", EmitDefaultValue = true)]
        public int? PagamentoId { get; set; }

        /// <summary>
        /// Gets or Sets Pagamento
        /// </summary>
        [DataMember(Name = "pagamento", EmitDefaultValue = false)]
        public Pagamento Pagamento { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;Listino&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;Listino&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "listinoId", EmitDefaultValue = true)]
        public int? ListinoId { get; set; }

        /// <summary>
        /// Gets or Sets Listino
        /// </summary>
        [DataMember(Name = "listino", EmitDefaultValue = false)]
        public Listino Listino { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;CapoArea&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;CapoArea&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "capoAreaId", EmitDefaultValue = true)]
        public int? CapoAreaId { get; set; }

        /// <summary>
        /// Gets or Sets CapoArea
        /// </summary>
        [DataMember(Name = "capoArea", EmitDefaultValue = false)]
        public Anagrafica CapoArea { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;CategoriaAnagrafica&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;CategoriaAnagrafica&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "categoriaAnagraficaId", EmitDefaultValue = true)]
        public int? CategoriaAnagraficaId { get; set; }

        /// <summary>
        /// Gets or Sets CategoriaAnagrafica
        /// </summary>
        [DataMember(Name = "categoriaAnagrafica", EmitDefaultValue = false)]
        public CategoriaAnagrafica CategoriaAnagrafica { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;CausaleIva&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;CausaleIva&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "causaleIvaId", EmitDefaultValue = true)]
        public int? CausaleIvaId { get; set; }

        /// <summary>
        /// Gets or Sets CausaleIva
        /// </summary>
        [DataMember(Name = "causaleIva", EmitDefaultValue = false)]
        public CausaleIva CausaleIva { get; set; }

        /// <summary>
        /// Fatturato realizzato nell&#39;anno corrente.
        /// </summary>
        /// <value>Fatturato realizzato nell&#39;anno corrente.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "fatturatoAnnoCorrente", EmitDefaultValue = false)]
        public double FatturatoAnnoCorrente { get; set; }

        /// <summary>
        /// Numero di fatture ricevute nell&#39;anno corrente.
        /// </summary>
        /// <value>Numero di fatture ricevute nell&#39;anno corrente.</value>
        /*
        <example>12</example>
        */
        [DataMember(Name = "numeroFattureAnnoCorrente", EmitDefaultValue = false)]
        public int NumeroFattureAnnoCorrente { get; set; }

        /// <summary>
        /// Importo insoluto nell&#39;anno corrente.
        /// </summary>
        /// <value>Importo insoluto nell&#39;anno corrente.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "insolutiAnnoCorrente", EmitDefaultValue = false)]
        public double InsolutiAnnoCorrente { get; set; }

        /// <summary>
        /// Numero di insoluti nell&#39;anno corrente.
        /// </summary>
        /// <value>Numero di insoluti nell&#39;anno corrente.</value>
        /*
        <example>2</example>
        */
        [DataMember(Name = "numeroInsolutiAnnoCorrente", EmitDefaultValue = false)]
        public int NumeroInsolutiAnnoCorrente { get; set; }

        /// <summary>
        /// Fatturato realizzato nell&#39;anno precedente.
        /// </summary>
        /// <value>Fatturato realizzato nell&#39;anno precedente.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "fatturatoAnnoPrecedente", EmitDefaultValue = false)]
        public double FatturatoAnnoPrecedente { get; set; }

        /// <summary>
        /// Numero di fatture emesse nell&#39;anno precedente.
        /// </summary>
        /// <value>Numero di fatture emesse nell&#39;anno precedente.</value>
        /*
        <example>12</example>
        */
        [DataMember(Name = "numeroFattureAnnoPrecedente", EmitDefaultValue = false)]
        public int NumeroFattureAnnoPrecedente { get; set; }

        /// <summary>
        /// Importo insoluto nell&#39;anno precedente.
        /// </summary>
        /// <value>Importo insoluto nell&#39;anno precedente.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "insolutiAnnoPrecedente", EmitDefaultValue = false)]
        public double InsolutiAnnoPrecedente { get; set; }

        /// <summary>
        /// Numero di insoluti verificatisi nell&#39;anno precedente.
        /// </summary>
        /// <value>Numero di insoluti verificatisi nell&#39;anno precedente.</value>
        /*
        <example>2</example>
        */
        [DataMember(Name = "numeroInsolutiAnnoPrecedente", EmitDefaultValue = false)]
        public int NumeroInsolutiAnnoPrecedente { get; set; }

        /// <summary>
        /// Data dell&#39;ultima fattura.
        /// </summary>
        /// <value>Data dell&#39;ultima fattura.</value>
        [DataMember(Name = "dataUltimaFattura", EmitDefaultValue = true)]
        public DateTime? DataUltimaFattura { get; set; }

        /// <summary>
        /// Data dell&#39;ultimo pagamento.
        /// </summary>
        /// <value>Data dell&#39;ultimo pagamento.</value>
        [DataMember(Name = "dataUltimoPagamento", EmitDefaultValue = true)]
        public DateTime? DataUltimoPagamento { get; set; }

        /// <summary>
        /// Data dell&#39;ultimo insoluto.
        /// </summary>
        /// <value>Data dell&#39;ultimo insoluto.</value>
        [DataMember(Name = "dataUltimoInsoluto", EmitDefaultValue = true)]
        public DateTime? DataUltimoInsoluto { get; set; }

        /// <summary>
        /// Numero di conto, in formato grezzo (es. \&quot;  100   20\&quot;). Sono ammessi fino a cinque livelli.
        /// </summary>
        /// <value>Numero di conto, in formato grezzo (es. \&quot;  100   20\&quot;). Sono ammessi fino a cinque livelli.</value>
        /*
        <example>  100   20</example>
        */
        [DataMember(Name = "contoRaw", EmitDefaultValue = true)]
        public string ContoRaw { get; set; }

        /// <summary>
        /// Numero di conto, in formato leggibile (es. \&quot;100.20\&quot;).
        /// </summary>
        /// <value>Numero di conto, in formato leggibile (es. \&quot;100.20\&quot;).</value>
        [DataMember(Name = "conto", EmitDefaultValue = true)]
        public string Conto { get; private set; }

        /// <summary>
        /// Returns false as Conto should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializeConto()
        {
            return false;
        }
        /// <summary>
        /// Numero di iscrizione a ruolo.
        /// </summary>
        /// <value>Numero di iscrizione a ruolo.</value>
        /*
        <example>123456</example>
        */
        [DataMember(Name = "iscrizioneRuolo", EmitDefaultValue = true)]
        public string IscrizioneRuolo { get; set; }

        /// <summary>
        /// Numero di registrazione Enasarco.
        /// </summary>
        /// <value>Numero di registrazione Enasarco.</value>
        /*
        <example>123456</example>
        */
        [DataMember(Name = "enasarco", EmitDefaultValue = true)]
        public string Enasarco { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class SchedaFornitore {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  AnagraficaId: ").Append(AnagraficaId).Append("\n");
            sb.Append("  PagamentoId: ").Append(PagamentoId).Append("\n");
            sb.Append("  Pagamento: ").Append(Pagamento).Append("\n");
            sb.Append("  ListinoId: ").Append(ListinoId).Append("\n");
            sb.Append("  Listino: ").Append(Listino).Append("\n");
            sb.Append("  CapoAreaId: ").Append(CapoAreaId).Append("\n");
            sb.Append("  CapoArea: ").Append(CapoArea).Append("\n");
            sb.Append("  CategoriaAnagraficaId: ").Append(CategoriaAnagraficaId).Append("\n");
            sb.Append("  CategoriaAnagrafica: ").Append(CategoriaAnagrafica).Append("\n");
            sb.Append("  CausaleIvaId: ").Append(CausaleIvaId).Append("\n");
            sb.Append("  CausaleIva: ").Append(CausaleIva).Append("\n");
            sb.Append("  FatturatoAnnoCorrente: ").Append(FatturatoAnnoCorrente).Append("\n");
            sb.Append("  NumeroFattureAnnoCorrente: ").Append(NumeroFattureAnnoCorrente).Append("\n");
            sb.Append("  InsolutiAnnoCorrente: ").Append(InsolutiAnnoCorrente).Append("\n");
            sb.Append("  NumeroInsolutiAnnoCorrente: ").Append(NumeroInsolutiAnnoCorrente).Append("\n");
            sb.Append("  FatturatoAnnoPrecedente: ").Append(FatturatoAnnoPrecedente).Append("\n");
            sb.Append("  NumeroFattureAnnoPrecedente: ").Append(NumeroFattureAnnoPrecedente).Append("\n");
            sb.Append("  InsolutiAnnoPrecedente: ").Append(InsolutiAnnoPrecedente).Append("\n");
            sb.Append("  NumeroInsolutiAnnoPrecedente: ").Append(NumeroInsolutiAnnoPrecedente).Append("\n");
            sb.Append("  DataUltimaFattura: ").Append(DataUltimaFattura).Append("\n");
            sb.Append("  DataUltimoPagamento: ").Append(DataUltimoPagamento).Append("\n");
            sb.Append("  DataUltimoInsoluto: ").Append(DataUltimoInsoluto).Append("\n");
            sb.Append("  ContoRaw: ").Append(ContoRaw).Append("\n");
            sb.Append("  Conto: ").Append(Conto).Append("\n");
            sb.Append("  IscrizioneRuolo: ").Append(IscrizioneRuolo).Append("\n");
            sb.Append("  Enasarco: ").Append(Enasarco).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            // ContoRaw (string) maxLength
            if (this.ContoRaw != null && this.ContoRaw.Length > 25)
            {
                yield return new ValidationResult("Invalid value for ContoRaw, length must be less than 25.", new [] { "ContoRaw" });
            }

            // ContoRaw (string) minLength
            if (this.ContoRaw != null && this.ContoRaw.Length < 0)
            {
                yield return new ValidationResult("Invalid value for ContoRaw, length must be greater than 0.", new [] { "ContoRaw" });
            }

            // IscrizioneRuolo (string) maxLength
            if (this.IscrizioneRuolo != null && this.IscrizioneRuolo.Length > 12)
            {
                yield return new ValidationResult("Invalid value for IscrizioneRuolo, length must be less than 12.", new [] { "IscrizioneRuolo" });
            }

            // IscrizioneRuolo (string) minLength
            if (this.IscrizioneRuolo != null && this.IscrizioneRuolo.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IscrizioneRuolo, length must be greater than 0.", new [] { "IscrizioneRuolo" });
            }

            // Enasarco (string) maxLength
            if (this.Enasarco != null && this.Enasarco.Length > 12)
            {
                yield return new ValidationResult("Invalid value for Enasarco, length must be less than 12.", new [] { "Enasarco" });
            }

            // Enasarco (string) minLength
            if (this.Enasarco != null && this.Enasarco.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Enasarco, length must be greater than 0.", new [] { "Enasarco" });
            }

            yield break;
        }
    }

}

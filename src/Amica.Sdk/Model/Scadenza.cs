/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.2.0
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
    /// Scadenza di pagamento.
    /// </summary>
    [DataContract(Name = "Scadenza")]
    public partial class Scadenza : IValidatableObject
    {
        /// <summary>
        /// Tipo di scadenza.
        /// </summary>
        /// <value>Tipo di scadenza.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TipoScadenzaEnum
        {
            /// <summary>
            /// Enum IncassoCliente for value: IncassoCliente
            /// </summary>
            [EnumMember(Value = "IncassoCliente")]
            IncassoCliente = 1,

            /// <summary>
            /// Enum PagamentoCliente for value: PagamentoCliente
            /// </summary>
            [EnumMember(Value = "PagamentoCliente")]
            PagamentoCliente = 2,

            /// <summary>
            /// Enum IncassoFornitore for value: IncassoFornitore
            /// </summary>
            [EnumMember(Value = "IncassoFornitore")]
            IncassoFornitore = 3,

            /// <summary>
            /// Enum PagamentoFornitore for value: PagamentoFornitore
            /// </summary>
            [EnumMember(Value = "PagamentoFornitore")]
            PagamentoFornitore = 4
        }


        /// <summary>
        /// Tipo di scadenza.
        /// </summary>
        /// <value>Tipo di scadenza.</value>
        [DataMember(Name = "tipoScadenza", EmitDefaultValue = false)]
        public TipoScadenzaEnum? TipoScadenza { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Scadenza" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="registrazioneContabileId">Identificativo univoco di &#x60;RegistrazioneContabile&#x60;..</param>
        /// <param name="registrazioneContabile">registrazioneContabile.</param>
        /// <param name="anagraficaId">Identificativo univoco di &#x60;Anagrafica&#x60;..</param>
        /// <param name="anagrafica">anagrafica.</param>
        /// <param name="documentoId">Identificativo univoco di &#x60;Documento&#x60;..</param>
        /// <param name="documento">documento.</param>
        /// <param name="modalitaPagamentoId">Identificativo univoco di &#x60;ModalitaPagamento&#x60;..</param>
        /// <param name="modalitaPagamento">modalitaPagamento.</param>
        /// <param name="pagataManualmente">Indica se la scadenza è stata impostata manualmente come &#39;pagata&#39;..</param>
        /// <param name="data">Data di scadenza..</param>
        /// <param name="dataPagamento">Data di pagamento..</param>
        /// <param name="dataDocumento">Data del documento fiscale..</param>
        /// <param name="importo">Importo della scadenza..</param>
        /// <param name="partitaAnno">Anno della partita aperta..</param>
        /// <param name="partitaNumero">Numero della partita aperta..</param>
        /// <param name="note">Note..</param>
        /// <param name="descrizione">Descrizione..</param>
        /// <param name="documentoNumeroParteNumerica">Numero documento, la parte numerica (es. 99)..</param>
        /// <param name="documentoNumeroParteTesto">Numero documento, la parte alfanumerica (es. \&quot;bis\&quot;).</param>
        /// <param name="documentoNumero">Numero documento, es. \&quot;99/bis\&quot;. Il documento deve essere già salvato sul sistema..</param>
        /// <param name="tipoScadenza">Tipo di scadenza..</param>
        /// <param name="importoPagato">L&#39;importo già pagato della scadenza..</param>
        public Scadenza(int id = default(int), int concurrencyToken = default(int), int? registrazioneContabileId = default(int?), RegistrazioneContabile registrazioneContabile = default(RegistrazioneContabile), int? anagraficaId = default(int?), Anagrafica anagrafica = default(Anagrafica), int? documentoId = default(int?), Documento documento = default(Documento), int? modalitaPagamentoId = default(int?), ModalitaPagamento modalitaPagamento = default(ModalitaPagamento), bool pagataManualmente = default(bool), DateTime data = default(DateTime), DateTime? dataPagamento = default(DateTime?), DateTime? dataDocumento = default(DateTime?), double importo = default(double), int? partitaAnno = default(int?), int? partitaNumero = default(int?), string note = default(string), string descrizione = default(string), int? documentoNumeroParteNumerica = default(int?), string documentoNumeroParteTesto = default(string), string documentoNumero = default(string), TipoScadenzaEnum? tipoScadenza = default(TipoScadenzaEnum?), double importoPagato = default(double))
        {
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.RegistrazioneContabileId = registrazioneContabileId;
            this.RegistrazioneContabile = registrazioneContabile;
            this.AnagraficaId = anagraficaId;
            this.Anagrafica = anagrafica;
            this.DocumentoId = documentoId;
            this.Documento = documento;
            this.ModalitaPagamentoId = modalitaPagamentoId;
            this.ModalitaPagamento = modalitaPagamento;
            this.PagataManualmente = pagataManualmente;
            this.Data = data;
            this.DataPagamento = dataPagamento;
            this.DataDocumento = dataDocumento;
            this.Importo = importo;
            this.PartitaAnno = partitaAnno;
            this.PartitaNumero = partitaNumero;
            this.Note = note;
            this.Descrizione = descrizione;
            this.DocumentoNumeroParteNumerica = documentoNumeroParteNumerica;
            this.DocumentoNumeroParteTesto = documentoNumeroParteTesto;
            this.DocumentoNumero = documentoNumero;
            this.TipoScadenza = tipoScadenza;
            this.ImportoPagato = importoPagato;
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
        /// Identificativo univoco di &#x60;RegistrazioneContabile&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;RegistrazioneContabile&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "registrazioneContabileId", EmitDefaultValue = true)]
        public int? RegistrazioneContabileId { get; set; }

        /// <summary>
        /// Gets or Sets RegistrazioneContabile
        /// </summary>
        [DataMember(Name = "registrazioneContabile", EmitDefaultValue = false)]
        public RegistrazioneContabile RegistrazioneContabile { get; set; }

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
        /// Gets or Sets Anagrafica
        /// </summary>
        [DataMember(Name = "anagrafica", EmitDefaultValue = false)]
        public Anagrafica Anagrafica { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;Documento&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;Documento&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "documentoId", EmitDefaultValue = true)]
        public int? DocumentoId { get; set; }

        /// <summary>
        /// Gets or Sets Documento
        /// </summary>
        [DataMember(Name = "documento", EmitDefaultValue = false)]
        public Documento Documento { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;ModalitaPagamento&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;ModalitaPagamento&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "modalitaPagamentoId", EmitDefaultValue = true)]
        public int? ModalitaPagamentoId { get; set; }

        /// <summary>
        /// Gets or Sets ModalitaPagamento
        /// </summary>
        [DataMember(Name = "modalitaPagamento", EmitDefaultValue = false)]
        public ModalitaPagamento ModalitaPagamento { get; set; }

        /// <summary>
        /// Indica se la scadenza è stata impostata manualmente come &#39;pagata&#39;.
        /// </summary>
        /// <value>Indica se la scadenza è stata impostata manualmente come &#39;pagata&#39;.</value>
        /*
        <example>false</example>
        */
        [DataMember(Name = "pagataManualmente", EmitDefaultValue = true)]
        public bool PagataManualmente { get; set; }

        /// <summary>
        /// Indica se la scadenza è pagata.
        /// </summary>
        /// <value>Indica se la scadenza è pagata.</value>
        /*
        <example>false</example>
        */
        [DataMember(Name = "pagata", EmitDefaultValue = true)]
        public bool Pagata { get; private set; }

        /// <summary>
        /// Returns false as Pagata should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializePagata()
        {
            return false;
        }
        /// <summary>
        /// Data di scadenza.
        /// </summary>
        /// <value>Data di scadenza.</value>
        [DataMember(Name = "data", EmitDefaultValue = false)]
        public DateTime Data { get; set; }

        /// <summary>
        /// Data di pagamento.
        /// </summary>
        /// <value>Data di pagamento.</value>
        [DataMember(Name = "dataPagamento", EmitDefaultValue = true)]
        public DateTime? DataPagamento { get; set; }

        /// <summary>
        /// Data del documento fiscale.
        /// </summary>
        /// <value>Data del documento fiscale.</value>
        [DataMember(Name = "dataDocumento", EmitDefaultValue = true)]
        public DateTime? DataDocumento { get; set; }

        /// <summary>
        /// Importo della scadenza.
        /// </summary>
        /// <value>Importo della scadenza.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "importo", EmitDefaultValue = false)]
        public double Importo { get; set; }

        /// <summary>
        /// Anno della partita aperta.
        /// </summary>
        /// <value>Anno della partita aperta.</value>
        /*
        <example>2026</example>
        */
        [DataMember(Name = "partitaAnno", EmitDefaultValue = true)]
        public int? PartitaAnno { get; set; }

        /// <summary>
        /// Numero della partita aperta.
        /// </summary>
        /// <value>Numero della partita aperta.</value>
        /*
        <example>42</example>
        */
        [DataMember(Name = "partitaNumero", EmitDefaultValue = true)]
        public int? PartitaNumero { get; set; }

        /// <summary>
        /// Note.
        /// </summary>
        /// <value>Note.</value>
        /*
        <example>Scadenza prorogata su accordo con il cliente</example>
        */
        [DataMember(Name = "note", EmitDefaultValue = true)]
        public string Note { get; set; }

        /// <summary>
        /// Descrizione.
        /// </summary>
        /// <value>Descrizione.</value>
        /*
        <example>Prima rata fattura n. 99</example>
        */
        [DataMember(Name = "descrizione", EmitDefaultValue = true)]
        public string Descrizione { get; set; }

        /// <summary>
        /// Numero documento, la parte numerica (es. 99).
        /// </summary>
        /// <value>Numero documento, la parte numerica (es. 99).</value>
        /*
        <example>99</example>
        */
        [DataMember(Name = "documentoNumeroParteNumerica", EmitDefaultValue = true)]
        public int? DocumentoNumeroParteNumerica { get; set; }

        /// <summary>
        /// Numero documento, la parte alfanumerica (es. \&quot;bis\&quot;)
        /// </summary>
        /// <value>Numero documento, la parte alfanumerica (es. \&quot;bis\&quot;)</value>
        /*
        <example>bis</example>
        */
        [DataMember(Name = "documentoNumeroParteTesto", EmitDefaultValue = true)]
        public string DocumentoNumeroParteTesto { get; set; }

        /// <summary>
        /// Numero documento, es. \&quot;99/bis\&quot;. Il documento deve essere già salvato sul sistema.
        /// </summary>
        /// <value>Numero documento, es. \&quot;99/bis\&quot;. Il documento deve essere già salvato sul sistema.</value>
        /*
        <example>99/bis</example>
        */
        [DataMember(Name = "documentoNumero", EmitDefaultValue = true)]
        public string DocumentoNumero { get; set; }

        /// <summary>
        /// L&#39;importo già pagato della scadenza.
        /// </summary>
        /// <value>L&#39;importo già pagato della scadenza.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "importoPagato", EmitDefaultValue = false)]
        public double ImportoPagato { get; set; }

        /// <summary>
        /// Rimanenza che risulta ancora da pagare.
        /// </summary>
        /// <value>Rimanenza che risulta ancora da pagare.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "importoDaPagare", EmitDefaultValue = false)]
        public double ImportoDaPagare { get; private set; }

        /// <summary>
        /// Returns false as ImportoDaPagare should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializeImportoDaPagare()
        {
            return false;
        }
        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Scadenza {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  RegistrazioneContabileId: ").Append(RegistrazioneContabileId).Append("\n");
            sb.Append("  RegistrazioneContabile: ").Append(RegistrazioneContabile).Append("\n");
            sb.Append("  AnagraficaId: ").Append(AnagraficaId).Append("\n");
            sb.Append("  Anagrafica: ").Append(Anagrafica).Append("\n");
            sb.Append("  DocumentoId: ").Append(DocumentoId).Append("\n");
            sb.Append("  Documento: ").Append(Documento).Append("\n");
            sb.Append("  ModalitaPagamentoId: ").Append(ModalitaPagamentoId).Append("\n");
            sb.Append("  ModalitaPagamento: ").Append(ModalitaPagamento).Append("\n");
            sb.Append("  PagataManualmente: ").Append(PagataManualmente).Append("\n");
            sb.Append("  Pagata: ").Append(Pagata).Append("\n");
            sb.Append("  Data: ").Append(Data).Append("\n");
            sb.Append("  DataPagamento: ").Append(DataPagamento).Append("\n");
            sb.Append("  DataDocumento: ").Append(DataDocumento).Append("\n");
            sb.Append("  Importo: ").Append(Importo).Append("\n");
            sb.Append("  PartitaAnno: ").Append(PartitaAnno).Append("\n");
            sb.Append("  PartitaNumero: ").Append(PartitaNumero).Append("\n");
            sb.Append("  Note: ").Append(Note).Append("\n");
            sb.Append("  Descrizione: ").Append(Descrizione).Append("\n");
            sb.Append("  DocumentoNumeroParteNumerica: ").Append(DocumentoNumeroParteNumerica).Append("\n");
            sb.Append("  DocumentoNumeroParteTesto: ").Append(DocumentoNumeroParteTesto).Append("\n");
            sb.Append("  DocumentoNumero: ").Append(DocumentoNumero).Append("\n");
            sb.Append("  TipoScadenza: ").Append(TipoScadenza).Append("\n");
            sb.Append("  ImportoPagato: ").Append(ImportoPagato).Append("\n");
            sb.Append("  ImportoDaPagare: ").Append(ImportoDaPagare).Append("\n");
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
            // DocumentoNumeroParteTesto (string) maxLength
            if (this.DocumentoNumeroParteTesto != null && this.DocumentoNumeroParteTesto.Length > 40)
            {
                yield return new ValidationResult("Invalid value for DocumentoNumeroParteTesto, length must be less than 40.", new [] { "DocumentoNumeroParteTesto" });
            }

            // DocumentoNumeroParteTesto (string) minLength
            if (this.DocumentoNumeroParteTesto != null && this.DocumentoNumeroParteTesto.Length < 0)
            {
                yield return new ValidationResult("Invalid value for DocumentoNumeroParteTesto, length must be greater than 0.", new [] { "DocumentoNumeroParteTesto" });
            }

            yield break;
        }
    }

}

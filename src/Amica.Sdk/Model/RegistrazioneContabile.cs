/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.2.2
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
    /// Registrazione contabile.
    /// </summary>
    [DataContract(Name = "RegistrazioneContabile")]
    public partial class RegistrazioneContabile : IValidatableObject
    {
        /// <summary>
        /// Tipo di registrazione.
        /// </summary>
        /// <value>Tipo di registrazione.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TipoRegistrazioneEnum
        {
            /// <summary>
            /// Enum Acquisto for value: Acquisto
            /// </summary>
            [EnumMember(Value = "Acquisto")]
            Acquisto = 1,

            /// <summary>
            /// Enum Vendita for value: Vendita
            /// </summary>
            [EnumMember(Value = "Vendita")]
            Vendita = 2,

            /// <summary>
            /// Enum Contabile for value: Contabile
            /// </summary>
            [EnumMember(Value = "Contabile")]
            Contabile = 3
        }


        /// <summary>
        /// Tipo di registrazione.
        /// </summary>
        /// <value>Tipo di registrazione.</value>
        [DataMember(Name = "tipoRegistrazione", EmitDefaultValue = false)]
        public TipoRegistrazioneEnum? TipoRegistrazione { get; set; }
        /// <summary>
        /// Positività o negatività dell&#39;IVA.
        /// </summary>
        /// <value>Positività o negatività dell&#39;IVA.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SegnoIvaEnum
        {
            /// <summary>
            /// Enum Positivo for value: Positivo
            /// </summary>
            [EnumMember(Value = "Positivo")]
            Positivo = 1,

            /// <summary>
            /// Enum Negativo for value: Negativo
            /// </summary>
            [EnumMember(Value = "Negativo")]
            Negativo = 2
        }


        /// <summary>
        /// Positività o negatività dell&#39;IVA.
        /// </summary>
        /// <value>Positività o negatività dell&#39;IVA.</value>
        [DataMember(Name = "segnoIva", EmitDefaultValue = false)]
        public SegnoIvaEnum? SegnoIva { get; set; }
        /// <summary>
        /// Tipo di operazione contabile.
        /// </summary>
        /// <value>Tipo di operazione contabile.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TipoOperazioneEnum
        {
            /// <summary>
            /// Enum Apertura for value: Apertura
            /// </summary>
            [EnumMember(Value = "Apertura")]
            Apertura = 1,

            /// <summary>
            /// Enum Chiusura for value: Chiusura
            /// </summary>
            [EnumMember(Value = "Chiusura")]
            Chiusura = 2,

            /// <summary>
            /// Enum Normale for value: Normale
            /// </summary>
            [EnumMember(Value = "Normale")]
            Normale = 3
        }


        /// <summary>
        /// Tipo di operazione contabile.
        /// </summary>
        /// <value>Tipo di operazione contabile.</value>
        [DataMember(Name = "tipoOperazione", EmitDefaultValue = false)]
        public TipoOperazioneEnum? TipoOperazione { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrazioneContabile" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="data">Data della registrazione contabile..</param>
        /// <param name="dataCompetenza">Data di competenza..</param>
        /// <param name="numero">Numero della registrazione contabile..</param>
        /// <param name="tipoRegistrazione">Tipo di registrazione..</param>
        /// <param name="segnoIva">Positività o negatività dell&#39;IVA..</param>
        /// <param name="tipoOperazione">Tipo di operazione contabile..</param>
        /// <param name="registroIvaId">Identificativo univoco di &#x60;RegistroIva&#x60;..</param>
        /// <param name="anagraficaId">Identificativo univoco di &#x60;Anagrafica&#x60;..</param>
        /// <param name="pagamentoId">Identificativo univoco di &#x60;Pagamento&#x60;..</param>
        /// <param name="documentoId">Identificativo univoco di &#x60;Documento&#x60;..</param>
        /// <param name="documento">documento.</param>
        /// <param name="dataDocumento">Data del documento di riferimento..</param>
        /// <param name="documentoNumeroParteTesto">La parte alfanumerica del numero documento, se presente (es. \&quot;bis\&quot;)..</param>
        /// <param name="documentoNumeroParteNumerica">La parte numerica del numero documento (es. \&quot;99\&quot;)..</param>
        /// <param name="documentoNumero">Numero documento completo, es. \&quot;99/bis\&quot;. Il documento deve essere già stato salvato sul backend..</param>
        /// <param name="protocollo">Numero di protocollo..</param>
        /// <param name="descrizione">Descrizione..</param>
        /// <param name="note">Note..</param>
        /// <param name="righe">Righe della registrazione contabile..</param>
        /// <param name="castellettoIva">Importo IVA da versare..</param>
        /// <param name="scadenze">Scadenze..</param>
        public RegistrazioneContabile(int id = default(int), int concurrencyToken = default(int), DateTime data = default(DateTime), DateTime? dataCompetenza = default(DateTime?), int numero = default(int), TipoRegistrazioneEnum? tipoRegistrazione = default(TipoRegistrazioneEnum?), SegnoIvaEnum? segnoIva = default(SegnoIvaEnum?), TipoOperazioneEnum? tipoOperazione = default(TipoOperazioneEnum?), int? registroIvaId = default(int?), int? anagraficaId = default(int?), int? pagamentoId = default(int?), int? documentoId = default(int?), Documento documento = default(Documento), DateTime? dataDocumento = default(DateTime?), string documentoNumeroParteTesto = default(string), int? documentoNumeroParteNumerica = default(int?), string documentoNumero = default(string), int? protocollo = default(int?), string descrizione = default(string), string note = default(string), List<RegistrazioneContabileRiga> righe = default(List<RegistrazioneContabileRiga>), List<CastellettoIva> castellettoIva = default(List<CastellettoIva>), List<Scadenza> scadenze = default(List<Scadenza>))
        {
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.Data = data;
            this.DataCompetenza = dataCompetenza;
            this.Numero = numero;
            this.TipoRegistrazione = tipoRegistrazione;
            this.SegnoIva = segnoIva;
            this.TipoOperazione = tipoOperazione;
            this.RegistroIvaId = registroIvaId;
            this.AnagraficaId = anagraficaId;
            this.PagamentoId = pagamentoId;
            this.DocumentoId = documentoId;
            this.Documento = documento;
            this.DataDocumento = dataDocumento;
            this.DocumentoNumeroParteTesto = documentoNumeroParteTesto;
            this.DocumentoNumeroParteNumerica = documentoNumeroParteNumerica;
            this.DocumentoNumero = documentoNumero;
            this.Protocollo = protocollo;
            this.Descrizione = descrizione;
            this.Note = note;
            this.Righe = righe;
            this.CastellettoIva = castellettoIva;
            this.Scadenze = scadenze;
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
        /// Data della registrazione contabile.
        /// </summary>
        /// <value>Data della registrazione contabile.</value>
        [DataMember(Name = "data", EmitDefaultValue = false)]
        public DateTime Data { get; set; }

        /// <summary>
        /// Data di competenza.
        /// </summary>
        /// <value>Data di competenza.</value>
        [DataMember(Name = "dataCompetenza", EmitDefaultValue = true)]
        public DateTime? DataCompetenza { get; set; }

        /// <summary>
        /// Numero della registrazione contabile.
        /// </summary>
        /// <value>Numero della registrazione contabile.</value>
        /*
        <example>42</example>
        */
        [DataMember(Name = "numero", EmitDefaultValue = false)]
        public int Numero { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;RegistroIva&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;RegistroIva&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "registroIvaId", EmitDefaultValue = true)]
        public int? RegistroIvaId { get; set; }

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
        /// Data del documento di riferimento.
        /// </summary>
        /// <value>Data del documento di riferimento.</value>
        [DataMember(Name = "dataDocumento", EmitDefaultValue = true)]
        public DateTime? DataDocumento { get; set; }

        /// <summary>
        /// La parte alfanumerica del numero documento, se presente (es. \&quot;bis\&quot;).
        /// </summary>
        /// <value>La parte alfanumerica del numero documento, se presente (es. \&quot;bis\&quot;).</value>
        /*
        <example>bis</example>
        */
        [DataMember(Name = "documentoNumeroParteTesto", EmitDefaultValue = true)]
        public string DocumentoNumeroParteTesto { get; set; }

        /// <summary>
        /// La parte numerica del numero documento (es. \&quot;99\&quot;).
        /// </summary>
        /// <value>La parte numerica del numero documento (es. \&quot;99\&quot;).</value>
        /*
        <example>99</example>
        */
        [DataMember(Name = "documentoNumeroParteNumerica", EmitDefaultValue = true)]
        public int? DocumentoNumeroParteNumerica { get; set; }

        /// <summary>
        /// Numero documento completo, es. \&quot;99/bis\&quot;. Il documento deve essere già stato salvato sul backend.
        /// </summary>
        /// <value>Numero documento completo, es. \&quot;99/bis\&quot;. Il documento deve essere già stato salvato sul backend.</value>
        /*
        <example>99/bis</example>
        */
        [DataMember(Name = "documentoNumero", EmitDefaultValue = true)]
        public string DocumentoNumero { get; set; }

        /// <summary>
        /// Numero di protocollo.
        /// </summary>
        /// <value>Numero di protocollo.</value>
        /*
        <example>42</example>
        */
        [DataMember(Name = "protocollo", EmitDefaultValue = true)]
        public int? Protocollo { get; set; }

        /// <summary>
        /// Descrizione.
        /// </summary>
        /// <value>Descrizione.</value>
        /*
        <example>Fattura di acquisto materiali</example>
        */
        [DataMember(Name = "descrizione", EmitDefaultValue = true)]
        public string Descrizione { get; set; }

        /// <summary>
        /// Note.
        /// </summary>
        /// <value>Note.</value>
        /*
        <example>Registrazione da verificare</example>
        */
        [DataMember(Name = "note", EmitDefaultValue = true)]
        public string Note { get; set; }

        /// <summary>
        /// Righe della registrazione contabile.
        /// </summary>
        /// <value>Righe della registrazione contabile.</value>
        [DataMember(Name = "righe", EmitDefaultValue = true)]
        public List<RegistrazioneContabileRiga> Righe { get; set; }

        /// <summary>
        /// Importo IVA da versare.
        /// </summary>
        /// <value>Importo IVA da versare.</value>
        [DataMember(Name = "castellettoIva", EmitDefaultValue = true)]
        public List<CastellettoIva> CastellettoIva { get; set; }

        /// <summary>
        /// Scadenze.
        /// </summary>
        /// <value>Scadenze.</value>
        [DataMember(Name = "scadenze", EmitDefaultValue = true)]
        public List<Scadenza> Scadenze { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class RegistrazioneContabile {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  Data: ").Append(Data).Append("\n");
            sb.Append("  DataCompetenza: ").Append(DataCompetenza).Append("\n");
            sb.Append("  Numero: ").Append(Numero).Append("\n");
            sb.Append("  TipoRegistrazione: ").Append(TipoRegistrazione).Append("\n");
            sb.Append("  SegnoIva: ").Append(SegnoIva).Append("\n");
            sb.Append("  TipoOperazione: ").Append(TipoOperazione).Append("\n");
            sb.Append("  RegistroIvaId: ").Append(RegistroIvaId).Append("\n");
            sb.Append("  AnagraficaId: ").Append(AnagraficaId).Append("\n");
            sb.Append("  PagamentoId: ").Append(PagamentoId).Append("\n");
            sb.Append("  DocumentoId: ").Append(DocumentoId).Append("\n");
            sb.Append("  Documento: ").Append(Documento).Append("\n");
            sb.Append("  DataDocumento: ").Append(DataDocumento).Append("\n");
            sb.Append("  DocumentoNumeroParteTesto: ").Append(DocumentoNumeroParteTesto).Append("\n");
            sb.Append("  DocumentoNumeroParteNumerica: ").Append(DocumentoNumeroParteNumerica).Append("\n");
            sb.Append("  DocumentoNumero: ").Append(DocumentoNumero).Append("\n");
            sb.Append("  Protocollo: ").Append(Protocollo).Append("\n");
            sb.Append("  Descrizione: ").Append(Descrizione).Append("\n");
            sb.Append("  Note: ").Append(Note).Append("\n");
            sb.Append("  Righe: ").Append(Righe).Append("\n");
            sb.Append("  CastellettoIva: ").Append(CastellettoIva).Append("\n");
            sb.Append("  Scadenze: ").Append(Scadenze).Append("\n");
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
            // Numero (int) maximum
            if (this.Numero > (int)2147483647)
            {
                yield return new ValidationResult("Invalid value for Numero, must be a value less than or equal to 2147483647.", new [] { "Numero" });
            }

            // Numero (int) minimum
            if (this.Numero < (int)1)
            {
                yield return new ValidationResult("Invalid value for Numero, must be a value greater than or equal to 1.", new [] { "Numero" });
            }

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

            // DocumentoNumeroParteNumerica (int?) maximum
            if (this.DocumentoNumeroParteNumerica > (int?)2147483647)
            {
                yield return new ValidationResult("Invalid value for DocumentoNumeroParteNumerica, must be a value less than or equal to 2147483647.", new [] { "DocumentoNumeroParteNumerica" });
            }

            // DocumentoNumeroParteNumerica (int?) minimum
            if (this.DocumentoNumeroParteNumerica < (int?)1)
            {
                yield return new ValidationResult("Invalid value for DocumentoNumeroParteNumerica, must be a value greater than or equal to 1.", new [] { "DocumentoNumeroParteNumerica" });
            }

            // Protocollo (int?) maximum
            if (this.Protocollo > (int?)2147483647)
            {
                yield return new ValidationResult("Invalid value for Protocollo, must be a value less than or equal to 2147483647.", new [] { "Protocollo" });
            }

            // Protocollo (int?) minimum
            if (this.Protocollo < (int?)1)
            {
                yield return new ValidationResult("Invalid value for Protocollo, must be a value greater than or equal to 1.", new [] { "Protocollo" });
            }

            // Descrizione (string) maxLength
            if (this.Descrizione != null && this.Descrizione.Length > 60)
            {
                yield return new ValidationResult("Invalid value for Descrizione, length must be less than 60.", new [] { "Descrizione" });
            }

            // Descrizione (string) minLength
            if (this.Descrizione != null && this.Descrizione.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Descrizione, length must be greater than 0.", new [] { "Descrizione" });
            }

            yield break;
        }
    }

}

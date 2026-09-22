/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.1.0
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
    /// RisorsaFinanziaria
    /// </summary>
    [DataContract(Name = "RisorsaFinanziaria")]
    public partial class RisorsaFinanziaria : IValidatableObject
    {
        /// <summary>
        /// Defines Tipologia
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TipologiaEnum
        {
            /// <summary>
            /// Enum Banca for value: Banca
            /// </summary>
            [EnumMember(Value = "Banca")]
            Banca = 1,

            /// <summary>
            /// Enum Cassa for value: Cassa
            /// </summary>
            [EnumMember(Value = "Cassa")]
            Cassa = 2,

            /// <summary>
            /// Enum CartaCredito for value: CartaCredito
            /// </summary>
            [EnumMember(Value = "CartaCredito")]
            CartaCredito = 3,

            /// <summary>
            /// Enum Altro for value: Altro
            /// </summary>
            [EnumMember(Value = "Altro")]
            Altro = 4
        }


        /// <summary>
        /// Gets or Sets Tipologia
        /// </summary>
        [DataMember(Name = "tipologia", EmitDefaultValue = false)]
        public TipologiaEnum? Tipologia { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="RisorsaFinanziaria" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected RisorsaFinanziaria() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RisorsaFinanziaria" /> class.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="concurrencyToken">concurrencyToken.</param>
        /// <param name="nome">nome (required).</param>
        /// <param name="indirizzo">indirizzo.</param>
        /// <param name="localit">localit.</param>
        /// <param name="cap">cap.</param>
        /// <param name="provincia">provincia.</param>
        /// <param name="telefono1">telefono1.</param>
        /// <param name="telefono2">telefono2.</param>
        /// <param name="fax">fax.</param>
        /// <param name="contatto1Nome">contatto1Nome.</param>
        /// <param name="contatto1Incarico">contatto1Incarico.</param>
        /// <param name="contatto1Telefono1">contatto1Telefono1.</param>
        /// <param name="contatto1Telefono2">contatto1Telefono2.</param>
        /// <param name="contatto1EMail">contatto1EMail.</param>
        /// <param name="contatto2Nome">contatto2Nome.</param>
        /// <param name="contatto2Incarico">contatto2Incarico.</param>
        /// <param name="contatto2Telefono1">contatto2Telefono1.</param>
        /// <param name="contatto2Telefono2">contatto2Telefono2.</param>
        /// <param name="contatto2EMail">contatto2EMail.</param>
        /// <param name="codiceAbi">codiceAbi.</param>
        /// <param name="codiceCab">codiceCab.</param>
        /// <param name="contoCorrente">contoCorrente.</param>
        /// <param name="codiceCin">codiceCin.</param>
        /// <param name="codiceCinEur">codiceCinEur.</param>
        /// <param name="codiceCinPaese">codiceCinPaese.</param>
        /// <param name="codiceSwift">codiceSwift.</param>
        /// <param name="codiceIban">codiceIban.</param>
        /// <param name="web">web.</param>
        /// <param name="tipologia">tipologia.</param>
        public RisorsaFinanziaria(int id = default(int), int concurrencyToken = default(int), string nome = default(string), string indirizzo = default(string), string localit = default(string), string cap = default(string), string provincia = default(string), string telefono1 = default(string), string telefono2 = default(string), string fax = default(string), string contatto1Nome = default(string), string contatto1Incarico = default(string), string contatto1Telefono1 = default(string), string contatto1Telefono2 = default(string), string contatto1EMail = default(string), string contatto2Nome = default(string), string contatto2Incarico = default(string), string contatto2Telefono1 = default(string), string contatto2Telefono2 = default(string), string contatto2EMail = default(string), string codiceAbi = default(string), string codiceCab = default(string), string contoCorrente = default(string), string codiceCin = default(string), string codiceCinEur = default(string), string codiceCinPaese = default(string), string codiceSwift = default(string), string codiceIban = default(string), string web = default(string), TipologiaEnum? tipologia = default(TipologiaEnum?))
        {
            // to ensure "nome" is required (not null)
            if (nome == null)
            {
                throw new ArgumentNullException("nome is a required property for RisorsaFinanziaria and cannot be null");
            }
            this.Nome = nome;
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.Indirizzo = indirizzo;
            this.Localit = localit;
            this.Cap = cap;
            this.Provincia = provincia;
            this.Telefono1 = telefono1;
            this.Telefono2 = telefono2;
            this.Fax = fax;
            this.Contatto1Nome = contatto1Nome;
            this.Contatto1Incarico = contatto1Incarico;
            this.Contatto1Telefono1 = contatto1Telefono1;
            this.Contatto1Telefono2 = contatto1Telefono2;
            this.Contatto1EMail = contatto1EMail;
            this.Contatto2Nome = contatto2Nome;
            this.Contatto2Incarico = contatto2Incarico;
            this.Contatto2Telefono1 = contatto2Telefono1;
            this.Contatto2Telefono2 = contatto2Telefono2;
            this.Contatto2EMail = contatto2EMail;
            this.CodiceAbi = codiceAbi;
            this.CodiceCab = codiceCab;
            this.ContoCorrente = contoCorrente;
            this.CodiceCin = codiceCin;
            this.CodiceCinEur = codiceCinEur;
            this.CodiceCinPaese = codiceCinPaese;
            this.CodiceSwift = codiceSwift;
            this.CodiceIban = codiceIban;
            this.Web = web;
            this.Tipologia = tipologia;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or Sets ConcurrencyToken
        /// </summary>
        [DataMember(Name = "concurrencyToken", EmitDefaultValue = false)]
        public int ConcurrencyToken { get; set; }

        /// <summary>
        /// Gets or Sets Nome
        /// </summary>
        [DataMember(Name = "nome", IsRequired = true, EmitDefaultValue = true)]
        public string Nome { get; set; }

        /// <summary>
        /// Gets or Sets Indirizzo
        /// </summary>
        [DataMember(Name = "indirizzo", EmitDefaultValue = true)]
        public string Indirizzo { get; set; }

        /// <summary>
        /// Gets or Sets Localit
        /// </summary>
        [DataMember(Name = "località", EmitDefaultValue = true)]
        public string Localit { get; set; }

        /// <summary>
        /// Gets or Sets Cap
        /// </summary>
        [DataMember(Name = "cap", EmitDefaultValue = true)]
        public string Cap { get; set; }

        /// <summary>
        /// Gets or Sets Provincia
        /// </summary>
        [DataMember(Name = "provincia", EmitDefaultValue = true)]
        public string Provincia { get; set; }

        /// <summary>
        /// Gets or Sets Telefono1
        /// </summary>
        [DataMember(Name = "telefono1", EmitDefaultValue = true)]
        public string Telefono1 { get; set; }

        /// <summary>
        /// Gets or Sets Telefono2
        /// </summary>
        [DataMember(Name = "telefono2", EmitDefaultValue = true)]
        public string Telefono2 { get; set; }

        /// <summary>
        /// Gets or Sets Fax
        /// </summary>
        [DataMember(Name = "fax", EmitDefaultValue = true)]
        public string Fax { get; set; }

        /// <summary>
        /// Gets or Sets Contatto1Nome
        /// </summary>
        [DataMember(Name = "contatto1Nome", EmitDefaultValue = true)]
        public string Contatto1Nome { get; set; }

        /// <summary>
        /// Gets or Sets Contatto1Incarico
        /// </summary>
        [DataMember(Name = "contatto1Incarico", EmitDefaultValue = true)]
        public string Contatto1Incarico { get; set; }

        /// <summary>
        /// Gets or Sets Contatto1Telefono1
        /// </summary>
        [DataMember(Name = "contatto1Telefono1", EmitDefaultValue = true)]
        public string Contatto1Telefono1 { get; set; }

        /// <summary>
        /// Gets or Sets Contatto1Telefono2
        /// </summary>
        [DataMember(Name = "contatto1Telefono2", EmitDefaultValue = true)]
        public string Contatto1Telefono2 { get; set; }

        /// <summary>
        /// Gets or Sets Contatto1EMail
        /// </summary>
        [DataMember(Name = "contatto1EMail", EmitDefaultValue = true)]
        public string Contatto1EMail { get; set; }

        /// <summary>
        /// Gets or Sets Contatto2Nome
        /// </summary>
        [DataMember(Name = "contatto2Nome", EmitDefaultValue = true)]
        public string Contatto2Nome { get; set; }

        /// <summary>
        /// Gets or Sets Contatto2Incarico
        /// </summary>
        [DataMember(Name = "contatto2Incarico", EmitDefaultValue = true)]
        public string Contatto2Incarico { get; set; }

        /// <summary>
        /// Gets or Sets Contatto2Telefono1
        /// </summary>
        [DataMember(Name = "contatto2Telefono1", EmitDefaultValue = true)]
        public string Contatto2Telefono1 { get; set; }

        /// <summary>
        /// Gets or Sets Contatto2Telefono2
        /// </summary>
        [DataMember(Name = "contatto2Telefono2", EmitDefaultValue = true)]
        public string Contatto2Telefono2 { get; set; }

        /// <summary>
        /// Gets or Sets Contatto2EMail
        /// </summary>
        [DataMember(Name = "contatto2EMail", EmitDefaultValue = true)]
        public string Contatto2EMail { get; set; }

        /// <summary>
        /// Gets or Sets CodiceAbi
        /// </summary>
        [DataMember(Name = "codiceAbi", EmitDefaultValue = true)]
        public string CodiceAbi { get; set; }

        /// <summary>
        /// Gets or Sets CodiceCab
        /// </summary>
        [DataMember(Name = "codiceCab", EmitDefaultValue = true)]
        public string CodiceCab { get; set; }

        /// <summary>
        /// Gets or Sets ContoCorrente
        /// </summary>
        [DataMember(Name = "contoCorrente", EmitDefaultValue = true)]
        public string ContoCorrente { get; set; }

        /// <summary>
        /// Gets or Sets CodiceCin
        /// </summary>
        [DataMember(Name = "codiceCin", EmitDefaultValue = true)]
        public string CodiceCin { get; set; }

        /// <summary>
        /// Gets or Sets CodiceCinEur
        /// </summary>
        [DataMember(Name = "codiceCinEur", EmitDefaultValue = true)]
        public string CodiceCinEur { get; set; }

        /// <summary>
        /// Gets or Sets CodiceCinPaese
        /// </summary>
        [DataMember(Name = "codiceCinPaese", EmitDefaultValue = true)]
        public string CodiceCinPaese { get; set; }

        /// <summary>
        /// Gets or Sets CodiceSwift
        /// </summary>
        [DataMember(Name = "codiceSwift", EmitDefaultValue = true)]
        public string CodiceSwift { get; set; }

        /// <summary>
        /// Gets or Sets CodiceIban
        /// </summary>
        [DataMember(Name = "codiceIban", EmitDefaultValue = true)]
        public string CodiceIban { get; set; }

        /// <summary>
        /// Gets or Sets Web
        /// </summary>
        [DataMember(Name = "web", EmitDefaultValue = true)]
        public string Web { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class RisorsaFinanziaria {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  Nome: ").Append(Nome).Append("\n");
            sb.Append("  Indirizzo: ").Append(Indirizzo).Append("\n");
            sb.Append("  Localit: ").Append(Localit).Append("\n");
            sb.Append("  Cap: ").Append(Cap).Append("\n");
            sb.Append("  Provincia: ").Append(Provincia).Append("\n");
            sb.Append("  Telefono1: ").Append(Telefono1).Append("\n");
            sb.Append("  Telefono2: ").Append(Telefono2).Append("\n");
            sb.Append("  Fax: ").Append(Fax).Append("\n");
            sb.Append("  Contatto1Nome: ").Append(Contatto1Nome).Append("\n");
            sb.Append("  Contatto1Incarico: ").Append(Contatto1Incarico).Append("\n");
            sb.Append("  Contatto1Telefono1: ").Append(Contatto1Telefono1).Append("\n");
            sb.Append("  Contatto1Telefono2: ").Append(Contatto1Telefono2).Append("\n");
            sb.Append("  Contatto1EMail: ").Append(Contatto1EMail).Append("\n");
            sb.Append("  Contatto2Nome: ").Append(Contatto2Nome).Append("\n");
            sb.Append("  Contatto2Incarico: ").Append(Contatto2Incarico).Append("\n");
            sb.Append("  Contatto2Telefono1: ").Append(Contatto2Telefono1).Append("\n");
            sb.Append("  Contatto2Telefono2: ").Append(Contatto2Telefono2).Append("\n");
            sb.Append("  Contatto2EMail: ").Append(Contatto2EMail).Append("\n");
            sb.Append("  CodiceAbi: ").Append(CodiceAbi).Append("\n");
            sb.Append("  CodiceCab: ").Append(CodiceCab).Append("\n");
            sb.Append("  ContoCorrente: ").Append(ContoCorrente).Append("\n");
            sb.Append("  CodiceCin: ").Append(CodiceCin).Append("\n");
            sb.Append("  CodiceCinEur: ").Append(CodiceCinEur).Append("\n");
            sb.Append("  CodiceCinPaese: ").Append(CodiceCinPaese).Append("\n");
            sb.Append("  CodiceSwift: ").Append(CodiceSwift).Append("\n");
            sb.Append("  CodiceIban: ").Append(CodiceIban).Append("\n");
            sb.Append("  Web: ").Append(Web).Append("\n");
            sb.Append("  Tipologia: ").Append(Tipologia).Append("\n");
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
            // Nome (string) maxLength
            if (this.Nome != null && this.Nome.Length > 60)
            {
                yield return new ValidationResult("Invalid value for Nome, length must be less than 60.", new [] { "Nome" });
            }

            // Nome (string) minLength
            if (this.Nome != null && this.Nome.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Nome, length must be greater than 0.", new [] { "Nome" });
            }

            // Indirizzo (string) maxLength
            if (this.Indirizzo != null && this.Indirizzo.Length > 60)
            {
                yield return new ValidationResult("Invalid value for Indirizzo, length must be less than 60.", new [] { "Indirizzo" });
            }

            // Indirizzo (string) minLength
            if (this.Indirizzo != null && this.Indirizzo.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Indirizzo, length must be greater than 0.", new [] { "Indirizzo" });
            }

            // Localit (string) maxLength
            if (this.Localit != null && this.Localit.Length > 60)
            {
                yield return new ValidationResult("Invalid value for Localit, length must be less than 60.", new [] { "Localit" });
            }

            // Localit (string) minLength
            if (this.Localit != null && this.Localit.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Localit, length must be greater than 0.", new [] { "Localit" });
            }

            // Cap (string) maxLength
            if (this.Cap != null && this.Cap.Length > 8)
            {
                yield return new ValidationResult("Invalid value for Cap, length must be less than 8.", new [] { "Cap" });
            }

            // Cap (string) minLength
            if (this.Cap != null && this.Cap.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Cap, length must be greater than 0.", new [] { "Cap" });
            }

            // Provincia (string) maxLength
            if (this.Provincia != null && this.Provincia.Length > 3)
            {
                yield return new ValidationResult("Invalid value for Provincia, length must be less than 3.", new [] { "Provincia" });
            }

            // Provincia (string) minLength
            if (this.Provincia != null && this.Provincia.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Provincia, length must be greater than 0.", new [] { "Provincia" });
            }

            // Telefono1 (string) maxLength
            if (this.Telefono1 != null && this.Telefono1.Length > 25)
            {
                yield return new ValidationResult("Invalid value for Telefono1, length must be less than 25.", new [] { "Telefono1" });
            }

            // Telefono1 (string) minLength
            if (this.Telefono1 != null && this.Telefono1.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Telefono1, length must be greater than 0.", new [] { "Telefono1" });
            }

            // Telefono2 (string) maxLength
            if (this.Telefono2 != null && this.Telefono2.Length > 25)
            {
                yield return new ValidationResult("Invalid value for Telefono2, length must be less than 25.", new [] { "Telefono2" });
            }

            // Telefono2 (string) minLength
            if (this.Telefono2 != null && this.Telefono2.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Telefono2, length must be greater than 0.", new [] { "Telefono2" });
            }

            // Fax (string) maxLength
            if (this.Fax != null && this.Fax.Length > 25)
            {
                yield return new ValidationResult("Invalid value for Fax, length must be less than 25.", new [] { "Fax" });
            }

            // Fax (string) minLength
            if (this.Fax != null && this.Fax.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Fax, length must be greater than 0.", new [] { "Fax" });
            }

            // Contatto1Nome (string) maxLength
            if (this.Contatto1Nome != null && this.Contatto1Nome.Length > 60)
            {
                yield return new ValidationResult("Invalid value for Contatto1Nome, length must be less than 60.", new [] { "Contatto1Nome" });
            }

            // Contatto1Nome (string) minLength
            if (this.Contatto1Nome != null && this.Contatto1Nome.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Contatto1Nome, length must be greater than 0.", new [] { "Contatto1Nome" });
            }

            // Contatto1Incarico (string) maxLength
            if (this.Contatto1Incarico != null && this.Contatto1Incarico.Length > 40)
            {
                yield return new ValidationResult("Invalid value for Contatto1Incarico, length must be less than 40.", new [] { "Contatto1Incarico" });
            }

            // Contatto1Incarico (string) minLength
            if (this.Contatto1Incarico != null && this.Contatto1Incarico.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Contatto1Incarico, length must be greater than 0.", new [] { "Contatto1Incarico" });
            }

            // Contatto1Telefono1 (string) maxLength
            if (this.Contatto1Telefono1 != null && this.Contatto1Telefono1.Length > 25)
            {
                yield return new ValidationResult("Invalid value for Contatto1Telefono1, length must be less than 25.", new [] { "Contatto1Telefono1" });
            }

            // Contatto1Telefono1 (string) minLength
            if (this.Contatto1Telefono1 != null && this.Contatto1Telefono1.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Contatto1Telefono1, length must be greater than 0.", new [] { "Contatto1Telefono1" });
            }

            // Contatto1Telefono2 (string) maxLength
            if (this.Contatto1Telefono2 != null && this.Contatto1Telefono2.Length > 25)
            {
                yield return new ValidationResult("Invalid value for Contatto1Telefono2, length must be less than 25.", new [] { "Contatto1Telefono2" });
            }

            // Contatto1Telefono2 (string) minLength
            if (this.Contatto1Telefono2 != null && this.Contatto1Telefono2.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Contatto1Telefono2, length must be greater than 0.", new [] { "Contatto1Telefono2" });
            }

            // Contatto1EMail (string) maxLength
            if (this.Contatto1EMail != null && this.Contatto1EMail.Length > 50)
            {
                yield return new ValidationResult("Invalid value for Contatto1EMail, length must be less than 50.", new [] { "Contatto1EMail" });
            }

            // Contatto1EMail (string) minLength
            if (this.Contatto1EMail != null && this.Contatto1EMail.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Contatto1EMail, length must be greater than 0.", new [] { "Contatto1EMail" });
            }

            // Contatto2Nome (string) maxLength
            if (this.Contatto2Nome != null && this.Contatto2Nome.Length > 60)
            {
                yield return new ValidationResult("Invalid value for Contatto2Nome, length must be less than 60.", new [] { "Contatto2Nome" });
            }

            // Contatto2Nome (string) minLength
            if (this.Contatto2Nome != null && this.Contatto2Nome.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Contatto2Nome, length must be greater than 0.", new [] { "Contatto2Nome" });
            }

            // Contatto2Incarico (string) maxLength
            if (this.Contatto2Incarico != null && this.Contatto2Incarico.Length > 40)
            {
                yield return new ValidationResult("Invalid value for Contatto2Incarico, length must be less than 40.", new [] { "Contatto2Incarico" });
            }

            // Contatto2Incarico (string) minLength
            if (this.Contatto2Incarico != null && this.Contatto2Incarico.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Contatto2Incarico, length must be greater than 0.", new [] { "Contatto2Incarico" });
            }

            // Contatto2Telefono1 (string) maxLength
            if (this.Contatto2Telefono1 != null && this.Contatto2Telefono1.Length > 25)
            {
                yield return new ValidationResult("Invalid value for Contatto2Telefono1, length must be less than 25.", new [] { "Contatto2Telefono1" });
            }

            // Contatto2Telefono1 (string) minLength
            if (this.Contatto2Telefono1 != null && this.Contatto2Telefono1.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Contatto2Telefono1, length must be greater than 0.", new [] { "Contatto2Telefono1" });
            }

            // Contatto2Telefono2 (string) maxLength
            if (this.Contatto2Telefono2 != null && this.Contatto2Telefono2.Length > 25)
            {
                yield return new ValidationResult("Invalid value for Contatto2Telefono2, length must be less than 25.", new [] { "Contatto2Telefono2" });
            }

            // Contatto2Telefono2 (string) minLength
            if (this.Contatto2Telefono2 != null && this.Contatto2Telefono2.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Contatto2Telefono2, length must be greater than 0.", new [] { "Contatto2Telefono2" });
            }

            // Contatto2EMail (string) maxLength
            if (this.Contatto2EMail != null && this.Contatto2EMail.Length > 50)
            {
                yield return new ValidationResult("Invalid value for Contatto2EMail, length must be less than 50.", new [] { "Contatto2EMail" });
            }

            // Contatto2EMail (string) minLength
            if (this.Contatto2EMail != null && this.Contatto2EMail.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Contatto2EMail, length must be greater than 0.", new [] { "Contatto2EMail" });
            }

            // CodiceAbi (string) maxLength
            if (this.CodiceAbi != null && this.CodiceAbi.Length > 10)
            {
                yield return new ValidationResult("Invalid value for CodiceAbi, length must be less than 10.", new [] { "CodiceAbi" });
            }

            // CodiceAbi (string) minLength
            if (this.CodiceAbi != null && this.CodiceAbi.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceAbi, length must be greater than 0.", new [] { "CodiceAbi" });
            }

            // CodiceCab (string) maxLength
            if (this.CodiceCab != null && this.CodiceCab.Length > 10)
            {
                yield return new ValidationResult("Invalid value for CodiceCab, length must be less than 10.", new [] { "CodiceCab" });
            }

            // CodiceCab (string) minLength
            if (this.CodiceCab != null && this.CodiceCab.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceCab, length must be greater than 0.", new [] { "CodiceCab" });
            }

            // ContoCorrente (string) maxLength
            if (this.ContoCorrente != null && this.ContoCorrente.Length > 20)
            {
                yield return new ValidationResult("Invalid value for ContoCorrente, length must be less than 20.", new [] { "ContoCorrente" });
            }

            // ContoCorrente (string) minLength
            if (this.ContoCorrente != null && this.ContoCorrente.Length < 0)
            {
                yield return new ValidationResult("Invalid value for ContoCorrente, length must be greater than 0.", new [] { "ContoCorrente" });
            }

            // CodiceCin (string) maxLength
            if (this.CodiceCin != null && this.CodiceCin.Length > 1)
            {
                yield return new ValidationResult("Invalid value for CodiceCin, length must be less than 1.", new [] { "CodiceCin" });
            }

            // CodiceCin (string) minLength
            if (this.CodiceCin != null && this.CodiceCin.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceCin, length must be greater than 0.", new [] { "CodiceCin" });
            }

            // CodiceCinEur (string) maxLength
            if (this.CodiceCinEur != null && this.CodiceCinEur.Length > 2)
            {
                yield return new ValidationResult("Invalid value for CodiceCinEur, length must be less than 2.", new [] { "CodiceCinEur" });
            }

            // CodiceCinEur (string) minLength
            if (this.CodiceCinEur != null && this.CodiceCinEur.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceCinEur, length must be greater than 0.", new [] { "CodiceCinEur" });
            }

            // CodiceCinPaese (string) maxLength
            if (this.CodiceCinPaese != null && this.CodiceCinPaese.Length > 2)
            {
                yield return new ValidationResult("Invalid value for CodiceCinPaese, length must be less than 2.", new [] { "CodiceCinPaese" });
            }

            // CodiceCinPaese (string) minLength
            if (this.CodiceCinPaese != null && this.CodiceCinPaese.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceCinPaese, length must be greater than 0.", new [] { "CodiceCinPaese" });
            }

            // CodiceSwift (string) maxLength
            if (this.CodiceSwift != null && this.CodiceSwift.Length > 15)
            {
                yield return new ValidationResult("Invalid value for CodiceSwift, length must be less than 15.", new [] { "CodiceSwift" });
            }

            // CodiceSwift (string) minLength
            if (this.CodiceSwift != null && this.CodiceSwift.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceSwift, length must be greater than 0.", new [] { "CodiceSwift" });
            }

            // CodiceIban (string) maxLength
            if (this.CodiceIban != null && this.CodiceIban.Length > 34)
            {
                yield return new ValidationResult("Invalid value for CodiceIban, length must be less than 34.", new [] { "CodiceIban" });
            }

            // CodiceIban (string) minLength
            if (this.CodiceIban != null && this.CodiceIban.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceIban, length must be greater than 0.", new [] { "CodiceIban" });
            }

            yield break;
        }
    }

}

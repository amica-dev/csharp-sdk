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
    /// Listino.
    /// </summary>
    [DataContract(Name = "Listino")]
    public partial class Listino : IValidatableObject
    {
        /// <summary>
        /// Tipo di listino.
        /// </summary>
        /// <value>Tipo di listino.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TipoListinoEnum
        {
            /// <summary>
            /// Enum Vendite for value: Vendite
            /// </summary>
            [EnumMember(Value = "Vendite")]
            Vendite = 1,

            /// <summary>
            /// Enum Acquisti for value: Acquisti
            /// </summary>
            [EnumMember(Value = "Acquisti")]
            Acquisti = 2
        }


        /// <summary>
        /// Tipo di listino.
        /// </summary>
        /// <value>Tipo di listino.</value>
        /*
        <example>Vendite</example>
        */
        [DataMember(Name = "tipoListino", EmitDefaultValue = false)]
        public TipoListinoEnum? TipoListino { get; set; }
        /// <summary>
        /// Modalità di calcolo dei prezzi del listino.
        /// </summary>
        /// <value>Modalità di calcolo dei prezzi del listino.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CalcoloPrezziEnum
        {
            /// <summary>
            /// Enum Manuale for value: Manuale
            /// </summary>
            [EnumMember(Value = "Manuale")]
            Manuale = 1,

            /// <summary>
            /// Enum CostoUltimoPerListino for value: CostoUltimoPerListino
            /// </summary>
            [EnumMember(Value = "CostoUltimoPerListino")]
            CostoUltimoPerListino = 2,

            /// <summary>
            /// Enum CostoUltimoPerArticolo for value: CostoUltimoPerArticolo
            /// </summary>
            [EnumMember(Value = "CostoUltimoPerArticolo")]
            CostoUltimoPerArticolo = 3,

            /// <summary>
            /// Enum CostoMedioPerListino for value: CostoMedioPerListino
            /// </summary>
            [EnumMember(Value = "CostoMedioPerListino")]
            CostoMedioPerListino = 4,

            /// <summary>
            /// Enum CostoMedioPerArticolo for value: CostoMedioPerArticolo
            /// </summary>
            [EnumMember(Value = "CostoMedioPerArticolo")]
            CostoMedioPerArticolo = 5
        }


        /// <summary>
        /// Modalità di calcolo dei prezzi del listino.
        /// </summary>
        /// <value>Modalità di calcolo dei prezzi del listino.</value>
        /*
        <example>Manuale</example>
        */
        [DataMember(Name = "calcoloPrezzi", EmitDefaultValue = false)]
        public CalcoloPrezziEnum? CalcoloPrezzi { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Listino" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected Listino() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Listino" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="fornitoreId">Identificativo univoco di &#x60;Anagrafica&#x60; (fornitore)..</param>
        /// <param name="fornitore">fornitore.</param>
        /// <param name="valutaId">Identificativo univoco di &#x60;Valuta&#x60; (valuta)..</param>
        /// <param name="valuta">valuta.</param>
        /// <param name="nome">Nome del listino. (required).</param>
        /// <param name="ivaInclusa">Indica se i prezzi base sono IVA inclusa oppure no..</param>
        /// <param name="ricarico">Ricarico da applicare ai prezzi base di tutte le righe del listino..</param>
        /// <param name="dataCreazione">Data di creazione..</param>
        /// <param name="validoDal">Inizio validità del listino..</param>
        /// <param name="validoAl">Fine validità del listino..</param>
        /// <param name="tipoListino">Tipo di listino..</param>
        /// <param name="calcoloPrezzi">Modalità di calcolo dei prezzi del listino..</param>
        /// <param name="note">Note..</param>
        /// <param name="righe">Righe del listino..</param>
        public Listino(int id = default(int), int concurrencyToken = default(int), int? fornitoreId = default(int?), Anagrafica fornitore = default(Anagrafica), int? valutaId = default(int?), Valuta valuta = default(Valuta), string nome = default(string), bool ivaInclusa = default(bool), double ricarico = default(double), DateTime? dataCreazione = default(DateTime?), DateTime? validoDal = default(DateTime?), DateTime? validoAl = default(DateTime?), TipoListinoEnum? tipoListino = default(TipoListinoEnum?), CalcoloPrezziEnum? calcoloPrezzi = default(CalcoloPrezziEnum?), string note = default(string), List<Prezzo> righe = default(List<Prezzo>))
        {
            // to ensure "nome" is required (not null)
            if (nome == null)
            {
                throw new ArgumentNullException("nome is a required property for Listino and cannot be null");
            }
            this.Nome = nome;
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.FornitoreId = fornitoreId;
            this.Fornitore = fornitore;
            this.ValutaId = valutaId;
            this.Valuta = valuta;
            this.IvaInclusa = ivaInclusa;
            this.Ricarico = ricarico;
            this.DataCreazione = dataCreazione;
            this.ValidoDal = validoDal;
            this.ValidoAl = validoAl;
            this.TipoListino = tipoListino;
            this.CalcoloPrezzi = calcoloPrezzi;
            this.Note = note;
            this.Righe = righe;
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
        /// Identificativo univoco di &#x60;Anagrafica&#x60; (fornitore).
        /// </summary>
        /// <value>Identificativo univoco di &#x60;Anagrafica&#x60; (fornitore).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "fornitoreId", EmitDefaultValue = true)]
        public int? FornitoreId { get; set; }

        /// <summary>
        /// Gets or Sets Fornitore
        /// </summary>
        [DataMember(Name = "fornitore", EmitDefaultValue = false)]
        public Anagrafica Fornitore { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;Valuta&#x60; (valuta).
        /// </summary>
        /// <value>Identificativo univoco di &#x60;Valuta&#x60; (valuta).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "valutaId", EmitDefaultValue = true)]
        public int? ValutaId { get; set; }

        /// <summary>
        /// Gets or Sets Valuta
        /// </summary>
        [DataMember(Name = "valuta", EmitDefaultValue = false)]
        public Valuta Valuta { get; set; }

        /// <summary>
        /// Nome del listino.
        /// </summary>
        /// <value>Nome del listino.</value>
        /*
        <example>Listino vendite 2026</example>
        */
        [DataMember(Name = "nome", IsRequired = true, EmitDefaultValue = true)]
        public string Nome { get; set; }

        /// <summary>
        /// Indica se i prezzi base sono IVA inclusa oppure no.
        /// </summary>
        /// <value>Indica se i prezzi base sono IVA inclusa oppure no.</value>
        /*
        <example>true</example>
        */
        [DataMember(Name = "ivaInclusa", EmitDefaultValue = true)]
        public bool IvaInclusa { get; set; }

        /// <summary>
        /// Ricarico da applicare ai prezzi base di tutte le righe del listino.
        /// </summary>
        /// <value>Ricarico da applicare ai prezzi base di tutte le righe del listino.</value>
        /*
        <example>15.0</example>
        */
        [DataMember(Name = "ricarico", EmitDefaultValue = false)]
        public double Ricarico { get; set; }

        /// <summary>
        /// Data di creazione.
        /// </summary>
        /// <value>Data di creazione.</value>
        [DataMember(Name = "dataCreazione", EmitDefaultValue = true)]
        public DateTime? DataCreazione { get; set; }

        /// <summary>
        /// Inizio validità del listino.
        /// </summary>
        /// <value>Inizio validità del listino.</value>
        [DataMember(Name = "validoDal", EmitDefaultValue = true)]
        public DateTime? ValidoDal { get; set; }

        /// <summary>
        /// Fine validità del listino.
        /// </summary>
        /// <value>Fine validità del listino.</value>
        [DataMember(Name = "validoAl", EmitDefaultValue = true)]
        public DateTime? ValidoAl { get; set; }

        /// <summary>
        /// Note.
        /// </summary>
        /// <value>Note.</value>
        /*
        <example>Listino riservato ai clienti rivenditori.</example>
        */
        [DataMember(Name = "note", EmitDefaultValue = true)]
        public string Note { get; set; }

        /// <summary>
        /// Righe del listino.
        /// </summary>
        /// <value>Righe del listino.</value>
        [DataMember(Name = "righe", EmitDefaultValue = true)]
        public List<Prezzo> Righe { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Listino {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  FornitoreId: ").Append(FornitoreId).Append("\n");
            sb.Append("  Fornitore: ").Append(Fornitore).Append("\n");
            sb.Append("  ValutaId: ").Append(ValutaId).Append("\n");
            sb.Append("  Valuta: ").Append(Valuta).Append("\n");
            sb.Append("  Nome: ").Append(Nome).Append("\n");
            sb.Append("  IvaInclusa: ").Append(IvaInclusa).Append("\n");
            sb.Append("  Ricarico: ").Append(Ricarico).Append("\n");
            sb.Append("  DataCreazione: ").Append(DataCreazione).Append("\n");
            sb.Append("  ValidoDal: ").Append(ValidoDal).Append("\n");
            sb.Append("  ValidoAl: ").Append(ValidoAl).Append("\n");
            sb.Append("  TipoListino: ").Append(TipoListino).Append("\n");
            sb.Append("  CalcoloPrezzi: ").Append(CalcoloPrezzi).Append("\n");
            sb.Append("  Note: ").Append(Note).Append("\n");
            sb.Append("  Righe: ").Append(Righe).Append("\n");
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
            if (this.Nome != null && this.Nome.Length > 40)
            {
                yield return new ValidationResult("Invalid value for Nome, length must be less than 40.", new [] { "Nome" });
            }

            // Nome (string) minLength
            if (this.Nome != null && this.Nome.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Nome, length must be greater than 0.", new [] { "Nome" });
            }

            yield break;
        }
    }

}

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
    /// Riga di un Amica.ModelloContabile.
    /// </summary>
    [DataContract(Name = "ModelloContabileRiga")]
    public partial class ModelloContabileRiga : IValidatableObject
    {
        /// <summary>
        /// Cosa rappresenta Amica.ModelloContabileRiga.Importo.
        /// </summary>
        /// <value>Cosa rappresenta Amica.ModelloContabileRiga.Importo.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TipoImportoEnum
        {
            /// <summary>
            /// Enum NettoAPagare for value: NettoAPagare
            /// </summary>
            [EnumMember(Value = "NettoAPagare")]
            NettoAPagare = 1,

            /// <summary>
            /// Enum TotaleImponibile for value: TotaleImponibile
            /// </summary>
            [EnumMember(Value = "TotaleImponibile")]
            TotaleImponibile = 2,

            /// <summary>
            /// Enum TotaleImposta for value: TotaleImposta
            /// </summary>
            [EnumMember(Value = "TotaleImposta")]
            TotaleImposta = 3,

            /// <summary>
            /// Enum Ignora for value: Ignora
            /// </summary>
            [EnumMember(Value = "Ignora")]
            Ignora = 4,

            /// <summary>
            /// Enum TotaleSpese for value: TotaleSpese
            /// </summary>
            [EnumMember(Value = "TotaleSpese")]
            TotaleSpese = 5,

            /// <summary>
            /// Enum ImpostaDetraibile for value: ImpostaDetraibile
            /// </summary>
            [EnumMember(Value = "ImpostaDetraibile")]
            ImpostaDetraibile = 6,

            /// <summary>
            /// Enum ImpostaIndetraibile for value: ImpostaIndetraibile
            /// </summary>
            [EnumMember(Value = "ImpostaIndetraibile")]
            ImpostaIndetraibile = 7,

            /// <summary>
            /// Enum Enasarco for value: Enasarco
            /// </summary>
            [EnumMember(Value = "Enasarco")]
            Enasarco = 8,

            /// <summary>
            /// Enum RitenutaAcconto for value: RitenutaAcconto
            /// </summary>
            [EnumMember(Value = "RitenutaAcconto")]
            RitenutaAcconto = 9,

            /// <summary>
            /// Enum Abbuono for value: Abbuono
            /// </summary>
            [EnumMember(Value = "Abbuono")]
            Abbuono = 10
        }


        /// <summary>
        /// Cosa rappresenta Amica.ModelloContabileRiga.Importo.
        /// </summary>
        /// <value>Cosa rappresenta Amica.ModelloContabileRiga.Importo.</value>
        [DataMember(Name = "tipoImporto", EmitDefaultValue = false)]
        public TipoImportoEnum? TipoImporto { get; set; }
        /// <summary>
        /// Dare o avere.
        /// </summary>
        /// <value>Dare o avere.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SegnoContabileEnum
        {
            /// <summary>
            /// Enum Dare for value: Dare
            /// </summary>
            [EnumMember(Value = "Dare")]
            Dare = 1,

            /// <summary>
            /// Enum Avere for value: Avere
            /// </summary>
            [EnumMember(Value = "Avere")]
            Avere = 2
        }


        /// <summary>
        /// Dare o avere.
        /// </summary>
        /// <value>Dare o avere.</value>
        [DataMember(Name = "segnoContabile", EmitDefaultValue = false)]
        public SegnoContabileEnum? SegnoContabile { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelloContabileRiga" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected ModelloContabileRiga() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelloContabileRiga" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="modelloContabileId">Identificativo univoco di Amica.ModelloContabile..</param>
        /// <param name="causaleContabileId">Identificativo univoco di &#x60;CausaleContabile&#x60;..</param>
        /// <param name="contoRaw">Numero di conto, in formato grezzo. Sono supportati fino a cinque livelli, ciascuno espresso da un massimo di cinque caratteri allineati a destra e riempiti con spazi, ad es. \&quot;  100   20\&quot; (dieci caratteri in totale) esprime \&quot;100.20\&quot;. (required).</param>
        /// <param name="importo">Importo..</param>
        /// <param name="tipoImporto">Cosa rappresenta Amica.ModelloContabileRiga.Importo..</param>
        /// <param name="segnoContabile">Dare o avere..</param>
        public ModelloContabileRiga(int id = default(int), int concurrencyToken = default(int), int modelloContabileId = default(int), int causaleContabileId = default(int), string contoRaw = default(string), double importo = default(double), TipoImportoEnum? tipoImporto = default(TipoImportoEnum?), SegnoContabileEnum? segnoContabile = default(SegnoContabileEnum?))
        {
            // to ensure "contoRaw" is required (not null)
            if (contoRaw == null)
            {
                throw new ArgumentNullException("contoRaw is a required property for ModelloContabileRiga and cannot be null");
            }
            this.ContoRaw = contoRaw;
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.ModelloContabileId = modelloContabileId;
            this.CausaleContabileId = causaleContabileId;
            this.Importo = importo;
            this.TipoImporto = tipoImporto;
            this.SegnoContabile = segnoContabile;
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
        /// Identificativo univoco di Amica.ModelloContabile.
        /// </summary>
        /// <value>Identificativo univoco di Amica.ModelloContabile.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "modelloContabileId", EmitDefaultValue = false)]
        public int ModelloContabileId { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;CausaleContabile&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;CausaleContabile&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "causaleContabileId", EmitDefaultValue = false)]
        public int CausaleContabileId { get; set; }

        /// <summary>
        /// Numero di conto, in formato grezzo. Sono supportati fino a cinque livelli, ciascuno espresso da un massimo di cinque caratteri allineati a destra e riempiti con spazi, ad es. \&quot;  100   20\&quot; (dieci caratteri in totale) esprime \&quot;100.20\&quot;.
        /// </summary>
        /// <value>Numero di conto, in formato grezzo. Sono supportati fino a cinque livelli, ciascuno espresso da un massimo di cinque caratteri allineati a destra e riempiti con spazi, ad es. \&quot;  100   20\&quot; (dieci caratteri in totale) esprime \&quot;100.20\&quot;.</value>
        /*
        <example>  100   20</example>
        */
        [DataMember(Name = "contoRaw", IsRequired = true, EmitDefaultValue = true)]
        public string ContoRaw { get; set; }

        /// <summary>
        /// Importo.
        /// </summary>
        /// <value>Importo.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "importo", EmitDefaultValue = false)]
        public double Importo { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class ModelloContabileRiga {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  ModelloContabileId: ").Append(ModelloContabileId).Append("\n");
            sb.Append("  CausaleContabileId: ").Append(CausaleContabileId).Append("\n");
            sb.Append("  ContoRaw: ").Append(ContoRaw).Append("\n");
            sb.Append("  Importo: ").Append(Importo).Append("\n");
            sb.Append("  TipoImporto: ").Append(TipoImporto).Append("\n");
            sb.Append("  SegnoContabile: ").Append(SegnoContabile).Append("\n");
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

            yield break;
        }
    }

}

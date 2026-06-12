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
    /// Dati del Sistema di interscambio (Sdi).
    /// </summary>
    [DataContract(Name = "DatiSdi")]
    public partial class DatiSdi : IValidatableObject
    {
        /// <summary>
        /// Stato del Sistema di interscambio.
        /// </summary>
        /// <value>Stato del Sistema di interscambio.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatoSdiEnum
        {
            /// <summary>
            /// Enum DocumentoPresoInCarico for value: DocumentoPresoInCarico
            /// </summary>
            [EnumMember(Value = "DocumentoPresoInCarico")]
            DocumentoPresoInCarico = 1,

            /// <summary>
            /// Enum DocumentoInviato for value: DocumentoInviato
            /// </summary>
            [EnumMember(Value = "DocumentoInviato")]
            DocumentoInviato = 2,

            /// <summary>
            /// Enum ServizioNonDisponibile for value: ServizioNonDisponibile
            /// </summary>
            [EnumMember(Value = "ServizioNonDisponibile")]
            ServizioNonDisponibile = 3,

            /// <summary>
            /// Enum UtenteNonAbilitato for value: UtenteNonAbilitato
            /// </summary>
            [EnumMember(Value = "UtenteNonAbilitato")]
            UtenteNonAbilitato = 4,

            /// <summary>
            /// Enum DocumentoConsegnato for value: DocumentoConsegnato
            /// </summary>
            [EnumMember(Value = "DocumentoConsegnato")]
            DocumentoConsegnato = 5,

            /// <summary>
            /// Enum DocumentoNonConsegnato for value: DocumentoNonConsegnato
            /// </summary>
            [EnumMember(Value = "DocumentoNonConsegnato")]
            DocumentoNonConsegnato = 6,

            /// <summary>
            /// Enum DocumentoScartato for value: DocumentoScartato
            /// </summary>
            [EnumMember(Value = "DocumentoScartato")]
            DocumentoScartato = 7,

            /// <summary>
            /// Enum DocumentoAccettatoDalDestinatario for value: DocumentoAccettatoDalDestinatario
            /// </summary>
            [EnumMember(Value = "DocumentoAccettatoDalDestinatario")]
            DocumentoAccettatoDalDestinatario = 8,

            /// <summary>
            /// Enum DocumentoRifiutatoDalDestinatario for value: DocumentoRifiutatoDalDestinatario
            /// </summary>
            [EnumMember(Value = "DocumentoRifiutatoDalDestinatario")]
            DocumentoRifiutatoDalDestinatario = 9,

            /// <summary>
            /// Enum DocumentoConImpossibilitDiRecapito for value: DocumentoConImpossibilitàDiRecapito
            /// </summary>
            [EnumMember(Value = "DocumentoConImpossibilitàDiRecapito")]
            DocumentoConImpossibilitDiRecapito = 10,

            /// <summary>
            /// Enum DecorrenzaTermini for value: DecorrenzaTermini
            /// </summary>
            [EnumMember(Value = "DecorrenzaTermini")]
            DecorrenzaTermini = 11
        }


        /// <summary>
        /// Stato del Sistema di interscambio.
        /// </summary>
        /// <value>Stato del Sistema di interscambio.</value>
        [DataMember(Name = "statoSdi", EmitDefaultValue = true)]
        public StatoSdiEnum? StatoSdi { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="DatiSdi" /> class.
        /// </summary>
        /// <param name="identificativoSdi">Numero identificativo assegnato dal Sistema di interscambio (Sdi)..</param>
        /// <param name="statoSdi">Stato del Sistema di interscambio..</param>
        /// <param name="descrizioneStatoSdi">Stato del documento così come riportato dal Sistema di interscambio..</param>
        /// <param name="dataConsegnaSdi">Data di consegna così come riportata dal Sistema di interscambio..</param>
        public DatiSdi(string identificativoSdi = default(string), StatoSdiEnum? statoSdi = default(StatoSdiEnum?), string descrizioneStatoSdi = default(string), DateTime? dataConsegnaSdi = default(DateTime?))
        {
            this.IdentificativoSdi = identificativoSdi;
            this.StatoSdi = statoSdi;
            this.DescrizioneStatoSdi = descrizioneStatoSdi;
            this.DataConsegnaSdi = dataConsegnaSdi;
        }

        /// <summary>
        /// Numero identificativo assegnato dal Sistema di interscambio (Sdi).
        /// </summary>
        /// <value>Numero identificativo assegnato dal Sistema di interscambio (Sdi).</value>
        /*
        <example>1234567890ab</example>
        */
        [DataMember(Name = "identificativoSdi", EmitDefaultValue = true)]
        public string IdentificativoSdi { get; set; }

        /// <summary>
        /// Stato del documento così come riportato dal Sistema di interscambio.
        /// </summary>
        /// <value>Stato del documento così come riportato dal Sistema di interscambio.</value>
        /*
        <example>Consegnato al destinatario.</example>
        */
        [DataMember(Name = "descrizioneStatoSdi", EmitDefaultValue = true)]
        public string DescrizioneStatoSdi { get; set; }

        /// <summary>
        /// Data di consegna così come riportata dal Sistema di interscambio.
        /// </summary>
        /// <value>Data di consegna così come riportata dal Sistema di interscambio.</value>
        [DataMember(Name = "dataConsegnaSdi", EmitDefaultValue = true)]
        public DateTime? DataConsegnaSdi { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class DatiSdi {\n");
            sb.Append("  IdentificativoSdi: ").Append(IdentificativoSdi).Append("\n");
            sb.Append("  StatoSdi: ").Append(StatoSdi).Append("\n");
            sb.Append("  DescrizioneStatoSdi: ").Append(DescrizioneStatoSdi).Append("\n");
            sb.Append("  DataConsegnaSdi: ").Append(DataConsegnaSdi).Append("\n");
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
            // IdentificativoSdi (string) maxLength
            if (this.IdentificativoSdi != null && this.IdentificativoSdi.Length > 12)
            {
                yield return new ValidationResult("Invalid value for IdentificativoSdi, length must be less than 12.", new [] { "IdentificativoSdi" });
            }

            // IdentificativoSdi (string) minLength
            if (this.IdentificativoSdi != null && this.IdentificativoSdi.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IdentificativoSdi, length must be greater than 0.", new [] { "IdentificativoSdi" });
            }

            yield break;
        }
    }

}

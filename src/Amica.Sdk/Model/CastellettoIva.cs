/*
 * Customers API
 *
 * La Customers API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare la Customers API registra la tua applicazione sull'**App Store di Amica**. Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  - **Authorization Code con PKCE** (consigliato per app che operano per conto di un utente Amica): l'utente autorizza la tua app tramite la pagina di consenso dell'identity provider; la tua app riceve un `code` da scambiare con un access token.    ```   GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256   POST {identity-provider}/connect/token        grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri>   ```  - **Client Credentials** (per integrazioni server-to-server, senza utente interattivo):    ```   POST {identity-provider}/connect/token   Content-Type: application/x-www-form-urlencoded    grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=read write documenti contabilita   ```    > **Nota:** il flusso Client Credentials è disponibile solo per le **app private**, non per quelle pubblicate sul marketplace.  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — il token non possiede lo scope richiesto. | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Alcune risposte `400` includono un campo `overridable` che indica se l'errore di validazione può essere ignorato impostando il parametro `validationOverride` a `true`.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
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
    /// Importo IVA a debito associato a una Amica.CastellettoIva.RegistrazioneContabile.
    /// </summary>
    [DataContract(Name = "CastellettoIva")]
    public partial class CastellettoIva : IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CastellettoIva" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="registrazioneContabileId">Identificativo univoco di Amica.CastellettoIva.RegistrazioneContabile (registrazione contabile)..</param>
        /// <param name="registrazioneContabile">registrazioneContabile.</param>
        /// <param name="causaleIvaId">Identificativo univoco di Amica.CastellettoIva.CausaleIva..</param>
        /// <param name="causaleIva">causaleIva.</param>
        /// <param name="registroIvaId">Identificativo univoco di &#x60;RegistroIva&#x60;..</param>
        /// <param name="imponibile">Imponibile..</param>
        /// <param name="imposta">Imposta..</param>
        /// <param name="detraibile">Quota detraibile..</param>
        /// <param name="indetraibile">Quota non detraibile..</param>
        public CastellettoIva(int id = default(int), int concurrencyToken = default(int), int registrazioneContabileId = default(int), RegistrazioneContabile registrazioneContabile = default(RegistrazioneContabile), int causaleIvaId = default(int), CausaleIva causaleIva = default(CausaleIva), int registroIvaId = default(int), double imponibile = default(double), double imposta = default(double), double detraibile = default(double), double indetraibile = default(double))
        {
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.RegistrazioneContabileId = registrazioneContabileId;
            this.RegistrazioneContabile = registrazioneContabile;
            this.CausaleIvaId = causaleIvaId;
            this.CausaleIva = causaleIva;
            this.RegistroIvaId = registroIvaId;
            this.Imponibile = imponibile;
            this.Imposta = imposta;
            this.Detraibile = detraibile;
            this.Indetraibile = indetraibile;
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
        /// Identificativo univoco di Amica.CastellettoIva.RegistrazioneContabile (registrazione contabile).
        /// </summary>
        /// <value>Identificativo univoco di Amica.CastellettoIva.RegistrazioneContabile (registrazione contabile).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "registrazioneContabileId", EmitDefaultValue = false)]
        public int RegistrazioneContabileId { get; set; }

        /// <summary>
        /// Gets or Sets RegistrazioneContabile
        /// </summary>
        [DataMember(Name = "registrazioneContabile", EmitDefaultValue = false)]
        public RegistrazioneContabile RegistrazioneContabile { get; set; }

        /// <summary>
        /// Identificativo univoco di Amica.CastellettoIva.CausaleIva.
        /// </summary>
        /// <value>Identificativo univoco di Amica.CastellettoIva.CausaleIva.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "causaleIvaId", EmitDefaultValue = false)]
        public int CausaleIvaId { get; set; }

        /// <summary>
        /// Gets or Sets CausaleIva
        /// </summary>
        [DataMember(Name = "causaleIva", EmitDefaultValue = false)]
        public CausaleIva CausaleIva { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;RegistroIva&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;RegistroIva&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "registroIvaId", EmitDefaultValue = false)]
        public int RegistroIvaId { get; set; }

        /// <summary>
        /// Imponibile.
        /// </summary>
        /// <value>Imponibile.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "imponibile", EmitDefaultValue = false)]
        public double Imponibile { get; set; }

        /// <summary>
        /// Imposta.
        /// </summary>
        /// <value>Imposta.</value>
        /*
        <example>275.0</example>
        */
        [DataMember(Name = "imposta", EmitDefaultValue = false)]
        public double Imposta { get; set; }

        /// <summary>
        /// Quota detraibile.
        /// </summary>
        /// <value>Quota detraibile.</value>
        /*
        <example>275.0</example>
        */
        [DataMember(Name = "detraibile", EmitDefaultValue = false)]
        public double Detraibile { get; set; }

        /// <summary>
        /// Quota non detraibile.
        /// </summary>
        /// <value>Quota non detraibile.</value>
        /*
        <example>0.0</example>
        */
        [DataMember(Name = "indetraibile", EmitDefaultValue = false)]
        public double Indetraibile { get; set; }

        /// <summary>
        /// Importo totale (imponibile + imposta).
        /// </summary>
        /// <value>Importo totale (imponibile + imposta).</value>
        /*
        <example>1525.0</example>
        */
        [DataMember(Name = "totale", EmitDefaultValue = false)]
        public double Totale { get; private set; }

        /// <summary>
        /// Returns false as Totale should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializeTotale()
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
            sb.Append("class CastellettoIva {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  RegistrazioneContabileId: ").Append(RegistrazioneContabileId).Append("\n");
            sb.Append("  RegistrazioneContabile: ").Append(RegistrazioneContabile).Append("\n");
            sb.Append("  CausaleIvaId: ").Append(CausaleIvaId).Append("\n");
            sb.Append("  CausaleIva: ").Append(CausaleIva).Append("\n");
            sb.Append("  RegistroIvaId: ").Append(RegistroIvaId).Append("\n");
            sb.Append("  Imponibile: ").Append(Imponibile).Append("\n");
            sb.Append("  Imposta: ").Append(Imposta).Append("\n");
            sb.Append("  Detraibile: ").Append(Detraibile).Append("\n");
            sb.Append("  Indetraibile: ").Append(Indetraibile).Append("\n");
            sb.Append("  Totale: ").Append(Totale).Append("\n");
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
            yield break;
        }
    }

}

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
    /// Indirizzo.
    /// </summary>
    [DataContract(Name = "Indirizzo")]
    public partial class Indirizzo : IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Indirizzo" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="anagraficaId">Identificativo univoco di Amica.Anagrafica..</param>
        /// <param name="ragioneSociale1">Ragione sociale, oppure nome e cognome della persona fisica..</param>
        /// <param name="ragioneSociale2">Eventuale seconda riga della ragione sociale..</param>
        /// <param name="via">Indirizzo (via)..</param>
        /// <param name="localit">Località..</param>
        /// <param name="cap">CAP o codice di avviamento postale..</param>
        /// <param name="provincia">Provincia, contea o nome dell&#39;area..</param>
        /// <param name="telefono1">Numero di telefono principale..</param>
        /// <param name="telefono2">Numero di telefono secondario..</param>
        /// <param name="fax">Numero di fax..</param>
        /// <param name="email">Indirizzo email..</param>
        /// <param name="stato">Indica se l&#39;indirizzo è attivo oppure no..</param>
        public Indirizzo(int id = default(int), int concurrencyToken = default(int), int anagraficaId = default(int), string ragioneSociale1 = default(string), string ragioneSociale2 = default(string), string via = default(string), string localit = default(string), string cap = default(string), string provincia = default(string), string telefono1 = default(string), string telefono2 = default(string), string fax = default(string), string email = default(string), bool stato = default(bool))
        {
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.AnagraficaId = anagraficaId;
            this.RagioneSociale1 = ragioneSociale1;
            this.RagioneSociale2 = ragioneSociale2;
            this.Via = via;
            this.Localit = localit;
            this.Cap = cap;
            this.Provincia = provincia;
            this.Telefono1 = telefono1;
            this.Telefono2 = telefono2;
            this.Fax = fax;
            this.Email = email;
            this.Stato = stato;
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
        /// Identificativo univoco di Amica.Anagrafica.
        /// </summary>
        /// <value>Identificativo univoco di Amica.Anagrafica.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "anagraficaId", EmitDefaultValue = false)]
        public int AnagraficaId { get; set; }

        /// <summary>
        /// Ragione sociale, oppure nome e cognome della persona fisica.
        /// </summary>
        /// <value>Ragione sociale, oppure nome e cognome della persona fisica.</value>
        /*
        <example>Acme S.r.l.</example>
        */
        [DataMember(Name = "ragioneSociale1", EmitDefaultValue = true)]
        public string RagioneSociale1 { get; set; }

        /// <summary>
        /// Eventuale seconda riga della ragione sociale.
        /// </summary>
        /// <value>Eventuale seconda riga della ragione sociale.</value>
        /*
        <example>Divisione Logistica</example>
        */
        [DataMember(Name = "ragioneSociale2", EmitDefaultValue = true)]
        public string RagioneSociale2 { get; set; }

        /// <summary>
        /// Indirizzo (via).
        /// </summary>
        /// <value>Indirizzo (via).</value>
        /*
        <example>Via Garibaldi 10</example>
        */
        [DataMember(Name = "via", EmitDefaultValue = true)]
        public string Via { get; set; }

        /// <summary>
        /// Località.
        /// </summary>
        /// <value>Località.</value>
        /*
        <example>Milano</example>
        */
        [DataMember(Name = "località", EmitDefaultValue = true)]
        public string Localit { get; set; }

        /// <summary>
        /// CAP o codice di avviamento postale.
        /// </summary>
        /// <value>CAP o codice di avviamento postale.</value>
        /*
        <example>20121</example>
        */
        [DataMember(Name = "cap", EmitDefaultValue = true)]
        public string Cap { get; set; }

        /// <summary>
        /// Provincia, contea o nome dell&#39;area.
        /// </summary>
        /// <value>Provincia, contea o nome dell&#39;area.</value>
        /*
        <example>MI</example>
        */
        [DataMember(Name = "provincia", EmitDefaultValue = true)]
        public string Provincia { get; set; }

        /// <summary>
        /// Numero di telefono principale.
        /// </summary>
        /// <value>Numero di telefono principale.</value>
        /*
        <example>02 1234567</example>
        */
        [DataMember(Name = "telefono1", EmitDefaultValue = true)]
        public string Telefono1 { get; set; }

        /// <summary>
        /// Numero di telefono secondario.
        /// </summary>
        /// <value>Numero di telefono secondario.</value>
        /*
        <example>02 7654321</example>
        */
        [DataMember(Name = "telefono2", EmitDefaultValue = true)]
        public string Telefono2 { get; set; }

        /// <summary>
        /// Numero di fax.
        /// </summary>
        /// <value>Numero di fax.</value>
        /*
        <example>02 1234568</example>
        */
        [DataMember(Name = "fax", EmitDefaultValue = true)]
        public string Fax { get; set; }

        /// <summary>
        /// Indirizzo email.
        /// </summary>
        /// <value>Indirizzo email.</value>
        /*
        <example>sede@acme.it</example>
        */
        [DataMember(Name = "email", EmitDefaultValue = true)]
        public string Email { get; set; }

        /// <summary>
        /// Indica se l&#39;indirizzo è attivo oppure no.
        /// </summary>
        /// <value>Indica se l&#39;indirizzo è attivo oppure no.</value>
        /*
        <example>true</example>
        */
        [DataMember(Name = "stato", EmitDefaultValue = true)]
        public bool Stato { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Indirizzo {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  AnagraficaId: ").Append(AnagraficaId).Append("\n");
            sb.Append("  RagioneSociale1: ").Append(RagioneSociale1).Append("\n");
            sb.Append("  RagioneSociale2: ").Append(RagioneSociale2).Append("\n");
            sb.Append("  Via: ").Append(Via).Append("\n");
            sb.Append("  Localit: ").Append(Localit).Append("\n");
            sb.Append("  Cap: ").Append(Cap).Append("\n");
            sb.Append("  Provincia: ").Append(Provincia).Append("\n");
            sb.Append("  Telefono1: ").Append(Telefono1).Append("\n");
            sb.Append("  Telefono2: ").Append(Telefono2).Append("\n");
            sb.Append("  Fax: ").Append(Fax).Append("\n");
            sb.Append("  Email: ").Append(Email).Append("\n");
            sb.Append("  Stato: ").Append(Stato).Append("\n");
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
            // RagioneSociale1 (string) maxLength
            if (this.RagioneSociale1 != null && this.RagioneSociale1.Length > 60)
            {
                yield return new ValidationResult("Invalid value for RagioneSociale1, length must be less than 60.", new [] { "RagioneSociale1" });
            }

            // RagioneSociale1 (string) minLength
            if (this.RagioneSociale1 != null && this.RagioneSociale1.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RagioneSociale1, length must be greater than 0.", new [] { "RagioneSociale1" });
            }

            // RagioneSociale2 (string) maxLength
            if (this.RagioneSociale2 != null && this.RagioneSociale2.Length > 60)
            {
                yield return new ValidationResult("Invalid value for RagioneSociale2, length must be less than 60.", new [] { "RagioneSociale2" });
            }

            // RagioneSociale2 (string) minLength
            if (this.RagioneSociale2 != null && this.RagioneSociale2.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RagioneSociale2, length must be greater than 0.", new [] { "RagioneSociale2" });
            }

            // Via (string) maxLength
            if (this.Via != null && this.Via.Length > 60)
            {
                yield return new ValidationResult("Invalid value for Via, length must be less than 60.", new [] { "Via" });
            }

            // Via (string) minLength
            if (this.Via != null && this.Via.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Via, length must be greater than 0.", new [] { "Via" });
            }

            // Localit (string) maxLength
            if (this.Localit != null && this.Localit.Length > 40)
            {
                yield return new ValidationResult("Invalid value for Localit, length must be less than 40.", new [] { "Localit" });
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

            // Email (string) maxLength
            if (this.Email != null && this.Email.Length > 25)
            {
                yield return new ValidationResult("Invalid value for Email, length must be less than 25.", new [] { "Email" });
            }

            // Email (string) minLength
            if (this.Email != null && this.Email.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Email, length must be greater than 0.", new [] { "Email" });
            }

            yield break;
        }
    }

}

/*
 * Customers API
 *
 * La Customers API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare la Customers API registra la tua applicazione sull'**App Store di Amica**. Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  - **Authorization Code con PKCE** (consigliato per app che operano per conto di un utente Amica): l'utente autorizza la tua app tramite la pagina di consenso dell'identity provider; la tua app riceve un `code` da scambiare con un access token.    ```   GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256   POST {identity-provider}/connect/token        grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri>   ```  - **Client Credentials** (per integrazioni server-to-server, senza utente interattivo):    ```   POST {identity-provider}/connect/token   Content-Type: application/x-www-form-urlencoded    grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=read write documenti contabilita   ```    > **Nota:** il flusso Client Credentials è disponibile solo per le **app private**, non per quelle pubblicate sul marketplace.  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |- -- -- --|- -- -- -- -- -- --| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |- -- -- -- -- --|- -- -- -- -- -- --| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |- -- -- -- -|- -- -- -- -- -- --| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |- -- -- -- -- --|- -- -- -- -- -- --| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |- -- -- -- -|- -- -- -- -- -- --| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — il token non possiede lo scope richiesto. | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Alcune risposte `400` includono un campo `overridable` che indica se l'errore di validazione può essere ignorato impostando il parametro `validationOverride` a `true`.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
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
    /// Utilizzato dal metodo M:Amica.DocumentoService.Compute(Amica.DocumentoComputeData,Amica.RequestSettings,System.Boolean) di &#x60;DocumentoService&#x60;.
    /// </summary>
    [DataContract(Name = "DocumentoComputeData")]
    public partial class DocumentoComputeData : IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentoComputeData" /> class.
        /// </summary>
        /// <param name="righe">Metadati delle righe del documento..</param>
        /// <param name="speseDocumento">Metadati dell&#39;elenco delle spese..</param>
        /// <param name="scontoPercentuale">Sconto, in percentuale, da calcolare sull&#39;intero importo del documento..</param>
        /// <param name="scontoPagamentoPercentuale">Sconto riconosciuto in base alla modalità di pagamento utilizzata (es.: +5% in caso di rimessa diretta)..</param>
        /// <param name="scontoIncondizionatoImporto">Sconto, in valuta, da applicare all&#39;intero importo del documento..</param>
        /// <param name="cassaPrevidenzialePercentuale">Contributo cassa previdenziale (percentuale) da applicare al documento..</param>
        /// <param name="cassaPrevidenzialeCausaleIvaId">Identificativo univoco della causale IVA della cassa previdenziale..</param>
        /// <param name="ritenutaIncludeCassaPrevidenziale">Indica se la ritenuta debba essere applicata all&#39;importo della cassa previdenziale riportato nel documento..</param>
        /// <param name="quotaRitenutaSuImponibile">Quota dell&#39;imponibile su cui applicare la ritenuta..</param>
        /// <param name="ritenutaPercentuale">Quota di ritenuta da applicare al documento..</param>
        /// <param name="enasarcoImporto">Importo Enasarco..</param>
        /// <param name="abbuonoImporto">Importo dell&#39;abbuono..</param>
        /// <param name="bonusFiscalePercentuale">Importo del bonus fiscale..</param>
        public DocumentoComputeData(List<DocumentoComputeDataBeneServizio> righe = default(List<DocumentoComputeDataBeneServizio>), List<DocumentoComputeDataSpesa> speseDocumento = default(List<DocumentoComputeDataSpesa>), double scontoPercentuale = default(double), double scontoPagamentoPercentuale = default(double), double scontoIncondizionatoImporto = default(double), double cassaPrevidenzialePercentuale = default(double), int? cassaPrevidenzialeCausaleIvaId = default(int?), bool ritenutaIncludeCassaPrevidenziale = default(bool), double quotaRitenutaSuImponibile = default(double), double ritenutaPercentuale = default(double), double enasarcoImporto = default(double), double abbuonoImporto = default(double), double bonusFiscalePercentuale = default(double))
        {
            this.Righe = righe;
            this.SpeseDocumento = speseDocumento;
            this.ScontoPercentuale = scontoPercentuale;
            this.ScontoPagamentoPercentuale = scontoPagamentoPercentuale;
            this.ScontoIncondizionatoImporto = scontoIncondizionatoImporto;
            this.CassaPrevidenzialePercentuale = cassaPrevidenzialePercentuale;
            this.CassaPrevidenzialeCausaleIvaId = cassaPrevidenzialeCausaleIvaId;
            this.RitenutaIncludeCassaPrevidenziale = ritenutaIncludeCassaPrevidenziale;
            this.QuotaRitenutaSuImponibile = quotaRitenutaSuImponibile;
            this.RitenutaPercentuale = ritenutaPercentuale;
            this.EnasarcoImporto = enasarcoImporto;
            this.AbbuonoImporto = abbuonoImporto;
            this.BonusFiscalePercentuale = bonusFiscalePercentuale;
        }

        /// <summary>
        /// Metadati delle righe del documento.
        /// </summary>
        /// <value>Metadati delle righe del documento.</value>
        [DataMember(Name = "righe", EmitDefaultValue = true)]
        public List<DocumentoComputeDataBeneServizio> Righe { get; set; }

        /// <summary>
        /// Metadati dell&#39;elenco delle spese.
        /// </summary>
        /// <value>Metadati dell&#39;elenco delle spese.</value>
        [DataMember(Name = "speseDocumento", EmitDefaultValue = true)]
        public List<DocumentoComputeDataSpesa> SpeseDocumento { get; set; }

        /// <summary>
        /// Sconto, in percentuale, da calcolare sull&#39;intero importo del documento.
        /// </summary>
        /// <value>Sconto, in percentuale, da calcolare sull&#39;intero importo del documento.</value>
        /*
        <example>5.0</example>
        */
        [DataMember(Name = "scontoPercentuale", EmitDefaultValue = false)]
        public double ScontoPercentuale { get; set; }

        /// <summary>
        /// Sconto riconosciuto in base alla modalità di pagamento utilizzata (es.: +5% in caso di rimessa diretta).
        /// </summary>
        /// <value>Sconto riconosciuto in base alla modalità di pagamento utilizzata (es.: +5% in caso di rimessa diretta).</value>
        /*
        <example>5.0</example>
        */
        [DataMember(Name = "scontoPagamentoPercentuale", EmitDefaultValue = false)]
        public double ScontoPagamentoPercentuale { get; set; }

        /// <summary>
        /// Sconto, in valuta, da applicare all&#39;intero importo del documento.
        /// </summary>
        /// <value>Sconto, in valuta, da applicare all&#39;intero importo del documento.</value>
        /*
        <example>10.0</example>
        */
        [DataMember(Name = "scontoIncondizionatoImporto", EmitDefaultValue = false)]
        public double ScontoIncondizionatoImporto { get; set; }

        /// <summary>
        /// Contributo cassa previdenziale (percentuale) da applicare al documento.
        /// </summary>
        /// <value>Contributo cassa previdenziale (percentuale) da applicare al documento.</value>
        /*
        <example>4.0</example>
        */
        [DataMember(Name = "cassaPrevidenzialePercentuale", EmitDefaultValue = false)]
        public double CassaPrevidenzialePercentuale { get; set; }

        /// <summary>
        /// Identificativo univoco della causale IVA della cassa previdenziale.
        /// </summary>
        /// <value>Identificativo univoco della causale IVA della cassa previdenziale.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "cassaPrevidenzialeCausaleIvaId", EmitDefaultValue = true)]
        public int? CassaPrevidenzialeCausaleIvaId { get; set; }

        /// <summary>
        /// Indica se la ritenuta debba essere applicata all&#39;importo della cassa previdenziale riportato nel documento.
        /// </summary>
        /// <value>Indica se la ritenuta debba essere applicata all&#39;importo della cassa previdenziale riportato nel documento.</value>
        /*
        <example>false</example>
        */
        [DataMember(Name = "ritenutaIncludeCassaPrevidenziale", EmitDefaultValue = true)]
        public bool RitenutaIncludeCassaPrevidenziale { get; set; }

        /// <summary>
        /// Quota dell&#39;imponibile su cui applicare la ritenuta.
        /// </summary>
        /// <value>Quota dell&#39;imponibile su cui applicare la ritenuta.</value>
        /*
        <example>100.0</example>
        */
        [DataMember(Name = "quotaRitenutaSuImponibile", EmitDefaultValue = false)]
        public double QuotaRitenutaSuImponibile { get; set; }

        /// <summary>
        /// Quota di ritenuta da applicare al documento.
        /// </summary>
        /// <value>Quota di ritenuta da applicare al documento.</value>
        /*
        <example>20.0</example>
        */
        [DataMember(Name = "ritenutaPercentuale", EmitDefaultValue = false)]
        public double RitenutaPercentuale { get; set; }

        /// <summary>
        /// Importo Enasarco.
        /// </summary>
        /// <value>Importo Enasarco.</value>
        /*
        <example>0.0</example>
        */
        [DataMember(Name = "enasarcoImporto", EmitDefaultValue = false)]
        public double EnasarcoImporto { get; set; }

        /// <summary>
        /// Importo dell&#39;abbuono.
        /// </summary>
        /// <value>Importo dell&#39;abbuono.</value>
        /*
        <example>0.0</example>
        */
        [DataMember(Name = "abbuonoImporto", EmitDefaultValue = false)]
        public double AbbuonoImporto { get; set; }

        /// <summary>
        /// Importo del bonus fiscale.
        /// </summary>
        /// <value>Importo del bonus fiscale.</value>
        /*
        <example>0.0</example>
        */
        [DataMember(Name = "bonusFiscalePercentuale", EmitDefaultValue = false)]
        public double BonusFiscalePercentuale { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class DocumentoComputeData {\n");
            sb.Append("  Righe: ").Append(Righe).Append("\n");
            sb.Append("  SpeseDocumento: ").Append(SpeseDocumento).Append("\n");
            sb.Append("  ScontoPercentuale: ").Append(ScontoPercentuale).Append("\n");
            sb.Append("  ScontoPagamentoPercentuale: ").Append(ScontoPagamentoPercentuale).Append("\n");
            sb.Append("  ScontoIncondizionatoImporto: ").Append(ScontoIncondizionatoImporto).Append("\n");
            sb.Append("  CassaPrevidenzialePercentuale: ").Append(CassaPrevidenzialePercentuale).Append("\n");
            sb.Append("  CassaPrevidenzialeCausaleIvaId: ").Append(CassaPrevidenzialeCausaleIvaId).Append("\n");
            sb.Append("  RitenutaIncludeCassaPrevidenziale: ").Append(RitenutaIncludeCassaPrevidenziale).Append("\n");
            sb.Append("  QuotaRitenutaSuImponibile: ").Append(QuotaRitenutaSuImponibile).Append("\n");
            sb.Append("  RitenutaPercentuale: ").Append(RitenutaPercentuale).Append("\n");
            sb.Append("  EnasarcoImporto: ").Append(EnasarcoImporto).Append("\n");
            sb.Append("  AbbuonoImporto: ").Append(AbbuonoImporto).Append("\n");
            sb.Append("  BonusFiscalePercentuale: ").Append(BonusFiscalePercentuale).Append("\n");
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

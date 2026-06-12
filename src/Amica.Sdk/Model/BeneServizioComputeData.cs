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
    /// Utilizzato dal metodo di calcolo M:Amica.BeneServizioService.Compute(Amica.BeneServizioComputeData,Amica.RequestSettings) di &#x60;BeneServizioService&#x60;.
    /// </summary>
    [DataContract(Name = "BeneServizioComputeData")]
    public partial class BeneServizioComputeData : IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeneServizioComputeData" /> class.
        /// </summary>
        /// <param name="ivaEsclusa">Indica se il calcolo debba essere effettuato a partire da Amica.BeneServizioComputeData.Prezzo o da Amica.BeneServizioComputeData.PrezzoIvato..</param>
        /// <param name="causaleIvaId">Identificativo univoco di Amica.CausaleIva..</param>
        /// <param name="listinoId">Identificativo univoco di Amica.Listino..</param>
        /// <param name="quantit">Quantità..</param>
        /// <param name="prezzo">Prezzo IVA esclusa..</param>
        /// <param name="prezzoIvato">Prezzo IVA inclusa..</param>
        /// <param name="sconto1">Primo sconto percentuale, applicato al prezzo del bene o servizio..</param>
        /// <param name="sconto2">Secondo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto1.</param>
        /// <param name="sconto3">Terzo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto2.</param>
        /// <param name="sconto4">Quarto sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto3.</param>
        /// <param name="scontoIncondizionato">Sconto incondizionato, in valuta, applicato al prezzo del bene o servizio..</param>
        public BeneServizioComputeData(bool ivaEsclusa = default(bool), int? causaleIvaId = default(int?), int? listinoId = default(int?), double quantit = default(double), double prezzo = default(double), double prezzoIvato = default(double), double sconto1 = default(double), double sconto2 = default(double), double sconto3 = default(double), double sconto4 = default(double), double scontoIncondizionato = default(double))
        {
            this.IvaEsclusa = ivaEsclusa;
            this.CausaleIvaId = causaleIvaId;
            this.ListinoId = listinoId;
            this.Quantit = quantit;
            this.Prezzo = prezzo;
            this.PrezzoIvato = prezzoIvato;
            this.Sconto1 = sconto1;
            this.Sconto2 = sconto2;
            this.Sconto3 = sconto3;
            this.Sconto4 = sconto4;
            this.ScontoIncondizionato = scontoIncondizionato;
        }

        /// <summary>
        /// Indica se il calcolo debba essere effettuato a partire da Amica.BeneServizioComputeData.Prezzo o da Amica.BeneServizioComputeData.PrezzoIvato.
        /// </summary>
        /// <value>Indica se il calcolo debba essere effettuato a partire da Amica.BeneServizioComputeData.Prezzo o da Amica.BeneServizioComputeData.PrezzoIvato.</value>
        /*
        <example>true</example>
        */
        [DataMember(Name = "ivaEsclusa", EmitDefaultValue = true)]
        public bool IvaEsclusa { get; set; }

        /// <summary>
        /// Identificativo univoco di Amica.CausaleIva.
        /// </summary>
        /// <value>Identificativo univoco di Amica.CausaleIva.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "causaleIvaId", EmitDefaultValue = true)]
        public int? CausaleIvaId { get; set; }

        /// <summary>
        /// Identificativo univoco di Amica.Listino.
        /// </summary>
        /// <value>Identificativo univoco di Amica.Listino.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "listinoId", EmitDefaultValue = true)]
        public int? ListinoId { get; set; }

        /// <summary>
        /// Quantità.
        /// </summary>
        /// <value>Quantità.</value>
        /*
        <example>10</example>
        */
        [DataMember(Name = "quantità", EmitDefaultValue = false)]
        public double Quantit { get; set; }

        /// <summary>
        /// Prezzo IVA esclusa.
        /// </summary>
        /// <value>Prezzo IVA esclusa.</value>
        /*
        <example>125.0</example>
        */
        [DataMember(Name = "prezzo", EmitDefaultValue = false)]
        public double Prezzo { get; set; }

        /// <summary>
        /// Prezzo IVA inclusa.
        /// </summary>
        /// <value>Prezzo IVA inclusa.</value>
        /*
        <example>152.5</example>
        */
        [DataMember(Name = "prezzoIvato", EmitDefaultValue = false)]
        public double PrezzoIvato { get; set; }

        /// <summary>
        /// Primo sconto percentuale, applicato al prezzo del bene o servizio.
        /// </summary>
        /// <value>Primo sconto percentuale, applicato al prezzo del bene o servizio.</value>
        /*
        <example>5.0</example>
        */
        [DataMember(Name = "sconto1", EmitDefaultValue = false)]
        public double Sconto1 { get; set; }

        /// <summary>
        /// Secondo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto1
        /// </summary>
        /// <value>Secondo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto1</value>
        /*
        <example>0.0</example>
        */
        [DataMember(Name = "sconto2", EmitDefaultValue = false)]
        public double Sconto2 { get; set; }

        /// <summary>
        /// Terzo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto2
        /// </summary>
        /// <value>Terzo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto2</value>
        /*
        <example>0.0</example>
        */
        [DataMember(Name = "sconto3", EmitDefaultValue = false)]
        public double Sconto3 { get; set; }

        /// <summary>
        /// Quarto sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto3
        /// </summary>
        /// <value>Quarto sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto3</value>
        /*
        <example>0.0</example>
        */
        [DataMember(Name = "sconto4", EmitDefaultValue = false)]
        public double Sconto4 { get; set; }

        /// <summary>
        /// Sconto incondizionato, in valuta, applicato al prezzo del bene o servizio.
        /// </summary>
        /// <value>Sconto incondizionato, in valuta, applicato al prezzo del bene o servizio.</value>
        /*
        <example>0.0</example>
        */
        [DataMember(Name = "scontoIncondizionato", EmitDefaultValue = false)]
        public double ScontoIncondizionato { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class BeneServizioComputeData {\n");
            sb.Append("  IvaEsclusa: ").Append(IvaEsclusa).Append("\n");
            sb.Append("  CausaleIvaId: ").Append(CausaleIvaId).Append("\n");
            sb.Append("  ListinoId: ").Append(ListinoId).Append("\n");
            sb.Append("  Quantit: ").Append(Quantit).Append("\n");
            sb.Append("  Prezzo: ").Append(Prezzo).Append("\n");
            sb.Append("  PrezzoIvato: ").Append(PrezzoIvato).Append("\n");
            sb.Append("  Sconto1: ").Append(Sconto1).Append("\n");
            sb.Append("  Sconto2: ").Append(Sconto2).Append("\n");
            sb.Append("  Sconto3: ").Append(Sconto3).Append("\n");
            sb.Append("  Sconto4: ").Append(Sconto4).Append("\n");
            sb.Append("  ScontoIncondizionato: ").Append(ScontoIncondizionato).Append("\n");
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

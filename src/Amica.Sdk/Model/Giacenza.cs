/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.3.4
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
    /// Giacenza corrente (inventario).
    /// </summary>
    [DataContract(Name = "Giacenza")]
    public partial class Giacenza : IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Giacenza" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="magazzinoId">Identificativo univoco di &#x60;Magazzino&#x60;..</param>
        /// <param name="magazzino">magazzino.</param>
        /// <param name="articoloId">Identificativo univoco di &#x60;Articolo&#x60;..</param>
        /// <param name="articolo">articolo.</param>
        /// <param name="quantitInGiacenza">Quantità in giacenza..</param>
        /// <param name="quantitImpegnata">Quantità impegnata..</param>
        /// <param name="quantitOrdinata">Quantità ordinata..</param>
        /// <param name="scortaMinima">Scorta minima..</param>
        /// <param name="gestioneSottoscorta">Indica se devono essere generati gli avvisi di sottoscorta..</param>
        /// <param name="dataUltimoCarico">Data dell&#39;ultimo carico..</param>
        /// <param name="dataUltimoScarico">Data dell&#39;ultimo scarico..</param>
        /// <param name="caricoQuantit">Quantità totale caricata..</param>
        /// <param name="caricoValore">Valore totale caricato..</param>
        /// <param name="scaricoQuantit">Quantità totale scaricata..</param>
        /// <param name="scaricoValore">Valore totale scaricato..</param>
        /// <param name="identificativo">Numero di serie, o qualsiasi altro identificativo..</param>
        /// <param name="identificativoEsteso">Identificativo esteso..</param>
        /// <param name="data">Data di scadenza, produzione, fine garanzia o altro..</param>
        /// <param name="colore">Colore..</param>
        public Giacenza(int id = default(int), int concurrencyToken = default(int), int magazzinoId = default(int), Magazzino magazzino = default(Magazzino), int articoloId = default(int), Articolo articolo = default(Articolo), double quantitInGiacenza = default(double), double quantitImpegnata = default(double), double quantitOrdinata = default(double), double scortaMinima = default(double), bool gestioneSottoscorta = default(bool), DateTime? dataUltimoCarico = default(DateTime?), DateTime? dataUltimoScarico = default(DateTime?), double caricoQuantit = default(double), double caricoValore = default(double), double scaricoQuantit = default(double), double scaricoValore = default(double), string identificativo = default(string), string identificativoEsteso = default(string), DateTime? data = default(DateTime?), string colore = default(string))
        {
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.MagazzinoId = magazzinoId;
            this.Magazzino = magazzino;
            this.ArticoloId = articoloId;
            this.Articolo = articolo;
            this.QuantitInGiacenza = quantitInGiacenza;
            this.QuantitImpegnata = quantitImpegnata;
            this.QuantitOrdinata = quantitOrdinata;
            this.ScortaMinima = scortaMinima;
            this.GestioneSottoscorta = gestioneSottoscorta;
            this.DataUltimoCarico = dataUltimoCarico;
            this.DataUltimoScarico = dataUltimoScarico;
            this.CaricoQuantit = caricoQuantit;
            this.CaricoValore = caricoValore;
            this.ScaricoQuantit = scaricoQuantit;
            this.ScaricoValore = scaricoValore;
            this.Identificativo = identificativo;
            this.IdentificativoEsteso = identificativoEsteso;
            this.Data = data;
            this.Colore = colore;
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
        /// Identificativo univoco di &#x60;Magazzino&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;Magazzino&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "magazzinoId", EmitDefaultValue = false)]
        public int MagazzinoId { get; set; }

        /// <summary>
        /// Gets or Sets Magazzino
        /// </summary>
        [DataMember(Name = "magazzino", EmitDefaultValue = false)]
        public Magazzino Magazzino { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;Articolo&#x60;.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;Articolo&#x60;.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "articoloId", EmitDefaultValue = false)]
        public int ArticoloId { get; set; }

        /// <summary>
        /// Gets or Sets Articolo
        /// </summary>
        [DataMember(Name = "articolo", EmitDefaultValue = false)]
        public Articolo Articolo { get; set; }

        /// <summary>
        /// Quantità in giacenza.
        /// </summary>
        /// <value>Quantità in giacenza.</value>
        /*
        <example>150.0</example>
        */
        [DataMember(Name = "quantitàInGiacenza", EmitDefaultValue = false)]
        public double QuantitInGiacenza { get; set; }

        /// <summary>
        /// Quantità impegnata.
        /// </summary>
        /// <value>Quantità impegnata.</value>
        /*
        <example>25.0</example>
        */
        [DataMember(Name = "quantitàImpegnata", EmitDefaultValue = false)]
        public double QuantitImpegnata { get; set; }

        /// <summary>
        /// Quantità ordinata.
        /// </summary>
        /// <value>Quantità ordinata.</value>
        /*
        <example>50.0</example>
        */
        [DataMember(Name = "quantitàOrdinata", EmitDefaultValue = false)]
        public double QuantitOrdinata { get; set; }

        /// <summary>
        /// Scorta minima.
        /// </summary>
        /// <value>Scorta minima.</value>
        /*
        <example>10.0</example>
        */
        [DataMember(Name = "scortaMinima", EmitDefaultValue = false)]
        public double ScortaMinima { get; set; }

        /// <summary>
        /// Indica se devono essere generati gli avvisi di sottoscorta.
        /// </summary>
        /// <value>Indica se devono essere generati gli avvisi di sottoscorta.</value>
        /*
        <example>true</example>
        */
        [DataMember(Name = "gestioneSottoscorta", EmitDefaultValue = true)]
        public bool GestioneSottoscorta { get; set; }

        /// <summary>
        /// Data dell&#39;ultimo carico.
        /// </summary>
        /// <value>Data dell&#39;ultimo carico.</value>
        [DataMember(Name = "dataUltimoCarico", EmitDefaultValue = true)]
        public DateTime? DataUltimoCarico { get; set; }

        /// <summary>
        /// Data dell&#39;ultimo scarico.
        /// </summary>
        /// <value>Data dell&#39;ultimo scarico.</value>
        [DataMember(Name = "dataUltimoScarico", EmitDefaultValue = true)]
        public DateTime? DataUltimoScarico { get; set; }

        /// <summary>
        /// Quantità totale caricata.
        /// </summary>
        /// <value>Quantità totale caricata.</value>
        /*
        <example>500.0</example>
        */
        [DataMember(Name = "caricoQuantità", EmitDefaultValue = false)]
        public double CaricoQuantit { get; set; }

        /// <summary>
        /// Valore totale caricato.
        /// </summary>
        /// <value>Valore totale caricato.</value>
        /*
        <example>6250.0</example>
        */
        [DataMember(Name = "caricoValore", EmitDefaultValue = false)]
        public double CaricoValore { get; set; }

        /// <summary>
        /// Quantità totale scaricata.
        /// </summary>
        /// <value>Quantità totale scaricata.</value>
        /*
        <example>350.0</example>
        */
        [DataMember(Name = "scaricoQuantità", EmitDefaultValue = false)]
        public double ScaricoQuantit { get; set; }

        /// <summary>
        /// Valore totale scaricato.
        /// </summary>
        /// <value>Valore totale scaricato.</value>
        /*
        <example>4375.0</example>
        */
        [DataMember(Name = "scaricoValore", EmitDefaultValue = false)]
        public double ScaricoValore { get; set; }

        /// <summary>
        /// Numero di serie, o qualsiasi altro identificativo.
        /// </summary>
        /// <value>Numero di serie, o qualsiasi altro identificativo.</value>
        /*
        <example>SN-2026-00123</example>
        */
        [DataMember(Name = "identificativo", EmitDefaultValue = true)]
        public string Identificativo { get; set; }

        /// <summary>
        /// Identificativo esteso.
        /// </summary>
        /// <value>Identificativo esteso.</value>
        /*
        <example>LOTTO-A-2026</example>
        */
        [DataMember(Name = "identificativoEsteso", EmitDefaultValue = true)]
        public string IdentificativoEsteso { get; set; }

        /// <summary>
        /// Data di scadenza, produzione, fine garanzia o altro.
        /// </summary>
        /// <value>Data di scadenza, produzione, fine garanzia o altro.</value>
        [DataMember(Name = "data", EmitDefaultValue = true)]
        public DateTime? Data { get; set; }

        /// <summary>
        /// Colore.
        /// </summary>
        /// <value>Colore.</value>
        /*
        <example>Rosso</example>
        */
        [DataMember(Name = "colore", EmitDefaultValue = true)]
        public string Colore { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Giacenza {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  MagazzinoId: ").Append(MagazzinoId).Append("\n");
            sb.Append("  Magazzino: ").Append(Magazzino).Append("\n");
            sb.Append("  ArticoloId: ").Append(ArticoloId).Append("\n");
            sb.Append("  Articolo: ").Append(Articolo).Append("\n");
            sb.Append("  QuantitInGiacenza: ").Append(QuantitInGiacenza).Append("\n");
            sb.Append("  QuantitImpegnata: ").Append(QuantitImpegnata).Append("\n");
            sb.Append("  QuantitOrdinata: ").Append(QuantitOrdinata).Append("\n");
            sb.Append("  ScortaMinima: ").Append(ScortaMinima).Append("\n");
            sb.Append("  GestioneSottoscorta: ").Append(GestioneSottoscorta).Append("\n");
            sb.Append("  DataUltimoCarico: ").Append(DataUltimoCarico).Append("\n");
            sb.Append("  DataUltimoScarico: ").Append(DataUltimoScarico).Append("\n");
            sb.Append("  CaricoQuantit: ").Append(CaricoQuantit).Append("\n");
            sb.Append("  CaricoValore: ").Append(CaricoValore).Append("\n");
            sb.Append("  ScaricoQuantit: ").Append(ScaricoQuantit).Append("\n");
            sb.Append("  ScaricoValore: ").Append(ScaricoValore).Append("\n");
            sb.Append("  Identificativo: ").Append(Identificativo).Append("\n");
            sb.Append("  IdentificativoEsteso: ").Append(IdentificativoEsteso).Append("\n");
            sb.Append("  Data: ").Append(Data).Append("\n");
            sb.Append("  Colore: ").Append(Colore).Append("\n");
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
            // Identificativo (string) maxLength
            if (this.Identificativo != null && this.Identificativo.Length > 20)
            {
                yield return new ValidationResult("Invalid value for Identificativo, length must be less than 20.", new [] { "Identificativo" });
            }

            // Identificativo (string) minLength
            if (this.Identificativo != null && this.Identificativo.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Identificativo, length must be greater than 0.", new [] { "Identificativo" });
            }

            // Colore (string) maxLength
            if (this.Colore != null && this.Colore.Length > 20)
            {
                yield return new ValidationResult("Invalid value for Colore, length must be less than 20.", new [] { "Colore" });
            }

            // Colore (string) minLength
            if (this.Colore != null && this.Colore.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Colore, length must be greater than 0.", new [] { "Colore" });
            }

            yield break;
        }
    }

}

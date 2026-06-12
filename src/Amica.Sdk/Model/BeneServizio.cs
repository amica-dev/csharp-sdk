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
    /// Bene o servizio fatturato.
    /// </summary>
    [DataContract(Name = "BeneServizio")]
    public partial class BeneServizio : IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BeneServizio" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected BeneServizio() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BeneServizio" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="documentoId">Identificativo univoco del Amica.Documento (documento)..</param>
        /// <param name="articoloId">Identificativo univoco dell&#39;Amica.BeneServizio.Articolo (articolo o prodotto)..</param>
        /// <param name="articolo">articolo.</param>
        /// <param name="prestazioneId">Identificativo univoco della Amica.BeneServizio.Prestazione (prestazione)..</param>
        /// <param name="prestazione">prestazione.</param>
        /// <param name="magazzinoId">Identificativo univoco del Amica.BeneServizio.Magazzino (magazzino). Obbligatorio quando viene usato Amica.BeneServizio.Codice..</param>
        /// <param name="magazzino">magazzino.</param>
        /// <param name="causaleIvaId">Identificativo univoco della Amica.BeneServizio.CausaleIva..</param>
        /// <param name="causaleIva">causaleIva.</param>
        /// <param name="listinoId">Identificativo univoco del Amica.BeneServizio.Listino (listino)..</param>
        /// <param name="listino">listino.</param>
        /// <param name="ordineId">Identificativo univoco del Amica.Documento (ordine)..</param>
        /// <param name="beneServizioOrdineId">Identificativo univoco del Amica.BeneServizio (bene o servizio) all&#39;interno dell&#39;ordine referenziato da Amica.BeneServizio.OrdineId..</param>
        /// <param name="tagliaId">Identificativo univoco della Amica.BeneServizio.Taglia (taglia)..</param>
        /// <param name="taglia">taglia.</param>
        /// <param name="ritenutaFiscale">Indica se al bene o servizio deve essere applicata una ritenuta fiscale. &#x60;true&#x60; se deve essere applicata, &#x60;false&#x60; altrimenti..</param>
        /// <param name="posizione">Indice di riga all&#39;interno del documento. Questa proprietà è utilizzata solo dal client desktop Amica 20 per Windows e deve essere considerata obsoleta..</param>
        /// <param name="codice">Codice del bene o servizio..</param>
        /// <param name="descrizione">Descrizione. (required).</param>
        /// <param name="unitMisura">Unità di misura..</param>
        /// <param name="quantit">Quantità movimentata..</param>
        /// <param name="quantitEvasa">Quantità dell&#39;ordine evasa. Questo valore viene aggiornato automaticamente quando viene emessa una fattura o un DDT che contiene un riferimento a un ordine ed è attivo &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/GestioneOrdiniPreventivi\&quot;&gt;Ordini e Preventivi&lt;/a&gt;..</param>
        /// <param name="sconto1">Primo sconto percentuale, applicato al prezzo del bene o servizio..</param>
        /// <param name="sconto2">Secondo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto1.</param>
        /// <param name="sconto3">Terzo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto2.</param>
        /// <param name="sconto4">Quarto sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto3.</param>
        /// <param name="scontoIncondizionato">Sconto incondizionato, in valuta, applicato al prezzo del bene o servizio..</param>
        /// <param name="provvigioneAgente">Provvigione corrisposta all&#39;agente, in percentuale..</param>
        /// <param name="provvigioneCapoArea">Provvigione corrisposta al capo area, in percentuale..</param>
        /// <param name="prezzoId">Identificativo univoco del Amica.BeneServizio.Prezzo (prezzo)..</param>
        /// <param name="prezzo">Prezzo..</param>
        /// <param name="prezzoScontato">Prezzo scontato, cioè al netto degli sconti percentuali indicati e dello sconto incondizionato..</param>
        /// <param name="prezzoIvato">Prezzo comprensivo di IVA..</param>
        /// <param name="importo">Importo, determinato come prodotto di Amica.BeneServizio.Quantità e Amica.BeneServizio.PrezzoScontato..</param>
        /// <param name="datiAggiuntivi">datiAggiuntivi.</param>
        public BeneServizio(int id = default(int), int concurrencyToken = default(int), int documentoId = default(int), int? articoloId = default(int?), Articolo articolo = default(Articolo), int? prestazioneId = default(int?), Articolo prestazione = default(Articolo), int? magazzinoId = default(int?), Magazzino magazzino = default(Magazzino), int? causaleIvaId = default(int?), CausaleIva causaleIva = default(CausaleIva), int? listinoId = default(int?), Listino listino = default(Listino), int? ordineId = default(int?), int? beneServizioOrdineId = default(int?), int? tagliaId = default(int?), Taglia taglia = default(Taglia), bool ritenutaFiscale = default(bool), int posizione = default(int), string codice = default(string), string descrizione = default(string), string unitMisura = default(string), double? quantit = default(double?), double quantitEvasa = default(double), double? sconto1 = default(double?), double? sconto2 = default(double?), double? sconto3 = default(double?), double? sconto4 = default(double?), double? scontoIncondizionato = default(double?), double? provvigioneAgente = default(double?), double? provvigioneCapoArea = default(double?), int? prezzoId = default(int?), double? prezzo = default(double?), double? prezzoScontato = default(double?), double? prezzoIvato = default(double?), double? importo = default(double?), DatiAggiuntivi datiAggiuntivi = default(DatiAggiuntivi))
        {
            // to ensure "descrizione" is required (not null)
            if (descrizione == null)
            {
                throw new ArgumentNullException("descrizione is a required property for BeneServizio and cannot be null");
            }
            this.Descrizione = descrizione;
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.DocumentoId = documentoId;
            this.ArticoloId = articoloId;
            this.Articolo = articolo;
            this.PrestazioneId = prestazioneId;
            this.Prestazione = prestazione;
            this.MagazzinoId = magazzinoId;
            this.Magazzino = magazzino;
            this.CausaleIvaId = causaleIvaId;
            this.CausaleIva = causaleIva;
            this.ListinoId = listinoId;
            this.Listino = listino;
            this.OrdineId = ordineId;
            this.BeneServizioOrdineId = beneServizioOrdineId;
            this.TagliaId = tagliaId;
            this.Taglia = taglia;
            this.RitenutaFiscale = ritenutaFiscale;
            this.Posizione = posizione;
            this.Codice = codice;
            this.UnitMisura = unitMisura;
            this.Quantit = quantit;
            this.QuantitEvasa = quantitEvasa;
            this.Sconto1 = sconto1;
            this.Sconto2 = sconto2;
            this.Sconto3 = sconto3;
            this.Sconto4 = sconto4;
            this.ScontoIncondizionato = scontoIncondizionato;
            this.ProvvigioneAgente = provvigioneAgente;
            this.ProvvigioneCapoArea = provvigioneCapoArea;
            this.PrezzoId = prezzoId;
            this.Prezzo = prezzo;
            this.PrezzoScontato = prezzoScontato;
            this.PrezzoIvato = prezzoIvato;
            this.Importo = importo;
            this.DatiAggiuntivi = datiAggiuntivi;
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
        /// Identificativo univoco del Amica.Documento (documento).
        /// </summary>
        /// <value>Identificativo univoco del Amica.Documento (documento).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "documentoId", EmitDefaultValue = false)]
        public int DocumentoId { get; set; }

        /// <summary>
        /// Identificativo univoco dell&#39;Amica.BeneServizio.Articolo (articolo o prodotto).
        /// </summary>
        /// <value>Identificativo univoco dell&#39;Amica.BeneServizio.Articolo (articolo o prodotto).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "articoloId", EmitDefaultValue = true)]
        public int? ArticoloId { get; set; }

        /// <summary>
        /// Gets or Sets Articolo
        /// </summary>
        [DataMember(Name = "articolo", EmitDefaultValue = false)]
        public Articolo Articolo { get; set; }

        /// <summary>
        /// Identificativo univoco della Amica.BeneServizio.Prestazione (prestazione).
        /// </summary>
        /// <value>Identificativo univoco della Amica.BeneServizio.Prestazione (prestazione).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "prestazioneId", EmitDefaultValue = true)]
        public int? PrestazioneId { get; set; }

        /// <summary>
        /// Gets or Sets Prestazione
        /// </summary>
        [DataMember(Name = "prestazione", EmitDefaultValue = false)]
        public Articolo Prestazione { get; set; }

        /// <summary>
        /// Identificativo univoco del Amica.BeneServizio.Magazzino (magazzino). Obbligatorio quando viene usato Amica.BeneServizio.Codice.
        /// </summary>
        /// <value>Identificativo univoco del Amica.BeneServizio.Magazzino (magazzino). Obbligatorio quando viene usato Amica.BeneServizio.Codice.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "magazzinoId", EmitDefaultValue = true)]
        public int? MagazzinoId { get; set; }

        /// <summary>
        /// Gets or Sets Magazzino
        /// </summary>
        [DataMember(Name = "magazzino", EmitDefaultValue = false)]
        public Magazzino Magazzino { get; set; }

        /// <summary>
        /// Identificativo univoco della Amica.BeneServizio.CausaleIva.
        /// </summary>
        /// <value>Identificativo univoco della Amica.BeneServizio.CausaleIva.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "causaleIvaId", EmitDefaultValue = true)]
        public int? CausaleIvaId { get; set; }

        /// <summary>
        /// Gets or Sets CausaleIva
        /// </summary>
        [DataMember(Name = "causaleIva", EmitDefaultValue = false)]
        public CausaleIva CausaleIva { get; set; }

        /// <summary>
        /// Identificativo univoco del Amica.BeneServizio.Listino (listino).
        /// </summary>
        /// <value>Identificativo univoco del Amica.BeneServizio.Listino (listino).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "listinoId", EmitDefaultValue = true)]
        public int? ListinoId { get; set; }

        /// <summary>
        /// Gets or Sets Listino
        /// </summary>
        [DataMember(Name = "listino", EmitDefaultValue = false)]
        public Listino Listino { get; set; }

        /// <summary>
        /// Identificativo univoco del Amica.Documento (ordine).
        /// </summary>
        /// <value>Identificativo univoco del Amica.Documento (ordine).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "ordineId", EmitDefaultValue = true)]
        public int? OrdineId { get; set; }

        /// <summary>
        /// Identificativo univoco del Amica.BeneServizio (bene o servizio) all&#39;interno dell&#39;ordine referenziato da Amica.BeneServizio.OrdineId.
        /// </summary>
        /// <value>Identificativo univoco del Amica.BeneServizio (bene o servizio) all&#39;interno dell&#39;ordine referenziato da Amica.BeneServizio.OrdineId.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "beneServizioOrdineId", EmitDefaultValue = true)]
        public int? BeneServizioOrdineId { get; set; }

        /// <summary>
        /// Identificativo univoco della Amica.BeneServizio.Taglia (taglia).
        /// </summary>
        /// <value>Identificativo univoco della Amica.BeneServizio.Taglia (taglia).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "tagliaId", EmitDefaultValue = true)]
        public int? TagliaId { get; set; }

        /// <summary>
        /// Gets or Sets Taglia
        /// </summary>
        [DataMember(Name = "taglia", EmitDefaultValue = false)]
        public Taglia Taglia { get; set; }

        /// <summary>
        /// Indica se al bene o servizio deve essere applicata una ritenuta fiscale. &#x60;true&#x60; se deve essere applicata, &#x60;false&#x60; altrimenti.
        /// </summary>
        /// <value>Indica se al bene o servizio deve essere applicata una ritenuta fiscale. &#x60;true&#x60; se deve essere applicata, &#x60;false&#x60; altrimenti.</value>
        /*
        <example>true</example>
        */
        [DataMember(Name = "ritenutaFiscale", EmitDefaultValue = true)]
        public bool RitenutaFiscale { get; set; }

        /// <summary>
        /// Indice di riga all&#39;interno del documento. Questa proprietà è utilizzata solo dal client desktop Amica 20 per Windows e deve essere considerata obsoleta.
        /// </summary>
        /// <value>Indice di riga all&#39;interno del documento. Questa proprietà è utilizzata solo dal client desktop Amica 20 per Windows e deve essere considerata obsoleta.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "posizione", EmitDefaultValue = false)]
        public int Posizione { get; set; }

        /// <summary>
        /// Codice del bene o servizio.
        /// </summary>
        /// <value>Codice del bene o servizio.</value>
        /*
        <example>ART001</example>
        */
        [DataMember(Name = "codice", EmitDefaultValue = true)]
        public string Codice { get; set; }

        /// <summary>
        /// Descrizione.
        /// </summary>
        /// <value>Descrizione.</value>
        /*
        <example>Cacciavite a croce</example>
        */
        [DataMember(Name = "descrizione", IsRequired = true, EmitDefaultValue = true)]
        public string Descrizione { get; set; }

        /// <summary>
        /// Unità di misura.
        /// </summary>
        /// <value>Unità di misura.</value>
        /*
        <example>PZ</example>
        */
        [DataMember(Name = "unitàMisura", EmitDefaultValue = true)]
        public string UnitMisura { get; set; }

        /// <summary>
        /// Quantità movimentata.
        /// </summary>
        /// <value>Quantità movimentata.</value>
        /*
        <example>10</example>
        */
        [DataMember(Name = "quantità", EmitDefaultValue = true)]
        public double? Quantit { get; set; }

        /// <summary>
        /// Quantità dell&#39;ordine evasa. Questo valore viene aggiornato automaticamente quando viene emessa una fattura o un DDT che contiene un riferimento a un ordine ed è attivo &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/GestioneOrdiniPreventivi\&quot;&gt;Ordini e Preventivi&lt;/a&gt;.
        /// </summary>
        /// <value>Quantità dell&#39;ordine evasa. Questo valore viene aggiornato automaticamente quando viene emessa una fattura o un DDT che contiene un riferimento a un ordine ed è attivo &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/GestioneOrdiniPreventivi\&quot;&gt;Ordini e Preventivi&lt;/a&gt;.</value>
        /*
        <example>4</example>
        */
        [DataMember(Name = "quantitàEvasa", EmitDefaultValue = false)]
        public double QuantitEvasa { get; set; }

        /// <summary>
        /// Quantità che resta da evadere.
        /// </summary>
        /// <value>Quantità che resta da evadere.</value>
        [DataMember(Name = "quantitàDaEvadere", EmitDefaultValue = false)]
        public double QuantitDaEvadere { get; private set; }

        /// <summary>
        /// Returns false as QuantitDaEvadere should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializeQuantitDaEvadere()
        {
            return false;
        }
        /// <summary>
        /// Primo sconto percentuale, applicato al prezzo del bene o servizio.
        /// </summary>
        /// <value>Primo sconto percentuale, applicato al prezzo del bene o servizio.</value>
        /*
        <example>5.0</example>
        */
        [DataMember(Name = "sconto1", EmitDefaultValue = true)]
        public double? Sconto1 { get; set; }

        /// <summary>
        /// Secondo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto1
        /// </summary>
        /// <value>Secondo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto1</value>
        /*
        <example>3.0</example>
        */
        [DataMember(Name = "sconto2", EmitDefaultValue = true)]
        public double? Sconto2 { get; set; }

        /// <summary>
        /// Terzo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto2
        /// </summary>
        /// <value>Terzo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto2</value>
        /*
        <example>2.0</example>
        */
        [DataMember(Name = "sconto3", EmitDefaultValue = true)]
        public double? Sconto3 { get; set; }

        /// <summary>
        /// Quarto sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto3
        /// </summary>
        /// <value>Quarto sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto3</value>
        /*
        <example>1.0</example>
        */
        [DataMember(Name = "sconto4", EmitDefaultValue = true)]
        public double? Sconto4 { get; set; }

        /// <summary>
        /// Sconto incondizionato, in valuta, applicato al prezzo del bene o servizio.
        /// </summary>
        /// <value>Sconto incondizionato, in valuta, applicato al prezzo del bene o servizio.</value>
        /*
        <example>5.0</example>
        */
        [DataMember(Name = "scontoIncondizionato", EmitDefaultValue = true)]
        public double? ScontoIncondizionato { get; set; }

        /// <summary>
        /// Provvigione corrisposta all&#39;agente, in percentuale.
        /// </summary>
        /// <value>Provvigione corrisposta all&#39;agente, in percentuale.</value>
        /*
        <example>10.0</example>
        */
        [DataMember(Name = "provvigioneAgente", EmitDefaultValue = true)]
        public double? ProvvigioneAgente { get; set; }

        /// <summary>
        /// Provvigione corrisposta al capo area, in percentuale.
        /// </summary>
        /// <value>Provvigione corrisposta al capo area, in percentuale.</value>
        /*
        <example>5.0</example>
        */
        [DataMember(Name = "provvigioneCapoArea", EmitDefaultValue = true)]
        public double? ProvvigioneCapoArea { get; set; }

        /// <summary>
        /// Identificativo univoco del Amica.BeneServizio.Prezzo (prezzo).
        /// </summary>
        /// <value>Identificativo univoco del Amica.BeneServizio.Prezzo (prezzo).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "prezzoId", EmitDefaultValue = true)]
        public int? PrezzoId { get; set; }

        /// <summary>
        /// Prezzo.
        /// </summary>
        /// <value>Prezzo.</value>
        /*
        <example>12.5</example>
        */
        [DataMember(Name = "prezzo", EmitDefaultValue = true)]
        public double? Prezzo { get; set; }

        /// <summary>
        /// Prezzo scontato, cioè al netto degli sconti percentuali indicati e dello sconto incondizionato.
        /// </summary>
        /// <value>Prezzo scontato, cioè al netto degli sconti percentuali indicati e dello sconto incondizionato.</value>
        /*
        <example>11.5</example>
        */
        [DataMember(Name = "prezzoScontato", EmitDefaultValue = true)]
        public double? PrezzoScontato { get; set; }

        /// <summary>
        /// Prezzo comprensivo di IVA.
        /// </summary>
        /// <value>Prezzo comprensivo di IVA.</value>
        /*
        <example>14.03</example>
        */
        [DataMember(Name = "prezzoIvato", EmitDefaultValue = true)]
        public double? PrezzoIvato { get; set; }

        /// <summary>
        /// Importo, determinato come prodotto di Amica.BeneServizio.Quantità e Amica.BeneServizio.PrezzoScontato.
        /// </summary>
        /// <value>Importo, determinato come prodotto di Amica.BeneServizio.Quantità e Amica.BeneServizio.PrezzoScontato.</value>
        /*
        <example>115.0</example>
        */
        [DataMember(Name = "importo", EmitDefaultValue = true)]
        public double? Importo { get; set; }

        /// <summary>
        /// Gets or Sets DatiAggiuntivi
        /// </summary>
        [DataMember(Name = "datiAggiuntivi", EmitDefaultValue = false)]
        public DatiAggiuntivi DatiAggiuntivi { get; set; }

        /// <summary>
        /// Descrizione di tutti gli sconti combinati (es: \&quot;10,5%+5€\&quot;).
        /// </summary>
        /// <value>Descrizione di tutti gli sconti combinati (es: \&quot;10,5%+5€\&quot;).</value>
        /*
        <example>10,5%+5€</example>
        */
        [DataMember(Name = "sconto", EmitDefaultValue = true)]
        public string Sconto { get; private set; }

        /// <summary>
        /// Returns false as Sconto should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializeSconto()
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
            sb.Append("class BeneServizio {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  DocumentoId: ").Append(DocumentoId).Append("\n");
            sb.Append("  ArticoloId: ").Append(ArticoloId).Append("\n");
            sb.Append("  Articolo: ").Append(Articolo).Append("\n");
            sb.Append("  PrestazioneId: ").Append(PrestazioneId).Append("\n");
            sb.Append("  Prestazione: ").Append(Prestazione).Append("\n");
            sb.Append("  MagazzinoId: ").Append(MagazzinoId).Append("\n");
            sb.Append("  Magazzino: ").Append(Magazzino).Append("\n");
            sb.Append("  CausaleIvaId: ").Append(CausaleIvaId).Append("\n");
            sb.Append("  CausaleIva: ").Append(CausaleIva).Append("\n");
            sb.Append("  ListinoId: ").Append(ListinoId).Append("\n");
            sb.Append("  Listino: ").Append(Listino).Append("\n");
            sb.Append("  OrdineId: ").Append(OrdineId).Append("\n");
            sb.Append("  BeneServizioOrdineId: ").Append(BeneServizioOrdineId).Append("\n");
            sb.Append("  TagliaId: ").Append(TagliaId).Append("\n");
            sb.Append("  Taglia: ").Append(Taglia).Append("\n");
            sb.Append("  RitenutaFiscale: ").Append(RitenutaFiscale).Append("\n");
            sb.Append("  Posizione: ").Append(Posizione).Append("\n");
            sb.Append("  Codice: ").Append(Codice).Append("\n");
            sb.Append("  Descrizione: ").Append(Descrizione).Append("\n");
            sb.Append("  UnitMisura: ").Append(UnitMisura).Append("\n");
            sb.Append("  Quantit: ").Append(Quantit).Append("\n");
            sb.Append("  QuantitEvasa: ").Append(QuantitEvasa).Append("\n");
            sb.Append("  QuantitDaEvadere: ").Append(QuantitDaEvadere).Append("\n");
            sb.Append("  Sconto1: ").Append(Sconto1).Append("\n");
            sb.Append("  Sconto2: ").Append(Sconto2).Append("\n");
            sb.Append("  Sconto3: ").Append(Sconto3).Append("\n");
            sb.Append("  Sconto4: ").Append(Sconto4).Append("\n");
            sb.Append("  ScontoIncondizionato: ").Append(ScontoIncondizionato).Append("\n");
            sb.Append("  ProvvigioneAgente: ").Append(ProvvigioneAgente).Append("\n");
            sb.Append("  ProvvigioneCapoArea: ").Append(ProvvigioneCapoArea).Append("\n");
            sb.Append("  PrezzoId: ").Append(PrezzoId).Append("\n");
            sb.Append("  Prezzo: ").Append(Prezzo).Append("\n");
            sb.Append("  PrezzoScontato: ").Append(PrezzoScontato).Append("\n");
            sb.Append("  PrezzoIvato: ").Append(PrezzoIvato).Append("\n");
            sb.Append("  Importo: ").Append(Importo).Append("\n");
            sb.Append("  DatiAggiuntivi: ").Append(DatiAggiuntivi).Append("\n");
            sb.Append("  Sconto: ").Append(Sconto).Append("\n");
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
            // Codice (string) maxLength
            if (this.Codice != null && this.Codice.Length > 30)
            {
                yield return new ValidationResult("Invalid value for Codice, length must be less than 30.", new [] { "Codice" });
            }

            // Codice (string) minLength
            if (this.Codice != null && this.Codice.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Codice, length must be greater than 0.", new [] { "Codice" });
            }

            // Descrizione (string) minLength
            if (this.Descrizione != null && this.Descrizione.Length < 1)
            {
                yield return new ValidationResult("Invalid value for Descrizione, length must be greater than 1.", new [] { "Descrizione" });
            }

            // UnitMisura (string) maxLength
            if (this.UnitMisura != null && this.UnitMisura.Length > 15)
            {
                yield return new ValidationResult("Invalid value for UnitMisura, length must be less than 15.", new [] { "UnitMisura" });
            }

            // UnitMisura (string) minLength
            if (this.UnitMisura != null && this.UnitMisura.Length < 0)
            {
                yield return new ValidationResult("Invalid value for UnitMisura, length must be greater than 0.", new [] { "UnitMisura" });
            }

            yield break;
        }
    }

}

/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.3.2
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
    /// Anagrafica di cliente, fornitore, agente o vettore.
    /// </summary>
    [DataContract(Name = "Anagrafica")]
    public partial class Anagrafica : IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Anagrafica" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected Anagrafica() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Anagrafica" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="modelloId">Identificativo univoco di &#x60;Modello&#x60;. Sempre null. Questa proprietà è usata solo dal client desktop Amica 20 per Windows ed è da considerarsi obsoleta..</param>
        /// <param name="nazioneId">Identificativo univoco di &#x60;Nazione&#x60; (nazione)..</param>
        /// <param name="nazione">nazione.</param>
        /// <param name="areaGeograficaId">Identificativo univoco di &#x60;AreaGeografica&#x60; (area geografica)..</param>
        /// <param name="areaGeografica">areaGeografica.</param>
        /// <param name="valutaId">Identificativo univoco di &#x60;Valuta&#x60; (valuta)..</param>
        /// <param name="valuta">valuta.</param>
        /// <param name="indirizzoPredefinitoId">Identificativo univoco dell&#39;&#x60;Indirizzo&#x60; (indirizzo) predefinito..</param>
        /// <param name="indirizzi">Elenco degli &#x60;Indirizzo&#x60; dell&#39;anagrafica..</param>
        /// <param name="schedaCliente">schedaCliente.</param>
        /// <param name="schedaFornitore">schedaFornitore.</param>
        /// <param name="stato">Indica se l&#39;anagrafica è attiva o meno..</param>
        /// <param name="cliente">Indica se l&#39;anagrafica è un cliente..</param>
        /// <param name="fornitore">Indica se l&#39;anagrafica è un fornitore..</param>
        /// <param name="agente">Indica se l&#39;anagrafica opera come agente di vendita per conto dell&#39;azienda..</param>
        /// <param name="vettore">Indica se l&#39;anagrafica opera come vettore..</param>
        /// <param name="capoArea">Indica se l&#39;anagrafica opera come capo area..</param>
        /// <param name="personaGiuridica">Indica se l&#39;anagrafica è una persona giuridica o meno. &#x60;true&#x60; indica una persona giuridica, &#x60;false&#x60; una persona fisica..</param>
        /// <param name="banca">banca.</param>
        /// <param name="nominativo1">nominativo1.</param>
        /// <param name="nominativo2">nominativo2.</param>
        /// <param name="sedeLegale">sedeLegale.</param>
        /// <param name="codice">Codice assegnato all&#39;anagrafica..</param>
        /// <param name="dataInserimento">Data di primo inserimento..</param>
        /// <param name="ragioneSociale1">Ragione sociale, oppure nome e cognome della persona fisica. (required).</param>
        /// <param name="ragioneSociale2">Eventuale seconda riga della ragione sociale. Usata principalmente nelle stampe, quando si vuole suddividere la ragione sociale su due righe..</param>
        /// <param name="eMail">Indirizzo email..</param>
        /// <param name="sitoWeb">Indirizzo web..</param>
        /// <param name="partitaIva">Partita IVA. La correttezza formale è verificata dal servizio al salvataggio: l&#39;errore che ne deriva è forzabile con &#x60;validationOverride&#x60;..</param>
        /// <param name="codiceFiscale">Codice fiscale. Solitamente diverso dalla &#x60;PartitaIva&#x60;. Come per la partita IVA, la correttezza formale è verificata dal servizio al salvataggio e l&#39;errore è forzabile con &#x60;validationOverride&#x60;..</param>
        /// <param name="note">Note..</param>
        /// <param name="noteImportanti">Note rilevanti. I client ufficiali Amica le evidenziano alla creazione di un nuovo documento fiscale..</param>
        /// <param name="pec">Indirizzo Pec (posta elettronica certificata)..</param>
        /// <param name="codiceDestinatario">Codice destinatario per il sistema fiscale italiano. Usato per il recapito delle fatture elettroniche italiane..</param>
        /// <param name="dichiarazioneIntento">Numero di protocollo della dichiarazione d&#39;intento..</param>
        /// <param name="riferimentoAmministrazione">Solitamente necessario quando si ha a che fare con enti della Pubblica Amministrazione italiana. Identifica univocamente l&#39;ufficio di destinazione all&#39;interno della PA..</param>
        public Anagrafica(int id = default(int), int concurrencyToken = default(int), int? modelloId = default(int?), int? nazioneId = default(int?), Nazione nazione = default(Nazione), int? areaGeograficaId = default(int?), AreaGeografica areaGeografica = default(AreaGeografica), int? valutaId = default(int?), Valuta valuta = default(Valuta), int? indirizzoPredefinitoId = default(int?), List<Indirizzo> indirizzi = default(List<Indirizzo>), SchedaCliente schedaCliente = default(SchedaCliente), SchedaFornitore schedaFornitore = default(SchedaFornitore), bool stato = default(bool), bool cliente = default(bool), bool fornitore = default(bool), bool agente = default(bool), bool vettore = default(bool), bool capoArea = default(bool), bool personaGiuridica = default(bool), Banca banca = default(Banca), AnagraficaNominativo1 nominativo1 = default(AnagraficaNominativo1), AnagraficaNominativo2 nominativo2 = default(AnagraficaNominativo2), AnagraficaSedeLegale sedeLegale = default(AnagraficaSedeLegale), string codice = default(string), DateTime? dataInserimento = default(DateTime?), string ragioneSociale1 = default(string), string ragioneSociale2 = default(string), string eMail = default(string), string sitoWeb = default(string), string partitaIva = default(string), string codiceFiscale = default(string), string note = default(string), string noteImportanti = default(string), string pec = default(string), string codiceDestinatario = default(string), string dichiarazioneIntento = default(string), string riferimentoAmministrazione = default(string))
        {
            // to ensure "ragioneSociale1" is required (not null)
            if (ragioneSociale1 == null)
            {
                throw new ArgumentNullException("ragioneSociale1 is a required property for Anagrafica and cannot be null");
            }
            this.RagioneSociale1 = ragioneSociale1;
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.ModelloId = modelloId;
            this.NazioneId = nazioneId;
            this.Nazione = nazione;
            this.AreaGeograficaId = areaGeograficaId;
            this.AreaGeografica = areaGeografica;
            this.ValutaId = valutaId;
            this.Valuta = valuta;
            this.IndirizzoPredefinitoId = indirizzoPredefinitoId;
            this.Indirizzi = indirizzi;
            this.SchedaCliente = schedaCliente;
            this.SchedaFornitore = schedaFornitore;
            this.Stato = stato;
            this.Cliente = cliente;
            this.Fornitore = fornitore;
            this.Agente = agente;
            this.Vettore = vettore;
            this.CapoArea = capoArea;
            this.PersonaGiuridica = personaGiuridica;
            this.Banca = banca;
            this.Nominativo1 = nominativo1;
            this.Nominativo2 = nominativo2;
            this.SedeLegale = sedeLegale;
            this.Codice = codice;
            this.DataInserimento = dataInserimento;
            this.RagioneSociale2 = ragioneSociale2;
            this.EMail = eMail;
            this.SitoWeb = sitoWeb;
            this.PartitaIva = partitaIva;
            this.CodiceFiscale = codiceFiscale;
            this.Note = note;
            this.NoteImportanti = noteImportanti;
            this.Pec = pec;
            this.CodiceDestinatario = codiceDestinatario;
            this.DichiarazioneIntento = dichiarazioneIntento;
            this.RiferimentoAmministrazione = riferimentoAmministrazione;
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
        /// Identificativo univoco di &#x60;Modello&#x60;. Sempre null. Questa proprietà è usata solo dal client desktop Amica 20 per Windows ed è da considerarsi obsoleta.
        /// </summary>
        /// <value>Identificativo univoco di &#x60;Modello&#x60;. Sempre null. Questa proprietà è usata solo dal client desktop Amica 20 per Windows ed è da considerarsi obsoleta.</value>
        [DataMember(Name = "modelloId", EmitDefaultValue = true)]
        public int? ModelloId { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;Nazione&#x60; (nazione).
        /// </summary>
        /// <value>Identificativo univoco di &#x60;Nazione&#x60; (nazione).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "nazioneId", EmitDefaultValue = true)]
        public int? NazioneId { get; set; }

        /// <summary>
        /// Gets or Sets Nazione
        /// </summary>
        [DataMember(Name = "nazione", EmitDefaultValue = false)]
        public Nazione Nazione { get; set; }

        /// <summary>
        /// Identificativo univoco di &#x60;AreaGeografica&#x60; (area geografica).
        /// </summary>
        /// <value>Identificativo univoco di &#x60;AreaGeografica&#x60; (area geografica).</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "areaGeograficaId", EmitDefaultValue = true)]
        public int? AreaGeograficaId { get; set; }

        /// <summary>
        /// Gets or Sets AreaGeografica
        /// </summary>
        [DataMember(Name = "areaGeografica", EmitDefaultValue = false)]
        public AreaGeografica AreaGeografica { get; set; }

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
        /// Identificativo univoco dell&#39;&#x60;Indirizzo&#x60; (indirizzo) predefinito.
        /// </summary>
        /// <value>Identificativo univoco dell&#39;&#x60;Indirizzo&#x60; (indirizzo) predefinito.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "indirizzoPredefinitoId", EmitDefaultValue = true)]
        public int? IndirizzoPredefinitoId { get; set; }

        /// <summary>
        /// Elenco degli &#x60;Indirizzo&#x60; dell&#39;anagrafica.
        /// </summary>
        /// <value>Elenco degli &#x60;Indirizzo&#x60; dell&#39;anagrafica.</value>
        [DataMember(Name = "indirizzi", EmitDefaultValue = true)]
        public List<Indirizzo> Indirizzi { get; set; }

        /// <summary>
        /// Gets or Sets SchedaCliente
        /// </summary>
        [DataMember(Name = "schedaCliente", EmitDefaultValue = false)]
        public SchedaCliente SchedaCliente { get; set; }

        /// <summary>
        /// Gets or Sets SchedaFornitore
        /// </summary>
        [DataMember(Name = "schedaFornitore", EmitDefaultValue = false)]
        public SchedaFornitore SchedaFornitore { get; set; }

        /// <summary>
        /// Indica se l&#39;anagrafica è attiva o meno.
        /// </summary>
        /// <value>Indica se l&#39;anagrafica è attiva o meno.</value>
        /*
        <example>true</example>
        */
        [DataMember(Name = "stato", EmitDefaultValue = true)]
        public bool Stato { get; set; }

        /// <summary>
        /// Indica se l&#39;anagrafica è un cliente.
        /// </summary>
        /// <value>Indica se l&#39;anagrafica è un cliente.</value>
        /*
        <example>true</example>
        */
        [DataMember(Name = "cliente", EmitDefaultValue = true)]
        public bool Cliente { get; set; }

        /// <summary>
        /// Indica se l&#39;anagrafica è un fornitore.
        /// </summary>
        /// <value>Indica se l&#39;anagrafica è un fornitore.</value>
        /*
        <example>false</example>
        */
        [DataMember(Name = "fornitore", EmitDefaultValue = true)]
        public bool Fornitore { get; set; }

        /// <summary>
        /// Indica se l&#39;anagrafica opera come agente di vendita per conto dell&#39;azienda.
        /// </summary>
        /// <value>Indica se l&#39;anagrafica opera come agente di vendita per conto dell&#39;azienda.</value>
        /*
        <example>false</example>
        */
        [DataMember(Name = "agente", EmitDefaultValue = true)]
        public bool Agente { get; set; }

        /// <summary>
        /// Indica se l&#39;anagrafica opera come vettore.
        /// </summary>
        /// <value>Indica se l&#39;anagrafica opera come vettore.</value>
        /*
        <example>false</example>
        */
        [DataMember(Name = "vettore", EmitDefaultValue = true)]
        public bool Vettore { get; set; }

        /// <summary>
        /// Indica se l&#39;anagrafica opera come capo area.
        /// </summary>
        /// <value>Indica se l&#39;anagrafica opera come capo area.</value>
        /*
        <example>false</example>
        */
        [DataMember(Name = "capoArea", EmitDefaultValue = true)]
        public bool CapoArea { get; set; }

        /// <summary>
        /// Indica se l&#39;anagrafica è una persona giuridica o meno. &#x60;true&#x60; indica una persona giuridica, &#x60;false&#x60; una persona fisica.
        /// </summary>
        /// <value>Indica se l&#39;anagrafica è una persona giuridica o meno. &#x60;true&#x60; indica una persona giuridica, &#x60;false&#x60; una persona fisica.</value>
        /*
        <example>true</example>
        */
        [DataMember(Name = "personaGiuridica", EmitDefaultValue = true)]
        public bool PersonaGiuridica { get; set; }

        /// <summary>
        /// Gets or Sets Banca
        /// </summary>
        [DataMember(Name = "banca", EmitDefaultValue = false)]
        public Banca Banca { get; set; }

        /// <summary>
        /// Gets or Sets Nominativo1
        /// </summary>
        [DataMember(Name = "nominativo1", EmitDefaultValue = false)]
        public AnagraficaNominativo1 Nominativo1 { get; set; }

        /// <summary>
        /// Gets or Sets Nominativo2
        /// </summary>
        [DataMember(Name = "nominativo2", EmitDefaultValue = false)]
        public AnagraficaNominativo2 Nominativo2 { get; set; }

        /// <summary>
        /// Gets or Sets SedeLegale
        /// </summary>
        [DataMember(Name = "sedeLegale", EmitDefaultValue = false)]
        public AnagraficaSedeLegale SedeLegale { get; set; }

        /// <summary>
        /// Codice assegnato all&#39;anagrafica.
        /// </summary>
        /// <value>Codice assegnato all&#39;anagrafica.</value>
        /*
        <example>C001</example>
        */
        [DataMember(Name = "codice", EmitDefaultValue = true)]
        public string Codice { get; set; }

        /// <summary>
        /// Data di primo inserimento.
        /// </summary>
        /// <value>Data di primo inserimento.</value>
        [DataMember(Name = "dataInserimento", EmitDefaultValue = true)]
        public DateTime? DataInserimento { get; set; }

        /// <summary>
        /// Ragione sociale, oppure nome e cognome della persona fisica.
        /// </summary>
        /// <value>Ragione sociale, oppure nome e cognome della persona fisica.</value>
        /*
        <example>Acme S.r.l.</example>
        */
        [DataMember(Name = "ragioneSociale1", IsRequired = true, EmitDefaultValue = true)]
        public string RagioneSociale1 { get; set; }

        /// <summary>
        /// Eventuale seconda riga della ragione sociale. Usata principalmente nelle stampe, quando si vuole suddividere la ragione sociale su due righe.
        /// </summary>
        /// <value>Eventuale seconda riga della ragione sociale. Usata principalmente nelle stampe, quando si vuole suddividere la ragione sociale su due righe.</value>
        /*
        <example>Divisione Commerciale</example>
        */
        [DataMember(Name = "ragioneSociale2", EmitDefaultValue = true)]
        public string RagioneSociale2 { get; set; }

        /// <summary>
        /// Indirizzo email.
        /// </summary>
        /// <value>Indirizzo email.</value>
        /*
        <example>info@acme.it</example>
        */
        [DataMember(Name = "eMail", EmitDefaultValue = true)]
        public string EMail { get; set; }

        /// <summary>
        /// Indirizzo web.
        /// </summary>
        /// <value>Indirizzo web.</value>
        /*
        <example>https://www.acme.it</example>
        */
        [DataMember(Name = "sitoWeb", EmitDefaultValue = true)]
        public string SitoWeb { get; set; }

        /// <summary>
        /// Partita IVA. La correttezza formale è verificata dal servizio al salvataggio: l&#39;errore che ne deriva è forzabile con &#x60;validationOverride&#x60;.
        /// </summary>
        /// <value>Partita IVA. La correttezza formale è verificata dal servizio al salvataggio: l&#39;errore che ne deriva è forzabile con &#x60;validationOverride&#x60;.</value>
        /*
        <example>IT01234567890</example>
        */
        [DataMember(Name = "partitaIva", EmitDefaultValue = true)]
        public string PartitaIva { get; set; }

        /// <summary>
        /// Codice fiscale. Solitamente diverso dalla &#x60;PartitaIva&#x60;. Come per la partita IVA, la correttezza formale è verificata dal servizio al salvataggio e l&#39;errore è forzabile con &#x60;validationOverride&#x60;.
        /// </summary>
        /// <value>Codice fiscale. Solitamente diverso dalla &#x60;PartitaIva&#x60;. Come per la partita IVA, la correttezza formale è verificata dal servizio al salvataggio e l&#39;errore è forzabile con &#x60;validationOverride&#x60;.</value>
        /*
        <example>RSSMRA85M01H501Z</example>
        */
        [DataMember(Name = "codiceFiscale", EmitDefaultValue = true)]
        public string CodiceFiscale { get; set; }

        /// <summary>
        /// Note.
        /// </summary>
        /// <value>Note.</value>
        /*
        <example>Cliente storico, pagamenti puntuali.</example>
        */
        [DataMember(Name = "note", EmitDefaultValue = true)]
        public string Note { get; set; }

        /// <summary>
        /// Note rilevanti. I client ufficiali Amica le evidenziano alla creazione di un nuovo documento fiscale.
        /// </summary>
        /// <value>Note rilevanti. I client ufficiali Amica le evidenziano alla creazione di un nuovo documento fiscale.</value>
        /*
        <example>Richiedere sempre ordine firmato.</example>
        */
        [DataMember(Name = "noteImportanti", EmitDefaultValue = true)]
        public string NoteImportanti { get; set; }

        /// <summary>
        /// Indirizzo Pec (posta elettronica certificata).
        /// </summary>
        /// <value>Indirizzo Pec (posta elettronica certificata).</value>
        /*
        <example>acme@pec.it</example>
        */
        [DataMember(Name = "pec", EmitDefaultValue = true)]
        public string Pec { get; set; }

        /// <summary>
        /// Codice destinatario per il sistema fiscale italiano. Usato per il recapito delle fatture elettroniche italiane.
        /// </summary>
        /// <value>Codice destinatario per il sistema fiscale italiano. Usato per il recapito delle fatture elettroniche italiane.</value>
        /*
        <example>0000000</example>
        */
        [DataMember(Name = "codiceDestinatario", EmitDefaultValue = true)]
        public string CodiceDestinatario { get; set; }

        /// <summary>
        /// Numero di protocollo della dichiarazione d&#39;intento.
        /// </summary>
        /// <value>Numero di protocollo della dichiarazione d&#39;intento.</value>
        /*
        <example>08060120240000000123</example>
        */
        [DataMember(Name = "dichiarazioneIntento", EmitDefaultValue = true)]
        public string DichiarazioneIntento { get; set; }

        /// <summary>
        /// Solitamente necessario quando si ha a che fare con enti della Pubblica Amministrazione italiana. Identifica univocamente l&#39;ufficio di destinazione all&#39;interno della PA.
        /// </summary>
        /// <value>Solitamente necessario quando si ha a che fare con enti della Pubblica Amministrazione italiana. Identifica univocamente l&#39;ufficio di destinazione all&#39;interno della PA.</value>
        /*
        <example>UFFICIO01</example>
        */
        [DataMember(Name = "riferimentoAmministrazione", EmitDefaultValue = true)]
        public string RiferimentoAmministrazione { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Anagrafica {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  ModelloId: ").Append(ModelloId).Append("\n");
            sb.Append("  NazioneId: ").Append(NazioneId).Append("\n");
            sb.Append("  Nazione: ").Append(Nazione).Append("\n");
            sb.Append("  AreaGeograficaId: ").Append(AreaGeograficaId).Append("\n");
            sb.Append("  AreaGeografica: ").Append(AreaGeografica).Append("\n");
            sb.Append("  ValutaId: ").Append(ValutaId).Append("\n");
            sb.Append("  Valuta: ").Append(Valuta).Append("\n");
            sb.Append("  IndirizzoPredefinitoId: ").Append(IndirizzoPredefinitoId).Append("\n");
            sb.Append("  Indirizzi: ").Append(Indirizzi).Append("\n");
            sb.Append("  SchedaCliente: ").Append(SchedaCliente).Append("\n");
            sb.Append("  SchedaFornitore: ").Append(SchedaFornitore).Append("\n");
            sb.Append("  Stato: ").Append(Stato).Append("\n");
            sb.Append("  Cliente: ").Append(Cliente).Append("\n");
            sb.Append("  Fornitore: ").Append(Fornitore).Append("\n");
            sb.Append("  Agente: ").Append(Agente).Append("\n");
            sb.Append("  Vettore: ").Append(Vettore).Append("\n");
            sb.Append("  CapoArea: ").Append(CapoArea).Append("\n");
            sb.Append("  PersonaGiuridica: ").Append(PersonaGiuridica).Append("\n");
            sb.Append("  Banca: ").Append(Banca).Append("\n");
            sb.Append("  Nominativo1: ").Append(Nominativo1).Append("\n");
            sb.Append("  Nominativo2: ").Append(Nominativo2).Append("\n");
            sb.Append("  SedeLegale: ").Append(SedeLegale).Append("\n");
            sb.Append("  Codice: ").Append(Codice).Append("\n");
            sb.Append("  DataInserimento: ").Append(DataInserimento).Append("\n");
            sb.Append("  RagioneSociale1: ").Append(RagioneSociale1).Append("\n");
            sb.Append("  RagioneSociale2: ").Append(RagioneSociale2).Append("\n");
            sb.Append("  EMail: ").Append(EMail).Append("\n");
            sb.Append("  SitoWeb: ").Append(SitoWeb).Append("\n");
            sb.Append("  PartitaIva: ").Append(PartitaIva).Append("\n");
            sb.Append("  CodiceFiscale: ").Append(CodiceFiscale).Append("\n");
            sb.Append("  Note: ").Append(Note).Append("\n");
            sb.Append("  NoteImportanti: ").Append(NoteImportanti).Append("\n");
            sb.Append("  Pec: ").Append(Pec).Append("\n");
            sb.Append("  CodiceDestinatario: ").Append(CodiceDestinatario).Append("\n");
            sb.Append("  DichiarazioneIntento: ").Append(DichiarazioneIntento).Append("\n");
            sb.Append("  RiferimentoAmministrazione: ").Append(RiferimentoAmministrazione).Append("\n");
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
            if (this.Codice != null && this.Codice.Length > 10)
            {
                yield return new ValidationResult("Invalid value for Codice, length must be less than 10.", new [] { "Codice" });
            }

            // Codice (string) minLength
            if (this.Codice != null && this.Codice.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Codice, length must be greater than 0.", new [] { "Codice" });
            }

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

            // EMail (string) maxLength
            if (this.EMail != null && this.EMail.Length > 50)
            {
                yield return new ValidationResult("Invalid value for EMail, length must be less than 50.", new [] { "EMail" });
            }

            // EMail (string) minLength
            if (this.EMail != null && this.EMail.Length < 0)
            {
                yield return new ValidationResult("Invalid value for EMail, length must be greater than 0.", new [] { "EMail" });
            }

            // SitoWeb (string) maxLength
            if (this.SitoWeb != null && this.SitoWeb.Length > 50)
            {
                yield return new ValidationResult("Invalid value for SitoWeb, length must be less than 50.", new [] { "SitoWeb" });
            }

            // SitoWeb (string) minLength
            if (this.SitoWeb != null && this.SitoWeb.Length < 0)
            {
                yield return new ValidationResult("Invalid value for SitoWeb, length must be greater than 0.", new [] { "SitoWeb" });
            }

            // PartitaIva (string) maxLength
            if (this.PartitaIva != null && this.PartitaIva.Length > 30)
            {
                yield return new ValidationResult("Invalid value for PartitaIva, length must be less than 30.", new [] { "PartitaIva" });
            }

            // PartitaIva (string) minLength
            if (this.PartitaIva != null && this.PartitaIva.Length < 0)
            {
                yield return new ValidationResult("Invalid value for PartitaIva, length must be greater than 0.", new [] { "PartitaIva" });
            }

            // CodiceFiscale (string) maxLength
            if (this.CodiceFiscale != null && this.CodiceFiscale.Length > 20)
            {
                yield return new ValidationResult("Invalid value for CodiceFiscale, length must be less than 20.", new [] { "CodiceFiscale" });
            }

            // CodiceFiscale (string) minLength
            if (this.CodiceFiscale != null && this.CodiceFiscale.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceFiscale, length must be greater than 0.", new [] { "CodiceFiscale" });
            }

            // Pec (string) maxLength
            if (this.Pec != null && this.Pec.Length > 120)
            {
                yield return new ValidationResult("Invalid value for Pec, length must be less than 120.", new [] { "Pec" });
            }

            // Pec (string) minLength
            if (this.Pec != null && this.Pec.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Pec, length must be greater than 0.", new [] { "Pec" });
            }

            // CodiceDestinatario (string) maxLength
            if (this.CodiceDestinatario != null && this.CodiceDestinatario.Length > 7)
            {
                yield return new ValidationResult("Invalid value for CodiceDestinatario, length must be less than 7.", new [] { "CodiceDestinatario" });
            }

            // CodiceDestinatario (string) minLength
            if (this.CodiceDestinatario != null && this.CodiceDestinatario.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceDestinatario, length must be greater than 0.", new [] { "CodiceDestinatario" });
            }

            // DichiarazioneIntento (string) maxLength
            if (this.DichiarazioneIntento != null && this.DichiarazioneIntento.Length > 200)
            {
                yield return new ValidationResult("Invalid value for DichiarazioneIntento, length must be less than 200.", new [] { "DichiarazioneIntento" });
            }

            // DichiarazioneIntento (string) minLength
            if (this.DichiarazioneIntento != null && this.DichiarazioneIntento.Length < 0)
            {
                yield return new ValidationResult("Invalid value for DichiarazioneIntento, length must be greater than 0.", new [] { "DichiarazioneIntento" });
            }

            // RiferimentoAmministrazione (string) maxLength
            if (this.RiferimentoAmministrazione != null && this.RiferimentoAmministrazione.Length > 20)
            {
                yield return new ValidationResult("Invalid value for RiferimentoAmministrazione, length must be less than 20.", new [] { "RiferimentoAmministrazione" });
            }

            // RiferimentoAmministrazione (string) minLength
            if (this.RiferimentoAmministrazione != null && this.RiferimentoAmministrazione.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RiferimentoAmministrazione, length must be greater than 0.", new [] { "RiferimentoAmministrazione" });
            }

            yield break;
        }
    }

}

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
    /// Modello standard di fattura elettronica.
    /// </summary>
    [DataContract(Name = "FatturaElettronicaXmlTemplate")]
    public partial class FatturaElettronicaXmlTemplate : IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FatturaElettronicaXmlTemplate" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="soggettoEmittente">soggettoEmittente.</param>
        /// <param name="idTrasmittenteIdPaese">idTrasmittenteIdPaese.</param>
        /// <param name="cedentePrestatoreIdPaese">cedentePrestatoreIdPaese.</param>
        /// <param name="progressivoInvio">progressivoInvio.</param>
        /// <param name="alboProfessionale">alboProfessionale.</param>
        /// <param name="provinciaAlbo">provinciaAlbo.</param>
        /// <param name="numeroIscrizioneAlbo">numeroIscrizioneAlbo.</param>
        /// <param name="dataIscrizioneAlbo">dataIscrizioneAlbo.</param>
        /// <param name="regimeFiscale">regimeFiscale.</param>
        /// <param name="sedeNazione">sedeNazione.</param>
        /// <param name="sedeNumeroCivico">sedeNumeroCivico.</param>
        /// <param name="stabileOrganizzazioneIndirizzo">stabileOrganizzazioneIndirizzo.</param>
        /// <param name="stabileOrganizzazioneNumeroCivico">stabileOrganizzazioneNumeroCivico.</param>
        /// <param name="stabileOrganizzazioneCap">stabileOrganizzazioneCap.</param>
        /// <param name="stabileOrganizzazioneComune">stabileOrganizzazioneComune.</param>
        /// <param name="stabileOrganizzazioneProvincia">stabileOrganizzazioneProvincia.</param>
        /// <param name="stabileOrganizzazioneNazione">stabileOrganizzazioneNazione.</param>
        /// <param name="reaNumero">reaNumero.</param>
        /// <param name="reaUfficio">reaUfficio.</param>
        /// <param name="reaCapitaleSociale">reaCapitaleSociale.</param>
        /// <param name="reaSocioUnico">reaSocioUnico.</param>
        /// <param name="reaStatoLiquidazione">reaStatoLiquidazione.</param>
        /// <param name="rappresentanteIdPaese">rappresentanteIdPaese.</param>
        /// <param name="rappresentanteIdCodice">rappresentanteIdCodice.</param>
        /// <param name="rappresentanteCodiceFiscale">rappresentanteCodiceFiscale.</param>
        /// <param name="rappresentanteDenominazione">rappresentanteDenominazione.</param>
        /// <param name="rappresentanteNome">rappresentanteNome.</param>
        /// <param name="rappresentanteCognome">rappresentanteCognome.</param>
        /// <param name="rappresentanteTitolo">rappresentanteTitolo.</param>
        /// <param name="rappresentanteCodEori">rappresentanteCodEori.</param>
        /// <param name="intermediarioIdPaese">intermediarioIdPaese.</param>
        /// <param name="intermediarioIdCodice">intermediarioIdCodice.</param>
        /// <param name="intermediarioCodiceFiscale">intermediarioCodiceFiscale.</param>
        /// <param name="intermediarioDenominazione">intermediarioDenominazione.</param>
        /// <param name="intermediarioNome">intermediarioNome.</param>
        /// <param name="intermediarioCognome">intermediarioCognome.</param>
        /// <param name="intermediarioTitolo">intermediarioTitolo.</param>
        /// <param name="intermediarioCodEori">intermediarioCodEori.</param>
        public FatturaElettronicaXmlTemplate(int id = default(int), int concurrencyToken = default(int), string soggettoEmittente = default(string), string idTrasmittenteIdPaese = default(string), string cedentePrestatoreIdPaese = default(string), string progressivoInvio = default(string), string alboProfessionale = default(string), string provinciaAlbo = default(string), string numeroIscrizioneAlbo = default(string), DateTime? dataIscrizioneAlbo = default(DateTime?), string regimeFiscale = default(string), string sedeNazione = default(string), string sedeNumeroCivico = default(string), string stabileOrganizzazioneIndirizzo = default(string), string stabileOrganizzazioneNumeroCivico = default(string), string stabileOrganizzazioneCap = default(string), string stabileOrganizzazioneComune = default(string), string stabileOrganizzazioneProvincia = default(string), string stabileOrganizzazioneNazione = default(string), string reaNumero = default(string), string reaUfficio = default(string), double? reaCapitaleSociale = default(double?), string reaSocioUnico = default(string), string reaStatoLiquidazione = default(string), string rappresentanteIdPaese = default(string), string rappresentanteIdCodice = default(string), string rappresentanteCodiceFiscale = default(string), string rappresentanteDenominazione = default(string), string rappresentanteNome = default(string), string rappresentanteCognome = default(string), string rappresentanteTitolo = default(string), string rappresentanteCodEori = default(string), string intermediarioIdPaese = default(string), string intermediarioIdCodice = default(string), string intermediarioCodiceFiscale = default(string), string intermediarioDenominazione = default(string), string intermediarioNome = default(string), string intermediarioCognome = default(string), string intermediarioTitolo = default(string), string intermediarioCodEori = default(string))
        {
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.SoggettoEmittente = soggettoEmittente;
            this.IdTrasmittenteIdPaese = idTrasmittenteIdPaese;
            this.CedentePrestatoreIdPaese = cedentePrestatoreIdPaese;
            this.ProgressivoInvio = progressivoInvio;
            this.AlboProfessionale = alboProfessionale;
            this.ProvinciaAlbo = provinciaAlbo;
            this.NumeroIscrizioneAlbo = numeroIscrizioneAlbo;
            this.DataIscrizioneAlbo = dataIscrizioneAlbo;
            this.RegimeFiscale = regimeFiscale;
            this.SedeNazione = sedeNazione;
            this.SedeNumeroCivico = sedeNumeroCivico;
            this.StabileOrganizzazioneIndirizzo = stabileOrganizzazioneIndirizzo;
            this.StabileOrganizzazioneNumeroCivico = stabileOrganizzazioneNumeroCivico;
            this.StabileOrganizzazioneCap = stabileOrganizzazioneCap;
            this.StabileOrganizzazioneComune = stabileOrganizzazioneComune;
            this.StabileOrganizzazioneProvincia = stabileOrganizzazioneProvincia;
            this.StabileOrganizzazioneNazione = stabileOrganizzazioneNazione;
            this.ReaNumero = reaNumero;
            this.ReaUfficio = reaUfficio;
            this.ReaCapitaleSociale = reaCapitaleSociale;
            this.ReaSocioUnico = reaSocioUnico;
            this.ReaStatoLiquidazione = reaStatoLiquidazione;
            this.RappresentanteIdPaese = rappresentanteIdPaese;
            this.RappresentanteIdCodice = rappresentanteIdCodice;
            this.RappresentanteCodiceFiscale = rappresentanteCodiceFiscale;
            this.RappresentanteDenominazione = rappresentanteDenominazione;
            this.RappresentanteNome = rappresentanteNome;
            this.RappresentanteCognome = rappresentanteCognome;
            this.RappresentanteTitolo = rappresentanteTitolo;
            this.RappresentanteCodEori = rappresentanteCodEori;
            this.IntermediarioIdPaese = intermediarioIdPaese;
            this.IntermediarioIdCodice = intermediarioIdCodice;
            this.IntermediarioCodiceFiscale = intermediarioCodiceFiscale;
            this.IntermediarioDenominazione = intermediarioDenominazione;
            this.IntermediarioNome = intermediarioNome;
            this.IntermediarioCognome = intermediarioCognome;
            this.IntermediarioTitolo = intermediarioTitolo;
            this.IntermediarioCodEori = intermediarioCodEori;
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
        /// Gets or Sets SoggettoEmittente
        /// </summary>
        [DataMember(Name = "soggettoEmittente", EmitDefaultValue = true)]
        public string SoggettoEmittente { get; set; }

        /// <summary>
        /// Gets or Sets IdTrasmittenteIdPaese
        /// </summary>
        [DataMember(Name = "idTrasmittenteIdPaese", EmitDefaultValue = true)]
        public string IdTrasmittenteIdPaese { get; set; }

        /// <summary>
        /// Gets or Sets CedentePrestatoreIdPaese
        /// </summary>
        [DataMember(Name = "cedentePrestatoreIdPaese", EmitDefaultValue = true)]
        public string CedentePrestatoreIdPaese { get; set; }

        /// <summary>
        /// Gets or Sets ProgressivoInvio
        /// </summary>
        [DataMember(Name = "progressivoInvio", EmitDefaultValue = true)]
        public string ProgressivoInvio { get; set; }

        /// <summary>
        /// Gets or Sets AlboProfessionale
        /// </summary>
        [DataMember(Name = "alboProfessionale", EmitDefaultValue = true)]
        public string AlboProfessionale { get; set; }

        /// <summary>
        /// Gets or Sets ProvinciaAlbo
        /// </summary>
        [DataMember(Name = "provinciaAlbo", EmitDefaultValue = true)]
        public string ProvinciaAlbo { get; set; }

        /// <summary>
        /// Gets or Sets NumeroIscrizioneAlbo
        /// </summary>
        [DataMember(Name = "numeroIscrizioneAlbo", EmitDefaultValue = true)]
        public string NumeroIscrizioneAlbo { get; set; }

        /// <summary>
        /// Gets or Sets DataIscrizioneAlbo
        /// </summary>
        [DataMember(Name = "dataIscrizioneAlbo", EmitDefaultValue = true)]
        public DateTime? DataIscrizioneAlbo { get; set; }

        /// <summary>
        /// Gets or Sets RegimeFiscale
        /// </summary>
        [DataMember(Name = "regimeFiscale", EmitDefaultValue = true)]
        public string RegimeFiscale { get; set; }

        /// <summary>
        /// Gets or Sets SedeNazione
        /// </summary>
        [DataMember(Name = "sedeNazione", EmitDefaultValue = true)]
        public string SedeNazione { get; set; }

        /// <summary>
        /// Gets or Sets SedeNumeroCivico
        /// </summary>
        [DataMember(Name = "sedeNumeroCivico", EmitDefaultValue = true)]
        public string SedeNumeroCivico { get; set; }

        /// <summary>
        /// Gets or Sets StabileOrganizzazioneIndirizzo
        /// </summary>
        [DataMember(Name = "stabileOrganizzazioneIndirizzo", EmitDefaultValue = true)]
        public string StabileOrganizzazioneIndirizzo { get; set; }

        /// <summary>
        /// Gets or Sets StabileOrganizzazioneNumeroCivico
        /// </summary>
        [DataMember(Name = "stabileOrganizzazioneNumeroCivico", EmitDefaultValue = true)]
        public string StabileOrganizzazioneNumeroCivico { get; set; }

        /// <summary>
        /// Gets or Sets StabileOrganizzazioneCap
        /// </summary>
        [DataMember(Name = "stabileOrganizzazioneCap", EmitDefaultValue = true)]
        public string StabileOrganizzazioneCap { get; set; }

        /// <summary>
        /// Gets or Sets StabileOrganizzazioneComune
        /// </summary>
        [DataMember(Name = "stabileOrganizzazioneComune", EmitDefaultValue = true)]
        public string StabileOrganizzazioneComune { get; set; }

        /// <summary>
        /// Gets or Sets StabileOrganizzazioneProvincia
        /// </summary>
        [DataMember(Name = "stabileOrganizzazioneProvincia", EmitDefaultValue = true)]
        public string StabileOrganizzazioneProvincia { get; set; }

        /// <summary>
        /// Gets or Sets StabileOrganizzazioneNazione
        /// </summary>
        [DataMember(Name = "stabileOrganizzazioneNazione", EmitDefaultValue = true)]
        public string StabileOrganizzazioneNazione { get; set; }

        /// <summary>
        /// Gets or Sets ReaNumero
        /// </summary>
        [DataMember(Name = "reaNumero", EmitDefaultValue = true)]
        public string ReaNumero { get; set; }

        /// <summary>
        /// Gets or Sets ReaUfficio
        /// </summary>
        [DataMember(Name = "reaUfficio", EmitDefaultValue = true)]
        public string ReaUfficio { get; set; }

        /// <summary>
        /// Gets or Sets ReaCapitaleSociale
        /// </summary>
        [DataMember(Name = "reaCapitaleSociale", EmitDefaultValue = true)]
        public double? ReaCapitaleSociale { get; set; }

        /// <summary>
        /// Gets or Sets ReaSocioUnico
        /// </summary>
        [DataMember(Name = "reaSocioUnico", EmitDefaultValue = true)]
        public string ReaSocioUnico { get; set; }

        /// <summary>
        /// Gets or Sets ReaStatoLiquidazione
        /// </summary>
        [DataMember(Name = "reaStatoLiquidazione", EmitDefaultValue = true)]
        public string ReaStatoLiquidazione { get; set; }

        /// <summary>
        /// Gets or Sets RappresentanteIdPaese
        /// </summary>
        [DataMember(Name = "rappresentanteIdPaese", EmitDefaultValue = true)]
        public string RappresentanteIdPaese { get; set; }

        /// <summary>
        /// Gets or Sets RappresentanteIdCodice
        /// </summary>
        [DataMember(Name = "rappresentanteIdCodice", EmitDefaultValue = true)]
        public string RappresentanteIdCodice { get; set; }

        /// <summary>
        /// Gets or Sets RappresentanteCodiceFiscale
        /// </summary>
        [DataMember(Name = "rappresentanteCodiceFiscale", EmitDefaultValue = true)]
        public string RappresentanteCodiceFiscale { get; set; }

        /// <summary>
        /// Gets or Sets RappresentanteDenominazione
        /// </summary>
        [DataMember(Name = "rappresentanteDenominazione", EmitDefaultValue = true)]
        public string RappresentanteDenominazione { get; set; }

        /// <summary>
        /// Gets or Sets RappresentanteNome
        /// </summary>
        [DataMember(Name = "rappresentanteNome", EmitDefaultValue = true)]
        public string RappresentanteNome { get; set; }

        /// <summary>
        /// Gets or Sets RappresentanteCognome
        /// </summary>
        [DataMember(Name = "rappresentanteCognome", EmitDefaultValue = true)]
        public string RappresentanteCognome { get; set; }

        /// <summary>
        /// Gets or Sets RappresentanteTitolo
        /// </summary>
        [DataMember(Name = "rappresentanteTitolo", EmitDefaultValue = true)]
        public string RappresentanteTitolo { get; set; }

        /// <summary>
        /// Gets or Sets RappresentanteCodEori
        /// </summary>
        [DataMember(Name = "rappresentanteCodEori", EmitDefaultValue = true)]
        public string RappresentanteCodEori { get; set; }

        /// <summary>
        /// Gets or Sets IntermediarioIdPaese
        /// </summary>
        [DataMember(Name = "intermediarioIdPaese", EmitDefaultValue = true)]
        public string IntermediarioIdPaese { get; set; }

        /// <summary>
        /// Gets or Sets IntermediarioIdCodice
        /// </summary>
        [DataMember(Name = "intermediarioIdCodice", EmitDefaultValue = true)]
        public string IntermediarioIdCodice { get; set; }

        /// <summary>
        /// Gets or Sets IntermediarioCodiceFiscale
        /// </summary>
        [DataMember(Name = "intermediarioCodiceFiscale", EmitDefaultValue = true)]
        public string IntermediarioCodiceFiscale { get; set; }

        /// <summary>
        /// Gets or Sets IntermediarioDenominazione
        /// </summary>
        [DataMember(Name = "intermediarioDenominazione", EmitDefaultValue = true)]
        public string IntermediarioDenominazione { get; set; }

        /// <summary>
        /// Gets or Sets IntermediarioNome
        /// </summary>
        [DataMember(Name = "intermediarioNome", EmitDefaultValue = true)]
        public string IntermediarioNome { get; set; }

        /// <summary>
        /// Gets or Sets IntermediarioCognome
        /// </summary>
        [DataMember(Name = "intermediarioCognome", EmitDefaultValue = true)]
        public string IntermediarioCognome { get; set; }

        /// <summary>
        /// Gets or Sets IntermediarioTitolo
        /// </summary>
        [DataMember(Name = "intermediarioTitolo", EmitDefaultValue = true)]
        public string IntermediarioTitolo { get; set; }

        /// <summary>
        /// Gets or Sets IntermediarioCodEori
        /// </summary>
        [DataMember(Name = "intermediarioCodEori", EmitDefaultValue = true)]
        public string IntermediarioCodEori { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class FatturaElettronicaXmlTemplate {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  SoggettoEmittente: ").Append(SoggettoEmittente).Append("\n");
            sb.Append("  IdTrasmittenteIdPaese: ").Append(IdTrasmittenteIdPaese).Append("\n");
            sb.Append("  CedentePrestatoreIdPaese: ").Append(CedentePrestatoreIdPaese).Append("\n");
            sb.Append("  ProgressivoInvio: ").Append(ProgressivoInvio).Append("\n");
            sb.Append("  AlboProfessionale: ").Append(AlboProfessionale).Append("\n");
            sb.Append("  ProvinciaAlbo: ").Append(ProvinciaAlbo).Append("\n");
            sb.Append("  NumeroIscrizioneAlbo: ").Append(NumeroIscrizioneAlbo).Append("\n");
            sb.Append("  DataIscrizioneAlbo: ").Append(DataIscrizioneAlbo).Append("\n");
            sb.Append("  RegimeFiscale: ").Append(RegimeFiscale).Append("\n");
            sb.Append("  SedeNazione: ").Append(SedeNazione).Append("\n");
            sb.Append("  SedeNumeroCivico: ").Append(SedeNumeroCivico).Append("\n");
            sb.Append("  StabileOrganizzazioneIndirizzo: ").Append(StabileOrganizzazioneIndirizzo).Append("\n");
            sb.Append("  StabileOrganizzazioneNumeroCivico: ").Append(StabileOrganizzazioneNumeroCivico).Append("\n");
            sb.Append("  StabileOrganizzazioneCap: ").Append(StabileOrganizzazioneCap).Append("\n");
            sb.Append("  StabileOrganizzazioneComune: ").Append(StabileOrganizzazioneComune).Append("\n");
            sb.Append("  StabileOrganizzazioneProvincia: ").Append(StabileOrganizzazioneProvincia).Append("\n");
            sb.Append("  StabileOrganizzazioneNazione: ").Append(StabileOrganizzazioneNazione).Append("\n");
            sb.Append("  ReaNumero: ").Append(ReaNumero).Append("\n");
            sb.Append("  ReaUfficio: ").Append(ReaUfficio).Append("\n");
            sb.Append("  ReaCapitaleSociale: ").Append(ReaCapitaleSociale).Append("\n");
            sb.Append("  ReaSocioUnico: ").Append(ReaSocioUnico).Append("\n");
            sb.Append("  ReaStatoLiquidazione: ").Append(ReaStatoLiquidazione).Append("\n");
            sb.Append("  RappresentanteIdPaese: ").Append(RappresentanteIdPaese).Append("\n");
            sb.Append("  RappresentanteIdCodice: ").Append(RappresentanteIdCodice).Append("\n");
            sb.Append("  RappresentanteCodiceFiscale: ").Append(RappresentanteCodiceFiscale).Append("\n");
            sb.Append("  RappresentanteDenominazione: ").Append(RappresentanteDenominazione).Append("\n");
            sb.Append("  RappresentanteNome: ").Append(RappresentanteNome).Append("\n");
            sb.Append("  RappresentanteCognome: ").Append(RappresentanteCognome).Append("\n");
            sb.Append("  RappresentanteTitolo: ").Append(RappresentanteTitolo).Append("\n");
            sb.Append("  RappresentanteCodEori: ").Append(RappresentanteCodEori).Append("\n");
            sb.Append("  IntermediarioIdPaese: ").Append(IntermediarioIdPaese).Append("\n");
            sb.Append("  IntermediarioIdCodice: ").Append(IntermediarioIdCodice).Append("\n");
            sb.Append("  IntermediarioCodiceFiscale: ").Append(IntermediarioCodiceFiscale).Append("\n");
            sb.Append("  IntermediarioDenominazione: ").Append(IntermediarioDenominazione).Append("\n");
            sb.Append("  IntermediarioNome: ").Append(IntermediarioNome).Append("\n");
            sb.Append("  IntermediarioCognome: ").Append(IntermediarioCognome).Append("\n");
            sb.Append("  IntermediarioTitolo: ").Append(IntermediarioTitolo).Append("\n");
            sb.Append("  IntermediarioCodEori: ").Append(IntermediarioCodEori).Append("\n");
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
            // SoggettoEmittente (string) maxLength
            if (this.SoggettoEmittente != null && this.SoggettoEmittente.Length > 2)
            {
                yield return new ValidationResult("Invalid value for SoggettoEmittente, length must be less than 2.", new [] { "SoggettoEmittente" });
            }

            // SoggettoEmittente (string) minLength
            if (this.SoggettoEmittente != null && this.SoggettoEmittente.Length < 0)
            {
                yield return new ValidationResult("Invalid value for SoggettoEmittente, length must be greater than 0.", new [] { "SoggettoEmittente" });
            }

            // IdTrasmittenteIdPaese (string) maxLength
            if (this.IdTrasmittenteIdPaese != null && this.IdTrasmittenteIdPaese.Length > 2)
            {
                yield return new ValidationResult("Invalid value for IdTrasmittenteIdPaese, length must be less than 2.", new [] { "IdTrasmittenteIdPaese" });
            }

            // IdTrasmittenteIdPaese (string) minLength
            if (this.IdTrasmittenteIdPaese != null && this.IdTrasmittenteIdPaese.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IdTrasmittenteIdPaese, length must be greater than 0.", new [] { "IdTrasmittenteIdPaese" });
            }

            // CedentePrestatoreIdPaese (string) maxLength
            if (this.CedentePrestatoreIdPaese != null && this.CedentePrestatoreIdPaese.Length > 2)
            {
                yield return new ValidationResult("Invalid value for CedentePrestatoreIdPaese, length must be less than 2.", new [] { "CedentePrestatoreIdPaese" });
            }

            // CedentePrestatoreIdPaese (string) minLength
            if (this.CedentePrestatoreIdPaese != null && this.CedentePrestatoreIdPaese.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CedentePrestatoreIdPaese, length must be greater than 0.", new [] { "CedentePrestatoreIdPaese" });
            }

            // ProgressivoInvio (string) maxLength
            if (this.ProgressivoInvio != null && this.ProgressivoInvio.Length > 10)
            {
                yield return new ValidationResult("Invalid value for ProgressivoInvio, length must be less than 10.", new [] { "ProgressivoInvio" });
            }

            // ProgressivoInvio (string) minLength
            if (this.ProgressivoInvio != null && this.ProgressivoInvio.Length < 0)
            {
                yield return new ValidationResult("Invalid value for ProgressivoInvio, length must be greater than 0.", new [] { "ProgressivoInvio" });
            }

            // AlboProfessionale (string) maxLength
            if (this.AlboProfessionale != null && this.AlboProfessionale.Length > 60)
            {
                yield return new ValidationResult("Invalid value for AlboProfessionale, length must be less than 60.", new [] { "AlboProfessionale" });
            }

            // AlboProfessionale (string) minLength
            if (this.AlboProfessionale != null && this.AlboProfessionale.Length < 0)
            {
                yield return new ValidationResult("Invalid value for AlboProfessionale, length must be greater than 0.", new [] { "AlboProfessionale" });
            }

            // ProvinciaAlbo (string) maxLength
            if (this.ProvinciaAlbo != null && this.ProvinciaAlbo.Length > 2)
            {
                yield return new ValidationResult("Invalid value for ProvinciaAlbo, length must be less than 2.", new [] { "ProvinciaAlbo" });
            }

            // ProvinciaAlbo (string) minLength
            if (this.ProvinciaAlbo != null && this.ProvinciaAlbo.Length < 0)
            {
                yield return new ValidationResult("Invalid value for ProvinciaAlbo, length must be greater than 0.", new [] { "ProvinciaAlbo" });
            }

            // NumeroIscrizioneAlbo (string) maxLength
            if (this.NumeroIscrizioneAlbo != null && this.NumeroIscrizioneAlbo.Length > 60)
            {
                yield return new ValidationResult("Invalid value for NumeroIscrizioneAlbo, length must be less than 60.", new [] { "NumeroIscrizioneAlbo" });
            }

            // NumeroIscrizioneAlbo (string) minLength
            if (this.NumeroIscrizioneAlbo != null && this.NumeroIscrizioneAlbo.Length < 0)
            {
                yield return new ValidationResult("Invalid value for NumeroIscrizioneAlbo, length must be greater than 0.", new [] { "NumeroIscrizioneAlbo" });
            }

            // RegimeFiscale (string) maxLength
            if (this.RegimeFiscale != null && this.RegimeFiscale.Length > 4)
            {
                yield return new ValidationResult("Invalid value for RegimeFiscale, length must be less than 4.", new [] { "RegimeFiscale" });
            }

            // RegimeFiscale (string) minLength
            if (this.RegimeFiscale != null && this.RegimeFiscale.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RegimeFiscale, length must be greater than 0.", new [] { "RegimeFiscale" });
            }

            // SedeNazione (string) maxLength
            if (this.SedeNazione != null && this.SedeNazione.Length > 2)
            {
                yield return new ValidationResult("Invalid value for SedeNazione, length must be less than 2.", new [] { "SedeNazione" });
            }

            // SedeNazione (string) minLength
            if (this.SedeNazione != null && this.SedeNazione.Length < 0)
            {
                yield return new ValidationResult("Invalid value for SedeNazione, length must be greater than 0.", new [] { "SedeNazione" });
            }

            // SedeNumeroCivico (string) maxLength
            if (this.SedeNumeroCivico != null && this.SedeNumeroCivico.Length > 8)
            {
                yield return new ValidationResult("Invalid value for SedeNumeroCivico, length must be less than 8.", new [] { "SedeNumeroCivico" });
            }

            // SedeNumeroCivico (string) minLength
            if (this.SedeNumeroCivico != null && this.SedeNumeroCivico.Length < 0)
            {
                yield return new ValidationResult("Invalid value for SedeNumeroCivico, length must be greater than 0.", new [] { "SedeNumeroCivico" });
            }

            // StabileOrganizzazioneIndirizzo (string) maxLength
            if (this.StabileOrganizzazioneIndirizzo != null && this.StabileOrganizzazioneIndirizzo.Length > 60)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneIndirizzo, length must be less than 60.", new [] { "StabileOrganizzazioneIndirizzo" });
            }

            // StabileOrganizzazioneIndirizzo (string) minLength
            if (this.StabileOrganizzazioneIndirizzo != null && this.StabileOrganizzazioneIndirizzo.Length < 0)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneIndirizzo, length must be greater than 0.", new [] { "StabileOrganizzazioneIndirizzo" });
            }

            // StabileOrganizzazioneNumeroCivico (string) maxLength
            if (this.StabileOrganizzazioneNumeroCivico != null && this.StabileOrganizzazioneNumeroCivico.Length > 8)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneNumeroCivico, length must be less than 8.", new [] { "StabileOrganizzazioneNumeroCivico" });
            }

            // StabileOrganizzazioneNumeroCivico (string) minLength
            if (this.StabileOrganizzazioneNumeroCivico != null && this.StabileOrganizzazioneNumeroCivico.Length < 0)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneNumeroCivico, length must be greater than 0.", new [] { "StabileOrganizzazioneNumeroCivico" });
            }

            // StabileOrganizzazioneCap (string) maxLength
            if (this.StabileOrganizzazioneCap != null && this.StabileOrganizzazioneCap.Length > 5)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneCap, length must be less than 5.", new [] { "StabileOrganizzazioneCap" });
            }

            // StabileOrganizzazioneCap (string) minLength
            if (this.StabileOrganizzazioneCap != null && this.StabileOrganizzazioneCap.Length < 0)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneCap, length must be greater than 0.", new [] { "StabileOrganizzazioneCap" });
            }

            // StabileOrganizzazioneComune (string) maxLength
            if (this.StabileOrganizzazioneComune != null && this.StabileOrganizzazioneComune.Length > 60)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneComune, length must be less than 60.", new [] { "StabileOrganizzazioneComune" });
            }

            // StabileOrganizzazioneComune (string) minLength
            if (this.StabileOrganizzazioneComune != null && this.StabileOrganizzazioneComune.Length < 0)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneComune, length must be greater than 0.", new [] { "StabileOrganizzazioneComune" });
            }

            // StabileOrganizzazioneProvincia (string) maxLength
            if (this.StabileOrganizzazioneProvincia != null && this.StabileOrganizzazioneProvincia.Length > 2)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneProvincia, length must be less than 2.", new [] { "StabileOrganizzazioneProvincia" });
            }

            // StabileOrganizzazioneProvincia (string) minLength
            if (this.StabileOrganizzazioneProvincia != null && this.StabileOrganizzazioneProvincia.Length < 0)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneProvincia, length must be greater than 0.", new [] { "StabileOrganizzazioneProvincia" });
            }

            // StabileOrganizzazioneNazione (string) maxLength
            if (this.StabileOrganizzazioneNazione != null && this.StabileOrganizzazioneNazione.Length > 2)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneNazione, length must be less than 2.", new [] { "StabileOrganizzazioneNazione" });
            }

            // StabileOrganizzazioneNazione (string) minLength
            if (this.StabileOrganizzazioneNazione != null && this.StabileOrganizzazioneNazione.Length < 0)
            {
                yield return new ValidationResult("Invalid value for StabileOrganizzazioneNazione, length must be greater than 0.", new [] { "StabileOrganizzazioneNazione" });
            }

            // ReaNumero (string) maxLength
            if (this.ReaNumero != null && this.ReaNumero.Length > 20)
            {
                yield return new ValidationResult("Invalid value for ReaNumero, length must be less than 20.", new [] { "ReaNumero" });
            }

            // ReaNumero (string) minLength
            if (this.ReaNumero != null && this.ReaNumero.Length < 0)
            {
                yield return new ValidationResult("Invalid value for ReaNumero, length must be greater than 0.", new [] { "ReaNumero" });
            }

            // ReaUfficio (string) maxLength
            if (this.ReaUfficio != null && this.ReaUfficio.Length > 2)
            {
                yield return new ValidationResult("Invalid value for ReaUfficio, length must be less than 2.", new [] { "ReaUfficio" });
            }

            // ReaUfficio (string) minLength
            if (this.ReaUfficio != null && this.ReaUfficio.Length < 0)
            {
                yield return new ValidationResult("Invalid value for ReaUfficio, length must be greater than 0.", new [] { "ReaUfficio" });
            }

            // ReaSocioUnico (string) maxLength
            if (this.ReaSocioUnico != null && this.ReaSocioUnico.Length > 2)
            {
                yield return new ValidationResult("Invalid value for ReaSocioUnico, length must be less than 2.", new [] { "ReaSocioUnico" });
            }

            // ReaSocioUnico (string) minLength
            if (this.ReaSocioUnico != null && this.ReaSocioUnico.Length < 0)
            {
                yield return new ValidationResult("Invalid value for ReaSocioUnico, length must be greater than 0.", new [] { "ReaSocioUnico" });
            }

            // ReaStatoLiquidazione (string) maxLength
            if (this.ReaStatoLiquidazione != null && this.ReaStatoLiquidazione.Length > 2)
            {
                yield return new ValidationResult("Invalid value for ReaStatoLiquidazione, length must be less than 2.", new [] { "ReaStatoLiquidazione" });
            }

            // ReaStatoLiquidazione (string) minLength
            if (this.ReaStatoLiquidazione != null && this.ReaStatoLiquidazione.Length < 0)
            {
                yield return new ValidationResult("Invalid value for ReaStatoLiquidazione, length must be greater than 0.", new [] { "ReaStatoLiquidazione" });
            }

            // RappresentanteIdPaese (string) maxLength
            if (this.RappresentanteIdPaese != null && this.RappresentanteIdPaese.Length > 2)
            {
                yield return new ValidationResult("Invalid value for RappresentanteIdPaese, length must be less than 2.", new [] { "RappresentanteIdPaese" });
            }

            // RappresentanteIdPaese (string) minLength
            if (this.RappresentanteIdPaese != null && this.RappresentanteIdPaese.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RappresentanteIdPaese, length must be greater than 0.", new [] { "RappresentanteIdPaese" });
            }

            // RappresentanteIdCodice (string) maxLength
            if (this.RappresentanteIdCodice != null && this.RappresentanteIdCodice.Length > 28)
            {
                yield return new ValidationResult("Invalid value for RappresentanteIdCodice, length must be less than 28.", new [] { "RappresentanteIdCodice" });
            }

            // RappresentanteIdCodice (string) minLength
            if (this.RappresentanteIdCodice != null && this.RappresentanteIdCodice.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RappresentanteIdCodice, length must be greater than 0.", new [] { "RappresentanteIdCodice" });
            }

            // RappresentanteCodiceFiscale (string) maxLength
            if (this.RappresentanteCodiceFiscale != null && this.RappresentanteCodiceFiscale.Length > 16)
            {
                yield return new ValidationResult("Invalid value for RappresentanteCodiceFiscale, length must be less than 16.", new [] { "RappresentanteCodiceFiscale" });
            }

            // RappresentanteCodiceFiscale (string) minLength
            if (this.RappresentanteCodiceFiscale != null && this.RappresentanteCodiceFiscale.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RappresentanteCodiceFiscale, length must be greater than 0.", new [] { "RappresentanteCodiceFiscale" });
            }

            // RappresentanteDenominazione (string) maxLength
            if (this.RappresentanteDenominazione != null && this.RappresentanteDenominazione.Length > 80)
            {
                yield return new ValidationResult("Invalid value for RappresentanteDenominazione, length must be less than 80.", new [] { "RappresentanteDenominazione" });
            }

            // RappresentanteDenominazione (string) minLength
            if (this.RappresentanteDenominazione != null && this.RappresentanteDenominazione.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RappresentanteDenominazione, length must be greater than 0.", new [] { "RappresentanteDenominazione" });
            }

            // RappresentanteNome (string) maxLength
            if (this.RappresentanteNome != null && this.RappresentanteNome.Length > 60)
            {
                yield return new ValidationResult("Invalid value for RappresentanteNome, length must be less than 60.", new [] { "RappresentanteNome" });
            }

            // RappresentanteNome (string) minLength
            if (this.RappresentanteNome != null && this.RappresentanteNome.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RappresentanteNome, length must be greater than 0.", new [] { "RappresentanteNome" });
            }

            // RappresentanteCognome (string) maxLength
            if (this.RappresentanteCognome != null && this.RappresentanteCognome.Length > 60)
            {
                yield return new ValidationResult("Invalid value for RappresentanteCognome, length must be less than 60.", new [] { "RappresentanteCognome" });
            }

            // RappresentanteCognome (string) minLength
            if (this.RappresentanteCognome != null && this.RappresentanteCognome.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RappresentanteCognome, length must be greater than 0.", new [] { "RappresentanteCognome" });
            }

            // RappresentanteTitolo (string) maxLength
            if (this.RappresentanteTitolo != null && this.RappresentanteTitolo.Length > 10)
            {
                yield return new ValidationResult("Invalid value for RappresentanteTitolo, length must be less than 10.", new [] { "RappresentanteTitolo" });
            }

            // RappresentanteTitolo (string) minLength
            if (this.RappresentanteTitolo != null && this.RappresentanteTitolo.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RappresentanteTitolo, length must be greater than 0.", new [] { "RappresentanteTitolo" });
            }

            // RappresentanteCodEori (string) maxLength
            if (this.RappresentanteCodEori != null && this.RappresentanteCodEori.Length > 17)
            {
                yield return new ValidationResult("Invalid value for RappresentanteCodEori, length must be less than 17.", new [] { "RappresentanteCodEori" });
            }

            // RappresentanteCodEori (string) minLength
            if (this.RappresentanteCodEori != null && this.RappresentanteCodEori.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RappresentanteCodEori, length must be greater than 0.", new [] { "RappresentanteCodEori" });
            }

            // IntermediarioIdPaese (string) maxLength
            if (this.IntermediarioIdPaese != null && this.IntermediarioIdPaese.Length > 2)
            {
                yield return new ValidationResult("Invalid value for IntermediarioIdPaese, length must be less than 2.", new [] { "IntermediarioIdPaese" });
            }

            // IntermediarioIdPaese (string) minLength
            if (this.IntermediarioIdPaese != null && this.IntermediarioIdPaese.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IntermediarioIdPaese, length must be greater than 0.", new [] { "IntermediarioIdPaese" });
            }

            // IntermediarioIdCodice (string) maxLength
            if (this.IntermediarioIdCodice != null && this.IntermediarioIdCodice.Length > 28)
            {
                yield return new ValidationResult("Invalid value for IntermediarioIdCodice, length must be less than 28.", new [] { "IntermediarioIdCodice" });
            }

            // IntermediarioIdCodice (string) minLength
            if (this.IntermediarioIdCodice != null && this.IntermediarioIdCodice.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IntermediarioIdCodice, length must be greater than 0.", new [] { "IntermediarioIdCodice" });
            }

            // IntermediarioCodiceFiscale (string) maxLength
            if (this.IntermediarioCodiceFiscale != null && this.IntermediarioCodiceFiscale.Length > 16)
            {
                yield return new ValidationResult("Invalid value for IntermediarioCodiceFiscale, length must be less than 16.", new [] { "IntermediarioCodiceFiscale" });
            }

            // IntermediarioCodiceFiscale (string) minLength
            if (this.IntermediarioCodiceFiscale != null && this.IntermediarioCodiceFiscale.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IntermediarioCodiceFiscale, length must be greater than 0.", new [] { "IntermediarioCodiceFiscale" });
            }

            // IntermediarioDenominazione (string) maxLength
            if (this.IntermediarioDenominazione != null && this.IntermediarioDenominazione.Length > 80)
            {
                yield return new ValidationResult("Invalid value for IntermediarioDenominazione, length must be less than 80.", new [] { "IntermediarioDenominazione" });
            }

            // IntermediarioDenominazione (string) minLength
            if (this.IntermediarioDenominazione != null && this.IntermediarioDenominazione.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IntermediarioDenominazione, length must be greater than 0.", new [] { "IntermediarioDenominazione" });
            }

            // IntermediarioNome (string) maxLength
            if (this.IntermediarioNome != null && this.IntermediarioNome.Length > 60)
            {
                yield return new ValidationResult("Invalid value for IntermediarioNome, length must be less than 60.", new [] { "IntermediarioNome" });
            }

            // IntermediarioNome (string) minLength
            if (this.IntermediarioNome != null && this.IntermediarioNome.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IntermediarioNome, length must be greater than 0.", new [] { "IntermediarioNome" });
            }

            // IntermediarioCognome (string) maxLength
            if (this.IntermediarioCognome != null && this.IntermediarioCognome.Length > 60)
            {
                yield return new ValidationResult("Invalid value for IntermediarioCognome, length must be less than 60.", new [] { "IntermediarioCognome" });
            }

            // IntermediarioCognome (string) minLength
            if (this.IntermediarioCognome != null && this.IntermediarioCognome.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IntermediarioCognome, length must be greater than 0.", new [] { "IntermediarioCognome" });
            }

            // IntermediarioTitolo (string) maxLength
            if (this.IntermediarioTitolo != null && this.IntermediarioTitolo.Length > 10)
            {
                yield return new ValidationResult("Invalid value for IntermediarioTitolo, length must be less than 10.", new [] { "IntermediarioTitolo" });
            }

            // IntermediarioTitolo (string) minLength
            if (this.IntermediarioTitolo != null && this.IntermediarioTitolo.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IntermediarioTitolo, length must be greater than 0.", new [] { "IntermediarioTitolo" });
            }

            // IntermediarioCodEori (string) maxLength
            if (this.IntermediarioCodEori != null && this.IntermediarioCodEori.Length > 17)
            {
                yield return new ValidationResult("Invalid value for IntermediarioCodEori, length must be less than 17.", new [] { "IntermediarioCodEori" });
            }

            // IntermediarioCodEori (string) minLength
            if (this.IntermediarioCodEori != null && this.IntermediarioCodEori.Length < 0)
            {
                yield return new ValidationResult("Invalid value for IntermediarioCodEori, length must be greater than 0.", new [] { "IntermediarioCodEori" });
            }

            yield break;
        }
    }

}

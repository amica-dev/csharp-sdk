/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.1.0
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
    /// Articolo
    /// </summary>
    [DataContract(Name = "Articolo")]
    public partial class Articolo : IValidatableObject
    {
        /// <summary>
        /// Defines GestioneArticolo
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum GestioneArticoloEnum
        {
            /// <summary>
            /// Enum Standard for value: Standard
            /// </summary>
            [EnumMember(Value = "Standard")]
            Standard = 1,

            /// <summary>
            /// Enum TaglieColori for value: TaglieColori
            /// </summary>
            [EnumMember(Value = "TaglieColori")]
            TaglieColori = 2,

            /// <summary>
            /// Enum NumeriSeriali for value: NumeriSeriali
            /// </summary>
            [EnumMember(Value = "NumeriSeriali")]
            NumeriSeriali = 3,

            /// <summary>
            /// Enum Lotti for value: Lotti
            /// </summary>
            [EnumMember(Value = "Lotti")]
            Lotti = 4
        }


        /// <summary>
        /// Gets or Sets GestioneArticolo
        /// </summary>
        [DataMember(Name = "gestioneArticolo", EmitDefaultValue = false)]
        public GestioneArticoloEnum? GestioneArticolo { get; set; }
        /// <summary>
        /// Defines ContestoArticolo
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ContestoArticoloEnum
        {
            /// <summary>
            /// Enum Sempre for value: Sempre
            /// </summary>
            [EnumMember(Value = "Sempre")]
            Sempre = 1,

            /// <summary>
            /// Enum Vendite for value: Vendite
            /// </summary>
            [EnumMember(Value = "Vendite")]
            Vendite = 2,

            /// <summary>
            /// Enum Acquisti for value: Acquisti
            /// </summary>
            [EnumMember(Value = "Acquisti")]
            Acquisti = 3,

            /// <summary>
            /// Enum Mai for value: Mai
            /// </summary>
            [EnumMember(Value = "Mai")]
            Mai = 4
        }


        /// <summary>
        /// Gets or Sets ContestoArticolo
        /// </summary>
        [DataMember(Name = "contestoArticolo", EmitDefaultValue = false)]
        public ContestoArticoloEnum? ContestoArticolo { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Articolo" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected Articolo() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Articolo" /> class.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="concurrencyToken">concurrencyToken.</param>
        /// <param name="categoriaMerceologicaId">categoriaMerceologicaId.</param>
        /// <param name="categoriaMerceologica">categoriaMerceologica.</param>
        /// <param name="gruppoMerceologicoId">gruppoMerceologicoId.</param>
        /// <param name="gruppoMerceologico">gruppoMerceologico.</param>
        /// <param name="causaleIvaId">causaleIvaId.</param>
        /// <param name="causaleIva">causaleIva.</param>
        /// <param name="tagliaId">tagliaId.</param>
        /// <param name="taglia">taglia.</param>
        /// <param name="marcaId">marcaId.</param>
        /// <param name="marca">marca.</param>
        /// <param name="produttoreId">produttoreId.</param>
        /// <param name="produttore">produttore.</param>
        /// <param name="repartoId">repartoId.</param>
        /// <param name="reparto">reparto.</param>
        /// <param name="nome">nome (required).</param>
        /// <param name="codice">codice (required).</param>
        /// <param name="nomeBreve">nomeBreve.</param>
        /// <param name="codiceFornitore">codiceFornitore.</param>
        /// <param name="codiceProduttore">codiceProduttore.</param>
        /// <param name="gestioneArticolo">gestioneArticolo.</param>
        /// <param name="contestoArticolo">contestoArticolo.</param>
        /// <param name="barCode">barCode.</param>
        /// <param name="costoUltimo">costoUltimo.</param>
        /// <param name="costoMedio">costoMedio.</param>
        /// <param name="unitMisuraIntera">unitMisuraIntera.</param>
        /// <param name="articoloComposto">articoloComposto.</param>
        /// <param name="stato">stato.</param>
        /// <param name="dataInserimento">dataInserimento.</param>
        /// <param name="unitMisuraMovimenti">unitMisuraMovimenti.</param>
        /// <param name="unitMisuraInventario">unitMisuraInventario.</param>
        /// <param name="fattoreConversioneUnitMisura">fattoreConversioneUnitMisura.</param>
        /// <param name="note">note.</param>
        /// <param name="mediaFile">mediaFile.</param>
        /// <param name="contoRawVendita">contoRawVendita.</param>
        /// <param name="contoRawAcquisto">contoRawAcquisto.</param>
        /// <param name="distintaBase">distintaBase.</param>
        public Articolo(int id = default(int), int concurrencyToken = default(int), int? categoriaMerceologicaId = default(int?), CategoriaMerceologica categoriaMerceologica = default(CategoriaMerceologica), int? gruppoMerceologicoId = default(int?), GruppoMerceologico gruppoMerceologico = default(GruppoMerceologico), int? causaleIvaId = default(int?), CausaleIva causaleIva = default(CausaleIva), int? tagliaId = default(int?), Taglia taglia = default(Taglia), int? marcaId = default(int?), Marca marca = default(Marca), int? produttoreId = default(int?), Produttore produttore = default(Produttore), int? repartoId = default(int?), Reparto reparto = default(Reparto), string nome = default(string), string codice = default(string), string nomeBreve = default(string), string codiceFornitore = default(string), string codiceProduttore = default(string), GestioneArticoloEnum? gestioneArticolo = default(GestioneArticoloEnum?), ContestoArticoloEnum? contestoArticolo = default(ContestoArticoloEnum?), string barCode = default(string), double costoUltimo = default(double), double costoMedio = default(double), bool unitMisuraIntera = default(bool), bool articoloComposto = default(bool), bool stato = default(bool), DateTime? dataInserimento = default(DateTime?), string unitMisuraMovimenti = default(string), string unitMisuraInventario = default(string), double fattoreConversioneUnitMisura = default(double), string note = default(string), ArticoloMediaFile mediaFile = default(ArticoloMediaFile), string contoRawVendita = default(string), string contoRawAcquisto = default(string), List<Componente> distintaBase = default(List<Componente>))
        {
            // to ensure "nome" is required (not null)
            if (nome == null)
            {
                throw new ArgumentNullException("nome is a required property for Articolo and cannot be null");
            }
            this.Nome = nome;
            // to ensure "codice" is required (not null)
            if (codice == null)
            {
                throw new ArgumentNullException("codice is a required property for Articolo and cannot be null");
            }
            this.Codice = codice;
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.CategoriaMerceologicaId = categoriaMerceologicaId;
            this.CategoriaMerceologica = categoriaMerceologica;
            this.GruppoMerceologicoId = gruppoMerceologicoId;
            this.GruppoMerceologico = gruppoMerceologico;
            this.CausaleIvaId = causaleIvaId;
            this.CausaleIva = causaleIva;
            this.TagliaId = tagliaId;
            this.Taglia = taglia;
            this.MarcaId = marcaId;
            this.Marca = marca;
            this.ProduttoreId = produttoreId;
            this.Produttore = produttore;
            this.RepartoId = repartoId;
            this.Reparto = reparto;
            this.NomeBreve = nomeBreve;
            this.CodiceFornitore = codiceFornitore;
            this.CodiceProduttore = codiceProduttore;
            this.GestioneArticolo = gestioneArticolo;
            this.ContestoArticolo = contestoArticolo;
            this.BarCode = barCode;
            this.CostoUltimo = costoUltimo;
            this.CostoMedio = costoMedio;
            this.UnitMisuraIntera = unitMisuraIntera;
            this.ArticoloComposto = articoloComposto;
            this.Stato = stato;
            this.DataInserimento = dataInserimento;
            this.UnitMisuraMovimenti = unitMisuraMovimenti;
            this.UnitMisuraInventario = unitMisuraInventario;
            this.FattoreConversioneUnitMisura = fattoreConversioneUnitMisura;
            this.Note = note;
            this.MediaFile = mediaFile;
            this.ContoRawVendita = contoRawVendita;
            this.ContoRawAcquisto = contoRawAcquisto;
            this.DistintaBase = distintaBase;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or Sets ConcurrencyToken
        /// </summary>
        [DataMember(Name = "concurrencyToken", EmitDefaultValue = false)]
        public int ConcurrencyToken { get; set; }

        /// <summary>
        /// Gets or Sets CategoriaMerceologicaId
        /// </summary>
        [DataMember(Name = "categoriaMerceologicaId", EmitDefaultValue = true)]
        public int? CategoriaMerceologicaId { get; set; }

        /// <summary>
        /// Gets or Sets CategoriaMerceologica
        /// </summary>
        [DataMember(Name = "categoriaMerceologica", EmitDefaultValue = false)]
        public CategoriaMerceologica CategoriaMerceologica { get; set; }

        /// <summary>
        /// Gets or Sets GruppoMerceologicoId
        /// </summary>
        [DataMember(Name = "gruppoMerceologicoId", EmitDefaultValue = true)]
        public int? GruppoMerceologicoId { get; set; }

        /// <summary>
        /// Gets or Sets GruppoMerceologico
        /// </summary>
        [DataMember(Name = "gruppoMerceologico", EmitDefaultValue = false)]
        public GruppoMerceologico GruppoMerceologico { get; set; }

        /// <summary>
        /// Gets or Sets CausaleIvaId
        /// </summary>
        [DataMember(Name = "causaleIvaId", EmitDefaultValue = true)]
        public int? CausaleIvaId { get; set; }

        /// <summary>
        /// Gets or Sets CausaleIva
        /// </summary>
        [DataMember(Name = "causaleIva", EmitDefaultValue = false)]
        public CausaleIva CausaleIva { get; set; }

        /// <summary>
        /// Gets or Sets TagliaId
        /// </summary>
        [DataMember(Name = "tagliaId", EmitDefaultValue = true)]
        public int? TagliaId { get; set; }

        /// <summary>
        /// Gets or Sets Taglia
        /// </summary>
        [DataMember(Name = "taglia", EmitDefaultValue = false)]
        public Taglia Taglia { get; set; }

        /// <summary>
        /// Gets or Sets MarcaId
        /// </summary>
        [DataMember(Name = "marcaId", EmitDefaultValue = true)]
        public int? MarcaId { get; set; }

        /// <summary>
        /// Gets or Sets Marca
        /// </summary>
        [DataMember(Name = "marca", EmitDefaultValue = false)]
        public Marca Marca { get; set; }

        /// <summary>
        /// Gets or Sets ProduttoreId
        /// </summary>
        [DataMember(Name = "produttoreId", EmitDefaultValue = true)]
        public int? ProduttoreId { get; set; }

        /// <summary>
        /// Gets or Sets Produttore
        /// </summary>
        [DataMember(Name = "produttore", EmitDefaultValue = false)]
        public Produttore Produttore { get; set; }

        /// <summary>
        /// Gets or Sets RepartoId
        /// </summary>
        [DataMember(Name = "repartoId", EmitDefaultValue = true)]
        public int? RepartoId { get; set; }

        /// <summary>
        /// Gets or Sets Reparto
        /// </summary>
        [DataMember(Name = "reparto", EmitDefaultValue = false)]
        public Reparto Reparto { get; set; }

        /// <summary>
        /// Gets or Sets Nome
        /// </summary>
        [DataMember(Name = "nome", IsRequired = true, EmitDefaultValue = true)]
        public string Nome { get; set; }

        /// <summary>
        /// Gets or Sets Codice
        /// </summary>
        [DataMember(Name = "codice", IsRequired = true, EmitDefaultValue = true)]
        public string Codice { get; set; }

        /// <summary>
        /// Gets or Sets NomeBreve
        /// </summary>
        [DataMember(Name = "nomeBreve", EmitDefaultValue = true)]
        public string NomeBreve { get; set; }

        /// <summary>
        /// Gets or Sets CodiceFornitore
        /// </summary>
        [DataMember(Name = "codiceFornitore", EmitDefaultValue = true)]
        public string CodiceFornitore { get; set; }

        /// <summary>
        /// Gets or Sets CodiceProduttore
        /// </summary>
        [DataMember(Name = "codiceProduttore", EmitDefaultValue = true)]
        public string CodiceProduttore { get; set; }

        /// <summary>
        /// Gets or Sets BarCode
        /// </summary>
        [DataMember(Name = "barCode", EmitDefaultValue = true)]
        public string BarCode { get; set; }

        /// <summary>
        /// Gets or Sets CostoUltimo
        /// </summary>
        [DataMember(Name = "costoUltimo", EmitDefaultValue = false)]
        public double CostoUltimo { get; set; }

        /// <summary>
        /// Gets or Sets CostoMedio
        /// </summary>
        [DataMember(Name = "costoMedio", EmitDefaultValue = false)]
        public double CostoMedio { get; set; }

        /// <summary>
        /// Gets or Sets UnitMisuraIntera
        /// </summary>
        [DataMember(Name = "unitàMisuraIntera", EmitDefaultValue = true)]
        public bool UnitMisuraIntera { get; set; }

        /// <summary>
        /// Gets or Sets ArticoloComposto
        /// </summary>
        [DataMember(Name = "articoloComposto", EmitDefaultValue = true)]
        public bool ArticoloComposto { get; set; }

        /// <summary>
        /// Gets or Sets Stato
        /// </summary>
        [DataMember(Name = "stato", EmitDefaultValue = true)]
        public bool Stato { get; set; }

        /// <summary>
        /// Gets or Sets DataInserimento
        /// </summary>
        [DataMember(Name = "dataInserimento", EmitDefaultValue = true)]
        public DateTime? DataInserimento { get; set; }

        /// <summary>
        /// Gets or Sets UnitMisuraMovimenti
        /// </summary>
        [DataMember(Name = "unitàMisuraMovimenti", EmitDefaultValue = true)]
        public string UnitMisuraMovimenti { get; set; }

        /// <summary>
        /// Gets or Sets UnitMisuraInventario
        /// </summary>
        [DataMember(Name = "unitàMisuraInventario", EmitDefaultValue = true)]
        public string UnitMisuraInventario { get; set; }

        /// <summary>
        /// Gets or Sets FattoreConversioneUnitMisura
        /// </summary>
        [DataMember(Name = "fattoreConversioneUnitàMisura", EmitDefaultValue = false)]
        public double FattoreConversioneUnitMisura { get; set; }

        /// <summary>
        /// Gets or Sets Note
        /// </summary>
        [DataMember(Name = "note", EmitDefaultValue = true)]
        public string Note { get; set; }

        /// <summary>
        /// Gets or Sets MediaFile
        /// </summary>
        [DataMember(Name = "mediaFile", EmitDefaultValue = false)]
        public ArticoloMediaFile MediaFile { get; set; }

        /// <summary>
        /// Gets or Sets ContoRawVendita
        /// </summary>
        [DataMember(Name = "contoRawVendita", EmitDefaultValue = true)]
        public string ContoRawVendita { get; set; }

        /// <summary>
        /// Gets or Sets ContoRawAcquisto
        /// </summary>
        [DataMember(Name = "contoRawAcquisto", EmitDefaultValue = true)]
        public string ContoRawAcquisto { get; set; }

        /// <summary>
        /// Gets or Sets DistintaBase
        /// </summary>
        [DataMember(Name = "distintaBase", EmitDefaultValue = true)]
        public List<Componente> DistintaBase { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Articolo {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  CategoriaMerceologicaId: ").Append(CategoriaMerceologicaId).Append("\n");
            sb.Append("  CategoriaMerceologica: ").Append(CategoriaMerceologica).Append("\n");
            sb.Append("  GruppoMerceologicoId: ").Append(GruppoMerceologicoId).Append("\n");
            sb.Append("  GruppoMerceologico: ").Append(GruppoMerceologico).Append("\n");
            sb.Append("  CausaleIvaId: ").Append(CausaleIvaId).Append("\n");
            sb.Append("  CausaleIva: ").Append(CausaleIva).Append("\n");
            sb.Append("  TagliaId: ").Append(TagliaId).Append("\n");
            sb.Append("  Taglia: ").Append(Taglia).Append("\n");
            sb.Append("  MarcaId: ").Append(MarcaId).Append("\n");
            sb.Append("  Marca: ").Append(Marca).Append("\n");
            sb.Append("  ProduttoreId: ").Append(ProduttoreId).Append("\n");
            sb.Append("  Produttore: ").Append(Produttore).Append("\n");
            sb.Append("  RepartoId: ").Append(RepartoId).Append("\n");
            sb.Append("  Reparto: ").Append(Reparto).Append("\n");
            sb.Append("  Nome: ").Append(Nome).Append("\n");
            sb.Append("  Codice: ").Append(Codice).Append("\n");
            sb.Append("  NomeBreve: ").Append(NomeBreve).Append("\n");
            sb.Append("  CodiceFornitore: ").Append(CodiceFornitore).Append("\n");
            sb.Append("  CodiceProduttore: ").Append(CodiceProduttore).Append("\n");
            sb.Append("  GestioneArticolo: ").Append(GestioneArticolo).Append("\n");
            sb.Append("  ContestoArticolo: ").Append(ContestoArticolo).Append("\n");
            sb.Append("  BarCode: ").Append(BarCode).Append("\n");
            sb.Append("  CostoUltimo: ").Append(CostoUltimo).Append("\n");
            sb.Append("  CostoMedio: ").Append(CostoMedio).Append("\n");
            sb.Append("  UnitMisuraIntera: ").Append(UnitMisuraIntera).Append("\n");
            sb.Append("  ArticoloComposto: ").Append(ArticoloComposto).Append("\n");
            sb.Append("  Stato: ").Append(Stato).Append("\n");
            sb.Append("  DataInserimento: ").Append(DataInserimento).Append("\n");
            sb.Append("  UnitMisuraMovimenti: ").Append(UnitMisuraMovimenti).Append("\n");
            sb.Append("  UnitMisuraInventario: ").Append(UnitMisuraInventario).Append("\n");
            sb.Append("  FattoreConversioneUnitMisura: ").Append(FattoreConversioneUnitMisura).Append("\n");
            sb.Append("  Note: ").Append(Note).Append("\n");
            sb.Append("  MediaFile: ").Append(MediaFile).Append("\n");
            sb.Append("  ContoRawVendita: ").Append(ContoRawVendita).Append("\n");
            sb.Append("  ContoRawAcquisto: ").Append(ContoRawAcquisto).Append("\n");
            sb.Append("  DistintaBase: ").Append(DistintaBase).Append("\n");
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
            // Nome (string) maxLength
            if (this.Nome != null && this.Nome.Length > 254)
            {
                yield return new ValidationResult("Invalid value for Nome, length must be less than 254.", new [] { "Nome" });
            }

            // Nome (string) minLength
            if (this.Nome != null && this.Nome.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Nome, length must be greater than 0.", new [] { "Nome" });
            }

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

            // NomeBreve (string) maxLength
            if (this.NomeBreve != null && this.NomeBreve.Length > 25)
            {
                yield return new ValidationResult("Invalid value for NomeBreve, length must be less than 25.", new [] { "NomeBreve" });
            }

            // NomeBreve (string) minLength
            if (this.NomeBreve != null && this.NomeBreve.Length < 0)
            {
                yield return new ValidationResult("Invalid value for NomeBreve, length must be greater than 0.", new [] { "NomeBreve" });
            }

            // CodiceFornitore (string) maxLength
            if (this.CodiceFornitore != null && this.CodiceFornitore.Length > 30)
            {
                yield return new ValidationResult("Invalid value for CodiceFornitore, length must be less than 30.", new [] { "CodiceFornitore" });
            }

            // CodiceFornitore (string) minLength
            if (this.CodiceFornitore != null && this.CodiceFornitore.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceFornitore, length must be greater than 0.", new [] { "CodiceFornitore" });
            }

            // CodiceProduttore (string) maxLength
            if (this.CodiceProduttore != null && this.CodiceProduttore.Length > 30)
            {
                yield return new ValidationResult("Invalid value for CodiceProduttore, length must be less than 30.", new [] { "CodiceProduttore" });
            }

            // CodiceProduttore (string) minLength
            if (this.CodiceProduttore != null && this.CodiceProduttore.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceProduttore, length must be greater than 0.", new [] { "CodiceProduttore" });
            }

            // BarCode (string) maxLength
            if (this.BarCode != null && this.BarCode.Length > 128)
            {
                yield return new ValidationResult("Invalid value for BarCode, length must be less than 128.", new [] { "BarCode" });
            }

            // BarCode (string) minLength
            if (this.BarCode != null && this.BarCode.Length < 0)
            {
                yield return new ValidationResult("Invalid value for BarCode, length must be greater than 0.", new [] { "BarCode" });
            }

            yield break;
        }
    }

}

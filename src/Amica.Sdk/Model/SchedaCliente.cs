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
    /// Impostazioni e informazioni di vendita di un&#39;Amica.Anagrafica.
    /// </summary>
    [DataContract(Name = "SchedaCliente")]
    public partial class SchedaCliente : IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchedaCliente" /> class.
        /// </summary>
        /// <param name="id">Identificativo univoco..</param>
        /// <param name="concurrencyToken">Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato..</param>
        /// <param name="anagraficaId">Identificativo univoco di Amica.Anagrafica..</param>
        /// <param name="agenteId">Identificativo univoco di Amica.Anagrafica dell&#39;agente..</param>
        /// <param name="agente">agente.</param>
        /// <param name="pagamentoId">Identificativo univoco di Amica.SchedaCliente.Pagamento..</param>
        /// <param name="pagamento">pagamento.</param>
        /// <param name="listinoId">Identificativo univoco di Amica.SchedaCliente.Listino..</param>
        /// <param name="listino">listino.</param>
        /// <param name="categoriaAnagraficaId">Identificativo univoco di Amica.SchedaCliente.CategoriaAnagrafica..</param>
        /// <param name="categoriaAnagrafica">categoriaAnagrafica.</param>
        /// <param name="causaleIvaId">Identificativo univoco di Amica.SchedaCliente.CausaleIva..</param>
        /// <param name="causaleIva">causaleIva.</param>
        /// <param name="fido">Fido concesso al cliente..</param>
        /// <param name="sconto">Sconto riservato al cliente (percentuale)..</param>
        /// <param name="fatturatoAnnoCorrente">Fatturato realizzato nell&#39;anno corrente..</param>
        /// <param name="numeroFattureAnnoCorrente">Numero di fatture emesse nell&#39;anno corrente..</param>
        /// <param name="insolutiAnnoCorrente">Importo insoluto nell&#39;anno corrente..</param>
        /// <param name="numeroInsolutiAnnoCorrente">Numero di insoluti nell&#39;anno corrente..</param>
        /// <param name="fatturatoAnnoPrecedente">Fatturato realizzato nell&#39;anno precedente..</param>
        /// <param name="numeroFattureAnnoPrecedente">Numero di fatture emesse nell&#39;anno precedente..</param>
        /// <param name="insolutiAnnoPrecedente">Importo insoluto nell&#39;anno precedente..</param>
        /// <param name="numeroInsolutiAnnoPrecedente">Numero di insoluti verificatisi nell&#39;anno precedente..</param>
        /// <param name="dataUltimaFattura">Data dell&#39;ultima fattura..</param>
        /// <param name="dataUltimoPagamento">Data dell&#39;ultimo pagamento..</param>
        /// <param name="dataUltimoInsoluto">Data dell&#39;ultimo insoluto..</param>
        /// <param name="contoRaw">Numero di conto, in formato grezzo (es. \&quot;  100   20\&quot;). Sono ammessi fino a cinque livelli..</param>
        public SchedaCliente(int id = default(int), int concurrencyToken = default(int), int? anagraficaId = default(int?), int? agenteId = default(int?), Anagrafica agente = default(Anagrafica), int? pagamentoId = default(int?), Pagamento pagamento = default(Pagamento), int? listinoId = default(int?), Listino listino = default(Listino), int? categoriaAnagraficaId = default(int?), CategoriaAnagrafica categoriaAnagrafica = default(CategoriaAnagrafica), int? causaleIvaId = default(int?), CausaleIva causaleIva = default(CausaleIva), double fido = default(double), double sconto = default(double), double fatturatoAnnoCorrente = default(double), int numeroFattureAnnoCorrente = default(int), double insolutiAnnoCorrente = default(double), int numeroInsolutiAnnoCorrente = default(int), double fatturatoAnnoPrecedente = default(double), int numeroFattureAnnoPrecedente = default(int), double insolutiAnnoPrecedente = default(double), int numeroInsolutiAnnoPrecedente = default(int), DateTime? dataUltimaFattura = default(DateTime?), DateTime? dataUltimoPagamento = default(DateTime?), DateTime? dataUltimoInsoluto = default(DateTime?), string contoRaw = default(string))
        {
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.AnagraficaId = anagraficaId;
            this.AgenteId = agenteId;
            this.Agente = agente;
            this.PagamentoId = pagamentoId;
            this.Pagamento = pagamento;
            this.ListinoId = listinoId;
            this.Listino = listino;
            this.CategoriaAnagraficaId = categoriaAnagraficaId;
            this.CategoriaAnagrafica = categoriaAnagrafica;
            this.CausaleIvaId = causaleIvaId;
            this.CausaleIva = causaleIva;
            this.Fido = fido;
            this.Sconto = sconto;
            this.FatturatoAnnoCorrente = fatturatoAnnoCorrente;
            this.NumeroFattureAnnoCorrente = numeroFattureAnnoCorrente;
            this.InsolutiAnnoCorrente = insolutiAnnoCorrente;
            this.NumeroInsolutiAnnoCorrente = numeroInsolutiAnnoCorrente;
            this.FatturatoAnnoPrecedente = fatturatoAnnoPrecedente;
            this.NumeroFattureAnnoPrecedente = numeroFattureAnnoPrecedente;
            this.InsolutiAnnoPrecedente = insolutiAnnoPrecedente;
            this.NumeroInsolutiAnnoPrecedente = numeroInsolutiAnnoPrecedente;
            this.DataUltimaFattura = dataUltimaFattura;
            this.DataUltimoPagamento = dataUltimoPagamento;
            this.DataUltimoInsoluto = dataUltimoInsoluto;
            this.ContoRaw = contoRaw;
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
        [DataMember(Name = "anagraficaId", EmitDefaultValue = true)]
        public int? AnagraficaId { get; set; }

        /// <summary>
        /// Identificativo univoco di Amica.Anagrafica dell&#39;agente.
        /// </summary>
        /// <value>Identificativo univoco di Amica.Anagrafica dell&#39;agente.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "agenteId", EmitDefaultValue = true)]
        public int? AgenteId { get; set; }

        /// <summary>
        /// Gets or Sets Agente
        /// </summary>
        [DataMember(Name = "agente", EmitDefaultValue = false)]
        public Anagrafica Agente { get; set; }

        /// <summary>
        /// Identificativo univoco di Amica.SchedaCliente.Pagamento.
        /// </summary>
        /// <value>Identificativo univoco di Amica.SchedaCliente.Pagamento.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "pagamentoId", EmitDefaultValue = true)]
        public int? PagamentoId { get; set; }

        /// <summary>
        /// Gets or Sets Pagamento
        /// </summary>
        [DataMember(Name = "pagamento", EmitDefaultValue = false)]
        public Pagamento Pagamento { get; set; }

        /// <summary>
        /// Identificativo univoco di Amica.SchedaCliente.Listino.
        /// </summary>
        /// <value>Identificativo univoco di Amica.SchedaCliente.Listino.</value>
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
        /// Identificativo univoco di Amica.SchedaCliente.CategoriaAnagrafica.
        /// </summary>
        /// <value>Identificativo univoco di Amica.SchedaCliente.CategoriaAnagrafica.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "categoriaAnagraficaId", EmitDefaultValue = true)]
        public int? CategoriaAnagraficaId { get; set; }

        /// <summary>
        /// Gets or Sets CategoriaAnagrafica
        /// </summary>
        [DataMember(Name = "categoriaAnagrafica", EmitDefaultValue = false)]
        public CategoriaAnagrafica CategoriaAnagrafica { get; set; }

        /// <summary>
        /// Identificativo univoco di Amica.SchedaCliente.CausaleIva.
        /// </summary>
        /// <value>Identificativo univoco di Amica.SchedaCliente.CausaleIva.</value>
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
        /// Fido concesso al cliente.
        /// </summary>
        /// <value>Fido concesso al cliente.</value>
        /*
        <example>5000.0</example>
        */
        [DataMember(Name = "fido", EmitDefaultValue = false)]
        public double Fido { get; set; }

        /// <summary>
        /// Sconto riservato al cliente (percentuale).
        /// </summary>
        /// <value>Sconto riservato al cliente (percentuale).</value>
        /*
        <example>10.0</example>
        */
        [DataMember(Name = "sconto", EmitDefaultValue = false)]
        public double Sconto { get; set; }

        /// <summary>
        /// Fatturato realizzato nell&#39;anno corrente.
        /// </summary>
        /// <value>Fatturato realizzato nell&#39;anno corrente.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "fatturatoAnnoCorrente", EmitDefaultValue = false)]
        public double FatturatoAnnoCorrente { get; set; }

        /// <summary>
        /// Numero di fatture emesse nell&#39;anno corrente.
        /// </summary>
        /// <value>Numero di fatture emesse nell&#39;anno corrente.</value>
        /*
        <example>12</example>
        */
        [DataMember(Name = "numeroFattureAnnoCorrente", EmitDefaultValue = false)]
        public int NumeroFattureAnnoCorrente { get; set; }

        /// <summary>
        /// Importo insoluto nell&#39;anno corrente.
        /// </summary>
        /// <value>Importo insoluto nell&#39;anno corrente.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "insolutiAnnoCorrente", EmitDefaultValue = false)]
        public double InsolutiAnnoCorrente { get; set; }

        /// <summary>
        /// Numero di insoluti nell&#39;anno corrente.
        /// </summary>
        /// <value>Numero di insoluti nell&#39;anno corrente.</value>
        /*
        <example>2</example>
        */
        [DataMember(Name = "numeroInsolutiAnnoCorrente", EmitDefaultValue = false)]
        public int NumeroInsolutiAnnoCorrente { get; set; }

        /// <summary>
        /// Fatturato realizzato nell&#39;anno precedente.
        /// </summary>
        /// <value>Fatturato realizzato nell&#39;anno precedente.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "fatturatoAnnoPrecedente", EmitDefaultValue = false)]
        public double FatturatoAnnoPrecedente { get; set; }

        /// <summary>
        /// Numero di fatture emesse nell&#39;anno precedente.
        /// </summary>
        /// <value>Numero di fatture emesse nell&#39;anno precedente.</value>
        /*
        <example>12</example>
        */
        [DataMember(Name = "numeroFattureAnnoPrecedente", EmitDefaultValue = false)]
        public int NumeroFattureAnnoPrecedente { get; set; }

        /// <summary>
        /// Importo insoluto nell&#39;anno precedente.
        /// </summary>
        /// <value>Importo insoluto nell&#39;anno precedente.</value>
        /*
        <example>1250.0</example>
        */
        [DataMember(Name = "insolutiAnnoPrecedente", EmitDefaultValue = false)]
        public double InsolutiAnnoPrecedente { get; set; }

        /// <summary>
        /// Numero di insoluti verificatisi nell&#39;anno precedente.
        /// </summary>
        /// <value>Numero di insoluti verificatisi nell&#39;anno precedente.</value>
        /*
        <example>2</example>
        */
        [DataMember(Name = "numeroInsolutiAnnoPrecedente", EmitDefaultValue = false)]
        public int NumeroInsolutiAnnoPrecedente { get; set; }

        /// <summary>
        /// Data dell&#39;ultima fattura.
        /// </summary>
        /// <value>Data dell&#39;ultima fattura.</value>
        [DataMember(Name = "dataUltimaFattura", EmitDefaultValue = true)]
        public DateTime? DataUltimaFattura { get; set; }

        /// <summary>
        /// Data dell&#39;ultimo pagamento.
        /// </summary>
        /// <value>Data dell&#39;ultimo pagamento.</value>
        [DataMember(Name = "dataUltimoPagamento", EmitDefaultValue = true)]
        public DateTime? DataUltimoPagamento { get; set; }

        /// <summary>
        /// Data dell&#39;ultimo insoluto.
        /// </summary>
        /// <value>Data dell&#39;ultimo insoluto.</value>
        [DataMember(Name = "dataUltimoInsoluto", EmitDefaultValue = true)]
        public DateTime? DataUltimoInsoluto { get; set; }

        /// <summary>
        /// Numero di conto, in formato grezzo (es. \&quot;  100   20\&quot;). Sono ammessi fino a cinque livelli.
        /// </summary>
        /// <value>Numero di conto, in formato grezzo (es. \&quot;  100   20\&quot;). Sono ammessi fino a cinque livelli.</value>
        /*
        <example>  100   20</example>
        */
        [DataMember(Name = "contoRaw", EmitDefaultValue = true)]
        public string ContoRaw { get; set; }

        /// <summary>
        /// Numero di conto, in formato leggibile (es. \&quot;100.20\&quot;).
        /// </summary>
        /// <value>Numero di conto, in formato leggibile (es. \&quot;100.20\&quot;).</value>
        [DataMember(Name = "conto", EmitDefaultValue = true)]
        public string Conto { get; private set; }

        /// <summary>
        /// Returns false as Conto should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializeConto()
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
            sb.Append("class SchedaCliente {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  AnagraficaId: ").Append(AnagraficaId).Append("\n");
            sb.Append("  AgenteId: ").Append(AgenteId).Append("\n");
            sb.Append("  Agente: ").Append(Agente).Append("\n");
            sb.Append("  PagamentoId: ").Append(PagamentoId).Append("\n");
            sb.Append("  Pagamento: ").Append(Pagamento).Append("\n");
            sb.Append("  ListinoId: ").Append(ListinoId).Append("\n");
            sb.Append("  Listino: ").Append(Listino).Append("\n");
            sb.Append("  CategoriaAnagraficaId: ").Append(CategoriaAnagraficaId).Append("\n");
            sb.Append("  CategoriaAnagrafica: ").Append(CategoriaAnagrafica).Append("\n");
            sb.Append("  CausaleIvaId: ").Append(CausaleIvaId).Append("\n");
            sb.Append("  CausaleIva: ").Append(CausaleIva).Append("\n");
            sb.Append("  Fido: ").Append(Fido).Append("\n");
            sb.Append("  Sconto: ").Append(Sconto).Append("\n");
            sb.Append("  FatturatoAnnoCorrente: ").Append(FatturatoAnnoCorrente).Append("\n");
            sb.Append("  NumeroFattureAnnoCorrente: ").Append(NumeroFattureAnnoCorrente).Append("\n");
            sb.Append("  InsolutiAnnoCorrente: ").Append(InsolutiAnnoCorrente).Append("\n");
            sb.Append("  NumeroInsolutiAnnoCorrente: ").Append(NumeroInsolutiAnnoCorrente).Append("\n");
            sb.Append("  FatturatoAnnoPrecedente: ").Append(FatturatoAnnoPrecedente).Append("\n");
            sb.Append("  NumeroFattureAnnoPrecedente: ").Append(NumeroFattureAnnoPrecedente).Append("\n");
            sb.Append("  InsolutiAnnoPrecedente: ").Append(InsolutiAnnoPrecedente).Append("\n");
            sb.Append("  NumeroInsolutiAnnoPrecedente: ").Append(NumeroInsolutiAnnoPrecedente).Append("\n");
            sb.Append("  DataUltimaFattura: ").Append(DataUltimaFattura).Append("\n");
            sb.Append("  DataUltimoPagamento: ").Append(DataUltimoPagamento).Append("\n");
            sb.Append("  DataUltimoInsoluto: ").Append(DataUltimoInsoluto).Append("\n");
            sb.Append("  ContoRaw: ").Append(ContoRaw).Append("\n");
            sb.Append("  Conto: ").Append(Conto).Append("\n");
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
            // ContoRaw (string) maxLength
            if (this.ContoRaw != null && this.ContoRaw.Length > 25)
            {
                yield return new ValidationResult("Invalid value for ContoRaw, length must be less than 25.", new [] { "ContoRaw" });
            }

            // ContoRaw (string) minLength
            if (this.ContoRaw != null && this.ContoRaw.Length < 0)
            {
                yield return new ValidationResult("Invalid value for ContoRaw, length must be greater than 0.", new [] { "ContoRaw" });
            }

            yield break;
        }
    }

}

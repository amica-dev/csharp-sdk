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
    /// Preferenze
    /// </summary>
    [DataContract(Name = "Preferenze")]
    public partial class Preferenze : IValidatableObject
    {
        /// <summary>
        /// Defines TipoBilancio
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TipoBilancioEnum
        {
            /// <summary>
            /// Enum Analitico for value: Analitico
            /// </summary>
            [EnumMember(Value = "Analitico")]
            Analitico = 1,

            /// <summary>
            /// Enum Sintentico for value: Sintentico
            /// </summary>
            [EnumMember(Value = "Sintentico")]
            Sintentico = 2
        }


        /// <summary>
        /// Gets or Sets TipoBilancio
        /// </summary>
        [DataMember(Name = "tipoBilancio", EmitDefaultValue = false)]
        public TipoBilancioEnum? TipoBilancio { get; set; }
        /// <summary>
        /// Defines PeriodoIva
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PeriodoIvaEnum
        {
            /// <summary>
            /// Enum Mensile for value: Mensile
            /// </summary>
            [EnumMember(Value = "Mensile")]
            Mensile = 1,

            /// <summary>
            /// Enum Trimestrale for value: Trimestrale
            /// </summary>
            [EnumMember(Value = "Trimestrale")]
            Trimestrale = 2
        }


        /// <summary>
        /// Gets or Sets PeriodoIva
        /// </summary>
        [DataMember(Name = "periodoIva", EmitDefaultValue = false)]
        public PeriodoIvaEnum? PeriodoIva { get; set; }
        /// <summary>
        /// Defines LiquidazioneIva
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum LiquidazioneIvaEnum
        {
            /// <summary>
            /// Enum Competenza for value: Competenza
            /// </summary>
            [EnumMember(Value = "Competenza")]
            Competenza = 1,

            /// <summary>
            /// Enum Cassa for value: Cassa
            /// </summary>
            [EnumMember(Value = "Cassa")]
            Cassa = 2
        }


        /// <summary>
        /// Gets or Sets LiquidazioneIva
        /// </summary>
        [DataMember(Name = "liquidazioneIva", EmitDefaultValue = false)]
        public LiquidazioneIvaEnum? LiquidazioneIva { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Preferenze" /> class.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="concurrencyToken">concurrencyToken.</param>
        /// <param name="ragioneSociale1">ragioneSociale1.</param>
        /// <param name="ragioneSociale2">ragioneSociale2.</param>
        /// <param name="partitaIva">partitaIva.</param>
        /// <param name="registroImprese">registroImprese.</param>
        /// <param name="codiceFiscale">codiceFiscale.</param>
        /// <param name="codiceSia">codiceSia.</param>
        /// <param name="codiceRea">codiceRea.</param>
        /// <param name="personaGiuridica">personaGiuridica.</param>
        /// <param name="dataNascita">dataNascita.</param>
        /// <param name="localitaNascita">localitaNascita.</param>
        /// <param name="provinciaNascita">provinciaNascita.</param>
        /// <param name="sesso">sesso.</param>
        /// <param name="attivita">attivita.</param>
        /// <param name="ivaEsclusa">ivaEsclusa.</param>
        /// <param name="decimaliPrezzoUnitario">decimaliPrezzoUnitario.</param>
        /// <param name="causaleProduzioneComponentiId">causaleProduzioneComponentiId.</param>
        /// <param name="causaleProduzioneCompostiId">causaleProduzioneCompostiId.</param>
        /// <param name="causaleMagazzinoVenditaId">causaleMagazzinoVenditaId.</param>
        /// <param name="causaleMagazzinoAcquistoId">causaleMagazzinoAcquistoId.</param>
        /// <param name="ricercaArticolo">ricercaArticolo.</param>
        /// <param name="dataSpeciale1">dataSpeciale1.</param>
        /// <param name="dataSpeciale2">dataSpeciale2.</param>
        /// <param name="giorniExtraDataSpeciale1">giorniExtraDataSpeciale1.</param>
        /// <param name="giorniExtraDataSpeciale2">giorniExtraDataSpeciale2.</param>
        /// <param name="pagatoManuale">pagatoManuale.</param>
        /// <param name="causaleMagazzinoMerciId">causaleMagazzinoMerciId.</param>
        /// <param name="magazzinoMerciId">magazzinoMerciId.</param>
        /// <param name="listinoMerciId">listinoMerciId.</param>
        /// <param name="modalitaPagamentoMerciId">modalitaPagamentoMerciId.</param>
        /// <param name="documentiCausaleIvaId">documentiCausaleIvaId.</param>
        /// <param name="documentiMagazzinoId">documentiMagazzinoId.</param>
        /// <param name="documentiListinoId">documentiListinoId.</param>
        /// <param name="eMail">eMail.</param>
        /// <param name="sitoWeb">sitoWeb.</param>
        /// <param name="annoContabile">annoContabile.</param>
        /// <param name="unitDiMisura">unitDiMisura.</param>
        /// <param name="causalePagamentoModelloCU">causalePagamentoModelloCU.</param>
        /// <param name="impostazioniSdi">impostazioniSdi.</param>
        /// <param name="tipoBilancio">tipoBilancio.</param>
        /// <param name="periodoIva">periodoIva.</param>
        /// <param name="liquidazioneIva">liquidazioneIva.</param>
        /// <param name="modificaRegistrazioneContabileAllaEmissione">modificaRegistrazioneContabileAllaEmissione.</param>
        /// <param name="ottimizzaEmissioneOrdini">ottimizzaEmissioneOrdini.</param>
        /// <param name="aggiungiAnnoInCorsoAlNumeroDocumento">aggiungiAnnoInCorsoAlNumeroDocumento.</param>
        /// <param name="consentiCambioManualeStatoDdt">consentiCambioManualeStatoDdt.</param>
        /// <param name="emailBody">emailBody.</param>
        /// <param name="emailAutoSend">emailAutoSend.</param>
        /// <param name="autoProduzione">autoProduzione.</param>
        /// <param name="codiceDestinatario">codiceDestinatario.</param>
        /// <param name="pec">pec.</param>
        /// <param name="modelloVenditeNotaDebitoId">modelloVenditeNotaDebitoId.</param>
        /// <param name="modelloVenditeNotaCreditoId">modelloVenditeNotaCreditoId.</param>
        /// <param name="modelloVenditeRicevutaFiscaleId">modelloVenditeRicevutaFiscaleId.</param>
        /// <param name="modelloVenditeFatturaProfessionistaId">modelloVenditeFatturaProfessionistaId.</param>
        /// <param name="modelloVenditeFatturaDifferitaId">modelloVenditeFatturaDifferitaId.</param>
        /// <param name="modelloVenditeFatturaAccontoId">modelloVenditeFatturaAccontoId.</param>
        /// <param name="modelloAcquistiFatturaAccompagnatoriaId">modelloAcquistiFatturaAccompagnatoriaId.</param>
        /// <param name="modelloVenditeFatturaAccompagnaoriaId">modelloVenditeFatturaAccompagnaoriaId.</param>
        /// <param name="modelloAcquistiNotaDebitoId">modelloAcquistiNotaDebitoId.</param>
        /// <param name="modelloAcquistiNotaCreditoId">modelloAcquistiNotaCreditoId.</param>
        /// <param name="modelloAcquistiFatturaProfessionistiId">modelloAcquistiFatturaProfessionistiId.</param>
        /// <param name="modelloAcquistiFatturaDifferitaId">modelloAcquistiFatturaDifferitaId.</param>
        /// <param name="modelloAcquistiFatturaAccontoId">modelloAcquistiFatturaAccontoId.</param>
        /// <param name="sedeLegale">sedeLegale.</param>
        /// <param name="sedeOperativa">sedeOperativa.</param>
        /// <param name="sedeAltra">sedeAltra.</param>
        public Preferenze(int id = default(int), int concurrencyToken = default(int), string ragioneSociale1 = default(string), string ragioneSociale2 = default(string), string partitaIva = default(string), string registroImprese = default(string), string codiceFiscale = default(string), string codiceSia = default(string), string codiceRea = default(string), bool personaGiuridica = default(bool), DateTime? dataNascita = default(DateTime?), string localitaNascita = default(string), string provinciaNascita = default(string), string sesso = default(string), string attivita = default(string), bool ivaEsclusa = default(bool), int decimaliPrezzoUnitario = default(int), int? causaleProduzioneComponentiId = default(int?), int? causaleProduzioneCompostiId = default(int?), int? causaleMagazzinoVenditaId = default(int?), int? causaleMagazzinoAcquistoId = default(int?), RicercaArticolo ricercaArticolo = default(RicercaArticolo), string dataSpeciale1 = default(string), string dataSpeciale2 = default(string), int? giorniExtraDataSpeciale1 = default(int?), int? giorniExtraDataSpeciale2 = default(int?), bool pagatoManuale = default(bool), int? causaleMagazzinoMerciId = default(int?), int? magazzinoMerciId = default(int?), int? listinoMerciId = default(int?), int? modalitaPagamentoMerciId = default(int?), int? documentiCausaleIvaId = default(int?), int? documentiMagazzinoId = default(int?), int? documentiListinoId = default(int?), string eMail = default(string), string sitoWeb = default(string), int? annoContabile = default(int?), string unitDiMisura = default(string), string causalePagamentoModelloCU = default(string), ImpostazioniSdi impostazioniSdi = default(ImpostazioniSdi), TipoBilancioEnum? tipoBilancio = default(TipoBilancioEnum?), PeriodoIvaEnum? periodoIva = default(PeriodoIvaEnum?), LiquidazioneIvaEnum? liquidazioneIva = default(LiquidazioneIvaEnum?), bool modificaRegistrazioneContabileAllaEmissione = default(bool), bool ottimizzaEmissioneOrdini = default(bool), bool aggiungiAnnoInCorsoAlNumeroDocumento = default(bool), bool consentiCambioManualeStatoDdt = default(bool), string emailBody = default(string), bool emailAutoSend = default(bool), bool autoProduzione = default(bool), string codiceDestinatario = default(string), string pec = default(string), int? modelloVenditeNotaDebitoId = default(int?), int? modelloVenditeNotaCreditoId = default(int?), int? modelloVenditeRicevutaFiscaleId = default(int?), int? modelloVenditeFatturaProfessionistaId = default(int?), int? modelloVenditeFatturaDifferitaId = default(int?), int? modelloVenditeFatturaAccontoId = default(int?), int? modelloAcquistiFatturaAccompagnatoriaId = default(int?), int? modelloVenditeFatturaAccompagnaoriaId = default(int?), int? modelloAcquistiNotaDebitoId = default(int?), int? modelloAcquistiNotaCreditoId = default(int?), int? modelloAcquistiFatturaProfessionistiId = default(int?), int? modelloAcquistiFatturaDifferitaId = default(int?), int? modelloAcquistiFatturaAccontoId = default(int?), SedeLegale sedeLegale = default(SedeLegale), SedeOperativa sedeOperativa = default(SedeOperativa), SedeAltra sedeAltra = default(SedeAltra))
        {
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.RagioneSociale1 = ragioneSociale1;
            this.RagioneSociale2 = ragioneSociale2;
            this.PartitaIva = partitaIva;
            this.RegistroImprese = registroImprese;
            this.CodiceFiscale = codiceFiscale;
            this.CodiceSia = codiceSia;
            this.CodiceRea = codiceRea;
            this.PersonaGiuridica = personaGiuridica;
            this.DataNascita = dataNascita;
            this.LocalitaNascita = localitaNascita;
            this.ProvinciaNascita = provinciaNascita;
            this.Sesso = sesso;
            this.Attivita = attivita;
            this.IvaEsclusa = ivaEsclusa;
            this.DecimaliPrezzoUnitario = decimaliPrezzoUnitario;
            this.CausaleProduzioneComponentiId = causaleProduzioneComponentiId;
            this.CausaleProduzioneCompostiId = causaleProduzioneCompostiId;
            this.CausaleMagazzinoVenditaId = causaleMagazzinoVenditaId;
            this.CausaleMagazzinoAcquistoId = causaleMagazzinoAcquistoId;
            this.RicercaArticolo = ricercaArticolo;
            this.DataSpeciale1 = dataSpeciale1;
            this.DataSpeciale2 = dataSpeciale2;
            this.GiorniExtraDataSpeciale1 = giorniExtraDataSpeciale1;
            this.GiorniExtraDataSpeciale2 = giorniExtraDataSpeciale2;
            this.PagatoManuale = pagatoManuale;
            this.CausaleMagazzinoMerciId = causaleMagazzinoMerciId;
            this.MagazzinoMerciId = magazzinoMerciId;
            this.ListinoMerciId = listinoMerciId;
            this.ModalitaPagamentoMerciId = modalitaPagamentoMerciId;
            this.DocumentiCausaleIvaId = documentiCausaleIvaId;
            this.DocumentiMagazzinoId = documentiMagazzinoId;
            this.DocumentiListinoId = documentiListinoId;
            this.EMail = eMail;
            this.SitoWeb = sitoWeb;
            this.AnnoContabile = annoContabile;
            this.UnitDiMisura = unitDiMisura;
            this.CausalePagamentoModelloCU = causalePagamentoModelloCU;
            this.ImpostazioniSdi = impostazioniSdi;
            this.TipoBilancio = tipoBilancio;
            this.PeriodoIva = periodoIva;
            this.LiquidazioneIva = liquidazioneIva;
            this.ModificaRegistrazioneContabileAllaEmissione = modificaRegistrazioneContabileAllaEmissione;
            this.OttimizzaEmissioneOrdini = ottimizzaEmissioneOrdini;
            this.AggiungiAnnoInCorsoAlNumeroDocumento = aggiungiAnnoInCorsoAlNumeroDocumento;
            this.ConsentiCambioManualeStatoDdt = consentiCambioManualeStatoDdt;
            this.EmailBody = emailBody;
            this.EmailAutoSend = emailAutoSend;
            this.AutoProduzione = autoProduzione;
            this.CodiceDestinatario = codiceDestinatario;
            this.Pec = pec;
            this.ModelloVenditeNotaDebitoId = modelloVenditeNotaDebitoId;
            this.ModelloVenditeNotaCreditoId = modelloVenditeNotaCreditoId;
            this.ModelloVenditeRicevutaFiscaleId = modelloVenditeRicevutaFiscaleId;
            this.ModelloVenditeFatturaProfessionistaId = modelloVenditeFatturaProfessionistaId;
            this.ModelloVenditeFatturaDifferitaId = modelloVenditeFatturaDifferitaId;
            this.ModelloVenditeFatturaAccontoId = modelloVenditeFatturaAccontoId;
            this.ModelloAcquistiFatturaAccompagnatoriaId = modelloAcquistiFatturaAccompagnatoriaId;
            this.ModelloVenditeFatturaAccompagnaoriaId = modelloVenditeFatturaAccompagnaoriaId;
            this.ModelloAcquistiNotaDebitoId = modelloAcquistiNotaDebitoId;
            this.ModelloAcquistiNotaCreditoId = modelloAcquistiNotaCreditoId;
            this.ModelloAcquistiFatturaProfessionistiId = modelloAcquistiFatturaProfessionistiId;
            this.ModelloAcquistiFatturaDifferitaId = modelloAcquistiFatturaDifferitaId;
            this.ModelloAcquistiFatturaAccontoId = modelloAcquistiFatturaAccontoId;
            this.SedeLegale = sedeLegale;
            this.SedeOperativa = sedeOperativa;
            this.SedeAltra = sedeAltra;
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
        /// Gets or Sets RagioneSociale1
        /// </summary>
        [DataMember(Name = "ragioneSociale1", EmitDefaultValue = true)]
        public string RagioneSociale1 { get; set; }

        /// <summary>
        /// Gets or Sets RagioneSociale2
        /// </summary>
        [DataMember(Name = "ragioneSociale2", EmitDefaultValue = true)]
        public string RagioneSociale2 { get; set; }

        /// <summary>
        /// Gets or Sets PartitaIva
        /// </summary>
        [DataMember(Name = "partitaIva", EmitDefaultValue = true)]
        public string PartitaIva { get; set; }

        /// <summary>
        /// Gets or Sets RegistroImprese
        /// </summary>
        [DataMember(Name = "registroImprese", EmitDefaultValue = true)]
        public string RegistroImprese { get; set; }

        /// <summary>
        /// Gets or Sets CodiceFiscale
        /// </summary>
        [DataMember(Name = "codiceFiscale", EmitDefaultValue = true)]
        public string CodiceFiscale { get; set; }

        /// <summary>
        /// Gets or Sets CodiceSia
        /// </summary>
        [DataMember(Name = "codiceSia", EmitDefaultValue = true)]
        public string CodiceSia { get; set; }

        /// <summary>
        /// Gets or Sets CodiceRea
        /// </summary>
        [DataMember(Name = "codiceRea", EmitDefaultValue = true)]
        public string CodiceRea { get; set; }

        /// <summary>
        /// Gets or Sets PersonaGiuridica
        /// </summary>
        [DataMember(Name = "personaGiuridica", EmitDefaultValue = true)]
        public bool PersonaGiuridica { get; set; }

        /// <summary>
        /// Gets or Sets DataNascita
        /// </summary>
        [DataMember(Name = "dataNascita", EmitDefaultValue = true)]
        public DateTime? DataNascita { get; set; }

        /// <summary>
        /// Gets or Sets LocalitaNascita
        /// </summary>
        [DataMember(Name = "localitaNascita", EmitDefaultValue = true)]
        public string LocalitaNascita { get; set; }

        /// <summary>
        /// Gets or Sets ProvinciaNascita
        /// </summary>
        [DataMember(Name = "provinciaNascita", EmitDefaultValue = true)]
        public string ProvinciaNascita { get; set; }

        /// <summary>
        /// Gets or Sets Sesso
        /// </summary>
        [DataMember(Name = "sesso", EmitDefaultValue = true)]
        public string Sesso { get; set; }

        /// <summary>
        /// Gets or Sets Attivita
        /// </summary>
        [DataMember(Name = "attivita", EmitDefaultValue = true)]
        public string Attivita { get; set; }

        /// <summary>
        /// Gets or Sets IvaEsclusa
        /// </summary>
        [DataMember(Name = "ivaEsclusa", EmitDefaultValue = true)]
        public bool IvaEsclusa { get; set; }

        /// <summary>
        /// Gets or Sets DecimaliPrezzoUnitario
        /// </summary>
        [DataMember(Name = "decimaliPrezzoUnitario", EmitDefaultValue = false)]
        public int DecimaliPrezzoUnitario { get; set; }

        /// <summary>
        /// Gets or Sets CausaleProduzioneComponentiId
        /// </summary>
        [DataMember(Name = "causaleProduzioneComponentiId", EmitDefaultValue = true)]
        public int? CausaleProduzioneComponentiId { get; set; }

        /// <summary>
        /// Gets or Sets CausaleProduzioneCompostiId
        /// </summary>
        [DataMember(Name = "causaleProduzioneCompostiId", EmitDefaultValue = true)]
        public int? CausaleProduzioneCompostiId { get; set; }

        /// <summary>
        /// Gets or Sets CausaleMagazzinoVenditaId
        /// </summary>
        [DataMember(Name = "causaleMagazzinoVenditaId", EmitDefaultValue = true)]
        public int? CausaleMagazzinoVenditaId { get; set; }

        /// <summary>
        /// Gets or Sets CausaleMagazzinoAcquistoId
        /// </summary>
        [DataMember(Name = "causaleMagazzinoAcquistoId", EmitDefaultValue = true)]
        public int? CausaleMagazzinoAcquistoId { get; set; }

        /// <summary>
        /// Gets or Sets RicercaArticolo
        /// </summary>
        [DataMember(Name = "ricercaArticolo", EmitDefaultValue = false)]
        public RicercaArticolo RicercaArticolo { get; set; }

        /// <summary>
        /// Gets or Sets DataSpeciale1
        /// </summary>
        [DataMember(Name = "dataSpeciale1", EmitDefaultValue = true)]
        public string DataSpeciale1 { get; set; }

        /// <summary>
        /// Gets or Sets DataSpeciale2
        /// </summary>
        [DataMember(Name = "dataSpeciale2", EmitDefaultValue = true)]
        public string DataSpeciale2 { get; set; }

        /// <summary>
        /// Gets or Sets GiorniExtraDataSpeciale1
        /// </summary>
        [DataMember(Name = "giorniExtraDataSpeciale1", EmitDefaultValue = true)]
        public int? GiorniExtraDataSpeciale1 { get; set; }

        /// <summary>
        /// Gets or Sets GiorniExtraDataSpeciale2
        /// </summary>
        [DataMember(Name = "giorniExtraDataSpeciale2", EmitDefaultValue = true)]
        public int? GiorniExtraDataSpeciale2 { get; set; }

        /// <summary>
        /// Gets or Sets PagatoManuale
        /// </summary>
        [DataMember(Name = "pagatoManuale", EmitDefaultValue = true)]
        public bool PagatoManuale { get; set; }

        /// <summary>
        /// Gets or Sets CausaleMagazzinoMerciId
        /// </summary>
        [DataMember(Name = "causaleMagazzinoMerciId", EmitDefaultValue = true)]
        public int? CausaleMagazzinoMerciId { get; set; }

        /// <summary>
        /// Gets or Sets MagazzinoMerciId
        /// </summary>
        [DataMember(Name = "magazzinoMerciId", EmitDefaultValue = true)]
        public int? MagazzinoMerciId { get; set; }

        /// <summary>
        /// Gets or Sets ListinoMerciId
        /// </summary>
        [DataMember(Name = "listinoMerciId", EmitDefaultValue = true)]
        public int? ListinoMerciId { get; set; }

        /// <summary>
        /// Gets or Sets ModalitaPagamentoMerciId
        /// </summary>
        [DataMember(Name = "modalitaPagamentoMerciId", EmitDefaultValue = true)]
        public int? ModalitaPagamentoMerciId { get; set; }

        /// <summary>
        /// Gets or Sets DocumentiCausaleIvaId
        /// </summary>
        [DataMember(Name = "documentiCausaleIvaId", EmitDefaultValue = true)]
        public int? DocumentiCausaleIvaId { get; set; }

        /// <summary>
        /// Gets or Sets DocumentiMagazzinoId
        /// </summary>
        [DataMember(Name = "documentiMagazzinoId", EmitDefaultValue = true)]
        public int? DocumentiMagazzinoId { get; set; }

        /// <summary>
        /// Gets or Sets DocumentiListinoId
        /// </summary>
        [DataMember(Name = "documentiListinoId", EmitDefaultValue = true)]
        public int? DocumentiListinoId { get; set; }

        /// <summary>
        /// Gets or Sets EMail
        /// </summary>
        [DataMember(Name = "eMail", EmitDefaultValue = true)]
        public string EMail { get; set; }

        /// <summary>
        /// Gets or Sets SitoWeb
        /// </summary>
        [DataMember(Name = "sitoWeb", EmitDefaultValue = true)]
        public string SitoWeb { get; set; }

        /// <summary>
        /// Gets or Sets AnnoContabile
        /// </summary>
        [DataMember(Name = "annoContabile", EmitDefaultValue = true)]
        public int? AnnoContabile { get; set; }

        /// <summary>
        /// Gets or Sets UnitDiMisura
        /// </summary>
        [DataMember(Name = "unitàDiMisura", EmitDefaultValue = true)]
        public string UnitDiMisura { get; set; }

        /// <summary>
        /// Gets or Sets CausalePagamentoModelloCU
        /// </summary>
        [DataMember(Name = "causalePagamentoModelloCU", EmitDefaultValue = true)]
        public string CausalePagamentoModelloCU { get; set; }

        /// <summary>
        /// Gets or Sets ImpostazioniSdi
        /// </summary>
        [DataMember(Name = "impostazioniSdi", EmitDefaultValue = false)]
        public ImpostazioniSdi ImpostazioniSdi { get; set; }

        /// <summary>
        /// Gets or Sets ModificaRegistrazioneContabileAllaEmissione
        /// </summary>
        [DataMember(Name = "modificaRegistrazioneContabileAllaEmissione", EmitDefaultValue = true)]
        public bool ModificaRegistrazioneContabileAllaEmissione { get; set; }

        /// <summary>
        /// Gets or Sets OttimizzaEmissioneOrdini
        /// </summary>
        [DataMember(Name = "ottimizzaEmissioneOrdini", EmitDefaultValue = true)]
        public bool OttimizzaEmissioneOrdini { get; set; }

        /// <summary>
        /// Gets or Sets AggiungiAnnoInCorsoAlNumeroDocumento
        /// </summary>
        [DataMember(Name = "aggiungiAnnoInCorsoAlNumeroDocumento", EmitDefaultValue = true)]
        public bool AggiungiAnnoInCorsoAlNumeroDocumento { get; set; }

        /// <summary>
        /// Gets or Sets ConsentiCambioManualeStatoDdt
        /// </summary>
        [DataMember(Name = "consentiCambioManualeStatoDdt", EmitDefaultValue = true)]
        public bool ConsentiCambioManualeStatoDdt { get; set; }

        /// <summary>
        /// Gets or Sets EmailBody
        /// </summary>
        [DataMember(Name = "emailBody", EmitDefaultValue = true)]
        public string EmailBody { get; set; }

        /// <summary>
        /// Gets or Sets EmailAutoSend
        /// </summary>
        [DataMember(Name = "emailAutoSend", EmitDefaultValue = true)]
        public bool EmailAutoSend { get; set; }

        /// <summary>
        /// Gets or Sets AutoProduzione
        /// </summary>
        [DataMember(Name = "autoProduzione", EmitDefaultValue = true)]
        public bool AutoProduzione { get; set; }

        /// <summary>
        /// Gets or Sets CodiceDestinatario
        /// </summary>
        [DataMember(Name = "codiceDestinatario", EmitDefaultValue = true)]
        public string CodiceDestinatario { get; set; }

        /// <summary>
        /// Gets or Sets Pec
        /// </summary>
        [DataMember(Name = "pec", EmitDefaultValue = true)]
        public string Pec { get; set; }

        /// <summary>
        /// Gets or Sets ModelloVenditeNotaDebitoId
        /// </summary>
        [DataMember(Name = "modelloVenditeNotaDebitoId", EmitDefaultValue = true)]
        public int? ModelloVenditeNotaDebitoId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloVenditeNotaCreditoId
        /// </summary>
        [DataMember(Name = "modelloVenditeNotaCreditoId", EmitDefaultValue = true)]
        public int? ModelloVenditeNotaCreditoId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloVenditeRicevutaFiscaleId
        /// </summary>
        [DataMember(Name = "modelloVenditeRicevutaFiscaleId", EmitDefaultValue = true)]
        public int? ModelloVenditeRicevutaFiscaleId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloVenditeFatturaProfessionistaId
        /// </summary>
        [DataMember(Name = "modelloVenditeFatturaProfessionistaId", EmitDefaultValue = true)]
        public int? ModelloVenditeFatturaProfessionistaId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloVenditeFatturaDifferitaId
        /// </summary>
        [DataMember(Name = "modelloVenditeFatturaDifferitaId", EmitDefaultValue = true)]
        public int? ModelloVenditeFatturaDifferitaId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloVenditeFatturaAccontoId
        /// </summary>
        [DataMember(Name = "modelloVenditeFatturaAccontoId", EmitDefaultValue = true)]
        public int? ModelloVenditeFatturaAccontoId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloAcquistiFatturaAccompagnatoriaId
        /// </summary>
        [DataMember(Name = "modelloAcquistiFatturaAccompagnatoriaId", EmitDefaultValue = true)]
        public int? ModelloAcquistiFatturaAccompagnatoriaId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloVenditeFatturaAccompagnaoriaId
        /// </summary>
        [DataMember(Name = "modelloVenditeFatturaAccompagnaoriaId", EmitDefaultValue = true)]
        public int? ModelloVenditeFatturaAccompagnaoriaId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloAcquistiNotaDebitoId
        /// </summary>
        [DataMember(Name = "modelloAcquistiNotaDebitoId", EmitDefaultValue = true)]
        public int? ModelloAcquistiNotaDebitoId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloAcquistiNotaCreditoId
        /// </summary>
        [DataMember(Name = "modelloAcquistiNotaCreditoId", EmitDefaultValue = true)]
        public int? ModelloAcquistiNotaCreditoId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloAcquistiFatturaProfessionistiId
        /// </summary>
        [DataMember(Name = "modelloAcquistiFatturaProfessionistiId", EmitDefaultValue = true)]
        public int? ModelloAcquistiFatturaProfessionistiId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloAcquistiFatturaDifferitaId
        /// </summary>
        [DataMember(Name = "modelloAcquistiFatturaDifferitaId", EmitDefaultValue = true)]
        public int? ModelloAcquistiFatturaDifferitaId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloAcquistiFatturaAccontoId
        /// </summary>
        [DataMember(Name = "modelloAcquistiFatturaAccontoId", EmitDefaultValue = true)]
        public int? ModelloAcquistiFatturaAccontoId { get; set; }

        /// <summary>
        /// Gets or Sets SedeLegale
        /// </summary>
        [DataMember(Name = "sedeLegale", EmitDefaultValue = false)]
        public SedeLegale SedeLegale { get; set; }

        /// <summary>
        /// Gets or Sets SedeOperativa
        /// </summary>
        [DataMember(Name = "sedeOperativa", EmitDefaultValue = false)]
        public SedeOperativa SedeOperativa { get; set; }

        /// <summary>
        /// Gets or Sets SedeAltra
        /// </summary>
        [DataMember(Name = "sedeAltra", EmitDefaultValue = false)]
        public SedeAltra SedeAltra { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Preferenze {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  RagioneSociale1: ").Append(RagioneSociale1).Append("\n");
            sb.Append("  RagioneSociale2: ").Append(RagioneSociale2).Append("\n");
            sb.Append("  PartitaIva: ").Append(PartitaIva).Append("\n");
            sb.Append("  RegistroImprese: ").Append(RegistroImprese).Append("\n");
            sb.Append("  CodiceFiscale: ").Append(CodiceFiscale).Append("\n");
            sb.Append("  CodiceSia: ").Append(CodiceSia).Append("\n");
            sb.Append("  CodiceRea: ").Append(CodiceRea).Append("\n");
            sb.Append("  PersonaGiuridica: ").Append(PersonaGiuridica).Append("\n");
            sb.Append("  DataNascita: ").Append(DataNascita).Append("\n");
            sb.Append("  LocalitaNascita: ").Append(LocalitaNascita).Append("\n");
            sb.Append("  ProvinciaNascita: ").Append(ProvinciaNascita).Append("\n");
            sb.Append("  Sesso: ").Append(Sesso).Append("\n");
            sb.Append("  Attivita: ").Append(Attivita).Append("\n");
            sb.Append("  IvaEsclusa: ").Append(IvaEsclusa).Append("\n");
            sb.Append("  DecimaliPrezzoUnitario: ").Append(DecimaliPrezzoUnitario).Append("\n");
            sb.Append("  CausaleProduzioneComponentiId: ").Append(CausaleProduzioneComponentiId).Append("\n");
            sb.Append("  CausaleProduzioneCompostiId: ").Append(CausaleProduzioneCompostiId).Append("\n");
            sb.Append("  CausaleMagazzinoVenditaId: ").Append(CausaleMagazzinoVenditaId).Append("\n");
            sb.Append("  CausaleMagazzinoAcquistoId: ").Append(CausaleMagazzinoAcquistoId).Append("\n");
            sb.Append("  RicercaArticolo: ").Append(RicercaArticolo).Append("\n");
            sb.Append("  DataSpeciale1: ").Append(DataSpeciale1).Append("\n");
            sb.Append("  DataSpeciale2: ").Append(DataSpeciale2).Append("\n");
            sb.Append("  GiorniExtraDataSpeciale1: ").Append(GiorniExtraDataSpeciale1).Append("\n");
            sb.Append("  GiorniExtraDataSpeciale2: ").Append(GiorniExtraDataSpeciale2).Append("\n");
            sb.Append("  PagatoManuale: ").Append(PagatoManuale).Append("\n");
            sb.Append("  CausaleMagazzinoMerciId: ").Append(CausaleMagazzinoMerciId).Append("\n");
            sb.Append("  MagazzinoMerciId: ").Append(MagazzinoMerciId).Append("\n");
            sb.Append("  ListinoMerciId: ").Append(ListinoMerciId).Append("\n");
            sb.Append("  ModalitaPagamentoMerciId: ").Append(ModalitaPagamentoMerciId).Append("\n");
            sb.Append("  DocumentiCausaleIvaId: ").Append(DocumentiCausaleIvaId).Append("\n");
            sb.Append("  DocumentiMagazzinoId: ").Append(DocumentiMagazzinoId).Append("\n");
            sb.Append("  DocumentiListinoId: ").Append(DocumentiListinoId).Append("\n");
            sb.Append("  EMail: ").Append(EMail).Append("\n");
            sb.Append("  SitoWeb: ").Append(SitoWeb).Append("\n");
            sb.Append("  AnnoContabile: ").Append(AnnoContabile).Append("\n");
            sb.Append("  UnitDiMisura: ").Append(UnitDiMisura).Append("\n");
            sb.Append("  CausalePagamentoModelloCU: ").Append(CausalePagamentoModelloCU).Append("\n");
            sb.Append("  ImpostazioniSdi: ").Append(ImpostazioniSdi).Append("\n");
            sb.Append("  TipoBilancio: ").Append(TipoBilancio).Append("\n");
            sb.Append("  PeriodoIva: ").Append(PeriodoIva).Append("\n");
            sb.Append("  LiquidazioneIva: ").Append(LiquidazioneIva).Append("\n");
            sb.Append("  ModificaRegistrazioneContabileAllaEmissione: ").Append(ModificaRegistrazioneContabileAllaEmissione).Append("\n");
            sb.Append("  OttimizzaEmissioneOrdini: ").Append(OttimizzaEmissioneOrdini).Append("\n");
            sb.Append("  AggiungiAnnoInCorsoAlNumeroDocumento: ").Append(AggiungiAnnoInCorsoAlNumeroDocumento).Append("\n");
            sb.Append("  ConsentiCambioManualeStatoDdt: ").Append(ConsentiCambioManualeStatoDdt).Append("\n");
            sb.Append("  EmailBody: ").Append(EmailBody).Append("\n");
            sb.Append("  EmailAutoSend: ").Append(EmailAutoSend).Append("\n");
            sb.Append("  AutoProduzione: ").Append(AutoProduzione).Append("\n");
            sb.Append("  CodiceDestinatario: ").Append(CodiceDestinatario).Append("\n");
            sb.Append("  Pec: ").Append(Pec).Append("\n");
            sb.Append("  ModelloVenditeNotaDebitoId: ").Append(ModelloVenditeNotaDebitoId).Append("\n");
            sb.Append("  ModelloVenditeNotaCreditoId: ").Append(ModelloVenditeNotaCreditoId).Append("\n");
            sb.Append("  ModelloVenditeRicevutaFiscaleId: ").Append(ModelloVenditeRicevutaFiscaleId).Append("\n");
            sb.Append("  ModelloVenditeFatturaProfessionistaId: ").Append(ModelloVenditeFatturaProfessionistaId).Append("\n");
            sb.Append("  ModelloVenditeFatturaDifferitaId: ").Append(ModelloVenditeFatturaDifferitaId).Append("\n");
            sb.Append("  ModelloVenditeFatturaAccontoId: ").Append(ModelloVenditeFatturaAccontoId).Append("\n");
            sb.Append("  ModelloAcquistiFatturaAccompagnatoriaId: ").Append(ModelloAcquistiFatturaAccompagnatoriaId).Append("\n");
            sb.Append("  ModelloVenditeFatturaAccompagnaoriaId: ").Append(ModelloVenditeFatturaAccompagnaoriaId).Append("\n");
            sb.Append("  ModelloAcquistiNotaDebitoId: ").Append(ModelloAcquistiNotaDebitoId).Append("\n");
            sb.Append("  ModelloAcquistiNotaCreditoId: ").Append(ModelloAcquistiNotaCreditoId).Append("\n");
            sb.Append("  ModelloAcquistiFatturaProfessionistiId: ").Append(ModelloAcquistiFatturaProfessionistiId).Append("\n");
            sb.Append("  ModelloAcquistiFatturaDifferitaId: ").Append(ModelloAcquistiFatturaDifferitaId).Append("\n");
            sb.Append("  ModelloAcquistiFatturaAccontoId: ").Append(ModelloAcquistiFatturaAccontoId).Append("\n");
            sb.Append("  SedeLegale: ").Append(SedeLegale).Append("\n");
            sb.Append("  SedeOperativa: ").Append(SedeOperativa).Append("\n");
            sb.Append("  SedeAltra: ").Append(SedeAltra).Append("\n");
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

            // RegistroImprese (string) maxLength
            if (this.RegistroImprese != null && this.RegistroImprese.Length > 15)
            {
                yield return new ValidationResult("Invalid value for RegistroImprese, length must be less than 15.", new [] { "RegistroImprese" });
            }

            // RegistroImprese (string) minLength
            if (this.RegistroImprese != null && this.RegistroImprese.Length < 0)
            {
                yield return new ValidationResult("Invalid value for RegistroImprese, length must be greater than 0.", new [] { "RegistroImprese" });
            }

            // CodiceSia (string) maxLength
            if (this.CodiceSia != null && this.CodiceSia.Length > 10)
            {
                yield return new ValidationResult("Invalid value for CodiceSia, length must be less than 10.", new [] { "CodiceSia" });
            }

            // CodiceSia (string) minLength
            if (this.CodiceSia != null && this.CodiceSia.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceSia, length must be greater than 0.", new [] { "CodiceSia" });
            }

            // CodiceRea (string) maxLength
            if (this.CodiceRea != null && this.CodiceRea.Length > 10)
            {
                yield return new ValidationResult("Invalid value for CodiceRea, length must be less than 10.", new [] { "CodiceRea" });
            }

            // CodiceRea (string) minLength
            if (this.CodiceRea != null && this.CodiceRea.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CodiceRea, length must be greater than 0.", new [] { "CodiceRea" });
            }

            // LocalitaNascita (string) maxLength
            if (this.LocalitaNascita != null && this.LocalitaNascita.Length > 40)
            {
                yield return new ValidationResult("Invalid value for LocalitaNascita, length must be less than 40.", new [] { "LocalitaNascita" });
            }

            // LocalitaNascita (string) minLength
            if (this.LocalitaNascita != null && this.LocalitaNascita.Length < 0)
            {
                yield return new ValidationResult("Invalid value for LocalitaNascita, length must be greater than 0.", new [] { "LocalitaNascita" });
            }

            // ProvinciaNascita (string) maxLength
            if (this.ProvinciaNascita != null && this.ProvinciaNascita.Length > 3)
            {
                yield return new ValidationResult("Invalid value for ProvinciaNascita, length must be less than 3.", new [] { "ProvinciaNascita" });
            }

            // ProvinciaNascita (string) minLength
            if (this.ProvinciaNascita != null && this.ProvinciaNascita.Length < 0)
            {
                yield return new ValidationResult("Invalid value for ProvinciaNascita, length must be greater than 0.", new [] { "ProvinciaNascita" });
            }

            // Attivita (string) maxLength
            if (this.Attivita != null && this.Attivita.Length > 60)
            {
                yield return new ValidationResult("Invalid value for Attivita, length must be less than 60.", new [] { "Attivita" });
            }

            // Attivita (string) minLength
            if (this.Attivita != null && this.Attivita.Length < 0)
            {
                yield return new ValidationResult("Invalid value for Attivita, length must be greater than 0.", new [] { "Attivita" });
            }

            // DecimaliPrezzoUnitario (int) maximum
            if (this.DecimaliPrezzoUnitario > (int)5)
            {
                yield return new ValidationResult("Invalid value for DecimaliPrezzoUnitario, must be a value less than or equal to 5.", new [] { "DecimaliPrezzoUnitario" });
            }

            // DecimaliPrezzoUnitario (int) minimum
            if (this.DecimaliPrezzoUnitario < (int)1)
            {
                yield return new ValidationResult("Invalid value for DecimaliPrezzoUnitario, must be a value greater than or equal to 1.", new [] { "DecimaliPrezzoUnitario" });
            }

            // DataSpeciale1 (string) maxLength
            if (this.DataSpeciale1 != null && this.DataSpeciale1.Length > 5)
            {
                yield return new ValidationResult("Invalid value for DataSpeciale1, length must be less than 5.", new [] { "DataSpeciale1" });
            }

            // DataSpeciale1 (string) minLength
            if (this.DataSpeciale1 != null && this.DataSpeciale1.Length < 0)
            {
                yield return new ValidationResult("Invalid value for DataSpeciale1, length must be greater than 0.", new [] { "DataSpeciale1" });
            }

            // DataSpeciale2 (string) maxLength
            if (this.DataSpeciale2 != null && this.DataSpeciale2.Length > 5)
            {
                yield return new ValidationResult("Invalid value for DataSpeciale2, length must be less than 5.", new [] { "DataSpeciale2" });
            }

            // DataSpeciale2 (string) minLength
            if (this.DataSpeciale2 != null && this.DataSpeciale2.Length < 0)
            {
                yield return new ValidationResult("Invalid value for DataSpeciale2, length must be greater than 0.", new [] { "DataSpeciale2" });
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

            // UnitDiMisura (string) maxLength
            if (this.UnitDiMisura != null && this.UnitDiMisura.Length > 15)
            {
                yield return new ValidationResult("Invalid value for UnitDiMisura, length must be less than 15.", new [] { "UnitDiMisura" });
            }

            // UnitDiMisura (string) minLength
            if (this.UnitDiMisura != null && this.UnitDiMisura.Length < 0)
            {
                yield return new ValidationResult("Invalid value for UnitDiMisura, length must be greater than 0.", new [] { "UnitDiMisura" });
            }

            // EmailBody (string) maxLength
            if (this.EmailBody != null && this.EmailBody.Length > 1024)
            {
                yield return new ValidationResult("Invalid value for EmailBody, length must be less than 1024.", new [] { "EmailBody" });
            }

            // EmailBody (string) minLength
            if (this.EmailBody != null && this.EmailBody.Length < 0)
            {
                yield return new ValidationResult("Invalid value for EmailBody, length must be greater than 0.", new [] { "EmailBody" });
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

            yield break;
        }
    }

}

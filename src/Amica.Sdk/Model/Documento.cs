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
    /// Documento
    /// </summary>
    [DataContract(Name = "Documento")]
    public partial class Documento : IValidatableObject
    {
        /// <summary>
        /// Defines TipoDocumento
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TipoDocumentoEnum
        {
            /// <summary>
            /// Enum Ddt for value: Ddt
            /// </summary>
            [EnumMember(Value = "Ddt")]
            Ddt = 1,

            /// <summary>
            /// Enum FatturaAccompagnatoria for value: FatturaAccompagnatoria
            /// </summary>
            [EnumMember(Value = "FatturaAccompagnatoria")]
            FatturaAccompagnatoria = 2,

            /// <summary>
            /// Enum FatturaAcconto for value: FatturaAcconto
            /// </summary>
            [EnumMember(Value = "FatturaAcconto")]
            FatturaAcconto = 3,

            /// <summary>
            /// Enum FatturaDifferita for value: FatturaDifferita
            /// </summary>
            [EnumMember(Value = "FatturaDifferita")]
            FatturaDifferita = 4,

            /// <summary>
            /// Enum FatturaProfessionista for value: FatturaProfessionista
            /// </summary>
            [EnumMember(Value = "FatturaProfessionista")]
            FatturaProfessionista = 5,

            /// <summary>
            /// Enum RicevutaFiscale for value: RicevutaFiscale
            /// </summary>
            [EnumMember(Value = "RicevutaFiscale")]
            RicevutaFiscale = 6,

            /// <summary>
            /// Enum NotaCredito for value: NotaCredito
            /// </summary>
            [EnumMember(Value = "NotaCredito")]
            NotaCredito = 7,

            /// <summary>
            /// Enum NotaDebito for value: NotaDebito
            /// </summary>
            [EnumMember(Value = "NotaDebito")]
            NotaDebito = 8,

            /// <summary>
            /// Enum Scontrino for value: Scontrino
            /// </summary>
            [EnumMember(Value = "Scontrino")]
            Scontrino = 9,

            /// <summary>
            /// Enum OrdineCliente for value: OrdineCliente
            /// </summary>
            [EnumMember(Value = "OrdineCliente")]
            OrdineCliente = 10,

            /// <summary>
            /// Enum Preventivo for value: Preventivo
            /// </summary>
            [EnumMember(Value = "Preventivo")]
            Preventivo = 11,

            /// <summary>
            /// Enum DdtFornitore for value: DdtFornitore
            /// </summary>
            [EnumMember(Value = "DdtFornitore")]
            DdtFornitore = 12,

            /// <summary>
            /// Enum FatturaAccompagnatoriaFornitore for value: FatturaAccompagnatoriaFornitore
            /// </summary>
            [EnumMember(Value = "FatturaAccompagnatoriaFornitore")]
            FatturaAccompagnatoriaFornitore = 13,

            /// <summary>
            /// Enum FatturaDifferitaFornitore for value: FatturaDifferitaFornitore
            /// </summary>
            [EnumMember(Value = "FatturaDifferitaFornitore")]
            FatturaDifferitaFornitore = 14,

            /// <summary>
            /// Enum FatturaProfessionistaFornitore for value: FatturaProfessionistaFornitore
            /// </summary>
            [EnumMember(Value = "FatturaProfessionistaFornitore")]
            FatturaProfessionistaFornitore = 15,

            /// <summary>
            /// Enum NotaCreditoFornitore for value: NotaCreditoFornitore
            /// </summary>
            [EnumMember(Value = "NotaCreditoFornitore")]
            NotaCreditoFornitore = 16,

            /// <summary>
            /// Enum NotaDebitoFornitore for value: NotaDebitoFornitore
            /// </summary>
            [EnumMember(Value = "NotaDebitoFornitore")]
            NotaDebitoFornitore = 17,

            /// <summary>
            /// Enum PreventivoFornitore for value: PreventivoFornitore
            /// </summary>
            [EnumMember(Value = "PreventivoFornitore")]
            PreventivoFornitore = 18,

            /// <summary>
            /// Enum OrdineFornitore for value: OrdineFornitore
            /// </summary>
            [EnumMember(Value = "OrdineFornitore")]
            OrdineFornitore = 19,

            /// <summary>
            /// Enum ResoCliente for value: ResoCliente
            /// </summary>
            [EnumMember(Value = "ResoCliente")]
            ResoCliente = 20,

            /// <summary>
            /// Enum FatturaAccontoFornitore for value: FatturaAccontoFornitore
            /// </summary>
            [EnumMember(Value = "FatturaAccontoFornitore")]
            FatturaAccontoFornitore = 21,

            /// <summary>
            /// Enum FatturaProForma for value: FatturaProForma
            /// </summary>
            [EnumMember(Value = "FatturaProForma")]
            FatturaProForma = 22,

            /// <summary>
            /// Enum IntegrazioneFatturaReverseChargeInterno for value: IntegrazioneFatturaReverseChargeInterno
            /// </summary>
            [EnumMember(Value = "IntegrazioneFatturaReverseChargeInterno")]
            IntegrazioneFatturaReverseChargeInterno = 23,

            /// <summary>
            /// Enum IntegrazioneAcquistoServiziEstero for value: IntegrazioneAcquistoServiziEstero
            /// </summary>
            [EnumMember(Value = "IntegrazioneAcquistoServiziEstero")]
            IntegrazioneAcquistoServiziEstero = 24,

            /// <summary>
            /// Enum IntegrazioneAcquistoBeniIntracomunitari for value: IntegrazioneAcquistoBeniIntracomunitari
            /// </summary>
            [EnumMember(Value = "IntegrazioneAcquistoBeniIntracomunitari")]
            IntegrazioneAcquistoBeniIntracomunitari = 25,

            /// <summary>
            /// Enum IntegrazioneAcquistoBeniArt17 for value: IntegrazioneAcquistoBeniArt17
            /// </summary>
            [EnumMember(Value = "IntegrazioneAcquistoBeniArt17")]
            IntegrazioneAcquistoBeniArt17 = 26,

            /// <summary>
            /// Enum AutofatturaRegolarizzazioneFatture for value: AutofatturaRegolarizzazioneFatture
            /// </summary>
            [EnumMember(Value = "AutofatturaRegolarizzazioneFatture")]
            AutofatturaRegolarizzazioneFatture = 27,

            /// <summary>
            /// Enum AutofatturaSplafonamento for value: AutofatturaSplafonamento
            /// </summary>
            [EnumMember(Value = "AutofatturaSplafonamento")]
            AutofatturaSplafonamento = 28,

            /// <summary>
            /// Enum EstrazioneBeniDepositoIva for value: EstrazioneBeniDepositoIva
            /// </summary>
            [EnumMember(Value = "EstrazioneBeniDepositoIva")]
            EstrazioneBeniDepositoIva = 29,

            /// <summary>
            /// Enum EstrazioneBeniDepositoIvaConVersamentoIva for value: EstrazioneBeniDepositoIvaConVersamentoIva
            /// </summary>
            [EnumMember(Value = "EstrazioneBeniDepositoIvaConVersamentoIva")]
            EstrazioneBeniDepositoIvaConVersamentoIva = 30,

            /// <summary>
            /// Enum FatturaDifferitaArt21Comma4LetteraA for value: FatturaDifferitaArt21Comma4LetteraA
            /// </summary>
            [EnumMember(Value = "FatturaDifferitaArt21Comma4LetteraA")]
            FatturaDifferitaArt21Comma4LetteraA = 31,

            /// <summary>
            /// Enum FatturaDifferitaArt21Comma4LetteraB for value: FatturaDifferitaArt21Comma4LetteraB
            /// </summary>
            [EnumMember(Value = "FatturaDifferitaArt21Comma4LetteraB")]
            FatturaDifferitaArt21Comma4LetteraB = 32,

            /// <summary>
            /// Enum CessioniBeniAmmortizzabiliEPassaggiInterni for value: CessioniBeniAmmortizzabiliEPassaggiInterni
            /// </summary>
            [EnumMember(Value = "CessioniBeniAmmortizzabiliEPassaggiInterni")]
            CessioniBeniAmmortizzabiliEPassaggiInterni = 33,

            /// <summary>
            /// Enum FatturaAutoconsumoOCessioniGratuite for value: FatturaAutoconsumoOCessioniGratuite
            /// </summary>
            [EnumMember(Value = "FatturaAutoconsumoOCessioniGratuite")]
            FatturaAutoconsumoOCessioniGratuite = 34,

            /// <summary>
            /// Enum IntegrazioneFatturaReverseChargeInternoFornitore for value: IntegrazioneFatturaReverseChargeInternoFornitore
            /// </summary>
            [EnumMember(Value = "IntegrazioneFatturaReverseChargeInternoFornitore")]
            IntegrazioneFatturaReverseChargeInternoFornitore = 35,

            /// <summary>
            /// Enum IntegrazioneAcquistoServiziEsteroFornitore for value: IntegrazioneAcquistoServiziEsteroFornitore
            /// </summary>
            [EnumMember(Value = "IntegrazioneAcquistoServiziEsteroFornitore")]
            IntegrazioneAcquistoServiziEsteroFornitore = 36,

            /// <summary>
            /// Enum IntegrazioneAcquistoBeniIntracomunitariFornitore for value: IntegrazioneAcquistoBeniIntracomunitariFornitore
            /// </summary>
            [EnumMember(Value = "IntegrazioneAcquistoBeniIntracomunitariFornitore")]
            IntegrazioneAcquistoBeniIntracomunitariFornitore = 37,

            /// <summary>
            /// Enum IntegrazioneAcquistoBeniArt17Fornitore for value: IntegrazioneAcquistoBeniArt17Fornitore
            /// </summary>
            [EnumMember(Value = "IntegrazioneAcquistoBeniArt17Fornitore")]
            IntegrazioneAcquistoBeniArt17Fornitore = 38,

            /// <summary>
            /// Enum AutofatturaRegolarizzazioneFattureFornitore for value: AutofatturaRegolarizzazioneFattureFornitore
            /// </summary>
            [EnumMember(Value = "AutofatturaRegolarizzazioneFattureFornitore")]
            AutofatturaRegolarizzazioneFattureFornitore = 39,

            /// <summary>
            /// Enum AutofatturaSplafonamentoFornitore for value: AutofatturaSplafonamentoFornitore
            /// </summary>
            [EnumMember(Value = "AutofatturaSplafonamentoFornitore")]
            AutofatturaSplafonamentoFornitore = 40,

            /// <summary>
            /// Enum EstrazioneBeniDepositoIvaFornitore for value: EstrazioneBeniDepositoIvaFornitore
            /// </summary>
            [EnumMember(Value = "EstrazioneBeniDepositoIvaFornitore")]
            EstrazioneBeniDepositoIvaFornitore = 41,

            /// <summary>
            /// Enum EstrazioneBeniDepositoIvaConVersamentoIvaFornitore for value: EstrazioneBeniDepositoIvaConVersamentoIvaFornitore
            /// </summary>
            [EnumMember(Value = "EstrazioneBeniDepositoIvaConVersamentoIvaFornitore")]
            EstrazioneBeniDepositoIvaConVersamentoIvaFornitore = 42,

            /// <summary>
            /// Enum FatturaDifferitaArt21Comma4LetteraAFornitore for value: FatturaDifferitaArt21Comma4LetteraAFornitore
            /// </summary>
            [EnumMember(Value = "FatturaDifferitaArt21Comma4LetteraAFornitore")]
            FatturaDifferitaArt21Comma4LetteraAFornitore = 43,

            /// <summary>
            /// Enum FatturaDifferitaArt21Comma4LetteraBFornitore for value: FatturaDifferitaArt21Comma4LetteraBFornitore
            /// </summary>
            [EnumMember(Value = "FatturaDifferitaArt21Comma4LetteraBFornitore")]
            FatturaDifferitaArt21Comma4LetteraBFornitore = 44,

            /// <summary>
            /// Enum CessioniBeniAmmortizzabiliEPassaggiInterniFornitore for value: CessioniBeniAmmortizzabiliEPassaggiInterniFornitore
            /// </summary>
            [EnumMember(Value = "CessioniBeniAmmortizzabiliEPassaggiInterniFornitore")]
            CessioniBeniAmmortizzabiliEPassaggiInterniFornitore = 45,

            /// <summary>
            /// Enum FatturaAutoconsumoOCessioniGratuiteFornitore for value: FatturaAutoconsumoOCessioniGratuiteFornitore
            /// </summary>
            [EnumMember(Value = "FatturaAutoconsumoOCessioniGratuiteFornitore")]
            FatturaAutoconsumoOCessioniGratuiteFornitore = 46,

            /// <summary>
            /// Enum AcquistoSanMarinoConIva for value: AcquistoSanMarinoConIva
            /// </summary>
            [EnumMember(Value = "AcquistoSanMarinoConIva")]
            AcquistoSanMarinoConIva = 47,

            /// <summary>
            /// Enum ComunicazioneOmessaOIrregolareFatturazione for value: ComunicazioneOmessaOIrregolareFatturazione
            /// </summary>
            [EnumMember(Value = "ComunicazioneOmessaOIrregolareFatturazione")]
            ComunicazioneOmessaOIrregolareFatturazione = 48,

            /// <summary>
            /// Enum ComunicazioneOmessaOIrregolareFatturazioneFornitore for value: ComunicazioneOmessaOIrregolareFatturazioneFornitore
            /// </summary>
            [EnumMember(Value = "ComunicazioneOmessaOIrregolareFatturazioneFornitore")]
            ComunicazioneOmessaOIrregolareFatturazioneFornitore = 49
        }


        /// <summary>
        /// Gets or Sets TipoDocumento
        /// </summary>
        [DataMember(Name = "tipoDocumento", EmitDefaultValue = false)]
        public TipoDocumentoEnum? TipoDocumento { get; set; }
        /// <summary>
        /// Defines DocumentoFiscale
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum DocumentoFiscaleEnum
        {
            /// <summary>
            /// Enum Vendita for value: Vendita
            /// </summary>
            [EnumMember(Value = "Vendita")]
            Vendita = 1,

            /// <summary>
            /// Enum Acquisto for value: Acquisto
            /// </summary>
            [EnumMember(Value = "Acquisto")]
            Acquisto = 2,

            /// <summary>
            /// Enum NonFiscale for value: NonFiscale
            /// </summary>
            [EnumMember(Value = "NonFiscale")]
            NonFiscale = 3
        }


        /// <summary>
        /// Gets or Sets DocumentoFiscale
        /// </summary>
        [DataMember(Name = "documentoFiscale", EmitDefaultValue = false)]
        public DocumentoFiscaleEnum? DocumentoFiscale { get; set; }
        /// <summary>
        /// Defines ClasseDocumento
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ClasseDocumentoEnum
        {
            /// <summary>
            /// Enum Vendita for value: Vendita
            /// </summary>
            [EnumMember(Value = "Vendita")]
            Vendita = 1,

            /// <summary>
            /// Enum Acquisto for value: Acquisto
            /// </summary>
            [EnumMember(Value = "Acquisto")]
            Acquisto = 2
        }


        /// <summary>
        /// Gets or Sets ClasseDocumento
        /// </summary>
        [DataMember(Name = "classeDocumento", EmitDefaultValue = false)]
        public ClasseDocumentoEnum? ClasseDocumento { get; set; }
        /// <summary>
        /// Defines Stato
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatoEnum
        {
            /// <summary>
            /// Enum Bozza for value: Bozza
            /// </summary>
            [EnumMember(Value = "Bozza")]
            Bozza = 1,

            /// <summary>
            /// Enum Fatturato for value: Fatturato
            /// </summary>
            [EnumMember(Value = "Fatturato")]
            Fatturato = 2,

            /// <summary>
            /// Enum Emesso for value: Emesso
            /// </summary>
            [EnumMember(Value = "Emesso")]
            Emesso = 3,

            /// <summary>
            /// Enum FatturaAccontoScontataInFattura for value: FatturaAccontoScontataInFattura
            /// </summary>
            [EnumMember(Value = "FatturaAccontoScontataInFattura")]
            FatturaAccontoScontataInFattura = 4,

            /// <summary>
            /// Enum PreventivoChiuso for value: PreventivoChiuso
            /// </summary>
            [EnumMember(Value = "PreventivoChiuso")]
            PreventivoChiuso = 5,

            /// <summary>
            /// Enum OrdineEvaso for value: OrdineEvaso
            /// </summary>
            [EnumMember(Value = "OrdineEvaso")]
            OrdineEvaso = 6,

            /// <summary>
            /// Enum OrdineParzialmenteEvaso for value: OrdineParzialmenteEvaso
            /// </summary>
            [EnumMember(Value = "OrdineParzialmenteEvaso")]
            OrdineParzialmenteEvaso = 7,

            /// <summary>
            /// Enum OrdineAnnullato for value: OrdineAnnullato
            /// </summary>
            [EnumMember(Value = "OrdineAnnullato")]
            OrdineAnnullato = 8,

            /// <summary>
            /// Enum ResoAccreditato for value: ResoAccreditato
            /// </summary>
            [EnumMember(Value = "ResoAccreditato")]
            ResoAccreditato = 9,

            /// <summary>
            /// Enum DdtNonFatturare for value: DdtNonFatturare
            /// </summary>
            [EnumMember(Value = "DdtNonFatturare")]
            DdtNonFatturare = 10
        }


        /// <summary>
        /// Gets or Sets Stato
        /// </summary>
        [DataMember(Name = "stato", EmitDefaultValue = false)]
        public StatoEnum? Stato { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Documento" /> class.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="concurrencyToken">concurrencyToken.</param>
        /// <param name="anagraficaId">anagraficaId.</param>
        /// <param name="anagrafica">anagrafica.</param>
        /// <param name="causaleDocumentoId">causaleDocumentoId.</param>
        /// <param name="causaleDocumento">causaleDocumento.</param>
        /// <param name="agenteId">agenteId.</param>
        /// <param name="agente">agente.</param>
        /// <param name="pagamentoId">pagamentoId.</param>
        /// <param name="pagamento">pagamento.</param>
        /// <param name="vettoreId">vettoreId.</param>
        /// <param name="vettore">vettore.</param>
        /// <param name="cassaPrevidenzialeCausaleIvaId">cassaPrevidenzialeCausaleIvaId.</param>
        /// <param name="modelloId">modelloId.</param>
        /// <param name="cassaPrevidenzialeCausaleIva">cassaPrevidenzialeCausaleIva.</param>
        /// <param name="destinazioneId">destinazioneId.</param>
        /// <param name="destinazione">destinazione.</param>
        /// <param name="valutaId">valutaId.</param>
        /// <param name="valuta">valuta.</param>
        /// <param name="causaleMagazzinoId">causaleMagazzinoId.</param>
        /// <param name="causaleMagazzino">causaleMagazzino.</param>
        /// <param name="iva1CausaleIvaId">iva1CausaleIvaId.</param>
        /// <param name="iva1CausaleIva">iva1CausaleIva.</param>
        /// <param name="iva2CausaleIvaId">iva2CausaleIvaId.</param>
        /// <param name="iva2CausaleIva">iva2CausaleIva.</param>
        /// <param name="iva3CausaleIvaId">iva3CausaleIvaId.</param>
        /// <param name="iva3CausaleIva">iva3CausaleIva.</param>
        /// <param name="iva4CausaleIvaId">iva4CausaleIvaId.</param>
        /// <param name="iva4CausaleIva">iva4CausaleIva.</param>
        /// <param name="tipoDocumento">tipoDocumento.</param>
        /// <param name="documentoFiscale">documentoFiscale.</param>
        /// <param name="destinazioneDiversa">destinazioneDiversa.</param>
        /// <param name="ivaEsclusa">ivaEsclusa.</param>
        /// <param name="ritenutaIncludeCassaPrevidenziale">ritenutaIncludeCassaPrevidenziale.</param>
        /// <param name="classeDocumento">classeDocumento.</param>
        /// <param name="contatto">contatto.</param>
        /// <param name="numeroParteTesto">numeroParteTesto.</param>
        /// <param name="numeroParteNumerica">numeroParteNumerica.</param>
        /// <param name="data">data.</param>
        /// <param name="stato">stato.</param>
        /// <param name="dataScadenzaPreventivo">dataScadenzaPreventivo.</param>
        /// <param name="dataInizioScadenze">dataInizioScadenze.</param>
        /// <param name="scontoPercentuale">scontoPercentuale.</param>
        /// <param name="scontoIncondizionatoImporto">scontoIncondizionatoImporto.</param>
        /// <param name="abbuonoImporto">abbuonoImporto.</param>
        /// <param name="ritenutaPercentuale">ritenutaPercentuale.</param>
        /// <param name="enasarcoPercentuale">enasarcoPercentuale.</param>
        /// <param name="enasarcoImporto">enasarcoImporto.</param>
        /// <param name="quotaRitenutaSuImponibile">quotaRitenutaSuImponibile.</param>
        /// <param name="cambio">cambio.</param>
        /// <param name="cassaPrevidenzialeNome">cassaPrevidenzialeNome.</param>
        /// <param name="cassaPrevidenzialePercentuale">cassaPrevidenzialePercentuale.</param>
        /// <param name="note">note.</param>
        /// <param name="scontoPagamentoPercentuale">scontoPagamentoPercentuale.</param>
        /// <param name="iva1Imponibile">iva1Imponibile.</param>
        /// <param name="iva2Imponibile">iva2Imponibile.</param>
        /// <param name="iva3Imponibile">iva3Imponibile.</param>
        /// <param name="iva4Imponibile">iva4Imponibile.</param>
        /// <param name="iva1ImponibileLordo">iva1ImponibileLordo.</param>
        /// <param name="iva2ImponibileLordo">iva2ImponibileLordo.</param>
        /// <param name="iva3ImponibileLordo">iva3ImponibileLordo.</param>
        /// <param name="iva4ImponibileLordo">iva4ImponibileLordo.</param>
        /// <param name="iva1Imposta">iva1Imposta.</param>
        /// <param name="iva2Imposta">iva2Imposta.</param>
        /// <param name="iva3Imposta">iva3Imposta.</param>
        /// <param name="iva4Imposta">iva4Imposta.</param>
        /// <param name="totaleRitenuta">totaleRitenuta.</param>
        /// <param name="totaleCassaPrevidenziale">totaleCassaPrevidenziale.</param>
        /// <param name="totaleSpese">totaleSpese.</param>
        /// <param name="totaleSpeseEsenti">totaleSpeseEsenti.</param>
        /// <param name="totaleSpeseNonEsenti">totaleSpeseNonEsenti.</param>
        /// <param name="totaleMerce">totaleMerce.</param>
        /// <param name="totaleMerceScontata">totaleMerceScontata.</param>
        /// <param name="totaleImponibile">totaleImponibile.</param>
        /// <param name="totaleImposta">totaleImposta.</param>
        /// <param name="totaleEsente">totaleEsente.</param>
        /// <param name="totaleDocumento">totaleDocumento.</param>
        /// <param name="nettoAPagare">nettoAPagare.</param>
        /// <param name="totaleSplitPayment">totaleSplitPayment.</param>
        /// <param name="bollo">bollo.</param>
        /// <param name="xmlFileName">xmlFileName.</param>
        /// <param name="riferimentoAmministrazione">riferimentoAmministrazione.</param>
        /// <param name="banca">banca.</param>
        /// <param name="datiTrasporto">datiTrasporto.</param>
        /// <param name="datiSdi">datiSdi.</param>
        /// <param name="datiPA">datiPA.</param>
        /// <param name="righe">righe.</param>
        /// <param name="speseDocumento">speseDocumento.</param>
        /// <param name="scadenze">scadenze.</param>
        /// <param name="numero">numero.</param>
        /// <param name="mediaFile">mediaFile.</param>
        /// <param name="allegato">allegato.</param>
        /// <param name="bonusFiscaleCausale">bonusFiscaleCausale.</param>
        /// <param name="bonusFiscaleImporto">bonusFiscaleImporto.</param>
        /// <param name="bonusFiscalePercentuale">bonusFiscalePercentuale.</param>
        /// <param name="progressivoInvio">progressivoInvio.</param>
        public Documento(int id = default(int), int concurrencyToken = default(int), int anagraficaId = default(int), Anagrafica anagrafica = default(Anagrafica), int? causaleDocumentoId = default(int?), CausaleDocumento causaleDocumento = default(CausaleDocumento), int? agenteId = default(int?), Anagrafica agente = default(Anagrafica), int? pagamentoId = default(int?), Pagamento pagamento = default(Pagamento), int? vettoreId = default(int?), Anagrafica vettore = default(Anagrafica), int? cassaPrevidenzialeCausaleIvaId = default(int?), int? modelloId = default(int?), CausaleIva cassaPrevidenzialeCausaleIva = default(CausaleIva), int? destinazioneId = default(int?), Indirizzo destinazione = default(Indirizzo), int? valutaId = default(int?), Valuta valuta = default(Valuta), int? causaleMagazzinoId = default(int?), CausaleMagazzino causaleMagazzino = default(CausaleMagazzino), int? iva1CausaleIvaId = default(int?), CausaleIva iva1CausaleIva = default(CausaleIva), int? iva2CausaleIvaId = default(int?), CausaleIva iva2CausaleIva = default(CausaleIva), int? iva3CausaleIvaId = default(int?), CausaleIva iva3CausaleIva = default(CausaleIva), int? iva4CausaleIvaId = default(int?), CausaleIva iva4CausaleIva = default(CausaleIva), TipoDocumentoEnum? tipoDocumento = default(TipoDocumentoEnum?), DocumentoFiscaleEnum? documentoFiscale = default(DocumentoFiscaleEnum?), bool destinazioneDiversa = default(bool), bool ivaEsclusa = default(bool), bool ritenutaIncludeCassaPrevidenziale = default(bool), ClasseDocumentoEnum? classeDocumento = default(ClasseDocumentoEnum?), int? contatto = default(int?), string numeroParteTesto = default(string), int numeroParteNumerica = default(int), DateTime data = default(DateTime), StatoEnum? stato = default(StatoEnum?), DateTime? dataScadenzaPreventivo = default(DateTime?), DateTime? dataInizioScadenze = default(DateTime?), double scontoPercentuale = default(double), double scontoIncondizionatoImporto = default(double), double abbuonoImporto = default(double), double ritenutaPercentuale = default(double), double enasarcoPercentuale = default(double), double enasarcoImporto = default(double), double quotaRitenutaSuImponibile = default(double), double cambio = default(double), string cassaPrevidenzialeNome = default(string), double cassaPrevidenzialePercentuale = default(double), string note = default(string), double scontoPagamentoPercentuale = default(double), double iva1Imponibile = default(double), double iva2Imponibile = default(double), double iva3Imponibile = default(double), double iva4Imponibile = default(double), double iva1ImponibileLordo = default(double), double iva2ImponibileLordo = default(double), double iva3ImponibileLordo = default(double), double iva4ImponibileLordo = default(double), double iva1Imposta = default(double), double iva2Imposta = default(double), double iva3Imposta = default(double), double iva4Imposta = default(double), double totaleRitenuta = default(double), double totaleCassaPrevidenziale = default(double), double totaleSpese = default(double), double totaleSpeseEsenti = default(double), double totaleSpeseNonEsenti = default(double), double totaleMerce = default(double), double totaleMerceScontata = default(double), double totaleImponibile = default(double), double totaleImposta = default(double), double totaleEsente = default(double), double totaleDocumento = default(double), double nettoAPagare = default(double), double totaleSplitPayment = default(double), double bollo = default(double), string xmlFileName = default(string), string riferimentoAmministrazione = default(string), BancaDocumento banca = default(BancaDocumento), DatiTrasporto datiTrasporto = default(DatiTrasporto), DatiSdi datiSdi = default(DatiSdi), DatiPA datiPA = default(DatiPA), List<BeneServizio> righe = default(List<BeneServizio>), List<SpesaDocumento> speseDocumento = default(List<SpesaDocumento>), List<Scadenza> scadenze = default(List<Scadenza>), string numero = default(string), DocumentoMediaFile mediaFile = default(DocumentoMediaFile), string allegato = default(string), string bonusFiscaleCausale = default(string), double bonusFiscaleImporto = default(double), double bonusFiscalePercentuale = default(double), List<ProgressivoInvio> progressivoInvio = default(List<ProgressivoInvio>))
        {
            this.Id = id;
            this.ConcurrencyToken = concurrencyToken;
            this.AnagraficaId = anagraficaId;
            this.Anagrafica = anagrafica;
            this.CausaleDocumentoId = causaleDocumentoId;
            this.CausaleDocumento = causaleDocumento;
            this.AgenteId = agenteId;
            this.Agente = agente;
            this.PagamentoId = pagamentoId;
            this.Pagamento = pagamento;
            this.VettoreId = vettoreId;
            this.Vettore = vettore;
            this.CassaPrevidenzialeCausaleIvaId = cassaPrevidenzialeCausaleIvaId;
            this.ModelloId = modelloId;
            this.CassaPrevidenzialeCausaleIva = cassaPrevidenzialeCausaleIva;
            this.DestinazioneId = destinazioneId;
            this.Destinazione = destinazione;
            this.ValutaId = valutaId;
            this.Valuta = valuta;
            this.CausaleMagazzinoId = causaleMagazzinoId;
            this.CausaleMagazzino = causaleMagazzino;
            this.Iva1CausaleIvaId = iva1CausaleIvaId;
            this.Iva1CausaleIva = iva1CausaleIva;
            this.Iva2CausaleIvaId = iva2CausaleIvaId;
            this.Iva2CausaleIva = iva2CausaleIva;
            this.Iva3CausaleIvaId = iva3CausaleIvaId;
            this.Iva3CausaleIva = iva3CausaleIva;
            this.Iva4CausaleIvaId = iva4CausaleIvaId;
            this.Iva4CausaleIva = iva4CausaleIva;
            this.TipoDocumento = tipoDocumento;
            this.DocumentoFiscale = documentoFiscale;
            this.DestinazioneDiversa = destinazioneDiversa;
            this.IvaEsclusa = ivaEsclusa;
            this.RitenutaIncludeCassaPrevidenziale = ritenutaIncludeCassaPrevidenziale;
            this.ClasseDocumento = classeDocumento;
            this.Contatto = contatto;
            this.NumeroParteTesto = numeroParteTesto;
            this.NumeroParteNumerica = numeroParteNumerica;
            this.Data = data;
            this.Stato = stato;
            this.DataScadenzaPreventivo = dataScadenzaPreventivo;
            this.DataInizioScadenze = dataInizioScadenze;
            this.ScontoPercentuale = scontoPercentuale;
            this.ScontoIncondizionatoImporto = scontoIncondizionatoImporto;
            this.AbbuonoImporto = abbuonoImporto;
            this.RitenutaPercentuale = ritenutaPercentuale;
            this.EnasarcoPercentuale = enasarcoPercentuale;
            this.EnasarcoImporto = enasarcoImporto;
            this.QuotaRitenutaSuImponibile = quotaRitenutaSuImponibile;
            this.Cambio = cambio;
            this.CassaPrevidenzialeNome = cassaPrevidenzialeNome;
            this.CassaPrevidenzialePercentuale = cassaPrevidenzialePercentuale;
            this.Note = note;
            this.ScontoPagamentoPercentuale = scontoPagamentoPercentuale;
            this.Iva1Imponibile = iva1Imponibile;
            this.Iva2Imponibile = iva2Imponibile;
            this.Iva3Imponibile = iva3Imponibile;
            this.Iva4Imponibile = iva4Imponibile;
            this.Iva1ImponibileLordo = iva1ImponibileLordo;
            this.Iva2ImponibileLordo = iva2ImponibileLordo;
            this.Iva3ImponibileLordo = iva3ImponibileLordo;
            this.Iva4ImponibileLordo = iva4ImponibileLordo;
            this.Iva1Imposta = iva1Imposta;
            this.Iva2Imposta = iva2Imposta;
            this.Iva3Imposta = iva3Imposta;
            this.Iva4Imposta = iva4Imposta;
            this.TotaleRitenuta = totaleRitenuta;
            this.TotaleCassaPrevidenziale = totaleCassaPrevidenziale;
            this.TotaleSpese = totaleSpese;
            this.TotaleSpeseEsenti = totaleSpeseEsenti;
            this.TotaleSpeseNonEsenti = totaleSpeseNonEsenti;
            this.TotaleMerce = totaleMerce;
            this.TotaleMerceScontata = totaleMerceScontata;
            this.TotaleImponibile = totaleImponibile;
            this.TotaleImposta = totaleImposta;
            this.TotaleEsente = totaleEsente;
            this.TotaleDocumento = totaleDocumento;
            this.NettoAPagare = nettoAPagare;
            this.TotaleSplitPayment = totaleSplitPayment;
            this.Bollo = bollo;
            this.XmlFileName = xmlFileName;
            this.RiferimentoAmministrazione = riferimentoAmministrazione;
            this.Banca = banca;
            this.DatiTrasporto = datiTrasporto;
            this.DatiSdi = datiSdi;
            this.DatiPA = datiPA;
            this.Righe = righe;
            this.SpeseDocumento = speseDocumento;
            this.Scadenze = scadenze;
            this.Numero = numero;
            this.MediaFile = mediaFile;
            this.Allegato = allegato;
            this.BonusFiscaleCausale = bonusFiscaleCausale;
            this.BonusFiscaleImporto = bonusFiscaleImporto;
            this.BonusFiscalePercentuale = bonusFiscalePercentuale;
            this.ProgressivoInvio = progressivoInvio;
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
        /// Gets or Sets AnagraficaId
        /// </summary>
        [DataMember(Name = "anagraficaId", EmitDefaultValue = false)]
        public int AnagraficaId { get; set; }

        /// <summary>
        /// Gets or Sets Anagrafica
        /// </summary>
        [DataMember(Name = "anagrafica", EmitDefaultValue = false)]
        public Anagrafica Anagrafica { get; set; }

        /// <summary>
        /// Gets or Sets CausaleDocumentoId
        /// </summary>
        [DataMember(Name = "causaleDocumentoId", EmitDefaultValue = true)]
        public int? CausaleDocumentoId { get; set; }

        /// <summary>
        /// Gets or Sets CausaleDocumento
        /// </summary>
        [DataMember(Name = "causaleDocumento", EmitDefaultValue = false)]
        public CausaleDocumento CausaleDocumento { get; set; }

        /// <summary>
        /// Gets or Sets AgenteId
        /// </summary>
        [DataMember(Name = "agenteId", EmitDefaultValue = true)]
        public int? AgenteId { get; set; }

        /// <summary>
        /// Gets or Sets Agente
        /// </summary>
        [DataMember(Name = "agente", EmitDefaultValue = false)]
        public Anagrafica Agente { get; set; }

        /// <summary>
        /// Gets or Sets PagamentoId
        /// </summary>
        [DataMember(Name = "pagamentoId", EmitDefaultValue = true)]
        public int? PagamentoId { get; set; }

        /// <summary>
        /// Gets or Sets Pagamento
        /// </summary>
        [DataMember(Name = "pagamento", EmitDefaultValue = false)]
        public Pagamento Pagamento { get; set; }

        /// <summary>
        /// Gets or Sets VettoreId
        /// </summary>
        [DataMember(Name = "vettoreId", EmitDefaultValue = true)]
        public int? VettoreId { get; set; }

        /// <summary>
        /// Gets or Sets Vettore
        /// </summary>
        [DataMember(Name = "vettore", EmitDefaultValue = false)]
        public Anagrafica Vettore { get; set; }

        /// <summary>
        /// Gets or Sets CassaPrevidenzialeCausaleIvaId
        /// </summary>
        [DataMember(Name = "cassaPrevidenzialeCausaleIvaId", EmitDefaultValue = true)]
        public int? CassaPrevidenzialeCausaleIvaId { get; set; }

        /// <summary>
        /// Gets or Sets ModelloId
        /// </summary>
        [DataMember(Name = "modelloId", EmitDefaultValue = true)]
        public int? ModelloId { get; set; }

        /// <summary>
        /// Gets or Sets CassaPrevidenzialeCausaleIva
        /// </summary>
        [DataMember(Name = "cassaPrevidenzialeCausaleIva", EmitDefaultValue = false)]
        public CausaleIva CassaPrevidenzialeCausaleIva { get; set; }

        /// <summary>
        /// Gets or Sets DestinazioneId
        /// </summary>
        [DataMember(Name = "destinazioneId", EmitDefaultValue = true)]
        public int? DestinazioneId { get; set; }

        /// <summary>
        /// Gets or Sets Destinazione
        /// </summary>
        [DataMember(Name = "destinazione", EmitDefaultValue = false)]
        public Indirizzo Destinazione { get; set; }

        /// <summary>
        /// Gets or Sets ValutaId
        /// </summary>
        [DataMember(Name = "valutaId", EmitDefaultValue = true)]
        public int? ValutaId { get; set; }

        /// <summary>
        /// Gets or Sets Valuta
        /// </summary>
        [DataMember(Name = "valuta", EmitDefaultValue = false)]
        public Valuta Valuta { get; set; }

        /// <summary>
        /// Gets or Sets CausaleMagazzinoId
        /// </summary>
        [DataMember(Name = "causaleMagazzinoId", EmitDefaultValue = true)]
        public int? CausaleMagazzinoId { get; set; }

        /// <summary>
        /// Gets or Sets CausaleMagazzino
        /// </summary>
        [DataMember(Name = "causaleMagazzino", EmitDefaultValue = false)]
        public CausaleMagazzino CausaleMagazzino { get; set; }

        /// <summary>
        /// Gets or Sets Iva1CausaleIvaId
        /// </summary>
        [DataMember(Name = "iva1CausaleIvaId", EmitDefaultValue = true)]
        public int? Iva1CausaleIvaId { get; set; }

        /// <summary>
        /// Gets or Sets Iva1CausaleIva
        /// </summary>
        [DataMember(Name = "iva1CausaleIva", EmitDefaultValue = false)]
        public CausaleIva Iva1CausaleIva { get; set; }

        /// <summary>
        /// Gets or Sets Iva2CausaleIvaId
        /// </summary>
        [DataMember(Name = "iva2CausaleIvaId", EmitDefaultValue = true)]
        public int? Iva2CausaleIvaId { get; set; }

        /// <summary>
        /// Gets or Sets Iva2CausaleIva
        /// </summary>
        [DataMember(Name = "iva2CausaleIva", EmitDefaultValue = false)]
        public CausaleIva Iva2CausaleIva { get; set; }

        /// <summary>
        /// Gets or Sets Iva3CausaleIvaId
        /// </summary>
        [DataMember(Name = "iva3CausaleIvaId", EmitDefaultValue = true)]
        public int? Iva3CausaleIvaId { get; set; }

        /// <summary>
        /// Gets or Sets Iva3CausaleIva
        /// </summary>
        [DataMember(Name = "iva3CausaleIva", EmitDefaultValue = false)]
        public CausaleIva Iva3CausaleIva { get; set; }

        /// <summary>
        /// Gets or Sets Iva4CausaleIvaId
        /// </summary>
        [DataMember(Name = "iva4CausaleIvaId", EmitDefaultValue = true)]
        public int? Iva4CausaleIvaId { get; set; }

        /// <summary>
        /// Gets or Sets Iva4CausaleIva
        /// </summary>
        [DataMember(Name = "iva4CausaleIva", EmitDefaultValue = false)]
        public CausaleIva Iva4CausaleIva { get; set; }

        /// <summary>
        /// Gets or Sets DestinazioneDiversa
        /// </summary>
        [DataMember(Name = "destinazioneDiversa", EmitDefaultValue = true)]
        public bool DestinazioneDiversa { get; set; }

        /// <summary>
        /// Gets or Sets IvaEsclusa
        /// </summary>
        [DataMember(Name = "ivaEsclusa", EmitDefaultValue = true)]
        public bool IvaEsclusa { get; set; }

        /// <summary>
        /// Gets or Sets RitenutaIncludeCassaPrevidenziale
        /// </summary>
        [DataMember(Name = "ritenutaIncludeCassaPrevidenziale", EmitDefaultValue = true)]
        public bool RitenutaIncludeCassaPrevidenziale { get; set; }

        /// <summary>
        /// Gets or Sets Contatto
        /// </summary>
        [DataMember(Name = "contatto", EmitDefaultValue = true)]
        public int? Contatto { get; set; }

        /// <summary>
        /// Gets or Sets NumeroParteTesto
        /// </summary>
        [DataMember(Name = "numeroParteTesto", EmitDefaultValue = true)]
        public string NumeroParteTesto { get; set; }

        /// <summary>
        /// Gets or Sets NumeroParteNumerica
        /// </summary>
        [DataMember(Name = "numeroParteNumerica", EmitDefaultValue = false)]
        public int NumeroParteNumerica { get; set; }

        /// <summary>
        /// Gets or Sets Data
        /// </summary>
        [DataMember(Name = "data", EmitDefaultValue = false)]
        public DateTime Data { get; set; }

        /// <summary>
        /// Gets or Sets DataScadenzaPreventivo
        /// </summary>
        [DataMember(Name = "dataScadenzaPreventivo", EmitDefaultValue = true)]
        public DateTime? DataScadenzaPreventivo { get; set; }

        /// <summary>
        /// Gets or Sets DataInizioScadenze
        /// </summary>
        [DataMember(Name = "dataInizioScadenze", EmitDefaultValue = true)]
        public DateTime? DataInizioScadenze { get; set; }

        /// <summary>
        /// Gets or Sets ScontoPercentuale
        /// </summary>
        [DataMember(Name = "scontoPercentuale", EmitDefaultValue = false)]
        public double ScontoPercentuale { get; set; }

        /// <summary>
        /// Gets or Sets ScontoIncondizionatoImporto
        /// </summary>
        [DataMember(Name = "scontoIncondizionatoImporto", EmitDefaultValue = false)]
        public double ScontoIncondizionatoImporto { get; set; }

        /// <summary>
        /// Gets or Sets AbbuonoImporto
        /// </summary>
        [DataMember(Name = "abbuonoImporto", EmitDefaultValue = false)]
        public double AbbuonoImporto { get; set; }

        /// <summary>
        /// Gets or Sets RitenutaPercentuale
        /// </summary>
        [DataMember(Name = "ritenutaPercentuale", EmitDefaultValue = false)]
        public double RitenutaPercentuale { get; set; }

        /// <summary>
        /// Gets or Sets EnasarcoPercentuale
        /// </summary>
        [DataMember(Name = "enasarcoPercentuale", EmitDefaultValue = false)]
        public double EnasarcoPercentuale { get; set; }

        /// <summary>
        /// Gets or Sets EnasarcoImporto
        /// </summary>
        [DataMember(Name = "enasarcoImporto", EmitDefaultValue = false)]
        public double EnasarcoImporto { get; set; }

        /// <summary>
        /// Gets or Sets QuotaRitenutaSuImponibile
        /// </summary>
        [DataMember(Name = "quotaRitenutaSuImponibile", EmitDefaultValue = false)]
        public double QuotaRitenutaSuImponibile { get; set; }

        /// <summary>
        /// Gets or Sets Cambio
        /// </summary>
        [DataMember(Name = "cambio", EmitDefaultValue = false)]
        public double Cambio { get; set; }

        /// <summary>
        /// Gets or Sets CassaPrevidenzialeNome
        /// </summary>
        [DataMember(Name = "cassaPrevidenzialeNome", EmitDefaultValue = true)]
        public string CassaPrevidenzialeNome { get; set; }

        /// <summary>
        /// Gets or Sets CassaPrevidenzialePercentuale
        /// </summary>
        [DataMember(Name = "cassaPrevidenzialePercentuale", EmitDefaultValue = false)]
        public double CassaPrevidenzialePercentuale { get; set; }

        /// <summary>
        /// Gets or Sets Note
        /// </summary>
        [DataMember(Name = "note", EmitDefaultValue = true)]
        public string Note { get; set; }

        /// <summary>
        /// Gets or Sets ScontoPagamentoPercentuale
        /// </summary>
        [DataMember(Name = "scontoPagamentoPercentuale", EmitDefaultValue = false)]
        public double ScontoPagamentoPercentuale { get; set; }

        /// <summary>
        /// Gets or Sets Iva1Imponibile
        /// </summary>
        [DataMember(Name = "iva1Imponibile", EmitDefaultValue = false)]
        public double Iva1Imponibile { get; set; }

        /// <summary>
        /// Gets or Sets Iva2Imponibile
        /// </summary>
        [DataMember(Name = "iva2Imponibile", EmitDefaultValue = false)]
        public double Iva2Imponibile { get; set; }

        /// <summary>
        /// Gets or Sets Iva3Imponibile
        /// </summary>
        [DataMember(Name = "iva3Imponibile", EmitDefaultValue = false)]
        public double Iva3Imponibile { get; set; }

        /// <summary>
        /// Gets or Sets Iva4Imponibile
        /// </summary>
        [DataMember(Name = "iva4Imponibile", EmitDefaultValue = false)]
        public double Iva4Imponibile { get; set; }

        /// <summary>
        /// Gets or Sets Iva1ImponibileLordo
        /// </summary>
        [DataMember(Name = "iva1ImponibileLordo", EmitDefaultValue = false)]
        public double Iva1ImponibileLordo { get; set; }

        /// <summary>
        /// Gets or Sets Iva2ImponibileLordo
        /// </summary>
        [DataMember(Name = "iva2ImponibileLordo", EmitDefaultValue = false)]
        public double Iva2ImponibileLordo { get; set; }

        /// <summary>
        /// Gets or Sets Iva3ImponibileLordo
        /// </summary>
        [DataMember(Name = "iva3ImponibileLordo", EmitDefaultValue = false)]
        public double Iva3ImponibileLordo { get; set; }

        /// <summary>
        /// Gets or Sets Iva4ImponibileLordo
        /// </summary>
        [DataMember(Name = "iva4ImponibileLordo", EmitDefaultValue = false)]
        public double Iva4ImponibileLordo { get; set; }

        /// <summary>
        /// Gets or Sets Iva1Imposta
        /// </summary>
        [DataMember(Name = "iva1Imposta", EmitDefaultValue = false)]
        public double Iva1Imposta { get; set; }

        /// <summary>
        /// Gets or Sets Iva2Imposta
        /// </summary>
        [DataMember(Name = "iva2Imposta", EmitDefaultValue = false)]
        public double Iva2Imposta { get; set; }

        /// <summary>
        /// Gets or Sets Iva3Imposta
        /// </summary>
        [DataMember(Name = "iva3Imposta", EmitDefaultValue = false)]
        public double Iva3Imposta { get; set; }

        /// <summary>
        /// Gets or Sets Iva4Imposta
        /// </summary>
        [DataMember(Name = "iva4Imposta", EmitDefaultValue = false)]
        public double Iva4Imposta { get; set; }

        /// <summary>
        /// Gets or Sets TotaleRitenuta
        /// </summary>
        [DataMember(Name = "totaleRitenuta", EmitDefaultValue = false)]
        public double TotaleRitenuta { get; set; }

        /// <summary>
        /// Gets or Sets TotaleCassaPrevidenziale
        /// </summary>
        [DataMember(Name = "totaleCassaPrevidenziale", EmitDefaultValue = false)]
        public double TotaleCassaPrevidenziale { get; set; }

        /// <summary>
        /// Gets or Sets TotaleSpese
        /// </summary>
        [DataMember(Name = "totaleSpese", EmitDefaultValue = false)]
        public double TotaleSpese { get; set; }

        /// <summary>
        /// Gets or Sets TotaleSpeseEsenti
        /// </summary>
        [DataMember(Name = "totaleSpeseEsenti", EmitDefaultValue = false)]
        public double TotaleSpeseEsenti { get; set; }

        /// <summary>
        /// Gets or Sets TotaleSpeseNonEsenti
        /// </summary>
        [DataMember(Name = "totaleSpeseNonEsenti", EmitDefaultValue = false)]
        public double TotaleSpeseNonEsenti { get; set; }

        /// <summary>
        /// Gets or Sets TotaleMerce
        /// </summary>
        [DataMember(Name = "totaleMerce", EmitDefaultValue = false)]
        public double TotaleMerce { get; set; }

        /// <summary>
        /// Gets or Sets TotaleMerceScontata
        /// </summary>
        [DataMember(Name = "totaleMerceScontata", EmitDefaultValue = false)]
        public double TotaleMerceScontata { get; set; }

        /// <summary>
        /// Gets or Sets TotaleImponibile
        /// </summary>
        [DataMember(Name = "totaleImponibile", EmitDefaultValue = false)]
        public double TotaleImponibile { get; set; }

        /// <summary>
        /// Gets or Sets TotaleImposta
        /// </summary>
        [DataMember(Name = "totaleImposta", EmitDefaultValue = false)]
        public double TotaleImposta { get; set; }

        /// <summary>
        /// Gets or Sets TotaleEsente
        /// </summary>
        [DataMember(Name = "totaleEsente", EmitDefaultValue = false)]
        public double TotaleEsente { get; set; }

        /// <summary>
        /// Gets or Sets TotaleDocumento
        /// </summary>
        [DataMember(Name = "totaleDocumento", EmitDefaultValue = false)]
        public double TotaleDocumento { get; set; }

        /// <summary>
        /// Gets or Sets NettoAPagare
        /// </summary>
        [DataMember(Name = "nettoAPagare", EmitDefaultValue = false)]
        public double NettoAPagare { get; set; }

        /// <summary>
        /// Gets or Sets TotaleSplitPayment
        /// </summary>
        [DataMember(Name = "totaleSplitPayment", EmitDefaultValue = false)]
        public double TotaleSplitPayment { get; set; }

        /// <summary>
        /// Gets or Sets Bollo
        /// </summary>
        [DataMember(Name = "bollo", EmitDefaultValue = false)]
        public double Bollo { get; set; }

        /// <summary>
        /// Gets or Sets XmlFileName
        /// </summary>
        [DataMember(Name = "xmlFileName", EmitDefaultValue = true)]
        public string XmlFileName { get; set; }

        /// <summary>
        /// Gets or Sets RiferimentoAmministrazione
        /// </summary>
        [DataMember(Name = "riferimentoAmministrazione", EmitDefaultValue = true)]
        public string RiferimentoAmministrazione { get; set; }

        /// <summary>
        /// Gets or Sets Banca
        /// </summary>
        [DataMember(Name = "banca", EmitDefaultValue = false)]
        public BancaDocumento Banca { get; set; }

        /// <summary>
        /// Gets or Sets DatiTrasporto
        /// </summary>
        [DataMember(Name = "datiTrasporto", EmitDefaultValue = false)]
        public DatiTrasporto DatiTrasporto { get; set; }

        /// <summary>
        /// Gets or Sets DatiSdi
        /// </summary>
        [DataMember(Name = "datiSdi", EmitDefaultValue = false)]
        public DatiSdi DatiSdi { get; set; }

        /// <summary>
        /// Gets or Sets DatiPA
        /// </summary>
        [DataMember(Name = "datiPA", EmitDefaultValue = false)]
        public DatiPA DatiPA { get; set; }

        /// <summary>
        /// Gets or Sets Righe
        /// </summary>
        [DataMember(Name = "righe", EmitDefaultValue = true)]
        public List<BeneServizio> Righe { get; set; }

        /// <summary>
        /// Gets or Sets SpeseDocumento
        /// </summary>
        [DataMember(Name = "speseDocumento", EmitDefaultValue = true)]
        public List<SpesaDocumento> SpeseDocumento { get; set; }

        /// <summary>
        /// Gets or Sets Scadenze
        /// </summary>
        [DataMember(Name = "scadenze", EmitDefaultValue = true)]
        public List<Scadenza> Scadenze { get; set; }

        /// <summary>
        /// Gets or Sets Numero
        /// </summary>
        [DataMember(Name = "numero", EmitDefaultValue = true)]
        public string Numero { get; set; }

        /// <summary>
        /// Gets or Sets MediaFile
        /// </summary>
        [DataMember(Name = "mediaFile", EmitDefaultValue = false)]
        public DocumentoMediaFile MediaFile { get; set; }

        /// <summary>
        /// Gets or Sets Allegato
        /// </summary>
        [DataMember(Name = "allegato", EmitDefaultValue = true)]
        public string Allegato { get; set; }

        /// <summary>
        /// Gets or Sets BonusFiscaleCausale
        /// </summary>
        [DataMember(Name = "bonusFiscaleCausale", EmitDefaultValue = true)]
        public string BonusFiscaleCausale { get; set; }

        /// <summary>
        /// Gets or Sets BonusFiscaleImporto
        /// </summary>
        [DataMember(Name = "bonusFiscaleImporto", EmitDefaultValue = false)]
        public double BonusFiscaleImporto { get; set; }

        /// <summary>
        /// Gets or Sets BonusFiscalePercentuale
        /// </summary>
        [DataMember(Name = "bonusFiscalePercentuale", EmitDefaultValue = false)]
        public double BonusFiscalePercentuale { get; set; }

        /// <summary>
        /// Gets or Sets ProgressivoInvio
        /// </summary>
        [DataMember(Name = "progressivoInvio", EmitDefaultValue = true)]
        public List<ProgressivoInvio> ProgressivoInvio { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class Documento {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  ConcurrencyToken: ").Append(ConcurrencyToken).Append("\n");
            sb.Append("  AnagraficaId: ").Append(AnagraficaId).Append("\n");
            sb.Append("  Anagrafica: ").Append(Anagrafica).Append("\n");
            sb.Append("  CausaleDocumentoId: ").Append(CausaleDocumentoId).Append("\n");
            sb.Append("  CausaleDocumento: ").Append(CausaleDocumento).Append("\n");
            sb.Append("  AgenteId: ").Append(AgenteId).Append("\n");
            sb.Append("  Agente: ").Append(Agente).Append("\n");
            sb.Append("  PagamentoId: ").Append(PagamentoId).Append("\n");
            sb.Append("  Pagamento: ").Append(Pagamento).Append("\n");
            sb.Append("  VettoreId: ").Append(VettoreId).Append("\n");
            sb.Append("  Vettore: ").Append(Vettore).Append("\n");
            sb.Append("  CassaPrevidenzialeCausaleIvaId: ").Append(CassaPrevidenzialeCausaleIvaId).Append("\n");
            sb.Append("  ModelloId: ").Append(ModelloId).Append("\n");
            sb.Append("  CassaPrevidenzialeCausaleIva: ").Append(CassaPrevidenzialeCausaleIva).Append("\n");
            sb.Append("  DestinazioneId: ").Append(DestinazioneId).Append("\n");
            sb.Append("  Destinazione: ").Append(Destinazione).Append("\n");
            sb.Append("  ValutaId: ").Append(ValutaId).Append("\n");
            sb.Append("  Valuta: ").Append(Valuta).Append("\n");
            sb.Append("  CausaleMagazzinoId: ").Append(CausaleMagazzinoId).Append("\n");
            sb.Append("  CausaleMagazzino: ").Append(CausaleMagazzino).Append("\n");
            sb.Append("  Iva1CausaleIvaId: ").Append(Iva1CausaleIvaId).Append("\n");
            sb.Append("  Iva1CausaleIva: ").Append(Iva1CausaleIva).Append("\n");
            sb.Append("  Iva2CausaleIvaId: ").Append(Iva2CausaleIvaId).Append("\n");
            sb.Append("  Iva2CausaleIva: ").Append(Iva2CausaleIva).Append("\n");
            sb.Append("  Iva3CausaleIvaId: ").Append(Iva3CausaleIvaId).Append("\n");
            sb.Append("  Iva3CausaleIva: ").Append(Iva3CausaleIva).Append("\n");
            sb.Append("  Iva4CausaleIvaId: ").Append(Iva4CausaleIvaId).Append("\n");
            sb.Append("  Iva4CausaleIva: ").Append(Iva4CausaleIva).Append("\n");
            sb.Append("  TipoDocumento: ").Append(TipoDocumento).Append("\n");
            sb.Append("  DocumentoFiscale: ").Append(DocumentoFiscale).Append("\n");
            sb.Append("  DestinazioneDiversa: ").Append(DestinazioneDiversa).Append("\n");
            sb.Append("  IvaEsclusa: ").Append(IvaEsclusa).Append("\n");
            sb.Append("  RitenutaIncludeCassaPrevidenziale: ").Append(RitenutaIncludeCassaPrevidenziale).Append("\n");
            sb.Append("  ClasseDocumento: ").Append(ClasseDocumento).Append("\n");
            sb.Append("  Contatto: ").Append(Contatto).Append("\n");
            sb.Append("  NumeroParteTesto: ").Append(NumeroParteTesto).Append("\n");
            sb.Append("  NumeroParteNumerica: ").Append(NumeroParteNumerica).Append("\n");
            sb.Append("  Data: ").Append(Data).Append("\n");
            sb.Append("  Stato: ").Append(Stato).Append("\n");
            sb.Append("  DataScadenzaPreventivo: ").Append(DataScadenzaPreventivo).Append("\n");
            sb.Append("  DataInizioScadenze: ").Append(DataInizioScadenze).Append("\n");
            sb.Append("  ScontoPercentuale: ").Append(ScontoPercentuale).Append("\n");
            sb.Append("  ScontoIncondizionatoImporto: ").Append(ScontoIncondizionatoImporto).Append("\n");
            sb.Append("  AbbuonoImporto: ").Append(AbbuonoImporto).Append("\n");
            sb.Append("  RitenutaPercentuale: ").Append(RitenutaPercentuale).Append("\n");
            sb.Append("  EnasarcoPercentuale: ").Append(EnasarcoPercentuale).Append("\n");
            sb.Append("  EnasarcoImporto: ").Append(EnasarcoImporto).Append("\n");
            sb.Append("  QuotaRitenutaSuImponibile: ").Append(QuotaRitenutaSuImponibile).Append("\n");
            sb.Append("  Cambio: ").Append(Cambio).Append("\n");
            sb.Append("  CassaPrevidenzialeNome: ").Append(CassaPrevidenzialeNome).Append("\n");
            sb.Append("  CassaPrevidenzialePercentuale: ").Append(CassaPrevidenzialePercentuale).Append("\n");
            sb.Append("  Note: ").Append(Note).Append("\n");
            sb.Append("  ScontoPagamentoPercentuale: ").Append(ScontoPagamentoPercentuale).Append("\n");
            sb.Append("  Iva1Imponibile: ").Append(Iva1Imponibile).Append("\n");
            sb.Append("  Iva2Imponibile: ").Append(Iva2Imponibile).Append("\n");
            sb.Append("  Iva3Imponibile: ").Append(Iva3Imponibile).Append("\n");
            sb.Append("  Iva4Imponibile: ").Append(Iva4Imponibile).Append("\n");
            sb.Append("  Iva1ImponibileLordo: ").Append(Iva1ImponibileLordo).Append("\n");
            sb.Append("  Iva2ImponibileLordo: ").Append(Iva2ImponibileLordo).Append("\n");
            sb.Append("  Iva3ImponibileLordo: ").Append(Iva3ImponibileLordo).Append("\n");
            sb.Append("  Iva4ImponibileLordo: ").Append(Iva4ImponibileLordo).Append("\n");
            sb.Append("  Iva1Imposta: ").Append(Iva1Imposta).Append("\n");
            sb.Append("  Iva2Imposta: ").Append(Iva2Imposta).Append("\n");
            sb.Append("  Iva3Imposta: ").Append(Iva3Imposta).Append("\n");
            sb.Append("  Iva4Imposta: ").Append(Iva4Imposta).Append("\n");
            sb.Append("  TotaleRitenuta: ").Append(TotaleRitenuta).Append("\n");
            sb.Append("  TotaleCassaPrevidenziale: ").Append(TotaleCassaPrevidenziale).Append("\n");
            sb.Append("  TotaleSpese: ").Append(TotaleSpese).Append("\n");
            sb.Append("  TotaleSpeseEsenti: ").Append(TotaleSpeseEsenti).Append("\n");
            sb.Append("  TotaleSpeseNonEsenti: ").Append(TotaleSpeseNonEsenti).Append("\n");
            sb.Append("  TotaleMerce: ").Append(TotaleMerce).Append("\n");
            sb.Append("  TotaleMerceScontata: ").Append(TotaleMerceScontata).Append("\n");
            sb.Append("  TotaleImponibile: ").Append(TotaleImponibile).Append("\n");
            sb.Append("  TotaleImposta: ").Append(TotaleImposta).Append("\n");
            sb.Append("  TotaleEsente: ").Append(TotaleEsente).Append("\n");
            sb.Append("  TotaleDocumento: ").Append(TotaleDocumento).Append("\n");
            sb.Append("  NettoAPagare: ").Append(NettoAPagare).Append("\n");
            sb.Append("  TotaleSplitPayment: ").Append(TotaleSplitPayment).Append("\n");
            sb.Append("  Bollo: ").Append(Bollo).Append("\n");
            sb.Append("  XmlFileName: ").Append(XmlFileName).Append("\n");
            sb.Append("  RiferimentoAmministrazione: ").Append(RiferimentoAmministrazione).Append("\n");
            sb.Append("  Banca: ").Append(Banca).Append("\n");
            sb.Append("  DatiTrasporto: ").Append(DatiTrasporto).Append("\n");
            sb.Append("  DatiSdi: ").Append(DatiSdi).Append("\n");
            sb.Append("  DatiPA: ").Append(DatiPA).Append("\n");
            sb.Append("  Righe: ").Append(Righe).Append("\n");
            sb.Append("  SpeseDocumento: ").Append(SpeseDocumento).Append("\n");
            sb.Append("  Scadenze: ").Append(Scadenze).Append("\n");
            sb.Append("  Numero: ").Append(Numero).Append("\n");
            sb.Append("  MediaFile: ").Append(MediaFile).Append("\n");
            sb.Append("  Allegato: ").Append(Allegato).Append("\n");
            sb.Append("  BonusFiscaleCausale: ").Append(BonusFiscaleCausale).Append("\n");
            sb.Append("  BonusFiscaleImporto: ").Append(BonusFiscaleImporto).Append("\n");
            sb.Append("  BonusFiscalePercentuale: ").Append(BonusFiscalePercentuale).Append("\n");
            sb.Append("  ProgressivoInvio: ").Append(ProgressivoInvio).Append("\n");
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
            // NumeroParteTesto (string) maxLength
            if (this.NumeroParteTesto != null && this.NumeroParteTesto.Length > 40)
            {
                yield return new ValidationResult("Invalid value for NumeroParteTesto, length must be less than 40.", new [] { "NumeroParteTesto" });
            }

            // NumeroParteTesto (string) minLength
            if (this.NumeroParteTesto != null && this.NumeroParteTesto.Length < 0)
            {
                yield return new ValidationResult("Invalid value for NumeroParteTesto, length must be greater than 0.", new [] { "NumeroParteTesto" });
            }

            // NumeroParteNumerica (int) maximum
            if (this.NumeroParteNumerica > (int)2147483647)
            {
                yield return new ValidationResult("Invalid value for NumeroParteNumerica, must be a value less than or equal to 2147483647.", new [] { "NumeroParteNumerica" });
            }

            // NumeroParteNumerica (int) minimum
            if (this.NumeroParteNumerica < (int)0)
            {
                yield return new ValidationResult("Invalid value for NumeroParteNumerica, must be a value greater than or equal to 0.", new [] { "NumeroParteNumerica" });
            }

            // CassaPrevidenzialeNome (string) maxLength
            if (this.CassaPrevidenzialeNome != null && this.CassaPrevidenzialeNome.Length > 50)
            {
                yield return new ValidationResult("Invalid value for CassaPrevidenzialeNome, length must be less than 50.", new [] { "CassaPrevidenzialeNome" });
            }

            // CassaPrevidenzialeNome (string) minLength
            if (this.CassaPrevidenzialeNome != null && this.CassaPrevidenzialeNome.Length < 0)
            {
                yield return new ValidationResult("Invalid value for CassaPrevidenzialeNome, length must be greater than 0.", new [] { "CassaPrevidenzialeNome" });
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

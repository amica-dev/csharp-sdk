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
    /// Dati di fatturazione periodica.
    /// </summary>
    [DataContract(Name = "PeriodicInvoicingData")]
    public partial class PeriodicInvoicingData : IValidatableObject
    {
        /// <summary>
        /// Tipo di importazione.
        /// </summary>
        /// <value>Tipo di importazione.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TipoImportazioneEnum
        {
            /// <summary>
            /// Enum Righe for value: Righe
            /// </summary>
            [EnumMember(Value = "Righe")]
            Righe = 1,

            /// <summary>
            /// Enum Totali for value: Totali
            /// </summary>
            [EnumMember(Value = "Totali")]
            Totali = 2,

            /// <summary>
            /// Enum AccorpaArticoli for value: AccorpaArticoli
            /// </summary>
            [EnumMember(Value = "AccorpaArticoli")]
            AccorpaArticoli = 3
        }


        /// <summary>
        /// Tipo di importazione.
        /// </summary>
        /// <value>Tipo di importazione.</value>
        [DataMember(Name = "tipoImportazione", EmitDefaultValue = false)]
        public TipoImportazioneEnum? TipoImportazione { get; set; }
        /// <summary>
        /// Tipo del documento generato.
        /// </summary>
        /// <value>Tipo del documento generato.</value>
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
        /// Tipo del documento generato.
        /// </summary>
        /// <value>Tipo del documento generato.</value>
        [DataMember(Name = "tipoDocumento", EmitDefaultValue = false)]
        public TipoDocumentoEnum? TipoDocumento { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="PeriodicInvoicingData" /> class.
        /// </summary>
        /// <param name="da">Data di inizio del periodo..</param>
        /// <param name="a">Data di fine del periodo..</param>
        /// <param name="tipoImportazione">Tipo di importazione..</param>
        /// <param name="causaleDocumentoId">Identificativo univoco della Amica.CausaleDocumento del documento generato..</param>
        /// <param name="dataDocumento">Data del documento generato..</param>
        /// <param name="tipoDocumento">Tipo del documento generato..</param>
        public PeriodicInvoicingData(DateTime da = default(DateTime), DateTime a = default(DateTime), TipoImportazioneEnum? tipoImportazione = default(TipoImportazioneEnum?), int? causaleDocumentoId = default(int?), DateTime dataDocumento = default(DateTime), TipoDocumentoEnum? tipoDocumento = default(TipoDocumentoEnum?))
        {
            this.Da = da;
            this.A = a;
            this.TipoImportazione = tipoImportazione;
            this.CausaleDocumentoId = causaleDocumentoId;
            this.DataDocumento = dataDocumento;
            this.TipoDocumento = tipoDocumento;
        }

        /// <summary>
        /// Data di inizio del periodo.
        /// </summary>
        /// <value>Data di inizio del periodo.</value>
        [DataMember(Name = "da", EmitDefaultValue = false)]
        public DateTime Da { get; set; }

        /// <summary>
        /// Data di fine del periodo.
        /// </summary>
        /// <value>Data di fine del periodo.</value>
        [DataMember(Name = "a", EmitDefaultValue = false)]
        public DateTime A { get; set; }

        /// <summary>
        /// Identificativo univoco della Amica.CausaleDocumento del documento generato.
        /// </summary>
        /// <value>Identificativo univoco della Amica.CausaleDocumento del documento generato.</value>
        /*
        <example>1</example>
        */
        [DataMember(Name = "causaleDocumentoId", EmitDefaultValue = true)]
        public int? CausaleDocumentoId { get; set; }

        /// <summary>
        /// Data del documento generato.
        /// </summary>
        /// <value>Data del documento generato.</value>
        [DataMember(Name = "dataDocumento", EmitDefaultValue = false)]
        public DateTime DataDocumento { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("class PeriodicInvoicingData {\n");
            sb.Append("  Da: ").Append(Da).Append("\n");
            sb.Append("  A: ").Append(A).Append("\n");
            sb.Append("  TipoImportazione: ").Append(TipoImportazione).Append("\n");
            sb.Append("  CausaleDocumentoId: ").Append(CausaleDocumentoId).Append("\n");
            sb.Append("  DataDocumento: ").Append(DataDocumento).Append("\n");
            sb.Append("  TipoDocumento: ").Append(TipoDocumento).Append("\n");
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

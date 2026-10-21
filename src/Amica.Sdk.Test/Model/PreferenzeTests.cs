/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.2.0
 * Contact: assistenza@gestionaleamica.com
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using Xunit;

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Amica.Sdk.Model;
using Amica.Sdk.Client;
using System.Reflection;
using Newtonsoft.Json;

namespace Amica.Sdk.Test.Model
{
    /// <summary>
    ///  Class for testing Preferenze
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by OpenAPI Generator (https://openapi-generator.tech).
    /// Please update the test case below to test the model.
    /// </remarks>
    public class PreferenzeTests : IDisposable
    {
        // TODO uncomment below to declare an instance variable for Preferenze
        //private Preferenze instance;

        public PreferenzeTests()
        {
            // TODO uncomment below to create an instance of Preferenze
            //instance = new Preferenze();
        }

        public void Dispose()
        {
            // Cleanup when everything is done.
        }

        /// <summary>
        /// Test an instance of Preferenze
        /// </summary>
        [Fact]
        public void PreferenzeInstanceTest()
        {
            // TODO uncomment below to test "IsType" Preferenze
            //Assert.IsType<Preferenze>(instance);
        }

        /// <summary>
        /// Test the property 'Id'
        /// </summary>
        [Fact]
        public void IdTest()
        {
            // TODO unit test for the property 'Id'
        }

        /// <summary>
        /// Test the property 'ConcurrencyToken'
        /// </summary>
        [Fact]
        public void ConcurrencyTokenTest()
        {
            // TODO unit test for the property 'ConcurrencyToken'
        }

        /// <summary>
        /// Test the property 'RagioneSociale1'
        /// </summary>
        [Fact]
        public void RagioneSociale1Test()
        {
            // TODO unit test for the property 'RagioneSociale1'
        }

        /// <summary>
        /// Test the property 'RagioneSociale2'
        /// </summary>
        [Fact]
        public void RagioneSociale2Test()
        {
            // TODO unit test for the property 'RagioneSociale2'
        }

        /// <summary>
        /// Test the property 'PartitaIva'
        /// </summary>
        [Fact]
        public void PartitaIvaTest()
        {
            // TODO unit test for the property 'PartitaIva'
        }

        /// <summary>
        /// Test the property 'RegistroImprese'
        /// </summary>
        [Fact]
        public void RegistroImpreseTest()
        {
            // TODO unit test for the property 'RegistroImprese'
        }

        /// <summary>
        /// Test the property 'CodiceFiscale'
        /// </summary>
        [Fact]
        public void CodiceFiscaleTest()
        {
            // TODO unit test for the property 'CodiceFiscale'
        }

        /// <summary>
        /// Test the property 'CodiceSia'
        /// </summary>
        [Fact]
        public void CodiceSiaTest()
        {
            // TODO unit test for the property 'CodiceSia'
        }

        /// <summary>
        /// Test the property 'CodiceRea'
        /// </summary>
        [Fact]
        public void CodiceReaTest()
        {
            // TODO unit test for the property 'CodiceRea'
        }

        /// <summary>
        /// Test the property 'PersonaGiuridica'
        /// </summary>
        [Fact]
        public void PersonaGiuridicaTest()
        {
            // TODO unit test for the property 'PersonaGiuridica'
        }

        /// <summary>
        /// Test the property 'DataNascita'
        /// </summary>
        [Fact]
        public void DataNascitaTest()
        {
            // TODO unit test for the property 'DataNascita'
        }

        /// <summary>
        /// Test the property 'LocalitaNascita'
        /// </summary>
        [Fact]
        public void LocalitaNascitaTest()
        {
            // TODO unit test for the property 'LocalitaNascita'
        }

        /// <summary>
        /// Test the property 'ProvinciaNascita'
        /// </summary>
        [Fact]
        public void ProvinciaNascitaTest()
        {
            // TODO unit test for the property 'ProvinciaNascita'
        }

        /// <summary>
        /// Test the property 'Sesso'
        /// </summary>
        [Fact]
        public void SessoTest()
        {
            // TODO unit test for the property 'Sesso'
        }

        /// <summary>
        /// Test the property 'Attivita'
        /// </summary>
        [Fact]
        public void AttivitaTest()
        {
            // TODO unit test for the property 'Attivita'
        }

        /// <summary>
        /// Test the property 'IvaEsclusa'
        /// </summary>
        [Fact]
        public void IvaEsclusaTest()
        {
            // TODO unit test for the property 'IvaEsclusa'
        }

        /// <summary>
        /// Test the property 'DecimaliPrezzoUnitario'
        /// </summary>
        [Fact]
        public void DecimaliPrezzoUnitarioTest()
        {
            // TODO unit test for the property 'DecimaliPrezzoUnitario'
        }

        /// <summary>
        /// Test the property 'CausaleProduzioneComponentiId'
        /// </summary>
        [Fact]
        public void CausaleProduzioneComponentiIdTest()
        {
            // TODO unit test for the property 'CausaleProduzioneComponentiId'
        }

        /// <summary>
        /// Test the property 'CausaleProduzioneCompostiId'
        /// </summary>
        [Fact]
        public void CausaleProduzioneCompostiIdTest()
        {
            // TODO unit test for the property 'CausaleProduzioneCompostiId'
        }

        /// <summary>
        /// Test the property 'CausaleMagazzinoVenditaId'
        /// </summary>
        [Fact]
        public void CausaleMagazzinoVenditaIdTest()
        {
            // TODO unit test for the property 'CausaleMagazzinoVenditaId'
        }

        /// <summary>
        /// Test the property 'CausaleMagazzinoAcquistoId'
        /// </summary>
        [Fact]
        public void CausaleMagazzinoAcquistoIdTest()
        {
            // TODO unit test for the property 'CausaleMagazzinoAcquistoId'
        }

        /// <summary>
        /// Test the property 'RicercaArticolo'
        /// </summary>
        [Fact]
        public void RicercaArticoloTest()
        {
            // TODO unit test for the property 'RicercaArticolo'
        }

        /// <summary>
        /// Test the property 'DataSpeciale1'
        /// </summary>
        [Fact]
        public void DataSpeciale1Test()
        {
            // TODO unit test for the property 'DataSpeciale1'
        }

        /// <summary>
        /// Test the property 'DataSpeciale2'
        /// </summary>
        [Fact]
        public void DataSpeciale2Test()
        {
            // TODO unit test for the property 'DataSpeciale2'
        }

        /// <summary>
        /// Test the property 'GiorniExtraDataSpeciale1'
        /// </summary>
        [Fact]
        public void GiorniExtraDataSpeciale1Test()
        {
            // TODO unit test for the property 'GiorniExtraDataSpeciale1'
        }

        /// <summary>
        /// Test the property 'GiorniExtraDataSpeciale2'
        /// </summary>
        [Fact]
        public void GiorniExtraDataSpeciale2Test()
        {
            // TODO unit test for the property 'GiorniExtraDataSpeciale2'
        }

        /// <summary>
        /// Test the property 'PagatoManuale'
        /// </summary>
        [Fact]
        public void PagatoManualeTest()
        {
            // TODO unit test for the property 'PagatoManuale'
        }

        /// <summary>
        /// Test the property 'CausaleMagazzinoMerciId'
        /// </summary>
        [Fact]
        public void CausaleMagazzinoMerciIdTest()
        {
            // TODO unit test for the property 'CausaleMagazzinoMerciId'
        }

        /// <summary>
        /// Test the property 'MagazzinoMerciId'
        /// </summary>
        [Fact]
        public void MagazzinoMerciIdTest()
        {
            // TODO unit test for the property 'MagazzinoMerciId'
        }

        /// <summary>
        /// Test the property 'ListinoMerciId'
        /// </summary>
        [Fact]
        public void ListinoMerciIdTest()
        {
            // TODO unit test for the property 'ListinoMerciId'
        }

        /// <summary>
        /// Test the property 'ModalitaPagamentoMerciId'
        /// </summary>
        [Fact]
        public void ModalitaPagamentoMerciIdTest()
        {
            // TODO unit test for the property 'ModalitaPagamentoMerciId'
        }

        /// <summary>
        /// Test the property 'DocumentiCausaleIvaId'
        /// </summary>
        [Fact]
        public void DocumentiCausaleIvaIdTest()
        {
            // TODO unit test for the property 'DocumentiCausaleIvaId'
        }

        /// <summary>
        /// Test the property 'DocumentiMagazzinoId'
        /// </summary>
        [Fact]
        public void DocumentiMagazzinoIdTest()
        {
            // TODO unit test for the property 'DocumentiMagazzinoId'
        }

        /// <summary>
        /// Test the property 'DocumentiListinoId'
        /// </summary>
        [Fact]
        public void DocumentiListinoIdTest()
        {
            // TODO unit test for the property 'DocumentiListinoId'
        }

        /// <summary>
        /// Test the property 'EMail'
        /// </summary>
        [Fact]
        public void EMailTest()
        {
            // TODO unit test for the property 'EMail'
        }

        /// <summary>
        /// Test the property 'SitoWeb'
        /// </summary>
        [Fact]
        public void SitoWebTest()
        {
            // TODO unit test for the property 'SitoWeb'
        }

        /// <summary>
        /// Test the property 'AnnoContabile'
        /// </summary>
        [Fact]
        public void AnnoContabileTest()
        {
            // TODO unit test for the property 'AnnoContabile'
        }

        /// <summary>
        /// Test the property 'UnitDiMisura'
        /// </summary>
        [Fact]
        public void UnitDiMisuraTest()
        {
            // TODO unit test for the property 'UnitDiMisura'
        }

        /// <summary>
        /// Test the property 'CausalePagamentoModelloCU'
        /// </summary>
        [Fact]
        public void CausalePagamentoModelloCUTest()
        {
            // TODO unit test for the property 'CausalePagamentoModelloCU'
        }

        /// <summary>
        /// Test the property 'ImpostazioniSdi'
        /// </summary>
        [Fact]
        public void ImpostazioniSdiTest()
        {
            // TODO unit test for the property 'ImpostazioniSdi'
        }

        /// <summary>
        /// Test the property 'TipoBilancio'
        /// </summary>
        [Fact]
        public void TipoBilancioTest()
        {
            // TODO unit test for the property 'TipoBilancio'
        }

        /// <summary>
        /// Test the property 'PeriodoIva'
        /// </summary>
        [Fact]
        public void PeriodoIvaTest()
        {
            // TODO unit test for the property 'PeriodoIva'
        }

        /// <summary>
        /// Test the property 'LiquidazioneIva'
        /// </summary>
        [Fact]
        public void LiquidazioneIvaTest()
        {
            // TODO unit test for the property 'LiquidazioneIva'
        }

        /// <summary>
        /// Test the property 'ModificaRegistrazioneContabileAllaEmissione'
        /// </summary>
        [Fact]
        public void ModificaRegistrazioneContabileAllaEmissioneTest()
        {
            // TODO unit test for the property 'ModificaRegistrazioneContabileAllaEmissione'
        }

        /// <summary>
        /// Test the property 'OttimizzaEmissioneOrdini'
        /// </summary>
        [Fact]
        public void OttimizzaEmissioneOrdiniTest()
        {
            // TODO unit test for the property 'OttimizzaEmissioneOrdini'
        }

        /// <summary>
        /// Test the property 'AggiungiAnnoInCorsoAlNumeroDocumento'
        /// </summary>
        [Fact]
        public void AggiungiAnnoInCorsoAlNumeroDocumentoTest()
        {
            // TODO unit test for the property 'AggiungiAnnoInCorsoAlNumeroDocumento'
        }

        /// <summary>
        /// Test the property 'ConsentiCambioManualeStatoDdt'
        /// </summary>
        [Fact]
        public void ConsentiCambioManualeStatoDdtTest()
        {
            // TODO unit test for the property 'ConsentiCambioManualeStatoDdt'
        }

        /// <summary>
        /// Test the property 'EmailBody'
        /// </summary>
        [Fact]
        public void EmailBodyTest()
        {
            // TODO unit test for the property 'EmailBody'
        }

        /// <summary>
        /// Test the property 'EmailAutoSend'
        /// </summary>
        [Fact]
        public void EmailAutoSendTest()
        {
            // TODO unit test for the property 'EmailAutoSend'
        }

        /// <summary>
        /// Test the property 'AutoProduzione'
        /// </summary>
        [Fact]
        public void AutoProduzioneTest()
        {
            // TODO unit test for the property 'AutoProduzione'
        }

        /// <summary>
        /// Test the property 'CodiceDestinatario'
        /// </summary>
        [Fact]
        public void CodiceDestinatarioTest()
        {
            // TODO unit test for the property 'CodiceDestinatario'
        }

        /// <summary>
        /// Test the property 'Pec'
        /// </summary>
        [Fact]
        public void PecTest()
        {
            // TODO unit test for the property 'Pec'
        }

        /// <summary>
        /// Test the property 'ModelloVenditeNotaDebitoId'
        /// </summary>
        [Fact]
        public void ModelloVenditeNotaDebitoIdTest()
        {
            // TODO unit test for the property 'ModelloVenditeNotaDebitoId'
        }

        /// <summary>
        /// Test the property 'ModelloVenditeNotaCreditoId'
        /// </summary>
        [Fact]
        public void ModelloVenditeNotaCreditoIdTest()
        {
            // TODO unit test for the property 'ModelloVenditeNotaCreditoId'
        }

        /// <summary>
        /// Test the property 'ModelloVenditeRicevutaFiscaleId'
        /// </summary>
        [Fact]
        public void ModelloVenditeRicevutaFiscaleIdTest()
        {
            // TODO unit test for the property 'ModelloVenditeRicevutaFiscaleId'
        }

        /// <summary>
        /// Test the property 'ModelloVenditeFatturaProfessionistaId'
        /// </summary>
        [Fact]
        public void ModelloVenditeFatturaProfessionistaIdTest()
        {
            // TODO unit test for the property 'ModelloVenditeFatturaProfessionistaId'
        }

        /// <summary>
        /// Test the property 'ModelloVenditeFatturaDifferitaId'
        /// </summary>
        [Fact]
        public void ModelloVenditeFatturaDifferitaIdTest()
        {
            // TODO unit test for the property 'ModelloVenditeFatturaDifferitaId'
        }

        /// <summary>
        /// Test the property 'ModelloVenditeFatturaAccontoId'
        /// </summary>
        [Fact]
        public void ModelloVenditeFatturaAccontoIdTest()
        {
            // TODO unit test for the property 'ModelloVenditeFatturaAccontoId'
        }

        /// <summary>
        /// Test the property 'ModelloAcquistiFatturaAccompagnatoriaId'
        /// </summary>
        [Fact]
        public void ModelloAcquistiFatturaAccompagnatoriaIdTest()
        {
            // TODO unit test for the property 'ModelloAcquistiFatturaAccompagnatoriaId'
        }

        /// <summary>
        /// Test the property 'ModelloVenditeFatturaAccompagnaoriaId'
        /// </summary>
        [Fact]
        public void ModelloVenditeFatturaAccompagnaoriaIdTest()
        {
            // TODO unit test for the property 'ModelloVenditeFatturaAccompagnaoriaId'
        }

        /// <summary>
        /// Test the property 'ModelloAcquistiNotaDebitoId'
        /// </summary>
        [Fact]
        public void ModelloAcquistiNotaDebitoIdTest()
        {
            // TODO unit test for the property 'ModelloAcquistiNotaDebitoId'
        }

        /// <summary>
        /// Test the property 'ModelloAcquistiNotaCreditoId'
        /// </summary>
        [Fact]
        public void ModelloAcquistiNotaCreditoIdTest()
        {
            // TODO unit test for the property 'ModelloAcquistiNotaCreditoId'
        }

        /// <summary>
        /// Test the property 'ModelloAcquistiFatturaProfessionistiId'
        /// </summary>
        [Fact]
        public void ModelloAcquistiFatturaProfessionistiIdTest()
        {
            // TODO unit test for the property 'ModelloAcquistiFatturaProfessionistiId'
        }

        /// <summary>
        /// Test the property 'ModelloAcquistiFatturaDifferitaId'
        /// </summary>
        [Fact]
        public void ModelloAcquistiFatturaDifferitaIdTest()
        {
            // TODO unit test for the property 'ModelloAcquistiFatturaDifferitaId'
        }

        /// <summary>
        /// Test the property 'ModelloAcquistiFatturaAccontoId'
        /// </summary>
        [Fact]
        public void ModelloAcquistiFatturaAccontoIdTest()
        {
            // TODO unit test for the property 'ModelloAcquistiFatturaAccontoId'
        }

        /// <summary>
        /// Test the property 'SedeLegale'
        /// </summary>
        [Fact]
        public void SedeLegaleTest()
        {
            // TODO unit test for the property 'SedeLegale'
        }

        /// <summary>
        /// Test the property 'SedeOperativa'
        /// </summary>
        [Fact]
        public void SedeOperativaTest()
        {
            // TODO unit test for the property 'SedeOperativa'
        }

        /// <summary>
        /// Test the property 'SedeAltra'
        /// </summary>
        [Fact]
        public void SedeAltraTest()
        {
            // TODO unit test for the property 'SedeAltra'
        }
    }
}

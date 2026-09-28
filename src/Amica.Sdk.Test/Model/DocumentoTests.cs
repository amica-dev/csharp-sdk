/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.1.1
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
    ///  Class for testing Documento
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by OpenAPI Generator (https://openapi-generator.tech).
    /// Please update the test case below to test the model.
    /// </remarks>
    public class DocumentoTests : IDisposable
    {
        // TODO uncomment below to declare an instance variable for Documento
        //private Documento instance;

        public DocumentoTests()
        {
            // TODO uncomment below to create an instance of Documento
            //instance = new Documento();
        }

        public void Dispose()
        {
            // Cleanup when everything is done.
        }

        /// <summary>
        /// Test an instance of Documento
        /// </summary>
        [Fact]
        public void DocumentoInstanceTest()
        {
            // TODO uncomment below to test "IsType" Documento
            //Assert.IsType<Documento>(instance);
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
        /// Test the property 'AnagraficaId'
        /// </summary>
        [Fact]
        public void AnagraficaIdTest()
        {
            // TODO unit test for the property 'AnagraficaId'
        }

        /// <summary>
        /// Test the property 'Anagrafica'
        /// </summary>
        [Fact]
        public void AnagraficaTest()
        {
            // TODO unit test for the property 'Anagrafica'
        }

        /// <summary>
        /// Test the property 'CausaleDocumentoId'
        /// </summary>
        [Fact]
        public void CausaleDocumentoIdTest()
        {
            // TODO unit test for the property 'CausaleDocumentoId'
        }

        /// <summary>
        /// Test the property 'CausaleDocumento'
        /// </summary>
        [Fact]
        public void CausaleDocumentoTest()
        {
            // TODO unit test for the property 'CausaleDocumento'
        }

        /// <summary>
        /// Test the property 'AgenteId'
        /// </summary>
        [Fact]
        public void AgenteIdTest()
        {
            // TODO unit test for the property 'AgenteId'
        }

        /// <summary>
        /// Test the property 'Agente'
        /// </summary>
        [Fact]
        public void AgenteTest()
        {
            // TODO unit test for the property 'Agente'
        }

        /// <summary>
        /// Test the property 'PagamentoId'
        /// </summary>
        [Fact]
        public void PagamentoIdTest()
        {
            // TODO unit test for the property 'PagamentoId'
        }

        /// <summary>
        /// Test the property 'Pagamento'
        /// </summary>
        [Fact]
        public void PagamentoTest()
        {
            // TODO unit test for the property 'Pagamento'
        }

        /// <summary>
        /// Test the property 'VettoreId'
        /// </summary>
        [Fact]
        public void VettoreIdTest()
        {
            // TODO unit test for the property 'VettoreId'
        }

        /// <summary>
        /// Test the property 'Vettore'
        /// </summary>
        [Fact]
        public void VettoreTest()
        {
            // TODO unit test for the property 'Vettore'
        }

        /// <summary>
        /// Test the property 'CassaPrevidenzialeCausaleIvaId'
        /// </summary>
        [Fact]
        public void CassaPrevidenzialeCausaleIvaIdTest()
        {
            // TODO unit test for the property 'CassaPrevidenzialeCausaleIvaId'
        }

        /// <summary>
        /// Test the property 'ModelloId'
        /// </summary>
        [Fact]
        public void ModelloIdTest()
        {
            // TODO unit test for the property 'ModelloId'
        }

        /// <summary>
        /// Test the property 'CassaPrevidenzialeCausaleIva'
        /// </summary>
        [Fact]
        public void CassaPrevidenzialeCausaleIvaTest()
        {
            // TODO unit test for the property 'CassaPrevidenzialeCausaleIva'
        }

        /// <summary>
        /// Test the property 'DestinazioneId'
        /// </summary>
        [Fact]
        public void DestinazioneIdTest()
        {
            // TODO unit test for the property 'DestinazioneId'
        }

        /// <summary>
        /// Test the property 'Destinazione'
        /// </summary>
        [Fact]
        public void DestinazioneTest()
        {
            // TODO unit test for the property 'Destinazione'
        }

        /// <summary>
        /// Test the property 'ValutaId'
        /// </summary>
        [Fact]
        public void ValutaIdTest()
        {
            // TODO unit test for the property 'ValutaId'
        }

        /// <summary>
        /// Test the property 'Valuta'
        /// </summary>
        [Fact]
        public void ValutaTest()
        {
            // TODO unit test for the property 'Valuta'
        }

        /// <summary>
        /// Test the property 'CausaleMagazzinoId'
        /// </summary>
        [Fact]
        public void CausaleMagazzinoIdTest()
        {
            // TODO unit test for the property 'CausaleMagazzinoId'
        }

        /// <summary>
        /// Test the property 'CausaleMagazzino'
        /// </summary>
        [Fact]
        public void CausaleMagazzinoTest()
        {
            // TODO unit test for the property 'CausaleMagazzino'
        }

        /// <summary>
        /// Test the property 'Iva1CausaleIvaId'
        /// </summary>
        [Fact]
        public void Iva1CausaleIvaIdTest()
        {
            // TODO unit test for the property 'Iva1CausaleIvaId'
        }

        /// <summary>
        /// Test the property 'Iva1CausaleIva'
        /// </summary>
        [Fact]
        public void Iva1CausaleIvaTest()
        {
            // TODO unit test for the property 'Iva1CausaleIva'
        }

        /// <summary>
        /// Test the property 'Iva2CausaleIvaId'
        /// </summary>
        [Fact]
        public void Iva2CausaleIvaIdTest()
        {
            // TODO unit test for the property 'Iva2CausaleIvaId'
        }

        /// <summary>
        /// Test the property 'Iva2CausaleIva'
        /// </summary>
        [Fact]
        public void Iva2CausaleIvaTest()
        {
            // TODO unit test for the property 'Iva2CausaleIva'
        }

        /// <summary>
        /// Test the property 'Iva3CausaleIvaId'
        /// </summary>
        [Fact]
        public void Iva3CausaleIvaIdTest()
        {
            // TODO unit test for the property 'Iva3CausaleIvaId'
        }

        /// <summary>
        /// Test the property 'Iva3CausaleIva'
        /// </summary>
        [Fact]
        public void Iva3CausaleIvaTest()
        {
            // TODO unit test for the property 'Iva3CausaleIva'
        }

        /// <summary>
        /// Test the property 'Iva4CausaleIvaId'
        /// </summary>
        [Fact]
        public void Iva4CausaleIvaIdTest()
        {
            // TODO unit test for the property 'Iva4CausaleIvaId'
        }

        /// <summary>
        /// Test the property 'Iva4CausaleIva'
        /// </summary>
        [Fact]
        public void Iva4CausaleIvaTest()
        {
            // TODO unit test for the property 'Iva4CausaleIva'
        }

        /// <summary>
        /// Test the property 'TipoDocumento'
        /// </summary>
        [Fact]
        public void TipoDocumentoTest()
        {
            // TODO unit test for the property 'TipoDocumento'
        }

        /// <summary>
        /// Test the property 'DocumentoFiscale'
        /// </summary>
        [Fact]
        public void DocumentoFiscaleTest()
        {
            // TODO unit test for the property 'DocumentoFiscale'
        }

        /// <summary>
        /// Test the property 'DestinazioneDiversa'
        /// </summary>
        [Fact]
        public void DestinazioneDiversaTest()
        {
            // TODO unit test for the property 'DestinazioneDiversa'
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
        /// Test the property 'RitenutaIncludeCassaPrevidenziale'
        /// </summary>
        [Fact]
        public void RitenutaIncludeCassaPrevidenzialeTest()
        {
            // TODO unit test for the property 'RitenutaIncludeCassaPrevidenziale'
        }

        /// <summary>
        /// Test the property 'ClasseDocumento'
        /// </summary>
        [Fact]
        public void ClasseDocumentoTest()
        {
            // TODO unit test for the property 'ClasseDocumento'
        }

        /// <summary>
        /// Test the property 'Contatto'
        /// </summary>
        [Fact]
        public void ContattoTest()
        {
            // TODO unit test for the property 'Contatto'
        }

        /// <summary>
        /// Test the property 'NumeroParteTesto'
        /// </summary>
        [Fact]
        public void NumeroParteTestoTest()
        {
            // TODO unit test for the property 'NumeroParteTesto'
        }

        /// <summary>
        /// Test the property 'NumeroParteNumerica'
        /// </summary>
        [Fact]
        public void NumeroParteNumericaTest()
        {
            // TODO unit test for the property 'NumeroParteNumerica'
        }

        /// <summary>
        /// Test the property 'Data'
        /// </summary>
        [Fact]
        public void DataTest()
        {
            // TODO unit test for the property 'Data'
        }

        /// <summary>
        /// Test the property 'Stato'
        /// </summary>
        [Fact]
        public void StatoTest()
        {
            // TODO unit test for the property 'Stato'
        }

        /// <summary>
        /// Test the property 'DataScadenzaPreventivo'
        /// </summary>
        [Fact]
        public void DataScadenzaPreventivoTest()
        {
            // TODO unit test for the property 'DataScadenzaPreventivo'
        }

        /// <summary>
        /// Test the property 'DataInizioScadenze'
        /// </summary>
        [Fact]
        public void DataInizioScadenzeTest()
        {
            // TODO unit test for the property 'DataInizioScadenze'
        }

        /// <summary>
        /// Test the property 'ScontoPercentuale'
        /// </summary>
        [Fact]
        public void ScontoPercentualeTest()
        {
            // TODO unit test for the property 'ScontoPercentuale'
        }

        /// <summary>
        /// Test the property 'ScontoIncondizionatoImporto'
        /// </summary>
        [Fact]
        public void ScontoIncondizionatoImportoTest()
        {
            // TODO unit test for the property 'ScontoIncondizionatoImporto'
        }

        /// <summary>
        /// Test the property 'AbbuonoImporto'
        /// </summary>
        [Fact]
        public void AbbuonoImportoTest()
        {
            // TODO unit test for the property 'AbbuonoImporto'
        }

        /// <summary>
        /// Test the property 'RitenutaPercentuale'
        /// </summary>
        [Fact]
        public void RitenutaPercentualeTest()
        {
            // TODO unit test for the property 'RitenutaPercentuale'
        }

        /// <summary>
        /// Test the property 'EnasarcoPercentuale'
        /// </summary>
        [Fact]
        public void EnasarcoPercentualeTest()
        {
            // TODO unit test for the property 'EnasarcoPercentuale'
        }

        /// <summary>
        /// Test the property 'EnasarcoImporto'
        /// </summary>
        [Fact]
        public void EnasarcoImportoTest()
        {
            // TODO unit test for the property 'EnasarcoImporto'
        }

        /// <summary>
        /// Test the property 'QuotaRitenutaSuImponibile'
        /// </summary>
        [Fact]
        public void QuotaRitenutaSuImponibileTest()
        {
            // TODO unit test for the property 'QuotaRitenutaSuImponibile'
        }

        /// <summary>
        /// Test the property 'Cambio'
        /// </summary>
        [Fact]
        public void CambioTest()
        {
            // TODO unit test for the property 'Cambio'
        }

        /// <summary>
        /// Test the property 'CassaPrevidenzialeNome'
        /// </summary>
        [Fact]
        public void CassaPrevidenzialeNomeTest()
        {
            // TODO unit test for the property 'CassaPrevidenzialeNome'
        }

        /// <summary>
        /// Test the property 'CassaPrevidenzialePercentuale'
        /// </summary>
        [Fact]
        public void CassaPrevidenzialePercentualeTest()
        {
            // TODO unit test for the property 'CassaPrevidenzialePercentuale'
        }

        /// <summary>
        /// Test the property 'Note'
        /// </summary>
        [Fact]
        public void NoteTest()
        {
            // TODO unit test for the property 'Note'
        }

        /// <summary>
        /// Test the property 'ScontoPagamentoPercentuale'
        /// </summary>
        [Fact]
        public void ScontoPagamentoPercentualeTest()
        {
            // TODO unit test for the property 'ScontoPagamentoPercentuale'
        }

        /// <summary>
        /// Test the property 'Iva1Imponibile'
        /// </summary>
        [Fact]
        public void Iva1ImponibileTest()
        {
            // TODO unit test for the property 'Iva1Imponibile'
        }

        /// <summary>
        /// Test the property 'Iva2Imponibile'
        /// </summary>
        [Fact]
        public void Iva2ImponibileTest()
        {
            // TODO unit test for the property 'Iva2Imponibile'
        }

        /// <summary>
        /// Test the property 'Iva3Imponibile'
        /// </summary>
        [Fact]
        public void Iva3ImponibileTest()
        {
            // TODO unit test for the property 'Iva3Imponibile'
        }

        /// <summary>
        /// Test the property 'Iva4Imponibile'
        /// </summary>
        [Fact]
        public void Iva4ImponibileTest()
        {
            // TODO unit test for the property 'Iva4Imponibile'
        }

        /// <summary>
        /// Test the property 'Iva1ImponibileLordo'
        /// </summary>
        [Fact]
        public void Iva1ImponibileLordoTest()
        {
            // TODO unit test for the property 'Iva1ImponibileLordo'
        }

        /// <summary>
        /// Test the property 'Iva2ImponibileLordo'
        /// </summary>
        [Fact]
        public void Iva2ImponibileLordoTest()
        {
            // TODO unit test for the property 'Iva2ImponibileLordo'
        }

        /// <summary>
        /// Test the property 'Iva3ImponibileLordo'
        /// </summary>
        [Fact]
        public void Iva3ImponibileLordoTest()
        {
            // TODO unit test for the property 'Iva3ImponibileLordo'
        }

        /// <summary>
        /// Test the property 'Iva4ImponibileLordo'
        /// </summary>
        [Fact]
        public void Iva4ImponibileLordoTest()
        {
            // TODO unit test for the property 'Iva4ImponibileLordo'
        }

        /// <summary>
        /// Test the property 'Iva1Imposta'
        /// </summary>
        [Fact]
        public void Iva1ImpostaTest()
        {
            // TODO unit test for the property 'Iva1Imposta'
        }

        /// <summary>
        /// Test the property 'Iva2Imposta'
        /// </summary>
        [Fact]
        public void Iva2ImpostaTest()
        {
            // TODO unit test for the property 'Iva2Imposta'
        }

        /// <summary>
        /// Test the property 'Iva3Imposta'
        /// </summary>
        [Fact]
        public void Iva3ImpostaTest()
        {
            // TODO unit test for the property 'Iva3Imposta'
        }

        /// <summary>
        /// Test the property 'Iva4Imposta'
        /// </summary>
        [Fact]
        public void Iva4ImpostaTest()
        {
            // TODO unit test for the property 'Iva4Imposta'
        }

        /// <summary>
        /// Test the property 'TotaleRitenuta'
        /// </summary>
        [Fact]
        public void TotaleRitenutaTest()
        {
            // TODO unit test for the property 'TotaleRitenuta'
        }

        /// <summary>
        /// Test the property 'TotaleCassaPrevidenziale'
        /// </summary>
        [Fact]
        public void TotaleCassaPrevidenzialeTest()
        {
            // TODO unit test for the property 'TotaleCassaPrevidenziale'
        }

        /// <summary>
        /// Test the property 'TotaleSpese'
        /// </summary>
        [Fact]
        public void TotaleSpeseTest()
        {
            // TODO unit test for the property 'TotaleSpese'
        }

        /// <summary>
        /// Test the property 'TotaleSpeseEsenti'
        /// </summary>
        [Fact]
        public void TotaleSpeseEsentiTest()
        {
            // TODO unit test for the property 'TotaleSpeseEsenti'
        }

        /// <summary>
        /// Test the property 'TotaleSpeseNonEsenti'
        /// </summary>
        [Fact]
        public void TotaleSpeseNonEsentiTest()
        {
            // TODO unit test for the property 'TotaleSpeseNonEsenti'
        }

        /// <summary>
        /// Test the property 'TotaleMerce'
        /// </summary>
        [Fact]
        public void TotaleMerceTest()
        {
            // TODO unit test for the property 'TotaleMerce'
        }

        /// <summary>
        /// Test the property 'TotaleMerceScontata'
        /// </summary>
        [Fact]
        public void TotaleMerceScontataTest()
        {
            // TODO unit test for the property 'TotaleMerceScontata'
        }

        /// <summary>
        /// Test the property 'TotaleImponibile'
        /// </summary>
        [Fact]
        public void TotaleImponibileTest()
        {
            // TODO unit test for the property 'TotaleImponibile'
        }

        /// <summary>
        /// Test the property 'TotaleImposta'
        /// </summary>
        [Fact]
        public void TotaleImpostaTest()
        {
            // TODO unit test for the property 'TotaleImposta'
        }

        /// <summary>
        /// Test the property 'TotaleEsente'
        /// </summary>
        [Fact]
        public void TotaleEsenteTest()
        {
            // TODO unit test for the property 'TotaleEsente'
        }

        /// <summary>
        /// Test the property 'TotaleDocumento'
        /// </summary>
        [Fact]
        public void TotaleDocumentoTest()
        {
            // TODO unit test for the property 'TotaleDocumento'
        }

        /// <summary>
        /// Test the property 'NettoAPagare'
        /// </summary>
        [Fact]
        public void NettoAPagareTest()
        {
            // TODO unit test for the property 'NettoAPagare'
        }

        /// <summary>
        /// Test the property 'TotaleSplitPayment'
        /// </summary>
        [Fact]
        public void TotaleSplitPaymentTest()
        {
            // TODO unit test for the property 'TotaleSplitPayment'
        }

        /// <summary>
        /// Test the property 'Bollo'
        /// </summary>
        [Fact]
        public void BolloTest()
        {
            // TODO unit test for the property 'Bollo'
        }

        /// <summary>
        /// Test the property 'XmlFileName'
        /// </summary>
        [Fact]
        public void XmlFileNameTest()
        {
            // TODO unit test for the property 'XmlFileName'
        }

        /// <summary>
        /// Test the property 'RiferimentoAmministrazione'
        /// </summary>
        [Fact]
        public void RiferimentoAmministrazioneTest()
        {
            // TODO unit test for the property 'RiferimentoAmministrazione'
        }

        /// <summary>
        /// Test the property 'Banca'
        /// </summary>
        [Fact]
        public void BancaTest()
        {
            // TODO unit test for the property 'Banca'
        }

        /// <summary>
        /// Test the property 'DatiTrasporto'
        /// </summary>
        [Fact]
        public void DatiTrasportoTest()
        {
            // TODO unit test for the property 'DatiTrasporto'
        }

        /// <summary>
        /// Test the property 'DatiSdi'
        /// </summary>
        [Fact]
        public void DatiSdiTest()
        {
            // TODO unit test for the property 'DatiSdi'
        }

        /// <summary>
        /// Test the property 'DatiPA'
        /// </summary>
        [Fact]
        public void DatiPATest()
        {
            // TODO unit test for the property 'DatiPA'
        }

        /// <summary>
        /// Test the property 'Righe'
        /// </summary>
        [Fact]
        public void RigheTest()
        {
            // TODO unit test for the property 'Righe'
        }

        /// <summary>
        /// Test the property 'SpeseDocumento'
        /// </summary>
        [Fact]
        public void SpeseDocumentoTest()
        {
            // TODO unit test for the property 'SpeseDocumento'
        }

        /// <summary>
        /// Test the property 'Scadenze'
        /// </summary>
        [Fact]
        public void ScadenzeTest()
        {
            // TODO unit test for the property 'Scadenze'
        }

        /// <summary>
        /// Test the property 'Numero'
        /// </summary>
        [Fact]
        public void NumeroTest()
        {
            // TODO unit test for the property 'Numero'
        }

        /// <summary>
        /// Test the property 'MediaFile'
        /// </summary>
        [Fact]
        public void MediaFileTest()
        {
            // TODO unit test for the property 'MediaFile'
        }

        /// <summary>
        /// Test the property 'Allegato'
        /// </summary>
        [Fact]
        public void AllegatoTest()
        {
            // TODO unit test for the property 'Allegato'
        }

        /// <summary>
        /// Test the property 'BonusFiscaleCausale'
        /// </summary>
        [Fact]
        public void BonusFiscaleCausaleTest()
        {
            // TODO unit test for the property 'BonusFiscaleCausale'
        }

        /// <summary>
        /// Test the property 'BonusFiscaleImporto'
        /// </summary>
        [Fact]
        public void BonusFiscaleImportoTest()
        {
            // TODO unit test for the property 'BonusFiscaleImporto'
        }

        /// <summary>
        /// Test the property 'BonusFiscalePercentuale'
        /// </summary>
        [Fact]
        public void BonusFiscalePercentualeTest()
        {
            // TODO unit test for the property 'BonusFiscalePercentuale'
        }

        /// <summary>
        /// Test the property 'ProgressivoInvio'
        /// </summary>
        [Fact]
        public void ProgressivoInvioTest()
        {
            // TODO unit test for the property 'ProgressivoInvio'
        }
    }
}

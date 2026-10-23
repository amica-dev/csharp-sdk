/*
 * Amica Cloud API
 *
 * L'Amica Cloud API fornisce accesso programmatico a tutti i dati aziendali gestiti da [Amica 20](https://gestionaleamica.com). Supporta il ciclo completo di documenti, fatture, magazzino, contabilità e anagrafiche. Per guide e documentazione completa, visitare [amica.dev](https://amica.dev/).  ## Autenticazione  Tutte le richieste API richiedono un token OAuth 2.0 Bearer valido, da includere nell'header `Authorization`:  ``` Authorization: Bearer <token> ```  ### Registra la tua app  Per integrare l'Amica Cloud API registra la tua applicazione sull'[**App Store di Amica**](https://apps.amica20.it). Al termine della registrazione ottieni una coppia di credenziali OAuth 2.0 — `client_id` e `client_secret` (il secret è mostrato **una sola volta**) — e scegli gli **scope** e il **flusso di autenticazione** desiderati.  ### Scegli il flusso  La scelta dipende da **come** la tua applicazione si autentica:  - C'è un **utente** che può accedere via browser (web app, SPA, app mobile)? → **Authorization Code con PKCE**. - È un **servizio headless** (backend, batch, integrazione server-to-server) di una **singola azienda**, senza un utente che effettua il login? → **Client Credentials**.  #### Authorization Code con PKCE  Consigliato per le app che operano per conto di un utente Amica ed è l'**unico flusso disponibile per le app pubblicate sul marketplace**. Una sola registrazione serve un numero qualsiasi di aziende: ogni utente autorizza la tua app per la propria azienda tramite la pagina di consenso dell'identity provider, e il token riceve automaticamente il contesto di quell'azienda. La tua app riceve un `code` da scambiare con un access token.  ``` GET  {identity-provider}/connect/authorize?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=openid%20read%20write%20documenti&code_challenge=<challenge>&code_challenge_method=S256 POST {identity-provider}/connect/token      grant_type=authorization_code&code=<code>&code_verifier=<verifier>&client_id=<client_id>&client_secret=<client_secret>&redirect_uri=<redirect_uri> ```  #### Client Credentials  Per integrazioni **server-to-server** senza utente interattivo. Disponibile solo per le **app private**, non per quelle pubblicate sul marketplace. Un'app Client Credentials è **legata a una singola azienda** — quella dell'utente che la registra — e il token ne riceve automaticamente il contesto. Per servire un'azienda diversa serve una registrazione separata; se devi distribuire la stessa app a più aziende usa invece **Authorization Code**.  ``` POST {identity-provider}/connect/token Content-Type: application/x-www-form-urlencoded  grant_type=client_credentials&client_id=<client_id>&client_secret=<client_secret>&scope=comune read write documenti ```  > **Importante:** non includere lo scope `openid` (né `profile`/`email`) in questo flusso. `openid` attiva OpenID Connect — richiede un utente che effettua il login e l'emissione di un ID token — cosa che un flusso server-to-server non prevede: la richiesta viene rifiutata con l'errore `Client cannot request OpenID scopes in client credentials flow`. Indica solo gli scope di risorsa (`comune`, `magazzino`, `documenti`, `contabilita`, ed eventualmente `read`/`write`).  In entrambi i casi la risposta contiene l'`access_token` (JWT con `aud: amica-cloud`) da usare nelle chiamate successive finché non scade (`expires_in`), dopodiché va richiesto un nuovo token. Ogni token contiene uno o più **scope** che determinano le aree accessibili (vedi sotto). Dalla [API Reference](https://amica.dev) è possibile incollare un access token per provare gli endpoint direttamente dal browser.  ## Scope di autorizzazione  L'accesso agli endpoint è governato dagli scope presenti nel token OAuth:  | Scope | Descrizione | |-|-| | `comune` | Anagrafiche: contatti, indirizzi, nazioni, valute, modalità di pagamento, ecc. | | `magazzino` | Magazzino: articoli, listini, movimenti di magazzino, giacenze, distinte base. | | `contabilita` | Contabilità: registrazioni contabili, scadenze, castelletti IVA, fatture elettroniche XML. | | `documenti` | Documenti: fatture, note di credito, DDT, ordini, preventivi, proforma ed emissione. |  Una richiesta verso un endpoint non coperto dallo scope del token riceverà una risposta `403 Forbidden`.  ## Company ID  La maggior parte degli endpoint è relativa a una specifica azienda e segue il pattern:  ``` /api/v3/{companyId}/risorsa ```  Il `companyId` è l'identificativo univoco di ciascun database aziendale. Usare l'endpoint `GET /api/v3/config/Azienda` per ottenere l'elenco delle aziende disponibili.  ## Paginazione  Gli endpoint che restituiscono liste supportano la paginazione tramite query parameter:  | Parametro | Descrizione | |-|-| | `Skip` | Numero di elementi da saltare dall'inizio del set di risultati. | | `Take` | Numero di elementi da includere nella risposta. |  La risposta include due header:  | Header | Descrizione | |-|-| | `X-Read-TotalCount` | Numero totale di elementi che soddisfano il filtro corrente. | | `X-Read-Count` | Numero di elementi effettivamente restituiti nel body della risposta. |  ## Ordinamento e filtri  Gli endpoint lista accettano parametri aggiuntivi:  | Parametro | Descrizione | |-|-| | `Order` | Lista di proprietà per l'ordinamento, separate da virgola. Un segno meno (`-`) prima del nome indica ordine discendente. Es. `-RagioneSociale1,Citta`. | | `Filter` | Condizioni di ricerca: lista di espressioni separate da virgola, ciascuna formata da nome campo, operatore, valore ed eventuale connettore logico. Operatori: `$eq`, `$ne`, `$gt`, `$lt`, `$lte`, `$gte`, `$ct` (contiene), `$nct`, `$sw` (inizia con), `$ew` (finisce con). Connettori: `$and` (default, omissibile) e `$or`; le sotto-query si racchiudono tra `[]`. Es. `RagioneSociale1 $ct acme $or, Fido $gt 1000`. | | `SkipTotalCount` | Se `true`, l'header `X-Read-TotalCount` viene omesso dalla risposta: migliori prestazioni a scapito del conteggio totale. Default `false`. |  ## Espansione oggetti correlati  Gli endpoint singoli e lista supportano il parametro `Expand`. Quando fornito, gli oggetti correlati vengono inclusi inline nella risposta anziché restituiti come riferimenti. È possibile specificare espansioni multiple separate da virgola.  ## Gestione errori  L'API usa codici di stato HTTP standard e restituisce [RFC 7807](https://tools.ietf.org/html/rfc7807) Problem Details in caso di errore:  | Codice | Significato | |-|-| | `400` | Bad Request — validazione fallita o parametro obbligatorio mancante. | | `401` | Unauthorized — token mancante o scaduto. | | `403` | Forbidden — accesso negato (vedi sotto le possibili cause). | | `404` | Not Found — la risorsa richiesta non esiste. | | `409` | Conflict — conflitto di concorrenza. | | `422` | Unprocessable Entity — entità non processabile (es. token di concorrenza obsoleto). |  Un `403` può avere tre cause; nelle ultime due il campo `code` della risposta indica quale:  - il token non possiede lo scope richiesto dall'endpoint; - `code: cloud_not_active` — l'account lavora con una connessione locale e la Cloud API non espone i suoi dati. Per attivare il servizio cloud contatta l'assistenza, poi richiedi un nuovo token: lo stato del servizio è letto al rilascio del token; - `code: company_license_not_active` — la licenza dell'azienda indicata nel percorso è scaduta o non è attiva. Le altre aziende con licenza attiva restano accessibili.  Il motivo dell'errore è sempre nel campo `detail`. Ogni risposta di errore riporta inoltre `overridable`: quando vale `true`, l'errore di validazione è *forzabile* e basta ripetere la stessa richiesta aggiungendo `validationOverride=true` alla query string perché il servizio la accetti. È il caso delle anomalie formali su dati reali — la partita IVA di un fornitore estero, per esempio — che il cliente ha comunque bisogno di registrare.  ## Controllo della concorrenza  Le entità contengono una proprietà `ConcurrencyToken`. Quando si aggiorna un'entità tramite `PUT`, il token nel body della richiesta viene confrontato con il valore memorizzato. Se differiscono, viene restituito un `422 Unprocessable Entity`. Usare sempre l'ultimo token ricevuto da una risposta `GET` o `POST`. 
 *
 * The version of the OpenAPI document: 3.3.0
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
    ///  Class for testing Anagrafica
    /// </summary>
    /// <remarks>
    /// This file is automatically generated by OpenAPI Generator (https://openapi-generator.tech).
    /// Please update the test case below to test the model.
    /// </remarks>
    public class AnagraficaTests : IDisposable
    {
        // TODO uncomment below to declare an instance variable for Anagrafica
        //private Anagrafica instance;

        public AnagraficaTests()
        {
            // TODO uncomment below to create an instance of Anagrafica
            //instance = new Anagrafica();
        }

        public void Dispose()
        {
            // Cleanup when everything is done.
        }

        /// <summary>
        /// Test an instance of Anagrafica
        /// </summary>
        [Fact]
        public void AnagraficaInstanceTest()
        {
            // TODO uncomment below to test "IsType" Anagrafica
            //Assert.IsType<Anagrafica>(instance);
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
        /// Test the property 'ModelloId'
        /// </summary>
        [Fact]
        public void ModelloIdTest()
        {
            // TODO unit test for the property 'ModelloId'
        }

        /// <summary>
        /// Test the property 'NazioneId'
        /// </summary>
        [Fact]
        public void NazioneIdTest()
        {
            // TODO unit test for the property 'NazioneId'
        }

        /// <summary>
        /// Test the property 'Nazione'
        /// </summary>
        [Fact]
        public void NazioneTest()
        {
            // TODO unit test for the property 'Nazione'
        }

        /// <summary>
        /// Test the property 'AreaGeograficaId'
        /// </summary>
        [Fact]
        public void AreaGeograficaIdTest()
        {
            // TODO unit test for the property 'AreaGeograficaId'
        }

        /// <summary>
        /// Test the property 'AreaGeografica'
        /// </summary>
        [Fact]
        public void AreaGeograficaTest()
        {
            // TODO unit test for the property 'AreaGeografica'
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
        /// Test the property 'IndirizzoPredefinitoId'
        /// </summary>
        [Fact]
        public void IndirizzoPredefinitoIdTest()
        {
            // TODO unit test for the property 'IndirizzoPredefinitoId'
        }

        /// <summary>
        /// Test the property 'Indirizzi'
        /// </summary>
        [Fact]
        public void IndirizziTest()
        {
            // TODO unit test for the property 'Indirizzi'
        }

        /// <summary>
        /// Test the property 'SchedaCliente'
        /// </summary>
        [Fact]
        public void SchedaClienteTest()
        {
            // TODO unit test for the property 'SchedaCliente'
        }

        /// <summary>
        /// Test the property 'SchedaFornitore'
        /// </summary>
        [Fact]
        public void SchedaFornitoreTest()
        {
            // TODO unit test for the property 'SchedaFornitore'
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
        /// Test the property 'Cliente'
        /// </summary>
        [Fact]
        public void ClienteTest()
        {
            // TODO unit test for the property 'Cliente'
        }

        /// <summary>
        /// Test the property 'Fornitore'
        /// </summary>
        [Fact]
        public void FornitoreTest()
        {
            // TODO unit test for the property 'Fornitore'
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
        /// Test the property 'Vettore'
        /// </summary>
        [Fact]
        public void VettoreTest()
        {
            // TODO unit test for the property 'Vettore'
        }

        /// <summary>
        /// Test the property 'CapoArea'
        /// </summary>
        [Fact]
        public void CapoAreaTest()
        {
            // TODO unit test for the property 'CapoArea'
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
        /// Test the property 'Banca'
        /// </summary>
        [Fact]
        public void BancaTest()
        {
            // TODO unit test for the property 'Banca'
        }

        /// <summary>
        /// Test the property 'Nominativo1'
        /// </summary>
        [Fact]
        public void Nominativo1Test()
        {
            // TODO unit test for the property 'Nominativo1'
        }

        /// <summary>
        /// Test the property 'Nominativo2'
        /// </summary>
        [Fact]
        public void Nominativo2Test()
        {
            // TODO unit test for the property 'Nominativo2'
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
        /// Test the property 'Codice'
        /// </summary>
        [Fact]
        public void CodiceTest()
        {
            // TODO unit test for the property 'Codice'
        }

        /// <summary>
        /// Test the property 'DataInserimento'
        /// </summary>
        [Fact]
        public void DataInserimentoTest()
        {
            // TODO unit test for the property 'DataInserimento'
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
        /// Test the property 'PartitaIva'
        /// </summary>
        [Fact]
        public void PartitaIvaTest()
        {
            // TODO unit test for the property 'PartitaIva'
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
        /// Test the property 'Note'
        /// </summary>
        [Fact]
        public void NoteTest()
        {
            // TODO unit test for the property 'Note'
        }

        /// <summary>
        /// Test the property 'NoteImportanti'
        /// </summary>
        [Fact]
        public void NoteImportantiTest()
        {
            // TODO unit test for the property 'NoteImportanti'
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
        /// Test the property 'CodiceDestinatario'
        /// </summary>
        [Fact]
        public void CodiceDestinatarioTest()
        {
            // TODO unit test for the property 'CodiceDestinatario'
        }

        /// <summary>
        /// Test the property 'DichiarazioneIntento'
        /// </summary>
        [Fact]
        public void DichiarazioneIntentoTest()
        {
            // TODO unit test for the property 'DichiarazioneIntento'
        }

        /// <summary>
        /// Test the property 'RiferimentoAmministrazione'
        /// </summary>
        [Fact]
        public void RiferimentoAmministrazioneTest()
        {
            // TODO unit test for the property 'RiferimentoAmministrazione'
        }
    }
}

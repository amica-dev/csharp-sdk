# Amica.Sdk.Model.Anagrafica
Anagrafica di cliente, fornitore, agente o vettore.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**ModelloId** | **int?** | Identificativo univoco di &#x60;Modello&#x60;. Sempre null. Questa proprietà è usata solo dal client desktop Amica 20 per Windows ed è da considerarsi obsoleta. | [optional] 
**NazioneId** | **int?** | Identificativo univoco di Amica.Anagrafica.Nazione (nazione). | [optional] 
**Nazione** | [**Nazione**](Nazione.md) |  | [optional] 
**AreaGeograficaId** | **int?** | Identificativo univoco di Amica.Anagrafica.AreaGeografica (area geografica). | [optional] 
**AreaGeografica** | [**AreaGeografica**](AreaGeografica.md) |  | [optional] 
**ValutaId** | **int?** | Identificativo univoco di Amica.Anagrafica.Valuta (valuta). | [optional] 
**Valuta** | [**Valuta**](Valuta.md) |  | [optional] 
**IndirizzoPredefinitoId** | **int?** | Identificativo univoco dell&#39;Amica.Indirizzo (indirizzo) predefinito. | [optional] 
**Indirizzi** | [**List&lt;Indirizzo&gt;**](Indirizzo.md) | Elenco degli Amica.Indirizzo dell&#39;anagrafica. | [optional] 
**SchedaCliente** | [**SchedaCliente**](SchedaCliente.md) |  | [optional] 
**SchedaFornitore** | [**SchedaFornitore**](SchedaFornitore.md) |  | [optional] 
**Stato** | **bool** | Indica se l&#39;anagrafica è attiva o meno. | [optional] 
**Cliente** | **bool** | Indica se l&#39;anagrafica è un cliente. | [optional] 
**Fornitore** | **bool** | Indica se l&#39;anagrafica è un fornitore. | [optional] 
**Agente** | **bool** | Indica se l&#39;anagrafica opera come agente di vendita per conto dell&#39;azienda. | [optional] 
**Vettore** | **bool** | Indica se l&#39;anagrafica opera come vettore. | [optional] 
**CapoArea** | **bool** | Indica se l&#39;anagrafica opera come capo area. | [optional] 
**PersonaGiuridica** | **bool** | Indica se l&#39;anagrafica è una persona giuridica o meno. &#x60;true&#x60; indica una persona giuridica, &#x60;false&#x60; una persona fisica. | [optional] 
**Banca** | [**Banca**](Banca.md) |  | [optional] 
**Nominativo1** | [**AnagraficaNominativo1**](AnagraficaNominativo1.md) |  | [optional] 
**Nominativo2** | [**AnagraficaNominativo2**](AnagraficaNominativo2.md) |  | [optional] 
**SedeLegale** | [**AnagraficaSedeLegale**](AnagraficaSedeLegale.md) |  | [optional] 
**Codice** | **string** | Codice assegnato all&#39;anagrafica. | [optional] 
**DataInserimento** | **DateTime?** | Data di primo inserimento. | [optional] 
**RagioneSociale1** | **string** | Ragione sociale, oppure nome e cognome della persona fisica. | 
**RagioneSociale2** | **string** | Eventuale seconda riga della ragione sociale. Usata principalmente nelle stampe, quando si vuole suddividere la ragione sociale su due righe. | [optional] 
**EMail** | **string** | Indirizzo email. | [optional] 
**SitoWeb** | **string** | Indirizzo web. | [optional] 
**PartitaIva** | **string** | Partita IVA. | [optional] 
**CodiceFiscale** | **string** | Codice fiscale. Solitamente diverso dalla Amica.Anagrafica.PartitaIva. | [optional] 
**Note** | **string** | Note. | [optional] 
**NoteImportanti** | **string** | Note rilevanti. I client ufficiali Amica le evidenziano alla creazione di un nuovo documento fiscale. | [optional] 
**Pec** | **string** | Indirizzo Pec (posta elettronica certificata). | [optional] 
**CodiceDestinatario** | **string** | Codice destinatario per il sistema fiscale italiano. Usato per il recapito delle fatture elettroniche italiane. | [optional] 
**DichiarazioneIntento** | **string** | Numero di protocollo della dichiarazione d&#39;intento. | [optional] 
**RiferimentoAmministrazione** | **string** | Solitamente necessario quando si ha a che fare con enti della Pubblica Amministrazione italiana. Identifica univocamente l&#39;ufficio di destinazione all&#39;interno della PA. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


# Amica.Sdk.Model.Indirizzo
Indirizzo.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**AnagraficaId** | **int** | Identificativo univoco di &#x60;Anagrafica&#x60;. | [optional] 
**RagioneSociale1** | **string** | Ragione sociale, oppure nome e cognome della persona fisica. | [optional] 
**RagioneSociale2** | **string** | Eventuale seconda riga della ragione sociale. | [optional] 
**Via** | **string** | Indirizzo (via). | [optional] 
**Localit** | **string** | Località. | [optional] 
**Cap** | **string** | CAP o codice di avviamento postale. | [optional] 
**Provincia** | **string** | Provincia, contea o nome dell&#39;area. | [optional] 
**Telefono1** | **string** | Numero di telefono principale. | [optional] 
**Telefono2** | **string** | Numero di telefono secondario. | [optional] 
**Fax** | **string** | Numero di fax. | [optional] 
**Email** | **string** | Indirizzo email. | [optional] 
**Stato** | **bool** | Indica se l&#39;indirizzo è attivo oppure no. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


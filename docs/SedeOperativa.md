# Amica.Sdk.Model.SedeOperativa
Sede operativa.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**Indirizzo** | **string** | Indirizzo. | [optional] 
**Localit** | **string** | Città, comune o altro nome di località. | [optional] 
**Cap** | **string** | CAP o codice di zona. | [optional] 
**Provincia** | **string** | Provincia o nome dell&#39;area. | [optional] 
**Telefono1** | **string** | Numero di telefono principale. | [optional] 
**Telefono2** | **string** | Numero di telefono secondario. | [optional] 
**Fax** | **string** | Numero di fax. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


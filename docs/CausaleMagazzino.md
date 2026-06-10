# Amica.Sdk.Model.CausaleMagazzino
Causale di un movimento di magazzino.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**Nome** | **string** | Nome o descrizione. | 
**Codice** | **string** | Codice o identificativo univoco. | 
**MovimentoDiCarico** | **bool** | Indica se l&#39;utilizzo causa la creazione di un movimento di carico. | [optional] 
**TipoCausaleMagazzino** | **string** | Tipologia del movimento di magazzino. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


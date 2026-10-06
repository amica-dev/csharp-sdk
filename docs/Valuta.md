# Amica.Sdk.Model.Valuta
Valuta.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**Nome** | **string** | Nome. | 
**Sigla** | **string** | Sigla (es. \&quot;EUR\&quot;). | 
**Cambio** | **double** | Tasso di cambio rispetto all&#39;Euro. | [optional] 
**Predefinita** | **bool** | Indica se questa è la valuta predefinita o meno. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


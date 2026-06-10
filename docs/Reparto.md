# Amica.Sdk.Model.Reparto
Reparto.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**CausaleIvaId** | **int** | Identificativo univoco di Amica.Reparto.CausaleIva. | [optional] 
**CausaleIva** | [**CausaleIva**](CausaleIva.md) |  | [optional] 
**Numero** | **int** | Numero del reparto. | [optional] 
**Prezzo** | **double** | Prezzo associato al reparto. | [optional] 
**Sconto** | **double** | Sconto riconosciuto all&#39;utilizzo del reparto (percentuale). | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


# Amica.Sdk.Model.Componente
Componente (distinta base). Può essere incluso in `DistintaBase`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**ArticoloId** | **int** | Identificativo univoco dell&#39;&#x60;Articolo&#x60; da comporre. | [optional] 
**Articolo** | [**Articolo**](Articolo.md) |  | [optional] 
**ArticoloComponenteId** | **int** | Identificativo univoco del componente (distinta base). | [optional] 
**ArticoloComponente** | [**Articolo**](Articolo.md) |  | [optional] 
**QuantitaNecessaria** | **double** | Quantità necessaria per l&#39;assemblaggio. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


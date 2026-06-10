# Amica.Sdk.Model.ArticoloMediaFile
Rappresentazione di un file multimediale associato al prodotto o articolo.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**Data** | **string** | Rappresentazione in Base64 del contenuto di un file multimediale. | [optional] 
**ArticoloId** | **int** | Identificativo univoco di Amica.Articolo. | [optional] 
**FieldName** | **string** | Nome (tipo) dell&#39;informazione. Solitamente &#39;immagine&#39;. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


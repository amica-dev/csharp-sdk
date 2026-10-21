# Amica.Sdk.Model.ArticoloColore
Colore disponibile per un articolo gestito a taglie e colori. Alimenta la scelta del colore nelle righe di documenti e movimenti merce, dove il colore resta comunque testo libero.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**ArticoloId** | **int** | Identificativo univoco di &#x60;Articolo&#x60;. | [optional] 
**Articolo** | [**Articolo**](Articolo.md) |  | [optional] 
**Colore** | **string** | Nome del colore. Il confronto ignora maiuscole e minuscole: &#x60;Rosso&#x60; e &#x60;rosso&#x60; sono lo stesso colore. | 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


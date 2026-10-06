# Amica.Sdk.Model.InventarioRiga
Riga di un inventario: quantità e valore di un articolo, per variante (identificativo, colore, data).

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**InventarioId** | **int?** | Identificativo univoco di &#x60;Inventario&#x60;. | [optional] 
**ArticoloId** | **int?** | Identificativo univoco di &#x60;Articolo&#x60;. | [optional] 
**Articolo** | [**Articolo**](Articolo.md) |  | [optional] 
**Quantit** | **double?** | Quantità inventariata. | [optional] 
**Valore** | **double?** | Valore totale della riga: quantità per costo unitario. | [optional] 
**Identificativo** | **string** | Numero di serie o altro identificativo del prodotto. | [optional] 
**IdentificativoEsteso** | **string** | Identificativo esteso. | [optional] 
**Data** | **DateTime?** | Data di scadenza, produzione, fine garanzia o altro. | [optional] 
**Colore** | **string** | Colore. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


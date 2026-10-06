# Amica.Sdk.Model.Inventario
Inventario di magazzino.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**MagazzinoId** | **int** | Identificativo univoco di &#x60;Magazzino&#x60;. Obbligatorio in scrittura; può mancare negli inventari creati dal client Windows, che non si possono elaborare. | 
**Magazzino** | [**Magazzino**](Magazzino.md) |  | [optional] 
**Data** | **DateTime** | Data dell&#39;inventario: l&#39;elaborazione considera i movimenti fino a questa data inclusa. | 
**Nome** | **string** | Nome o descrizione. | [optional] 
**Fiscale** | **bool?** | Indica se l&#39;inventario è fiscale. L&#39;elaborazione parte dall&#39;inventario fiscale precedente dello stesso magazzino. | [optional] 
**Valorizzazione** | **string** | Criterio di valorizzazione delle righe. | 
**EscludiArticoliInattivi** | **bool?** | Indica se l&#39;elaborazione deve escludere gli articoli non attivi. | [optional] 
**Note** | **string** | Note. | [optional] 
**Righe** | [**List&lt;InventarioRiga&gt;**](InventarioRiga.md) | Righe dell&#39;inventario. In sola lettura: le produce l&#39;elaborazione (&#x60;Process&#x60;). | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


# Amica.Sdk.Model.InventarioComputeResult
Restituito dai metodi `Compute` e `Process` di `InventarioService`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Righe** | [**List&lt;InventarioRiga&gt;**](InventarioRiga.md) | Righe dell&#39;inventario elaborato. | [optional] 
**Avvisi** | **List&lt;string&gt;** | Avvisi dell&#39;elaborazione, da mostrare all&#39;utente. | [optional] 
**ConcurrencyToken** | **int** | Impronta delle righe elaborate. Passata a &#x60;Process&#x60;, impedisce il salvataggio se nel frattempo i dati sono cambiati e l&#39;elaborazione darebbe righe diverse. | [optional] 
**GiacenzeAllineabili** | **bool** | Indica se &#x60;Process&#x60; con &#x60;alignStock&#x60; allineerebbe davvero le giacenze: non le allinea se nel magazzino ci sono movimenti successivi alla data dell&#39;inventario. | [optional] 
**Inventario** | [**Inventario**](Inventario.md) |  | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


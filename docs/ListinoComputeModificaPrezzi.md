# Amica.Sdk.Model.ListinoComputeModificaPrezzi
Parametri della procedura `ModificaPrezzi`. Va indicato uno solo fra `Percentuale` e `Importo`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Operazione** | **string** | Se aumentare o diminuire i prezzi. | [optional] 
**Percentuale** | **double?** | Variazione percentuale di ogni prezzo, come frazione: &#x60;0.05&#x60; corrisponde al 5%. Il risultato è arrotondato ai decimali dei prezzi unitari delle preferenze. | [optional] 
**Importo** | **double?** | Importo fisso da aggiungere o togliere a ogni prezzo. Il risultato non è arrotondato. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


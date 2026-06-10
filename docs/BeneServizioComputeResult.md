# Amica.Sdk.Model.BeneServizioComputeResult
Restituito dal metodo M:Amica.BeneServizioService.Compute(Amica.BeneServizioComputeData,Amica.RequestSettings) di `BeneServizioServizio`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Prezzo** | **double** | Prezzo IVA esclusa. | [optional] 
**PrezzoIvato** | **double** | Prezzo IVA inclusa. | [optional] 
**PrezzoScontato** | **double** | Prezzo scontato, ovvero al netto degli sconti percentuali indicati e dello sconto incondizionato. | [optional] 
**Importo** | **double?** | Importo, determinato come prodotto tra Amica.BeneServizio.Quantità e Amica.BeneServizio.PrezzoScontato. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


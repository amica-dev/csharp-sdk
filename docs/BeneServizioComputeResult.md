# Amica.Sdk.Model.BeneServizioComputeResult
Restituito dal metodo `Compute` di `BeneServizioServizio`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Prezzo** | **double** | Prezzo IVA esclusa. | [optional] 
**PrezzoIvato** | **double** | Prezzo IVA inclusa. | [optional] 
**PrezzoScontato** | **double** | Prezzo scontato, ovvero al netto degli sconti percentuali indicati e dello sconto incondizionato. | [optional] 
**Importo** | **double?** | Importo, determinato come prodotto tra &#x60;Quantità&#x60; e &#x60;PrezzoScontato&#x60;. | [optional] 
**ImportoIvato** | **double?** | Importo IVA inclusa. Con IVA inclusa è la base dell&#39;imposta del riepilogo: va passato nella riga di &#x60;documento/compute&#x60; (&#x60;ImportoIvato&#x60;). | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


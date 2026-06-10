# Amica.Sdk.Model.BeneServizioComputeData
Utilizzato dal metodo di calcolo M:Amica.BeneServizioService.Compute(Amica.BeneServizioComputeData,Amica.RequestSettings) di `BeneServizioService`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**IvaEsclusa** | **bool** | Indica se il calcolo debba essere effettuato a partire da Amica.BeneServizioComputeData.Prezzo o da Amica.BeneServizioComputeData.PrezzoIvato. | [optional] 
**CausaleIvaId** | **int?** | Identificativo univoco di Amica.CausaleIva. | [optional] 
**ListinoId** | **int?** | Identificativo univoco di Amica.Listino. | [optional] 
**Quantit** | **double** | Quantità. | [optional] 
**Prezzo** | **double** | Prezzo IVA esclusa. | [optional] 
**PrezzoIvato** | **double** | Prezzo IVA inclusa. | [optional] 
**Sconto1** | **double** | Primo sconto percentuale, applicato al prezzo del bene o servizio. | [optional] 
**Sconto2** | **double** | Secondo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto1 | [optional] 
**Sconto3** | **double** | Terzo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto2 | [optional] 
**Sconto4** | **double** | Quarto sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizioComputeData.Sconto3 | [optional] 
**ScontoIncondizionato** | **double** | Sconto incondizionato, in valuta, applicato al prezzo del bene o servizio. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


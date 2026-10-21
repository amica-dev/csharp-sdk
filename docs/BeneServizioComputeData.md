# Amica.Sdk.Model.BeneServizioComputeData
Utilizzato dal metodo di calcolo `Compute` di `BeneServizioService`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**IvaEsclusa** | **bool** | Indica se il calcolo debba essere effettuato a partire da &#x60;Prezzo&#x60; o da &#x60;PrezzoIvato&#x60;. | [optional] 
**CausaleIvaId** | **int?** | Identificativo univoco di &#x60;CausaleIva&#x60;. | [optional] 
**ListinoId** | **int?** | Identificativo univoco di &#x60;Listino&#x60;. | [optional] 
**Quantit** | **double** | Quantità. | [optional] 
**Prezzo** | **double** | Prezzo IVA esclusa. | [optional] 
**PrezzoIvato** | **double** | Prezzo IVA inclusa. | [optional] 
**Sconto1** | **double** | Primo sconto percentuale, applicato al prezzo del bene o servizio. Il valore è una frazione: &#x60;0.05&#x60; corrisponde al 5%. | [optional] 
**Sconto2** | **double** | Secondo sconto percentuale, applicato dopo l&#39;applicazione di &#x60;Sconto1&#x60; Il valore è una frazione: &#x60;0.05&#x60; corrisponde al 5%. | [optional] 
**Sconto3** | **double** | Terzo sconto percentuale, applicato dopo l&#39;applicazione di &#x60;Sconto2&#x60; Il valore è una frazione: &#x60;0.05&#x60; corrisponde al 5%. | [optional] 
**Sconto4** | **double** | Quarto sconto percentuale, applicato dopo l&#39;applicazione di &#x60;Sconto3&#x60; Il valore è una frazione: &#x60;0.05&#x60; corrisponde al 5%. | [optional] 
**ScontoIncondizionato** | **double** | Sconto incondizionato, in valuta, applicato al prezzo del bene o servizio. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


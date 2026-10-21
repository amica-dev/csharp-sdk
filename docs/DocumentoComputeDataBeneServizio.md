# Amica.Sdk.Model.DocumentoComputeDataBeneServizio
Metadati della riga di documento.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**RitenutaFiscale** | **bool** | Indica se la ritenuta fiscale è applicabile o meno. | [optional] 
**Importo** | **double?** | Importo. | [optional] 
**ImportoIvato** | **double?** | Importo IVA inclusa della riga, come restituito da &#x60;beneservizio/compute&#x60;. Serve con IVA inclusa (&#x60;IvaEsclusa&#x60; a &#x60;false&#x60;): l&#39;imposta del riepilogo è la differenza fra ivato e imponibile. Se non indicato si ricava da importo e aliquota. | [optional] 
**CausaleIvaId** | **int?** | Identificativo univoco del codice IVA. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


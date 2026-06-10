# Amica.Sdk.Model.FatturaElettronicaImportResult
Esito della chiamata al metodo M:Amica.FatturaElettronicaXmlService.XmlImport(Amica.FatturaElettronicaImportOptions,System.Byte[],System.String,Amica.RequestSettings) di `FatturaElettronicaXmlService`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**DocumentoId** | **int?** | Identificativo univoco di Amica.Documento. | [optional] 
**Messaggi** | **List&lt;string&gt;** | Messaggi relativi alla procedura di importazione. | [optional] 
**Esito** | **string** | Esito dell&#39;importazione (&#x60;OK&#x60;, &#x60;OK con messaggi&#x60;, &#x60;SCARTATO (DUPLICATO)&#x60;, ecc.) | [optional] 
**IsSuccess** | **bool** | Indica se l&#39;importazione è andata a buon fine. | [optional] [readonly] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


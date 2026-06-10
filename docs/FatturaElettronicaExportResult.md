# Amica.Sdk.Model.FatturaElettronicaExportResult
Esito della chiamata al metodo M:Amica.FatturaElettronicaXmlService.XmlExport(Amica.FatturaElettronicaExportOptions,Amica.RequestSettings) di `FatturaElettronicaXmlService`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**FileName** | **string** | Nome del file. | [optional] 
**Valido** | **bool** | Indica se la fattura elettronica (XML) generata ha superato il test di validazione secondo le specifiche del Sistema di Interscambio (SdI). Se &#x60;false&#x60;, vedi Amica.FatturaElettronicaExportResult.Errori per l&#39;elenco degli errori di validazione. | [optional] [readonly] 
**Errori** | **List&lt;string&gt;** | Errori di validazione, se presenti. | [optional] 
**Xml** | **string** | Contenuto del file. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


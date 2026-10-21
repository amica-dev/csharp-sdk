# Amica.Sdk.Model.DdtImportData
Dati di importazione del DDT.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **List&lt;int&gt;** | Identificativo del documento da importare. | [optional] 
**TipoImportazione** | **string** | Indica se importare i totali del documento o tutti i dati, e se gli articoli dello stesso tipo debbano essere accorpati. | [optional] 
**SpeseIncluse** | **bool** | Indica se importare nella fattura le spese dei DDT, in tutte le modalità di importazione. Le spese dello stesso tipo si sommano, anche a quelle che la fattura ha già. Se &#x60;false&#x60;, le spese dei DDT non vengono importate. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


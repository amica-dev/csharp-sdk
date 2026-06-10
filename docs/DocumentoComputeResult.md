# Amica.Sdk.Model.DocumentoComputeResult
Restituito dal metodo M:Amica.DocumentoService.Compute(Amica.DocumentoComputeData,Amica.RequestSettings,System.Boolean) di `DocumentoService`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**TotaleMerce** | **double** | Importo della merce. | [optional] 
**TotaleMerceScontata** | **double** | Importo della merce scontata. | [optional] 
**TotaleImponibile** | **double** | Totale imponibile, considerando tutte le causali IVA coinvolte. | [optional] 
**TotaleSpeseEsenti** | **double** | Totale delle spese esenti da IVA. | [optional] 
**TotaleSpeseNonEsenti** | **double** | Totale delle spese non esenti da IVA. | [optional] 
**TotaleSpese** | **double** | Totale delle spese esenti e non esenti. | [optional] 
**TotaleRitenuta** | **double** | Importo della ritenuta di acconto. | [optional] 
**TotaleCassaPrevidenziale** | **double** | Importo della cassa previdenziale. | [optional] 
**EnasarcoImporto** | **double** | Importo Enasarco. | [optional] 
**TotaleEsente** | **double** | Importi esenti da IVA. | [optional] 
**TotaleImposta** | **double** | Imposta IVA. | [optional] 
**TotaleSplitPayment** | **double** | Importo dello split payment. | [optional] 
**TotaleAbbuoni** | **double** | Importo degli abbuoni. | [optional] 
**TotaleDocumento** | **double** | Importo totale del documento. | [optional] 
**BonusFiscaleImporto** | **double** | Importo del bonus fiscale. | [optional] 
**NettoAPagare** | **double** | Netto a pagare. | [optional] 
**RiepiloghiIva** | [**List&lt;DocumentoComputeResultRiepilogoIva&gt;**](DocumentoComputeResultRiepilogoIva.md) | Riepiloghi IVA. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


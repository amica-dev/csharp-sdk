# Amica.Sdk.Model.DocumentoComputeData
Utilizzato dal metodo M:Amica.DocumentoService.Compute(Amica.DocumentoComputeData,Amica.RequestSettings,System.Boolean) di `DocumentoService`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Righe** | [**List&lt;DocumentoComputeDataBeneServizio&gt;**](DocumentoComputeDataBeneServizio.md) | Metadati delle righe del documento. | [optional] 
**SpeseDocumento** | [**List&lt;DocumentoComputeDataSpesa&gt;**](DocumentoComputeDataSpesa.md) | Metadati dell&#39;elenco delle spese. | [optional] 
**ScontoPercentuale** | **double** | Sconto, in percentuale, da calcolare sull&#39;intero importo del documento. | [optional] 
**ScontoPagamentoPercentuale** | **double** | Sconto riconosciuto in base alla modalità di pagamento utilizzata (es.: +5% in caso di rimessa diretta). | [optional] 
**ScontoIncondizionatoImporto** | **double** | Sconto, in valuta, da applicare all&#39;intero importo del documento. | [optional] 
**CassaPrevidenzialePercentuale** | **double** | Contributo cassa previdenziale (percentuale) da applicare al documento. | [optional] 
**CassaPrevidenzialeCausaleIvaId** | **int?** | Identificativo univoco della causale IVA della cassa previdenziale. | [optional] 
**RitenutaIncludeCassaPrevidenziale** | **bool** | Indica se la ritenuta debba essere applicata all&#39;importo della cassa previdenziale riportato nel documento. | [optional] 
**QuotaRitenutaSuImponibile** | **double** | Quota dell&#39;imponibile su cui applicare la ritenuta. | [optional] 
**RitenutaPercentuale** | **double** | Quota di ritenuta da applicare al documento. | [optional] 
**EnasarcoImporto** | **double** | Importo Enasarco. | [optional] 
**AbbuonoImporto** | **double** | Importo dell&#39;abbuono. | [optional] 
**BonusFiscalePercentuale** | **double** | Importo del bonus fiscale. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


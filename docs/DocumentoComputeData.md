# Amica.Sdk.Model.DocumentoComputeData
Utilizzato dal metodo `Compute` di `DocumentoService`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**IvaEsclusa** | **bool** | Indica se i prezzi del documento sono IVA esclusa. Con IVA inclusa l&#39;imposta di ogni riepilogo si ricava dall&#39;importo ivato delle righe (&#x60;ImportoIvato&#x60;), come nel client Windows. Se non indicato vale &#x60;true&#x60;. | [optional] 
**Righe** | [**List&lt;DocumentoComputeDataBeneServizio&gt;**](DocumentoComputeDataBeneServizio.md) | Metadati delle righe del documento. | [optional] 
**SpeseDocumento** | [**List&lt;DocumentoComputeDataSpesa&gt;**](DocumentoComputeDataSpesa.md) | Metadati dell&#39;elenco delle spese. | [optional] 
**ScontoPercentuale** | **double** | Sconto, in percentuale, da calcolare sull&#39;intero importo del documento. Il valore è una frazione: &#x60;0.05&#x60; corrisponde al 5%. | [optional] 
**ScontoPagamentoPercentuale** | **double** | Sconto riconosciuto in base alla modalità di pagamento utilizzata (es.: +5% in caso di rimessa diretta). Il valore è una frazione: &#x60;0.05&#x60; corrisponde al 5%. | [optional] 
**ScontoIncondizionatoImporto** | **double** | Sconto, in valuta, da applicare all&#39;intero importo del documento. | [optional] 
**CassaPrevidenzialePercentuale** | **double** | Contributo cassa previdenziale (percentuale) da applicare al documento. Il valore è una frazione: &#x60;0.04&#x60; corrisponde al 4%. | [optional] 
**CassaPrevidenzialeCausaleIvaId** | **int?** | Identificativo univoco della causale IVA della cassa previdenziale. | [optional] 
**RitenutaIncludeCassaPrevidenziale** | **bool** | Indica se la ritenuta debba essere applicata all&#39;importo della cassa previdenziale riportato nel documento. | [optional] 
**QuotaRitenutaSuImponibile** | **double** | Quota dell&#39;imponibile su cui applicare la ritenuta. Il valore è una frazione: &#x60;1.00&#x60; corrisponde al 100%. | [optional] 
**RitenutaPercentuale** | **double** | Quota di ritenuta da applicare al documento. Il valore è una frazione: &#x60;0.20&#x60; corrisponde al 20%. | [optional] 
**EnasarcoImporto** | **double** | Importo Enasarco. | [optional] 
**AbbuonoImporto** | **double** | Importo dell&#39;abbuono. | [optional] 
**BonusFiscalePercentuale** | **double** | Importo del bonus fiscale. Il valore è una frazione: &#x60;0.05&#x60; corrisponde al 5%. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


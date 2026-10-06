# Amica.Sdk.Model.DocumentoTotali
Totali di un gruppo di documenti, calcolati dal server. Le note di credito sono sottratte.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Periodo** | **DateTime?** | Primo giorno del mese o dell&#39;anno del gruppo, se il raggruppamento è per periodo. | [optional] 
**AnagraficaId** | **int?** | Identificativo univoco dell&#39;&#x60;Anagrafica&#x60; del gruppo, se il raggruppamento è per anagrafica. | [optional] 
**RagioneSociale** | **string** | Ragione sociale dell&#39;&#x60;Anagrafica&#x60; del gruppo, se il raggruppamento è per anagrafica. | [optional] 
**Documenti** | **int** | Numero di documenti del gruppo. | [optional] 
**TotaleImponibile** | **double** | Somma degli imponibili. | [optional] 
**TotaleEsente** | **double** | Somma degli importi esenti. | [optional] 
**TotaleImposta** | **double** | Somma delle imposte. | [optional] 
**TotaleDocumento** | **double** | Somma dei totali documento. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


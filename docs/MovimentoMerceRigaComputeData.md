# Amica.Sdk.Model.MovimentoMerceRigaComputeData
Utilizzato dal metodo di calcolo `ComputeRiga` di `MovimentoMerceService`. Contiene la riga da completare, i dati della testata del movimento che servono al calcolo e il campo della riga appena modificato. Il movimento può non essere ancora salvato.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Riga** | [**MovimentoMerceRiga**](MovimentoMerceRiga.md) |  | [optional] 
**Modificato** | **string** | Il campo della riga appena modificato, che decide cosa ricalcolare. Se non indicato si ricava il prezzo non modificabile da quello modificabile, come per &#x60;CausaleIva&#x60;. | [optional] 
**IvaEsclusa** | **bool** | &#x60;IvaEsclusa&#x60; della testata. Con IVA esclusa si scrive il prezzo netto e si             ricava quello ivato, con IVA inclusa il contrario. | [optional] 
**AnagraficaId** | **int?** | &#x60;AnagraficaId&#x60; della testata, se già scelta. Alla scelta dell&#39;articolo ne             determina listino e causale IVA. | [optional] 
**CausaleMagazzinoId** | **int?** | &#x60;CausaleMagazzinoId&#x60; della testata, se già scelta. Dice se il movimento è un             carico, e quindi se il listino dell&#39;anagrafica è quello del fornitore o del cliente. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


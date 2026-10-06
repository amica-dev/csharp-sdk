# Amica.Sdk.Model.MovimentoMerce
Movimento di magazzino.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**ModalitaPagamentoId** | **int?** | Identificativo univoco di &#x60;ModalitaPagamento&#x60;. | [optional] 
**ModalitaPagamento** | [**ModalitaPagamento**](ModalitaPagamento.md) |  | [optional] 
**CausaleMagazzinoId** | **int** | Identificativo univoco di &#x60;CausaleMagazzino&#x60;. | [optional] 
**CausaleMagazzino** | [**CausaleMagazzino**](CausaleMagazzino.md) |  | [optional] 
**AnagraficaId** | **int?** | Identificativo univoco di &#x60;Anagrafica&#x60;. | [optional] 
**Anagrafica** | [**Anagrafica**](Anagrafica.md) |  | [optional] 
**DocumentoId** | **int?** | Identificativo univoco di &#x60;Documento&#x60;. | [optional] 
**IvaEsclusa** | **bool** | Indica se i prezzi inseriti sono da considerarsi al netto dell&#39;IVA. Se &#x60;false&#x60;, l&#39;IVA verrà scorporata in fase di elaborazione (app ufficiali). | [optional] 
**Data** | **DateTime** | Data del movimento. | [optional] 
**Descrizione** | **string** | Descrizione. | [optional] 
**DocumentoNumero** | **string** | Numero del documento di riferimento. | [optional] 
**DocumentoData** | **DateTime?** | Data del documento di riferimento. | [optional] 
**Note** | **string** | Note. | [optional] 
**Righe** | [**List&lt;MovimentoMerceRiga&gt;**](MovimentoMerceRiga.md) | Righe del movimento. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


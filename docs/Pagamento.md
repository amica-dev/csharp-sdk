# Amica.Sdk.Model.Pagamento
Pagamento.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**ModalitaPagamentoId** | **int?** | Identificativo univoco di &#x60;ModalitaPagamento&#x60;. | [optional] 
**ModalitaPagamento** | [**ModalitaPagamento**](ModalitaPagamento.md) |  | [optional] 
**SpesaId** | **int?** | Identificativo univoco di &#x60;Spesa&#x60;. | [optional] 
**Spesa** | [**Spesa**](Spesa.md) |  | [optional] 
**RisorsaFinanziariaId** | **int?** | Identificativo univoco di &#x60;RisorsaFinanziaria&#x60;. | [optional] 
**RisorsaFinanziaria** | [**RisorsaFinanziaria**](RisorsaFinanziaria.md) |  | [optional] 
**Nome** | **string** | Nome, o descrizione. | 
**CalcoloPrimaRata** | **string** | Se e come l&#39;IVA e le spese devono essere considerate nel calcolo della prima rata. | [optional] 
**DecorrenzaPrimaRata** | **string** | Da quando decorre il pagamento della prima rata. | [optional] 
**GiorniAggiuntivi** | **int** | Giorni extra da aggiungere ad ogni scadenza. | [optional] 
**Sconto** | **double** | Sconto riconosciuto per l&#39;utilizzo del pagamento (percentuale). | [optional] 
**InizioScadenze** | **int** | Periodicità della prima rata. Per le successive, impostare &#x60;Periodicità&#x60;. | [optional] 
**Periodicit** | **int** | Periodicità delle rate successive alla prima. Per la prima rata, impostare &#x60;InizioScadenze&#x60;. Default impostato a 30. | [optional] 
**FineMese** | **bool** | Indica se le scadenze devono essere riportate a fine mese. | [optional] 
**GiorniEsatti** | **bool** | Indica se il pagamento è utilizzato nel caso di &#39;Art. 62 prodotti alimentari (giorni esatti)&#39;. | [optional] 
**Rate** | **int** | Numero di rate. | [optional] 
**Note** | **string** | Note. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


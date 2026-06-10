# Amica.Sdk.Model.SpesaDocumento
Spesa di documento.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**DocumentoId** | **int** | Identificativo univoco di Amica.Documento. | [optional] 
**SpesaId** | **int** | Identificativo univoco di Amica.SpesaDocumento.Spesa. | [optional] 
**Spesa** | [**Spesa**](Spesa.md) |  | [optional] 
**CausaleIvaId** | **int** | Identificativo univoco di Amica.SpesaDocumento.CausaleIva. | [optional] 
**CausaleIva** | [**CausaleIva**](CausaleIva.md) |  | [optional] 
**Importo** | **double** | Importo. | [optional] 
**Pagamento** | **bool** | Indica se è un pagamento o meno. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


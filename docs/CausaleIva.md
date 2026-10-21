# Amica.Sdk.Model.CausaleIva
Causale IVA.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**Codice** | **string** | Codice. | 
**Nome** | **string** | Nome, o descrizione. | 
**Aliquota** | **double** | Aliquota applicabile. Il valore è una frazione: &#x60;0.22&#x60; corrisponde al 22%. | [optional] 
**Indeducibilit** | **double** | Quota di indeducibilità. | [optional] 
**Intracomunitaria** | **bool** | Indica se l&#39;IVA è intracomunitaria. | [optional] 
**NonImponibile** | **bool** | Indica se l&#39;IVA non deve essere considerata nella determinazione dell&#39;imponibile. | [optional] 
**Natura** | **string** | Natura dell&#39;esenzione (codice), se applicabile (ad es. \&quot;N1\&quot;). | [optional] 
**SplitPayment** | **bool** | Indica se si applica lo split payment o meno. | [optional] 
**FatturaElettronica** | **bool** | Indica se l&#39;IVA è stata creata automaticamente durante l&#39;importazione di una fattura elettronica. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


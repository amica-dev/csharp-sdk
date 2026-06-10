# Amica.Sdk.Model.Evento
Un evento.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**EventGuid** | **Guid** | GUID dell&#39;evento. | [optional] 
**Type** | **string** | Tipo di evento. | [optional] 
**CompanyId** | **string** | Identificativo dell&#39;azienda per cui l&#39;evento è stato scatenato. | [optional] 
**DateTime** | **DateTime** | Data e ora in cui l&#39;evento si è verificato. | [optional] 
**ApiVersion** | **string** | Versione dell&#39;API. | [optional] 
**ApplicationId** | **string** | Application ID del client. Vedi &lt;a href&#x3D;\&quot;https://amica.dev/api/required-headers/\&quot;&gt;required headers&lt;/a&gt; per maggiori informazioni. | [optional] 
**Object** | **Object** | Payload. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


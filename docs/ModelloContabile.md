# Amica.Sdk.Model.ModelloContabile
Modello contabile.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**Codice** | **string** | Codice del modello. | 
**Nome** | **string** | Nome. | 
**Stato** | **bool** | Indica se il modello è attivo o meno. | [optional] 
**RegistroIvaId** | **int?** | Identificativo univoco di &#x60;RegistroIva&#x60;. | [optional] 
**SegnoIva** | **string** | Positività o negatività dell&#39;IVA. | [optional] 
**TipoOperazioneContabile** | **string** | Tipo di operazione contabile. | [optional] 
**TipoRegistrazioneContabile** | **string** | Tipo di registrazione contabile. | [optional] 
**AzionePartita** | **string** | Effetto sulla partita aperta. | [optional] 
**Righe** | [**List&lt;ModelloContabileRiga&gt;**](ModelloContabileRiga.md) | Righe del modello contabile. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


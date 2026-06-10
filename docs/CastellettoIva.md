# Amica.Sdk.Model.CastellettoIva
Importo IVA a debito associato a una Amica.CastellettoIva.RegistrazioneContabile.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**RegistrazioneContabileId** | **int** | Identificativo univoco di Amica.CastellettoIva.RegistrazioneContabile (registrazione contabile). | [optional] 
**RegistrazioneContabile** | [**RegistrazioneContabile**](RegistrazioneContabile.md) |  | [optional] 
**CausaleIvaId** | **int** | Identificativo univoco di Amica.CastellettoIva.CausaleIva. | [optional] 
**CausaleIva** | [**CausaleIva**](CausaleIva.md) |  | [optional] 
**RegistroIvaId** | **int** | Identificativo univoco di &#x60;RegistroIva&#x60;. | [optional] 
**Imponibile** | **double** | Imponibile. | [optional] 
**Imposta** | **double** | Imposta. | [optional] 
**Detraibile** | **double** | Quota detraibile. | [optional] 
**Indetraibile** | **double** | Quota non detraibile. | [optional] 
**Totale** | **double** | Importo totale (imponibile + imposta). | [optional] [readonly] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


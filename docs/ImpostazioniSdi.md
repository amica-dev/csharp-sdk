# Amica.Sdk.Model.ImpostazioniSdi
Impostazioni del servizio di interscambio fatture (SdI).

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**ApiKeySdi** | **string** | Chiave di licenza del servizio di interscambio fatture. | [optional] 
**UltimoDownloadSdi** | **DateTime?** | Data e ora dell&#39;ultimo aggiornamento ricevuto dal servizio di interscambio fatture (SdI). | [optional] 
**IncludiPdfSdi** | **bool** | Indica se allegare il PDF della fattura a ciascun XML inviato al sistema di interscambio fatture (SdI). | [optional] 
**AbbinaArticoliSdi** | **bool** | Per le fatture di acquisto scaricate dal servizio di interscambio fatture (SdI), indica se si desidera tentare l&#39;abbinamento degli articoli/prodotti della fattura con quelli presenti nei magazzini aziendali. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


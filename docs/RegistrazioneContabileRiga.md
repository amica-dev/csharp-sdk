# Amica.Sdk.Model.RegistrazioneContabileRiga
Riga di registrazione contabile.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**RegistrazioneContabileId** | **int** | Identificativo univoco di &#x60;RegistrazioneContabile&#x60;. | [optional] 
**ScadenzaId** | **int?** | Identificativo univoco di &#x60;Scadenza&#x60;. | [optional] 
**CausaleContabileId** | **int?** | Identificativo univoco di &#x60;CausaleContabile&#x60;. | [optional] 
**EmissioneId** | **int?** | Identificativo del &#x60;Documento&#x60; di riferimento. | [optional] 
**TipoConto** | **string** | Tipo di conto (piano dei conti). | [optional] 
**SezioneConto** | **string** | Sezione del piano dei conti. | [optional] 
**PartitaAnno** | **int** | Anno della partita aperta. | [optional] 
**PartitaNumero** | **int** | Numero della partita aperta. | [optional] 
**Conto** | **string** | Numero di conto (ad es. \&quot;100.20\&quot;). | [optional] 
**ContoRaw** | **string** | Numero di conto, in formato grezzo (ad es. \&quot;  100   20\&quot;). | [optional] 
**DescrizioneConto** | **string** | Descrizione. | [optional] 
**Dare** | **double** | Importo in dare. | [optional] 
**Avere** | **double** | Importo in avere. | [optional] 
**Note** | **string** | Note. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


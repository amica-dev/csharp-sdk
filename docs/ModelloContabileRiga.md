# Amica.Sdk.Model.ModelloContabileRiga
Riga di un Amica.ModelloContabile.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**ModelloContabileId** | **int** | Identificativo univoco di Amica.ModelloContabile. | [optional] 
**CausaleContabileId** | **int** | Identificativo univoco di &#x60;CausaleContabile&#x60;. | [optional] 
**ContoRaw** | **string** | Numero di conto, in formato grezzo. Sono supportati fino a cinque livelli, ciascuno espresso da un massimo di cinque caratteri allineati a destra e riempiti con spazi, ad es. \&quot;  100   20\&quot; (dieci caratteri in totale) esprime \&quot;100.20\&quot;. | 
**Importo** | **double** | Importo. | [optional] 
**TipoImporto** | **string** | Cosa rappresenta Amica.ModelloContabileRiga.Importo. | [optional] 
**SegnoContabile** | **string** | Dare o avere. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


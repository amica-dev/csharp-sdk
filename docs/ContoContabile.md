# Amica.Sdk.Model.ContoContabile
Conto contabile.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**BancaId** | **int?** | Identificativo univoco di Amica.Banca. | [optional] 
**Sezione** | **string** | Sezione di bilancio. | [optional] 
**Tipo** | **string** | Tipo di conto. | [optional] 
**Raw** | **string** | Numero di conto, in formato grezzo. Sono supportati fino a cinque livelli, ciascuno espresso da un massimo di cinque caratteri allineati a destra e riempiti con spazi, ad es. \&quot;  100   20\&quot; (dieci caratteri in totale) esprime \&quot;100.20\&quot;. | [optional] 
**Sottoconto** | **string** | Ultimo quintetto del numero di conto (sottoconto). Per \&quot;100.20\&quot;, il valore sarebbe \&quot;20\&quot;. | [optional] 
**Descrizione** | **string** | Descrizione. | [optional] 
**Conto** | **string** | Numero di conto (ad es. \&quot;100.20\&quot;). Sono supportati fino a 5 livelli. Il valore è ricavato da Amica.ContoContabile.Raw. | [optional] [readonly] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


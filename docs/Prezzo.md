# Amica.Sdk.Model.Prezzo
Prezzo di listino.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**ListinoId** | **int?** | Identificativo univoco di Amica.Listino. | [optional] 
**ArticoloId** | **int?** | Identificativo univoco di Amica.Prezzo.Articolo. In alternativa, usare Amica.Prezzo.PrestazioneId. | [optional] 
**Articolo** | [**Articolo**](Articolo.md) |  | [optional] 
**PrestazioneId** | **int?** | Identificativo univoco di Amica.Prezzo.Prestazione. In alternativa, usare Amica.Prezzo.ArticoloId. | [optional] 
**Prestazione** | [**Articolo**](Articolo.md) |  | [optional] 
**Importo** | **double** | Importo (prezzo). | [optional] 
**Sconto1** | **double** | Primo sconto percentuale, applicato sul prezzo del bene o della prestazione. | [optional] 
**Sconto2** | **double** | Secondo sconto percentuale, applicato a seguito dell&#39;applicazione di Amica.Prezzo.Sconto1 | [optional] 
**Sconto3** | **double** | Terzo sconto percentuale, applicato a seguito dell&#39;applicazione di Amica.Prezzo.Sconto2 | [optional] 
**Sconto4** | **double** | Quarto sconto percentuale, applicato a seguito dell&#39;applicazione di Amica.Prezzo.Sconto3 | [optional] 
**Ricarico** | **double** | Ricarico (percentuale) da applicare sul costo del prodotto (o della prestazione). Se utilizzato, Amica.Prezzo.Importo viene ignorato. | [optional] 
**ProvvigioneAgente** | **double?** | Provvigione corrisposta all&#39;agente, in percentuale. | [optional] 
**ProvvigioneCapoArea** | **double?** | Provvigione corrisposta al capo area, in percentuale. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


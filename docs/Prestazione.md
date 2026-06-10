# Amica.Sdk.Model.Prestazione
Erogazione di una prestazione.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**CategoriaMerceologicaId** | **int?** | Identificativo univoco di Amica.Prestazione.CategoriaMerceologica. | [optional] 
**CategoriaMerceologica** | [**CategoriaMerceologica**](CategoriaMerceologica.md) |  | [optional] 
**GruppoMerceologicoId** | **int?** | Identificativo univoco di Amica.Prestazione.GruppoMerceologico. | [optional] 
**GruppoMerceologico** | [**GruppoMerceologico**](GruppoMerceologico.md) |  | [optional] 
**CausaleIvaId** | **int?** | Identificativo univoco di Amica.Prestazione.CausaleIva. | [optional] 
**CausaleIva** | [**CausaleIva**](CausaleIva.md) |  | [optional] 
**Nome** | **string** | Nome. | 
**Codice** | **string** | Codice. | 
**BarCode** | **string** | Codice a barre. | [optional] 
**Stato** | **bool** | Indica se è attiva o meno. | [optional] 
**Note** | **string** | Note. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


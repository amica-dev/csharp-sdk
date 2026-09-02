# Amica.Sdk.Model.MovimentoMerceRiga
Riga di un movimento di magazzino.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**MovimentoMerceId** | **int** | Identificativo univoco di &#x60;MovimentoMerce&#x60;. | [optional] 
**ArticoloId** | **int** | Identificativo univoco di &#x60;Articolo&#x60;. | [optional] 
**Articolo** | [**Articolo**](Articolo.md) |  | [optional] 
**ListinoId** | **int?** | Identificativo univoco di &#x60;Listino&#x60;. | [optional] 
**MagazzinoId** | **int** | Identificativo univoco di &#x60;Magazzino&#x60;. | [optional] 
**PrezzoId** | **int?** | Identificativo univoco di &#x60;Prezzo&#x60;. | [optional] 
**Prezzo** | [**Prezzo**](Prezzo.md) |  | [optional] 
**CausaleIvaId** | **int?** | Identificativo univoco di &#x60;CausaleIva&#x60;. | [optional] 
**CausaleIva** | [**CausaleIva**](CausaleIva.md) |  | [optional] 
**TagliaId** | **int?** | Identificativo univoco di &#x60;Taglia&#x60;. | [optional] 
**Taglia** | [**Taglia**](Taglia.md) |  | [optional] 
**Quantit** | **double** | Quantità movimentata. | [optional] 
**PrezzoNetto** | **double** | Prezzo applicato, al netto dell&#39;IVA. | [optional] 
**PrezzoIvato** | **double** | Prezzo applicato, IVA inclusa. | [optional] 
**Colore** | **string** | Colore. | [optional] 
**Identificativo** | **string** | Numero di serie o altro identificativo del prodotto. | [optional] 
**IdentificativoEsteso** | **string** | Identificativo esteso del prodotto. | [optional] 
**Data** | **DateTime?** | Data di scadenza, produzione, fine garanzia o altro. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


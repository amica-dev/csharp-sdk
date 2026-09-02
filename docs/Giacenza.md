# Amica.Sdk.Model.Giacenza
Giacenza corrente (inventario).

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**MagazzinoId** | **int** | Identificativo univoco di &#x60;Magazzino&#x60;. | [optional] 
**Magazzino** | [**Magazzino**](Magazzino.md) |  | [optional] 
**ArticoloId** | **int** | Identificativo univoco di &#x60;Articolo&#x60;. | [optional] 
**Articolo** | [**Articolo**](Articolo.md) |  | [optional] 
**QuantitInGiacenza** | **double** | Quantità in giacenza. | [optional] 
**QuantitImpegnata** | **double** | Quantità impegnata. | [optional] 
**QuantitOrdinata** | **double** | Quantità ordinata. | [optional] 
**ScortaMinima** | **double** | Scorta minima. | [optional] 
**GestioneSottoscorta** | **bool** | Indica se devono essere generati gli avvisi di sottoscorta. | [optional] 
**DataUltimoCarico** | **DateTime?** | Data dell&#39;ultimo carico. | [optional] 
**DataUltimoScarico** | **DateTime?** | Data dell&#39;ultimo scarico. | [optional] 
**CaricoQuantit** | **double** | Quantità totale caricata. | [optional] 
**CaricoValore** | **double** | Valore totale caricato. | [optional] 
**ScaricoQuantit** | **double** | Quantità totale scaricata. | [optional] 
**ScaricoValore** | **double** | Valore totale scaricato. | [optional] 
**Identificativo** | **string** | Numero di serie, o qualsiasi altro identificativo. | [optional] 
**IdentificativoEsteso** | **string** | Identificativo esteso. | [optional] 
**Data** | **DateTime?** | Data di scadenza, produzione, fine garanzia o altro. | [optional] 
**Colore** | **string** | Colore. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


# Amica.Sdk.Model.Articolo
Articolo di magazzino o prodotto.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**CategoriaMerceologicaId** | **int?** | Identificativo univoco della &#x60;CategoriaMerceologica&#x60; (categoria merceologica). | [optional] 
**CategoriaMerceologica** | [**CategoriaMerceologica**](CategoriaMerceologica.md) |  | [optional] 
**GruppoMerceologicoId** | **int?** | Identificativo univoco del &#x60;GruppoMerceologico&#x60; (gruppo merceologico). | [optional] 
**GruppoMerceologico** | [**GruppoMerceologico**](GruppoMerceologico.md) |  | [optional] 
**CausaleIvaId** | **int?** | Identificativo univoco della &#x60;CausaleIva&#x60; (causale IVA). Id univoco della &lt;a href&#x3D;\&quot;#tag/CausaleIva\&quot;&gt;causale IVA&lt;/a&gt; predefinita per l&#39;articolo. | [optional] 
**CausaleIva** | [**CausaleIva**](CausaleIva.md) |  | [optional] 
**TagliaId** | **int?** | Identificativo univoco della &#x60;Taglia&#x60; (taglia). | [optional] 
**Taglia** | [**Taglia**](Taglia.md) |  | [optional] 
**MarcaId** | **int?** | Identificativo univoco della &#x60;Marca&#x60;. | [optional] 
**Marca** | [**Marca**](Marca.md) |  | [optional] 
**ProduttoreId** | **int?** | Identificativo univoco del &#x60;Produttore&#x60; (produttore). | [optional] 
**Produttore** | [**Produttore**](Produttore.md) |  | [optional] 
**RepartoId** | **int?** | Identificativo univoco del &#x60;Reparto&#x60; (reparto). | [optional] 
**Reparto** | [**Reparto**](Reparto.md) |  | [optional] 
**Nome** | **string** | Nome. | 
**Codice** | **string** | Identificativo, o codice. | 
**NomeBreve** | **string** | Nome breve. | [optional] 
**CodiceFornitore** | **string** | Codice dell&#39;articolo o prodotto presso il fornitore. | [optional] 
**CodiceProduttore** | **string** | Codice dell&#39;articolo o prodotto presso il produttore. | [optional] 
**GestioneArticolo** | **string** | Modalità di gestione. | [optional] 
**ContestoArticolo** | **string** | Contesto in cui viene utilizzato. | [optional] 
**BarCode** | **string** | Codice a barre. | [optional] 
**CostoUltimo** | **double** | Ultimo prezzo di costo. | [optional] 
**CostoMedio** | **double** | Prezzo di costo medio. | [optional] 
**UnitMisuraIntera** | **bool** | Indica se l&#39;unità di misura deve essere arrotondata all&#39;intero più vicino. | [optional] 
**ArticoloComposto** | **bool** | Indica se è composto assemblando altri articoli. | [optional] 
**Stato** | **bool** | Stato. &#x60;true&#x60; se attivo, &#x60;false&#x60; altrimenti. | [optional] 
**DataInserimento** | **DateTime?** | Data di primo inserimento. | [optional] 
**UnitMisuraMovimenti** | **string** | Unità di misura da utilizzare nei movimenti di magazzino. | [optional] 
**UnitMisuraInventario** | **string** | Unità di misura da utilizzare nelle elaborazioni di inventario. | [optional] 
**FattoreConversioneUnitMisura** | **double** | Fattore di conversione tra unità di movimento e unità di inventario. | [optional] 
**Note** | **string** | Note. | [optional] 
**MediaFile** | [**ArticoloMediaFile**](ArticoloMediaFile.md) |  | [optional] 
**ContoRawVendita** | **string** | Conto da utilizzare nelle registrazioni contabili di vendita. | [optional] 
**ContoRawAcquisto** | **string** | Conto da utilizzare nelle registrazioni contabili di acquisto. | [optional] 
**DistintaBase** | [**List&lt;Componente&gt;**](Componente.md) | Distinta base. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


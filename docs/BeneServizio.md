# Amica.Sdk.Model.BeneServizio
Bene o servizio fatturato.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**DocumentoId** | **int** | Identificativo univoco del Amica.Documento (documento). | [optional] 
**ArticoloId** | **int?** | Identificativo univoco dell&#39;Amica.BeneServizio.Articolo (articolo o prodotto). | [optional] 
**Articolo** | [**Articolo**](Articolo.md) |  | [optional] 
**PrestazioneId** | **int?** | Identificativo univoco della Amica.BeneServizio.Prestazione (prestazione). | [optional] 
**Prestazione** | [**Articolo**](Articolo.md) |  | [optional] 
**MagazzinoId** | **int?** | Identificativo univoco del Amica.BeneServizio.Magazzino (magazzino). Obbligatorio quando viene usato Amica.BeneServizio.Codice. | [optional] 
**Magazzino** | [**Magazzino**](Magazzino.md) |  | [optional] 
**CausaleIvaId** | **int?** | Identificativo univoco della Amica.BeneServizio.CausaleIva. | [optional] 
**CausaleIva** | [**CausaleIva**](CausaleIva.md) |  | [optional] 
**ListinoId** | **int?** | Identificativo univoco del Amica.BeneServizio.Listino (listino). | [optional] 
**Listino** | [**Listino**](Listino.md) |  | [optional] 
**OrdineId** | **int?** | Identificativo univoco del Amica.Documento (ordine). | [optional] 
**BeneServizioOrdineId** | **int?** | Identificativo univoco del Amica.BeneServizio (bene o servizio) all&#39;interno dell&#39;ordine referenziato da Amica.BeneServizio.OrdineId. | [optional] 
**TagliaId** | **int?** | Identificativo univoco della Amica.BeneServizio.Taglia (taglia). | [optional] 
**Taglia** | [**Taglia**](Taglia.md) |  | [optional] 
**RitenutaFiscale** | **bool** | Indica se al bene o servizio deve essere applicata una ritenuta fiscale. &#x60;true&#x60; se deve essere applicata, &#x60;false&#x60; altrimenti. | [optional] 
**Posizione** | **int** | Indice di riga all&#39;interno del documento. Questa proprietà è utilizzata solo dal client desktop Amica 20 per Windows e deve essere considerata obsoleta. | [optional] 
**Codice** | **string** | Codice del bene o servizio. | [optional] 
**Descrizione** | **string** | Descrizione. | 
**UnitMisura** | **string** | Unità di misura. | [optional] 
**Quantit** | **double?** | Quantità movimentata. | [optional] 
**QuantitEvasa** | **double** | Quantità dell&#39;ordine evasa. Questo valore viene aggiornato automaticamente quando viene emessa una fattura o un DDT che contiene un riferimento a un ordine ed è attivo &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/GestioneOrdiniPreventivi\&quot;&gt;Ordini e Preventivi&lt;/a&gt;. | [optional] 
**QuantitDaEvadere** | **double** | Quantità che resta da evadere. | [optional] [readonly] 
**Sconto1** | **double?** | Primo sconto percentuale, applicato al prezzo del bene o servizio. | [optional] 
**Sconto2** | **double?** | Secondo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto1 | [optional] 
**Sconto3** | **double?** | Terzo sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto2 | [optional] 
**Sconto4** | **double?** | Quarto sconto percentuale, applicato dopo l&#39;applicazione di Amica.BeneServizio.Sconto3 | [optional] 
**ScontoIncondizionato** | **double?** | Sconto incondizionato, in valuta, applicato al prezzo del bene o servizio. | [optional] 
**ProvvigioneAgente** | **double?** | Provvigione corrisposta all&#39;agente, in percentuale. | [optional] 
**ProvvigioneCapoArea** | **double?** | Provvigione corrisposta al capo area, in percentuale. | [optional] 
**PrezzoId** | **int?** | Identificativo univoco del Amica.BeneServizio.Prezzo (prezzo). | [optional] 
**Prezzo** | **double?** | Prezzo. | [optional] 
**PrezzoScontato** | **double?** | Prezzo scontato, cioè al netto degli sconti percentuali indicati e dello sconto incondizionato. | [optional] 
**PrezzoIvato** | **double?** | Prezzo comprensivo di IVA. | [optional] 
**Importo** | **double?** | Importo, determinato come prodotto di Amica.BeneServizio.Quantità e Amica.BeneServizio.PrezzoScontato. | [optional] 
**DatiAggiuntivi** | [**DatiAggiuntivi**](DatiAggiuntivi.md) |  | [optional] 
**Sconto** | **string** | Descrizione di tutti gli sconti combinati (es: \&quot;10,5%+5€\&quot;). | [optional] [readonly] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


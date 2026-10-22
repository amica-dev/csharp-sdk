# Amica.Sdk.Model.BeneServizio
Bene o servizio fatturato.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**DocumentoId** | **int** | Identificativo univoco del &#x60;Documento&#x60; (documento). | [optional] 
**Documento** | [**Documento**](Documento.md) |  | [optional] 
**ArticoloId** | **int?** | Identificativo univoco dell&#39;&#x60;Articolo&#x60; (articolo o prodotto). | [optional] 
**Articolo** | [**Articolo**](Articolo.md) |  | [optional] 
**PrestazioneId** | **int?** | Identificativo univoco della &#x60;Prestazione&#x60; (prestazione). | [optional] 
**Prestazione** | [**Prestazione**](Prestazione.md) |  | [optional] 
**MagazzinoId** | **int?** | Identificativo univoco del &#x60;Magazzino&#x60; (magazzino). Obbligatorio quando viene usato &#x60;Codice&#x60;. | [optional] 
**Magazzino** | [**Magazzino**](Magazzino.md) |  | [optional] 
**CausaleIvaId** | **int?** | Identificativo univoco della &#x60;CausaleIva&#x60;. | [optional] 
**CausaleIva** | [**CausaleIva**](CausaleIva.md) |  | [optional] 
**ListinoId** | **int?** | Identificativo univoco del &#x60;Listino&#x60; (listino). | [optional] 
**Listino** | [**Listino**](Listino.md) |  | [optional] 
**OrdineId** | **int?** | Identificativo univoco del &#x60;Documento&#x60; (ordine). | [optional] 
**BeneServizioOrdineId** | **int?** | Identificativo univoco del &#x60;BeneServizio&#x60; (bene o servizio) all&#39;interno dell&#39;ordine referenziato da &#x60;OrdineId&#x60;. | [optional] 
**TagliaId** | **int?** | Identificativo univoco della &#x60;Taglia&#x60; (taglia). | [optional] 
**Taglia** | [**Taglia**](Taglia.md) |  | [optional] 
**RitenutaFiscale** | **bool** | Indica se al bene o servizio deve essere applicata una ritenuta fiscale. &#x60;true&#x60; se deve essere applicata, &#x60;false&#x60; altrimenti. | [optional] 
**Posizione** | **int** | Indice di riga all&#39;interno del documento. Questa proprietà è utilizzata solo dal client desktop Amica 20 per Windows e deve essere considerata obsoleta. | [optional] 
**Codice** | **string** | Codice del bene o servizio. | [optional] 
**Descrizione** | **string** | Descrizione. Obbligatoria, tranne che sulle righe di totale per aliquota create dall&#39;importazione dei DDT con &#x60;Totali&#x60;: hanno &#x60;CausaleIvaId&#x60; e &#x60;Importo&#x60; ma niente &#x60;ArticoloId&#x60; né descrizione, e la fattura elettronica usa come loro descrizione il riferimento al DDT che le precede. | [optional] 
**UnitMisura** | **string** | Unità di misura. | [optional] 
**Quantit** | **double?** | Quantità movimentata. | [optional] 
**QuantitEvasa** | **double** | Quantità dell&#39;ordine evasa. Questo valore viene aggiornato automaticamente quando viene emessa una fattura o un DDT che contiene un riferimento a un ordine ed è attivo &lt;a href&#x3D;\&quot;https://gestionaleamica.com/Software/GestioneOrdiniPreventivi\&quot;&gt;Ordini e Preventivi&lt;/a&gt;. | [optional] 
**QuantitDaEvadere** | **double** | Quantità che resta da evadere. | [optional] [readonly] 
**Sconto1** | **double?** | Primo sconto percentuale, applicato al prezzo del bene o servizio. Il valore è una frazione: &#x60;0.05&#x60; corrisponde al 5%. | [optional] 
**Sconto2** | **double?** | Secondo sconto percentuale, applicato dopo l&#39;applicazione di &#x60;Sconto1&#x60; Il valore è una frazione: &#x60;0.03&#x60; corrisponde al 3%. | [optional] 
**Sconto3** | **double?** | Terzo sconto percentuale, applicato dopo l&#39;applicazione di &#x60;Sconto2&#x60; Il valore è una frazione: &#x60;0.02&#x60; corrisponde al 2%. | [optional] 
**Sconto4** | **double?** | Quarto sconto percentuale, applicato dopo l&#39;applicazione di &#x60;Sconto3&#x60; Il valore è una frazione: &#x60;0.01&#x60; corrisponde all&#39;1%. | [optional] 
**ScontoIncondizionato** | **double?** | Sconto incondizionato, in valuta, applicato al prezzo del bene o servizio. | [optional] 
**ProvvigioneAgente** | **double?** | Provvigione corrisposta all&#39;agente, in percentuale. | [optional] 
**ProvvigioneCapoArea** | **double?** | Provvigione corrisposta al capo area, in percentuale. | [optional] 
**PrezzoId** | **int?** | Identificativo univoco del &#x60;Prezzo&#x60; (prezzo). | [optional] 
**Prezzo** | **double?** | Prezzo. | [optional] 
**PrezzoScontato** | **double?** | Prezzo scontato, cioè al netto degli sconti percentuali indicati e dello sconto incondizionato. | [optional] 
**PrezzoIvato** | **double?** | Prezzo comprensivo di IVA. | [optional] 
**Importo** | **double?** | Importo, determinato come prodotto di &#x60;Quantità&#x60; e &#x60;PrezzoScontato&#x60;. | [optional] 
**ImportoIvato** | **double?** | Importo IVA inclusa, calcolato dalla API in lettura come &#x60;beneservizio/compute&#x60; a partire dai prezzi salvati e dal flag &#x60;IvaEsclusa&#x60; del documento. Non viene memorizzato: in &#x60;POST&#x60; e &#x60;PUT&#x60; è ignorato. Con IVA inclusa è la base dell&#39;imposta del riepilogo, da passare nella riga di &#x60;documento/compute&#x60; (&#x60;ImportoIvato&#x60;). Assente sulle righe senza prezzo. | [optional] 
**DatiAggiuntivi** | [**DatiAggiuntivi**](DatiAggiuntivi.md) |  | [optional] 
**Sconto** | **string** | Descrizione di tutti gli sconti combinati (es: \&quot;10,5%+5€\&quot;). | [optional] [readonly] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


# Amica.Sdk.Model.Scadenza
Scadenza di pagamento.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**RegistrazioneContabileId** | **int?** | Identificativo univoco di &#x60;RegistrazioneContabile&#x60;. | [optional] 
**RegistrazioneContabile** | [**RegistrazioneContabile**](RegistrazioneContabile.md) |  | [optional] 
**AnagraficaId** | **int?** | Identificativo univoco di &#x60;Anagrafica&#x60;. | [optional] 
**Anagrafica** | [**Anagrafica**](Anagrafica.md) |  | [optional] 
**DocumentoId** | **int?** | Identificativo univoco di &#x60;Documento&#x60;. | [optional] 
**Documento** | [**Documento**](Documento.md) |  | [optional] 
**ModalitaPagamentoId** | **int?** | Identificativo univoco di &#x60;ModalitaPagamento&#x60;. | [optional] 
**ModalitaPagamento** | [**ModalitaPagamento**](ModalitaPagamento.md) |  | [optional] 
**PagataManualmente** | **bool** | Indica se la scadenza è stata impostata manualmente come &#39;pagata&#39;. | [optional] 
**Pagata** | **bool** | Indica se la scadenza è pagata: &#x60;ImportoDaPagare&#x60;, arrotondato al centesimo, è zero. | [optional] [readonly] 
**Data** | **DateTime** | Data di scadenza. | [optional] 
**DataPagamento** | **DateTime?** | Data di pagamento. | [optional] 
**DataDocumento** | **DateTime?** | Data del documento fiscale. | [optional] 
**Importo** | **double** | Importo della scadenza. | [optional] 
**PartitaAnno** | **int?** | Anno della partita aperta. | [optional] 
**PartitaNumero** | **int?** | Numero della partita aperta. | [optional] 
**Note** | **string** | Note. | [optional] 
**Descrizione** | **string** | Descrizione. | [optional] 
**DocumentoNumeroParteNumerica** | **int?** | Numero documento, la parte numerica (es. 99). | [optional] 
**DocumentoNumeroParteTesto** | **string** | Numero documento, la parte alfanumerica (es. \&quot;bis\&quot;) | [optional] 
**DocumentoNumero** | **string** | Numero documento, es. \&quot;99/bis\&quot;. Il documento deve essere già salvato sul sistema. | [optional] 
**TipoScadenza** | **string** | Tipo di scadenza. | [optional] 
**ImportoPagato** | **double** | L&#39;importo già pagato della scadenza. | [optional] 
**ImportoDaPagare** | **double** | Rimanenza che risulta ancora da pagare, arrotondata al centesimo come nel client Windows. | [optional] [readonly] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


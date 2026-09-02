# Amica.Sdk.Model.RegistrazioneContabile
Registrazione contabile.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**Data** | **DateTime** | Data della registrazione contabile. | [optional] 
**DataCompetenza** | **DateTime?** | Data di competenza. | [optional] 
**Numero** | **int** | Numero della registrazione contabile. | [optional] 
**TipoRegistrazione** | **string** | Tipo di registrazione. | [optional] 
**SegnoIva** | **string** | Positività o negatività dell&#39;IVA. | [optional] 
**TipoOperazione** | **string** | Tipo di operazione contabile. | [optional] 
**RegistroIvaId** | **int?** | Identificativo univoco di &#x60;RegistroIva&#x60;. | [optional] 
**AnagraficaId** | **int?** | Identificativo univoco di &#x60;Anagrafica&#x60;. | [optional] 
**PagamentoId** | **int?** | Identificativo univoco di &#x60;Pagamento&#x60;. | [optional] 
**DocumentoId** | **int?** | Identificativo univoco di &#x60;Documento&#x60;. | [optional] 
**Documento** | [**Documento**](Documento.md) |  | [optional] 
**DataDocumento** | **DateTime?** | Data del documento di riferimento. | [optional] 
**DocumentoNumeroParteTesto** | **string** | La parte alfanumerica del numero documento, se presente (es. \&quot;bis\&quot;). | [optional] 
**DocumentoNumeroParteNumerica** | **int?** | La parte numerica del numero documento (es. \&quot;99\&quot;). | [optional] 
**DocumentoNumero** | **string** | Numero documento completo, es. \&quot;99/bis\&quot;. Il documento deve essere già stato salvato sul backend. | [optional] 
**Protocollo** | **int?** | Numero di protocollo. | [optional] 
**Descrizione** | **string** | Descrizione. | [optional] 
**Note** | **string** | Note. | [optional] 
**Righe** | [**List&lt;RegistrazioneContabileRiga&gt;**](RegistrazioneContabileRiga.md) | Righe della registrazione contabile. | [optional] 
**CastellettoIva** | [**List&lt;CastellettoIva&gt;**](CastellettoIva.md) | Importo IVA da versare. | [optional] 
**Scadenze** | [**List&lt;Scadenza&gt;**](Scadenza.md) | Scadenze. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


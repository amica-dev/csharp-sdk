# Amica.Sdk.Model.SchedaCliente
Impostazioni e informazioni di vendita di un'Amica.Anagrafica.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**AnagraficaId** | **int?** | Identificativo univoco di Amica.Anagrafica. | [optional] 
**AgenteId** | **int?** | Identificativo univoco di Amica.Anagrafica dell&#39;agente. | [optional] 
**Agente** | [**Anagrafica**](Anagrafica.md) |  | [optional] 
**PagamentoId** | **int?** | Identificativo univoco di Amica.SchedaCliente.Pagamento. | [optional] 
**Pagamento** | [**Pagamento**](Pagamento.md) |  | [optional] 
**ListinoId** | **int?** | Identificativo univoco di Amica.SchedaCliente.Listino. | [optional] 
**Listino** | [**Listino**](Listino.md) |  | [optional] 
**CategoriaAnagraficaId** | **int?** | Identificativo univoco di Amica.SchedaCliente.CategoriaAnagrafica. | [optional] 
**CategoriaAnagrafica** | [**CategoriaAnagrafica**](CategoriaAnagrafica.md) |  | [optional] 
**CausaleIvaId** | **int?** | Identificativo univoco di Amica.SchedaCliente.CausaleIva. | [optional] 
**CausaleIva** | [**CausaleIva**](CausaleIva.md) |  | [optional] 
**Fido** | **double** | Fido concesso al cliente. | [optional] 
**Sconto** | **double** | Sconto riservato al cliente (percentuale). | [optional] 
**FatturatoAnnoCorrente** | **double** | Fatturato realizzato nell&#39;anno corrente. | [optional] 
**NumeroFattureAnnoCorrente** | **int** | Numero di fatture emesse nell&#39;anno corrente. | [optional] 
**InsolutiAnnoCorrente** | **double** | Importo insoluto nell&#39;anno corrente. | [optional] 
**NumeroInsolutiAnnoCorrente** | **int** | Numero di insoluti nell&#39;anno corrente. | [optional] 
**FatturatoAnnoPrecedente** | **double** | Fatturato realizzato nell&#39;anno precedente. | [optional] 
**NumeroFattureAnnoPrecedente** | **int** | Numero di fatture emesse nell&#39;anno precedente. | [optional] 
**InsolutiAnnoPrecedente** | **double** | Importo insoluto nell&#39;anno precedente. | [optional] 
**NumeroInsolutiAnnoPrecedente** | **int** | Numero di insoluti verificatisi nell&#39;anno precedente. | [optional] 
**DataUltimaFattura** | **DateTime?** | Data dell&#39;ultima fattura. | [optional] 
**DataUltimoPagamento** | **DateTime?** | Data dell&#39;ultimo pagamento. | [optional] 
**DataUltimoInsoluto** | **DateTime?** | Data dell&#39;ultimo insoluto. | [optional] 
**ContoRaw** | **string** | Numero di conto, in formato grezzo (es. \&quot;  100   20\&quot;). Sono ammessi fino a cinque livelli. | [optional] 
**Conto** | **string** | Numero di conto, in formato leggibile (es. \&quot;100.20\&quot;). | [optional] [readonly] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


# Amica.Sdk.Model.SchedaFornitore
Impostazioni e informazioni di acquisto di un'`Anagrafica`.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**AnagraficaId** | **int?** | Identificativo univoco di &#x60;Anagrafica&#x60;. | [optional] 
**PagamentoId** | **int?** | Identificativo univoco di &#x60;Pagamento&#x60;. | [optional] 
**Pagamento** | [**Pagamento**](Pagamento.md) |  | [optional] 
**ListinoId** | **int?** | Identificativo univoco di &#x60;Listino&#x60;. | [optional] 
**Listino** | [**Listino**](Listino.md) |  | [optional] 
**CapoAreaId** | **int?** | Identificativo univoco di &#x60;CapoArea&#x60;. | [optional] 
**CapoArea** | [**Anagrafica**](Anagrafica.md) |  | [optional] 
**CategoriaAnagraficaId** | **int?** | Identificativo univoco di &#x60;CategoriaAnagrafica&#x60;. | [optional] 
**CategoriaAnagrafica** | [**CategoriaAnagrafica**](CategoriaAnagrafica.md) |  | [optional] 
**CausaleIvaId** | **int?** | Identificativo univoco di &#x60;CausaleIva&#x60;. | [optional] 
**CausaleIva** | [**CausaleIva**](CausaleIva.md) |  | [optional] 
**FatturatoAnnoCorrente** | **double** | Fatturato realizzato nell&#39;anno corrente. | [optional] 
**NumeroFattureAnnoCorrente** | **int** | Numero di fatture ricevute nell&#39;anno corrente. | [optional] 
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
**IscrizioneRuolo** | **string** | Numero di iscrizione a ruolo. | [optional] 
**Enasarco** | **string** | Numero di registrazione Enasarco. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


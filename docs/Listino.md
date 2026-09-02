# Amica.Sdk.Model.Listino
Listino.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**FornitoreId** | **int?** | Identificativo univoco di &#x60;Anagrafica&#x60; (fornitore). | [optional] 
**Fornitore** | [**Anagrafica**](Anagrafica.md) |  | [optional] 
**ValutaId** | **int?** | Identificativo univoco di &#x60;Valuta&#x60; (valuta). | [optional] 
**Valuta** | [**Valuta**](Valuta.md) |  | [optional] 
**Nome** | **string** | Nome del listino. | 
**IvaInclusa** | **bool** | Indica se i prezzi base sono IVA inclusa oppure no. | [optional] 
**Ricarico** | **double** | Ricarico da applicare ai prezzi base di tutte le righe del listino. | [optional] 
**DataCreazione** | **DateTime?** | Data di creazione. | [optional] 
**ValidoDal** | **DateTime?** | Inizio validità del listino. | [optional] 
**ValidoAl** | **DateTime?** | Fine validità del listino. | [optional] 
**TipoListino** | **string** | Tipo di listino. | [optional] 
**CalcoloPrezzi** | **string** | Modalità di calcolo dei prezzi del listino. | [optional] 
**Note** | **string** | Note. | [optional] 
**Righe** | [**List&lt;Prezzo&gt;**](Prezzo.md) | Righe del listino. | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


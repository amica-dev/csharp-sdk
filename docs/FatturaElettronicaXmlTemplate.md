# Amica.Sdk.Model.FatturaElettronicaXmlTemplate
Modello standard di fattura elettronica.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**Id** | **int** | Identificativo univoco. | [optional] 
**ConcurrencyToken** | **int** | Il token di concorrenza implementa il controllo di concorrenza ottimistica: a ogni operazione di aggiornamento o cancellazione il valore del token sul database viene confrontato con quello sull&#39;entità. Se i valori coincidono, l&#39;operazione viene eseguita; se differiscono, si assume che un altro utente abbia effettuato una scrittura nel frattempo e l&#39;aggiornamento viene rifiutato. | [optional] 
**SoggettoEmittente** | **string** |  | [optional] 
**IdTrasmittenteIdPaese** | **string** |  | [optional] 
**CedentePrestatoreIdPaese** | **string** |  | [optional] 
**ProgressivoInvio** | **string** |  | [optional] 
**AlboProfessionale** | **string** |  | [optional] 
**ProvinciaAlbo** | **string** |  | [optional] 
**NumeroIscrizioneAlbo** | **string** |  | [optional] 
**DataIscrizioneAlbo** | **DateTime?** |  | [optional] 
**RegimeFiscale** | **string** |  | [optional] 
**SedeNazione** | **string** |  | [optional] 
**SedeNumeroCivico** | **string** |  | [optional] 
**StabileOrganizzazioneIndirizzo** | **string** |  | [optional] 
**StabileOrganizzazioneNumeroCivico** | **string** |  | [optional] 
**StabileOrganizzazioneCap** | **string** |  | [optional] 
**StabileOrganizzazioneComune** | **string** |  | [optional] 
**StabileOrganizzazioneProvincia** | **string** |  | [optional] 
**StabileOrganizzazioneNazione** | **string** |  | [optional] 
**ReaNumero** | **string** |  | [optional] 
**ReaUfficio** | **string** |  | [optional] 
**ReaCapitaleSociale** | **double?** |  | [optional] 
**ReaSocioUnico** | **string** |  | [optional] 
**ReaStatoLiquidazione** | **string** |  | [optional] 
**RappresentanteIdPaese** | **string** |  | [optional] 
**RappresentanteIdCodice** | **string** |  | [optional] 
**RappresentanteCodiceFiscale** | **string** |  | [optional] 
**RappresentanteDenominazione** | **string** |  | [optional] 
**RappresentanteNome** | **string** |  | [optional] 
**RappresentanteCognome** | **string** |  | [optional] 
**RappresentanteTitolo** | **string** |  | [optional] 
**RappresentanteCodEori** | **string** |  | [optional] 
**IntermediarioIdPaese** | **string** |  | [optional] 
**IntermediarioIdCodice** | **string** |  | [optional] 
**IntermediarioCodiceFiscale** | **string** |  | [optional] 
**IntermediarioDenominazione** | **string** |  | [optional] 
**IntermediarioNome** | **string** |  | [optional] 
**IntermediarioCognome** | **string** |  | [optional] 
**IntermediarioTitolo** | **string** |  | [optional] 
**IntermediarioCodEori** | **string** |  | [optional] 

[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


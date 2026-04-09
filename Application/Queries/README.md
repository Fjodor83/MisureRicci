# Queries Layer (CQRS Stub)

Questa cartella contiene le query applicative (operazioni di sola lettura).

Linee guida:
- Una query non deve modificare stato.
- Le query devono essere ottimizzate per read-model e paging.
- Il risultato deve essere un DTO/ViewModel specifico del caso d'uso.
- Le policy multi-tenant devono essere applicate sempre nelle query.

Struttura consigliata per nuove feature:
- `Application/Queries/<Feature>/`
- `<Feature>Query.cs`
- `<Feature>QueryHandler.cs`

Nota evolutiva:
- Al momento l'invocazione è diretta.
- In futuro questa struttura potrà essere mappata a MediatR senza cambiare i casi d'uso.

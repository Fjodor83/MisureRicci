# Commands Layer (CQRS Stub)

Questa cartella contiene i comandi applicativi (azioni che modificano stato).

Linee guida:
- Un comando rappresenta una singola intenzione di business.
- Il comando non contiene logica di persistenza.
- Ogni comando ha un handler dedicato che orchestra i servizi di dominio.
- Il risultato deve essere esplicito (es. `Result`, `Result<T>`), senza eccezioni per flussi attesi.

Struttura consigliata per nuove feature:
- `Application/Commands/<Feature>/`
- `<Feature>Command.cs`
- `<Feature>CommandHandler.cs`

Nota evolutiva:
- Al momento l'invocazione è diretta (senza MediatR).
- In futuro i command handler potranno essere adattati a MediatR `IRequest` / `IRequestHandler` con modifiche minime.

# AI-Library

AI-powered library chat app: **.NET 10 Web API** + **Angular 19** + **Ollama** via [`Microsoft.Extensions.AI`](https://learn.microsoft.com/dotnet/ai/).

Adapted from the Microsoft Learn quickstart  
[Build an AI chat app with .NET](https://learn.microsoft.com/en-gb/dotnet/ai/quickstarts/build-chat-app?pivots=openai) — same core ideas (`IChatClient`, system prompt, chat history, streaming), shaped as a **catalog-aware librarian API + UI**.

---

## Architecture

```text
┌────────────────────┐     JSON / SSE      ┌──────────────────────────┐
│  Angular (:4200)   │ ─────────────────►  │  AiLibrary.Api (:5243)   │
│  Chat + shelf UI   │                     │  Controllers, CORS,      │
└────────────────────┘                     │  /health, exception mw   │
                                           └────────────┬─────────────┘
                                                        │ MediatR
                                           ┌────────────▼─────────────┐
                                           │  Application             │
                                           │  Chat / Stream commands  │
                                           │  Ports: IChat*, ICatalog │
                                           └────────────┬─────────────┘
                                                        │
                          ┌─────────────────────────────┼────────────────────┐
                          ▼                             ▼                    ▼
                   ChatHistoryStore              ChatService           BookCatalog
                   (session memory)              (IChatClient)         (seeded list)
                          │                             │
                          │                             ▼
                          │                      Ollama (:11434)
                          └──────── system prompt includes catalog ──┘
```

| Layer | Responsibility |
|-------|----------------|
| **Api** | HTTP, CORS, health, problem details, OpenAPI |
| **Application** | MediatR use cases, DTOs, abstractions |
| **Domain** | `Book` entity |
| **Infrastructure** | Ollama client, history, prompt, in-memory catalog |
| **Angular** | Chat UI, streaming toggle, catalog shelf |
| **Tests** | xUnit coverage for handlers, history, catalog |

---

## Prerequisites

| Tool | Notes |
|------|--------|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Backend + tests |
| [Node.js 20+](https://nodejs.org/) | Angular app |
| [Ollama](https://ollama.com/) | Local LLM |
| Optional REST Client | Run `src/AiLibrary.Api/Http/*.http` |

```bash
ollama pull gemma3:4b
```

Config: `src/AiLibrary.Api/appsettings.json` → `Ollama:Endpoint`, `Ollama:Model`.

---

## Quick start

```bash
# 1) API
dotnet restore AI-Library.slnx
dotnet run --project src/AiLibrary.Api --launch-profile http

# 2) Tests (another terminal)
dotnet test AI-Library.slnx

# 3) UI
cd "AI-Library app"
npm install
npm start
```

| URL | Purpose |
|-----|---------|
| http://localhost:5243/health | Health |
| http://localhost:5243/api/books | Catalog |
| http://localhost:5243/api/chat | Chat (JSON) |
| http://localhost:5243/api/chat/stream | Chat (SSE) |
| http://localhost:5243/openapi/v1.json | OpenAPI (Development) |
| http://localhost:4200 | Angular UI |

---

## Interview demo script (≈2 minutes)

1. Open UI → show **catalog shelf** loaded from `GET /api/books`.
2. Click a book card → send → Ava answers using **catalog-aware system prompt**.
3. Follow up: “similar books” → same **sessionId** (multi-turn history).
4. Follow up: “short summary of the first recommendation”.
5. Toggle **Stream replies** off/on → JSON vs SSE.
6. Show code: `IChatClient` registration, `ChatHistoryStore`, `PromptBuilder`, unit tests.
7. Be explicit about limits: in-memory history/catalog (no SQL/RAG/auth yet).

HTTP alternative: `src/AiLibrary.Api/Http/chat.http` + `books.http`.

---

## Microsoft Learn mapping

| Quickstart | This repo |
|------------|-----------|
| Console app | Web API + Angular |
| OpenAI `IChatClient` | Ollama `IChatClient` (swap-ready) |
| Hiking system prompt | Librarian + **LIBRARY CATALOG** block |
| `List<ChatMessage>` | Thread-safe multi-session `ChatHistoryStore` |
| `GetStreamingResponseAsync` | `POST /api/chat/stream` (SSE) |
| User secrets for keys | appsettings for Ollama; `UserSecretsId` ready for OpenAI |

Docs: [`docs/AI-Library-Architecture-Guide.pdf`](docs/AI-Library-Architecture-Guide.pdf)

---

## Solution layout

```text
AI-Library/
├── AI-Library.slnx
├── Directory.Build.props
├── src/
│   ├── AiLibrary.Api
│   ├── AiLibrary.Application
│   ├── AiLibrary.Domain
│   └── AiLibrary.Infrastructure
├── tests/AiLibrary.Tests
├── AI-Library app/          # Angular
└── docs/
```

---

## API cheat sheet

```http
GET  /health
GET  /api/books?q=fantasy&genre=Fantasy
GET  /api/books/{id}
POST /api/chat
POST /api/chat/stream
```

Chat body:

```json
{ "sessionId": "optional", "message": "Recommend a short mystery from the catalog." }
```

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| API won’t start | Ollama running; valid `Ollama:*` config |
| Slow / empty chat | `ollama list`; try `gemma3:1b` |
| Angular CORS | API up; origin `http://localhost:4200` |
| History lost | API restart clears in-memory sessions |
| Tests fail | `dotnet test` from repo root |

---

## Roadmap (intentional next steps)

- [ ] SQLite / EF Core catalog persistence  
- [ ] Tool calling: `search_catalog` function for the model  
- [ ] RAG over blurbs/reviews  
- [ ] Auth + durable conversation store  

---

## License

Learning / portfolio project unless otherwise noted.

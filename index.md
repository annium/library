---
_layout: landing
---

# Annium

A .NET 10 library: core framework, server-side infrastructure, Blazor client pieces, third-party
integrations and exchange providers — built and released together, on one version line.

## Core

The framework proper. Everything else builds on it, and it depends on nothing else here.

- **Annium** — core utilities, the `Annium.Analyzers` build-time conventions, and `Annium.Testing`
- **Architecture** — CQRS, mediator, HTTP and view-model composition
- **Configuration** — configuration from command line, JSON and YAML
- **Core** — dependency injection, entrypoint hosting, mapper, mediator, runtime type discovery
- **Data** — models, the `IResult` operation types and their serializers, in-memory tables
- **Execution** — background execution and flow control
- **Extensions** — arguments, composition, jobs, pooling, reactive, shell, validation, workers
- **Identity** — tokens and JWT
- **Localization** — abstractions with in-memory and YAML sources
- **Logging** — console, file, in-memory, Microsoft bridge, xunit bridge
- **Net** — HTTP, mail, sockets, WebSockets, socket and web servers, type modelling
- **NodaTime** — extensions and JSON serialization for NodaTime
- **Serialization** — abstractions with JSON, MessagePack, YAML and binary-string implementations

## Server

Pieces that need infrastructure behind them.

- **Cache** — abstractions with in-memory and Redis implementations
- **Infrastructure** — service hosting
- **Mesh** — the transport: domain, client, servers and serializers
- **MessageBus** — abstractions with in-memory, Kafka, NATS and RabbitMQ implementations
- **Storage** — abstractions with file-system, in-memory and S3 implementations

## Client

- **Blazor** — components, charts, CSS, interop, routing and state
- **Components** — client-side state: core, forms and operations

## Finance

- **Providers** — exchange provider integrations, with Binance spot and USD-M futures

## Integrations

Bridges to third-party libraries and services: AspNetCore, DbUp, EntityFrameworkCore, Graylog,
linq2db, MongoDb, Redis, Seq, OpenAI, Semantic Kernel and Telegram.

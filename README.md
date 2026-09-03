# Annium

[![Merge Request](https://github.com/annium/library/actions/workflows/merge-request.yml/badge.svg)](https://github.com/annium/library/actions/workflows/merge-request.yml)

A .NET 10 library: core framework, server-side infrastructure, Blazor client pieces, third-party
integrations, exchange providers and CLI tooling — built and released together.

Six repositories used to hold this code and released independently, which meant a change in the core
travelled outward one release at a time and every consumer saw a different version of it. They are one
repository now, and one release line: every package ships the same version, and inside the repository
they reference each other as projects rather than through nuget.org.

## Layout

| Group | What it holds |
|---|---|
| `core/` | The framework proper — DI, mediator, configuration, data and result types, serialization, networking, logging, identity, testing |
| `server/` | Pieces that need infrastructure — caching, message bus, storage, the Mesh transport, hosting |
| `client/` | Blazor components and client-side state |
| `finance/` | Exchange provider integrations |
| `integrations/` | Bridges to third-party libraries and services |
| `tools/` | CLI tools — doc linting, versioning, REST client generation |

Every area follows the same shape: `<group>/<Area>/src/<Package>/` beside
`<group>/<Area>/tests/<Package>.Tests/`.

## Getting started

```bash
just              # list every recipe
just setup        # restore the dotnet tools
just build        # build everything
just test         # run the tests
```

## License

MIT — see [LICENSE](LICENSE).

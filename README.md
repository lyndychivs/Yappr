# Yappr

![Yappr](resources/yappr-social.jpg)

[![Add to Discord](https://img.shields.io/badge/Add%20to-Discord-5865F2?logo=discord&logoColor=white)](https://discord.com/oauth2/authorize?client_id=1549884846585684152&permissions=68608&integration_type=0&scope=bot+applications.commands)
[![Build & Test](https://github.com/lyndychivs/Yappr/actions/workflows/build_test.yaml/badge.svg?branch=main)](https://github.com/lyndychivs/Yappr/actions/workflows/build_test.yaml)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Flyndychivs%2FYappr%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/lyndychivs/Yappr/main)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/lyndychivs/Yappr)

A Discord bot for one job, `/tldr`. Summarising a channel's recent activity into a short, skimmable TL;DR.

Yapping, summarized.

## Projects

| Project | Purpose |
| --- | --- |
| `src/Yappr` | Core logic: window resolution, prompt building, summarizer orchestration |
| `src/Yappr.Models` | Shared DTOs and options types |
| `src/Yappr.Discord` | The bot host — NetCord gateway client, `/tldr` and `/help` slash commands |
| `src/Yappr.Mcp.Ollama` | MCP server exposing a `chat` tool backed by a local Ollama model |
| `src/Yappr.ServiceDefaults` | OpenTelemetry/health-check wiring, shared with the AppHost |
| `src/Yappr.AppHost` | .NET Aspire orchestrator for local dev (bot + Aspire dashboard) |

## Make

### Quick Start

```
cp .env.template .env
# update .env
make compose
```

Run `make help` to list all targets. Common ones:

```
make build      # dotnet build (Release)
make test       # unit + integration tests
make compose    # build and start the bot via docker compose (also pulls the configured Ollama model)
make pull-model # re-pull the configured Ollama model, e.g. after `make stop-volumes` clears its cache
make stop       # stop containers
make mutate     # Stryker mutation testing
```

# Yappr

![Yappr](resources/yappr-social.jpg)

[![Build & Test](https://github.com/lyndychivs/Yappr/actions/workflows/build_test.yaml/badge.svg?branch=main)](https://github.com/lyndychivs/Yappr/actions/workflows/build_test.yaml)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Flyndychivs%2FYappr%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/lyndychivs/Yappr/main)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/lyndychivs/Yappr)

A Discord bot with one job: `/tldr`. It summarizes a window of a channel's recent activity — by day count, hour
count, or message count — into a short, skimmable TL;DR, using an LLM reached through an MCP server.

## High Level

```
Discord user runs /tldr days|hours|messages
        │
        ▼
 Yappr.Discord (NetCord slash command)
        │
        ▼
 TldrOrchestrator (Yappr core)
        │
        ├── validate window against configured caps
        ├── IMessageFetcher → Discord REST API (channel history)
        └── ISummarizer → McpSummarizer → MCP server ("chatgpt" MCP for now)
        │
        ▼
 Summary posted back to Discord
```

`ISummarizer` is a swap point: the initial implementation (`McpSummarizer`) talks to a configured MCP server via
the official `ModelContextProtocol` .NET SDK, but nothing in the command or orchestration layer depends on MCP
specifically — a direct API-based summarizer can be dropped in later without touching `Yappr.Discord`.

## Prerequisites

| Tool | Version |
| --- | --- |
| .NET SDK | 10.0.200+ (see `global.json`) |
| Docker | for `make compose` / running the bot |
| A Discord application | with the bot token, public key, and the privileged **Message Content Intent** enabled |
| An MCP server | exposing a chat/completion tool (e.g. a "chatgpt" MCP server) |

## Projects

| Project | Purpose |
| --- | --- |
| `src/Yappr` | Core logic: window resolution, prompt building, summarizer orchestration |
| `src/Yappr.Models` | Shared DTOs and options types |
| `src/Yappr.Discord` | The bot host — NetCord gateway client, `/tldr` and `/help` slash commands |
| `src/Yappr.ServiceDefaults` | OpenTelemetry/health-check wiring, shared with the AppHost |
| `src/Yappr.AppHost` | .NET Aspire orchestrator for local dev (bot + Aspire dashboard) |

## Testing

| Test type | Command | What it covers |
| --- | --- | --- |
| Unit | `make test` | Window resolution, prompt building, summarizer logic (mocked MCP client) |
| Unit (Discord) | `make test` | `/tldr` command surface (subcommand names/attributes) |
| Integration | `make test` | `McpClientToolInvoker` against a real MCP server (`Yappr.McpTestServer`) over stdio |

## Make

Run `make help` to list all targets. Common ones:

```
make build      # dotnet build (Release)
make test       # unit + integration tests
make compose    # build and start the bot via docker compose
make logs       # follow container logs
make stop       # stop containers
make mutate     # Stryker mutation testing
```

## Quick Start

```
cp .env.template .env
# fill in DISCORD_TOKEN, DISCORD_PUBLIC_KEY, and the MCP_* variables in .env
make compose
```

## Configuration

| Variable | Purpose |
| --- | --- |
| `DISCORD_TOKEN` / `DISCORD_PUBLIC_KEY` | Discord bot credentials |
| `MCP_TRANSPORT` | `Stdio` or `Http` |
| `MCP_COMMAND` / `MCP_ARGUMENTS` | Command to launch the MCP server (stdio transport) |
| `MCP_HTTP_ENDPOINT` | URL of an already-running MCP server (http transport) |
| `MCP_TOOL_NAME` | Name of the MCP tool to invoke for summarization |
| `TLDR_MAX_DAYS` / `TLDR_MAX_HOURS` / `TLDR_MAX_MESSAGES` | Caps enforced on `/tldr` requests |

Yapping, summarized.

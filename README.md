# Yappr

![Yappr](resources/yappr-social.jpg)

[![Build & Test](https://github.com/lyndychivs/Yappr/actions/workflows/build_test.yaml/badge.svg?branch=main)](https://github.com/lyndychivs/Yappr/actions/workflows/build_test.yaml)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Flyndychivs%2FYappr%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/lyndychivs/Yappr/main)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/lyndychivs/Yappr)

A Discord bot with one job: `/tldr`. It summarizes a channel's recent activity into a short, skimmable TL;DR.

Yapping, summarized.

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

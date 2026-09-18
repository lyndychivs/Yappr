# Yappr

![Yappr](resources/yappr-social.jpg)

[![Add to Discord](https://img.shields.io/badge/Add%20to-Discord-5865F2?logo=discord&logoColor=white)](https://discord.com/oauth2/authorize?client_id=1549884846585684152&permissions=68608&integration_type=0&scope=bot+applications.commands)
[![Build & Test](https://github.com/lyndychivs/Yappr/actions/workflows/build_test.yaml/badge.svg?branch=main)](https://github.com/lyndychivs/Yappr/actions/workflows/build_test.yaml)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Flyndychivs%2FYappr%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/lyndychivs/Yappr/main)
[![Ask DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/lyndychivs/Yappr)

A Discord bot for one job, `/tldr`. Summarising a channel's recent activity (yap) into a short, skimmable TL;DR.

Yapping, summarised.

## Project

| Project | Purpose |
| --- | --- |
| `src/Yappr` | Core logic |
| `src/Yappr.Models` | Shared DTOs |
| `src/Yappr.Discord` | The bot host |
| `src/Yappr.Mcp.Ollama` | MCP server |
| `src/Yappr.ServiceDefaults` | OpenTelemetry/health-check wiring |
| `src/Yappr.AppHost` | .NET Aspire orchestrator |

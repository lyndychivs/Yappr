namespace Yappr.McpTestServer;

using System.ComponentModel;

using ModelContextProtocol.Server;

/// <summary>
/// A deterministic stand-in for a real "chatgpt" MCP tool, used by <c>Yappr.Integration.Tests</c> to exercise
/// <c>McpClientToolInvoker</c> against a real MCP server process over stdio, without depending on an external
/// LLM provider being configured.
/// </summary>
[McpServerToolType]
public sealed class EchoTools
{
    [McpServerTool(Name = "chat")]
    [Description("Echoes the given prompt back with a fixed prefix, for integration testing.")]
    public static string Chat(string prompt) => $"ECHO: {prompt}";
}

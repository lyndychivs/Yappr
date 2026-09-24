namespace Yappr.McpTestServer;

using System.ComponentModel;

using ModelContextProtocol.Server;

/// <summary>
/// Deterministic stand-in for a "chatgpt" MCP tool, used to exercise <c>McpClientToolInvoker</c> without an
/// external LLM.
/// </summary>
[McpServerToolType]
public sealed class EchoTools
{
    [McpServerTool(Name = "chat")]
    [Description("Echoes the given prompt back with a fixed prefix, for integration testing.")]
    public static string Chat(string prompt)
    {
        return $"ECHO: {prompt}";
    }

    [McpServerTool(Name = "silent")]
    [Description("Returns no text content, for exercising McpClientToolInvoker's missing-content error path.")]
    public static void Silent(string prompt)
    {
    }
}

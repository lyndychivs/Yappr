namespace Yappr.McpTestServer;

using System.ComponentModel;

using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

/// <summary>
/// Deterministic stand-in for a "chatgpt" MCP tool, used to exercise <c>McpClientToolInvoker</c> without an external LLM.
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

    [McpServerTool(Name = "multi")]
    [Description("Returns two distinct text blocks, for exercising McpClientToolInvoker's first-text-block selection.")]
    public static CallToolResult Multi(string prompt)
    {
        return new CallToolResult
        {
            Content =
            [
                new TextContentBlock { Text = "FIRST" },
                new TextContentBlock { Text = "SECOND" },
            ],
        };
    }
}

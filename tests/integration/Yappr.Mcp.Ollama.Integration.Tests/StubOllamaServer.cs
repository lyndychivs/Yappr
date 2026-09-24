namespace Yappr.Mcp.Ollama.Integration.Tests;

using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed class StubOllamaServer : IDisposable
{
    private readonly HttpListener listener = new();

    private readonly CancellationTokenSource cts = new();

    private readonly int statusCode;

    private readonly string responseBody;

    private readonly TimeSpan delay;

    public StubOllamaServer(int statusCode = 200, string responseBody = """{"response":"stubbed summary"}""", TimeSpan delay = default)
    {
        this.statusCode = statusCode;
        this.responseBody = responseBody;
        this.delay = delay;

        int port = GetFreeTcpPort();
        Address = string.Create(CultureInfo.InvariantCulture, $"http://127.0.0.1:{port}");
        listener.Prefixes.Add($"{Address}/");
        listener.Start();
        _ = AcceptLoopAsync();
    }

    public string Address { get; }

    public void Dispose()
    {
        cts.Cancel();
        listener.Close();
        cts.Dispose();
    }

    private static int GetFreeTcpPort()
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.LocalEndPoint!).Port;
    }

    private async Task AcceptLoopAsync()
    {
        try
        {
            while (!cts.IsCancellationRequested)
            {
                HttpListenerContext context = await listener.GetContextAsync().WaitAsync(cts.Token);

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cts.Token);
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseBody);
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, cts.Token);
                context.Response.OutputStream.Close();
            }
        }
        catch (Exception ex) when (cts.IsCancellationRequested
            && ex is OperationCanceledException
            or ObjectDisposedException
            or HttpListenerException)
        {
        }
    }
}

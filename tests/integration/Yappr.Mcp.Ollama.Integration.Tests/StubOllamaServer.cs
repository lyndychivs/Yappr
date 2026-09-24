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
    private readonly HttpListener _listener = new();

    private readonly CancellationTokenSource _cts = new();

    private readonly int _statusCode;

    private readonly string _responseBody;

    private readonly TimeSpan _delay;

    private readonly int _failuresBeforeSuccess;

    private int _requestCount;

    public StubOllamaServer(
        int statusCode = 200,
        string responseBody = """{"response":"stubbed summary"}""",
        TimeSpan delay = default,
        int failuresBeforeSuccess = 0)
    {
        _statusCode = statusCode;
        _responseBody = responseBody;
        _delay = delay;
        _failuresBeforeSuccess = failuresBeforeSuccess;

        int port = GetFreeTcpPort();
        Address = string.Create(CultureInfo.InvariantCulture, $"http://127.0.0.1:{port}");
        _listener.Prefixes.Add($"{Address}/");
        _listener.Start();
        _ = AcceptLoopAsync();
    }

    public string Address { get; }

    /// <summary>
    /// Gets the number of requests received so far.
    /// </summary>
    public int RequestCount => _requestCount;

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Close();
        _cts.Dispose();
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
            while (!_cts.IsCancellationRequested)
            {
                HttpListenerContext context = await _listener.GetContextAsync().WaitAsync(_cts.Token);
                int requestNumber = Interlocked.Increment(ref _requestCount);

                if (_delay > TimeSpan.Zero)
                {
                    await Task.Delay(_delay, _cts.Token);
                }

                bool isTransientFailure = requestNumber <= _failuresBeforeSuccess;
                string body = isTransientFailure ? """{"error":"temporarily unavailable"}""" : _responseBody;
                byte[] buffer = Encoding.UTF8.GetBytes(body);
                context.Response.StatusCode = isTransientFailure ? 503 : _statusCode;
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, _cts.Token);
                context.Response.OutputStream.Close();
            }
        }
        catch (Exception ex) when (_cts.IsCancellationRequested
            && ex is OperationCanceledException
            or ObjectDisposedException
            or HttpListenerException)
        {
        }
    }
}

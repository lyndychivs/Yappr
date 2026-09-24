namespace Yappr.Discord.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

using Yappr.Models.Dto;
using Yappr.Summarization;
using Yappr.Windowing;

/// <summary>
/// Fetches channel history from Discord, walking backwards from the newest message until the count or time
/// cutoff is met.
/// </summary>
public sealed class NetCordMessageFetcher(GatewayClient gatewayClient) : IMessageFetcher
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ChannelMessage>> FetchAsync(ulong channelId, TldrWindowResolution window, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(window);

        Channel channel = await gatewayClient.Rest.GetChannelAsync(channelId, cancellationToken: cancellationToken);
        if (channel is not TextChannel textChannel)
        {
            return [];
        }

        var messages = new List<ChannelMessage>();

        await foreach (RestMessage message in textChannel.GetMessagesAsync().WithCancellation(cancellationToken))
        {
            if (message.Author.IsBot || string.IsNullOrWhiteSpace(message.Content))
            {
                continue;
            }

            if (window.SinceUtc is { } sinceUtc && message.CreatedAt < sinceUtc)
            {
                break;
            }

            messages.Add(new ChannelMessage(message.Author.Username, message.Content, message.CreatedAt));

            if (window.MessageLimit is { } messageLimit && messages.Count >= messageLimit)
            {
                break;
            }
        }

        messages.Reverse();
        return messages;
    }
}

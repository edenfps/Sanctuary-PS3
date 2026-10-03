using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Game;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class PacketClientIsReadyHandler
{
    private static ILogger _logger = null!;
    private static IResourceManager _resourceManager = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(PacketClientIsReadyHandler));

        _resourceManager = serviceProvider.GetRequiredService<IResourceManager>();
    }

    public static bool HandlePacket(GatewayConnection connection)
    {
        _logger.LogTrace("Received {name} packet.", nameof(PacketClientIsReady));

        if (connection.IsPs3Client)
            _logger.LogInformation("PS3 client ready: zone {ZoneType} ({ZoneId}), player {PlayerGuid}.",
                connection.Player.Zone.GetType().Name, connection.Player.Zone.Id, connection.Player.Guid);

        connection.Player.Zone.OnClientIsReady(connection.Player);

        return true;
    }
}

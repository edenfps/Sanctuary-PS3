using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Sanctuary.Core.IO;
using Sanctuary.Packet;
using Sanctuary.Packet.Common.Attributes;

namespace Sanctuary.Gateway.Handlers;

[PacketHandler]
public static class BaseVehicleRacePacketHandler
{
    private static ILogger _logger = null!;

    public static void ConfigureServices(IServiceProvider serviceProvider)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = loggerFactory.CreateLogger(nameof(BaseVehicleRacePacketHandler));
    }

    public static bool HandlePacket(GatewayConnection connection, PacketReader reader)
    {
        if (!reader.TryRead(out short subOpCode))
        {
            _logger.LogError("Failed to read VehicleRace sub-opcode. Data: {data}", Convert.ToHexString(reader.Span));
            return false;
        }

        var data = reader.RemainingSpan;

        return subOpCode switch
        {
            VehicleRacePacketUpdateVehicleRaceServerData.OpCode =>
                VehicleRacePacketUpdateVehicleRaceServerDataHandler.HandlePacket(connection, data),
            VehicleRacePacketRegisterVehicleRacePlayer.OpCode =>
                VehicleRacePacketRegisterVehicleRacePlayerHandler.HandlePacket(connection, data),
            _ => HandleUnhandled(subOpCode, data)
        };
    }

    private static bool HandleUnhandled(short subOpCode, ReadOnlySpan<byte> data)
    {
        _logger.LogInformation("Unhandled VehicleRace sub-opcode {subOpCode}. Payload: {payload}",
            subOpCode, Convert.ToHexString(data));
        return true;
    }
}

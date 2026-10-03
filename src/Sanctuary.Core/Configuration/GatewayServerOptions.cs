namespace Sanctuary.Core.Configuration;

public sealed class GatewayServerOptions : ServerOptions
{
    /// <example>live</example>
    public required string Environment { get; set; }

    /// <summary>
    /// Client version the server supports.
    /// </summary>
    /// <example>1.910.1.530630</example>
    public required string ClientVersion { get; set; }

    /// <summary>
    /// Client version for the 2009 client.
    /// </summary>
    /// <example>1.227.365.326.1135.280846</example>
    public string? ClientVersion2009 { get; set; }

    public required string ServerAddress { get; set; }

    public required string LoginGatewayAddress { get; set; }
    public required string LoginGatewayChallenge { get; set; }

    public bool ShowMemberNagScreen { get; set; }
}
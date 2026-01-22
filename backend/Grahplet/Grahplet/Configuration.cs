using System;
using System.Net;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Grahplet;

// 3 allowed service types are: "API", "EVENT", "SESSION_WORKER"

public enum ServiceType { API, EVENT, SESSION_WORKER }

public sealed class Configuration
{
    // Singleton instance
    private static readonly object _lock = new();
    private static Configuration? _instance;
    public static Configuration Instance
    {
        get
        {
            if (_instance != null) return _instance;
            lock (_lock)
            {
                _instance ??= new Configuration();
            }
            return _instance;
        }
    }

    public IPAddress HostAddress { get; private set; } = IPAddress.Loopback;
    public int Port { get; private set; } = 5000;
    public IReadOnlyList<ServiceType> AllowedServiceTypes { get; private set; } = new List<ServiceType> { ServiceType.API };
    
    // NATS configuration
    public string NatsUrl { get; private set; } = string.Empty;
    
    // Database configuration
    public string ConnectionString { get; private set; } = string.Empty;
    
    // Auth Configuration
    // *Session length in days*
    public int SessionLengthDays { get; private set; } = 7;

    // Load configuration from standard ASP.NET Core IConfiguration (appsettings.json, env vars, etc.)
    // Supported keys:
    // "HostAddress": "127.0.0.1"
    // "Port": 8080
    // "AllowedServiceTypes": ["API","EVENT","SESSION_WORKER"]
    // "SessionLengthDays": 14
    // "Nats:Url": "nats://localhost:4222"
    // "Database:ConnectionString": "..."
    public static void LoadFromConfiguration(IConfiguration configuration)
    {
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        var inst = Instance;

        var hostStr = configuration["HostAddress"];
        if (!string.IsNullOrWhiteSpace(hostStr) && IPAddress.TryParse(hostStr, out var ip))
        {
            inst.HostAddress = ip;
        }

        if (int.TryParse(configuration["Port"], out var port) && port > 0)
        {
            inst.Port = port;
        }

        if (int.TryParse(configuration["SessionLengthDays"], out var sessionDays) && sessionDays > 0)
        {
            inst.SessionLengthDays = sessionDays;
        }

        var allowedSection = configuration.GetSection("AllowedServiceTypes");
        if (allowedSection.Exists())
        {
            var types = new List<ServiceType>();
            foreach (var child in allowedSection.GetChildren())
            {
                var value = child.Value;
                if (!string.IsNullOrWhiteSpace(value) && Enum.TryParse<ServiceType>(value, true, out var st))
                {
                    types.Add(st);
                }
            }
            if (types.Count > 0)
            {
                inst.AllowedServiceTypes = types.AsReadOnly();
            }
        }

        var natsUrl = configuration["Nats:Url"];
        if (!string.IsNullOrWhiteSpace(natsUrl))
        {
            inst.NatsUrl = natsUrl;
        }

        var connStr = configuration["Database:ConnectionString"];
        if (!string.IsNullOrWhiteSpace(connStr))
        {
            inst.ConnectionString = connStr;
        }
    }
}

using MediaServer.ICE.Interfaces;
using MediaServer.ICE.Models;
using MediaServer.ICE.Services;
using MediaServer.Kernel;
using MediaServer.Kernel.Interfaces;
using MediaServer.Kernel.Middlewares;
using MediaServer.Kernel.Models;
using MediaServer.Kernel.Services;
using MediaServer.Media.Interfaces;
using MediaServer.Media.Services; 
using MediaServer.SDP.Interfaces;
using MediaServer.SDP.Services;
using MediaServer.SignalizationServer;
using MediaServer.SignalizationServer.Interfaces;
using MediaServer.SignalizationServer.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

public static class BuilderExtensions
{
    public class MediaServeroptions
    {

        public LogRotationOptions LogRotationOptions { get; set; } = new LogRotationOptions()
        {
            MaxTotalLogSizeMB = 500,
            MaxLogAge = TimeSpan.FromDays(30),
            LogDirectory = "logs"
        };

        public CandidatePrioritizationOptions CandidatePrioritizationOptions { get; set; } = new CandidatePrioritizationOptions();

    }
    public static void AddMediaServerSignaller(this IServiceCollection services, Action<MediaServeroptions> options)
    {

        var ops = new MediaServeroptions();
        options(ops);


        services.Configure<StunClientOptions>(op =>
        {

            op.MaxPoolSize = 100;
            op.ConnectTimeout = 5000;
            op.ReceiveTimeout = 5000;
            op.SendTimeout = 5000;
        });

        services.Configure<ErrorManagementOptions>(op =>
        {
            op.RetryDelay = TimeSpan.FromMicroseconds(5000);
            op.NotificationChannels = new List<string> { "email", "sms" };
            op.MaxRetryAttempts = 3;
            op.EnableDetailedLogging = true;
        });

        services.Configure<CandidatePrioritizationOptions>((op) =>
        {
            op.PacketLossWeightFactor = ops.CandidatePrioritizationOptions.PacketLossWeightFactor;
            op.BandwidthWeightFactor = ops.CandidatePrioritizationOptions.BandwidthWeightFactor;
            op.LatencyWeightFactor = ops.CandidatePrioritizationOptions.LatencyWeightFactor;
            op.BasePeerReflexivePriority = ops.CandidatePrioritizationOptions.BasePeerReflexivePriority;
            op.BaseHostPriority = ops.CandidatePrioritizationOptions.BaseHostPriority;
            op.BaseServerReflexivePriority = ops.CandidatePrioritizationOptions.BaseServerReflexivePriority;
            op.BaseRelayedPriority = ops.CandidatePrioritizationOptions.BaseRelayedPriority;
            op.GeographicalProximityWeightFactor = ops.CandidatePrioritizationOptions.GeographicalProximityWeightFactor;

        });

        services.AddHttpClient();
        //services.AddSingleton<SignalizationServer>();
        services.AddSingleton<IWebSocketManager, MediaServer.SignalizationServer.WebSocketManager>();
        services.AddSingleton<IPeerDiscoveryService, PeerDiscoveryService>();
        services.AddSingleton<IWebSocketMediator, WebSocketMediator>();



        services.AddSingleton<INetworkInterfaceService, DefaultNetworkInterfaceService>();
        services.AddSingleton<IStunClient, DefaultStunClient>();
        services.AddSingleton<ITurnClient, DefaultTurnClient>();

        services.AddSingleton<ICECandidateCollector>();
        services.AddSingleton<IICECandidateProvider, LocalICECandidateProvider>();
        services.AddSingleton<IICECandidateProvider, StunICECandidateProvider>();
        services.AddSingleton<IICECandidateProvider, TurnICECandidateProvider>();


        services.AddSingleton<ISDPParser, SDPParser>();
        services.AddSingleton<ISDPGenerator, SDPGenerator>();
        services.AddSingleton<ISDPValidator, SDPValidator>();
        services.AddSingleton<ISDPProcessor, SDPProcessor>();


        services.AddSingleton<IConnectionManager, ConnectionManager>();
        services.AddSingleton<SDPManager>();
        services.AddSingleton<ISDPHandler, SDPHandler>();

        services.AddSingleton<MediaServer.Media.Interfaces.IMediaRouter, MediaRouterService>();
        services.AddSingleton<IMediaHandler, MediaHandler>();
        services.AddTransient<AudioMediaHandler>();
        services.AddTransient<VideoMediaHandler>();
        services.AddSingleton<IMediaHandlerFactory, MediaHandlerFactory>();

        services.AddSingleton<IErrorClassifier, DefaultErrorClassifier>();
        services.AddSingleton<IErrorLogger, DefaultErrorLogger>();
        services.AddSingleton<ITelemetryService, TelemetryService>();
        services.AddSingleton<IErrorNotificationService, DefaultErrorNotificationService>();
        services.AddSingleton<IErrorHandler, CentralErrorManager>();


        services.Configure<LogRotationOptions>(op =>
        {
            op.MaxTotalLogSizeMB = ops.LogRotationOptions.MaxTotalLogSizeMB;
            op.MaxLogAge = ops.LogRotationOptions.MaxLogAge;
            op.LogDirectory = ops.LogRotationOptions.LogDirectory;

        });
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<ILogRotationPolicy, DefaultLogRotationPolicy>();

        services.AddSingleton<ILogFileService, LogFileService>();


        services.AddSingleton<INetworkConditionService, AdvancedNetworkConditionService>();
        services.AddSingleton<IGeoLocationService, AdvancedGeoLocationService>();
        services.AddSingleton<ICandidatePrioritizationService, AdvancedCandidatePrioritizationService>();
        services.AddSingleton<CandidatePrioritizationManager>();
        services.AddSingleton<ICECandidateManager>();


        
        //});
    }

    public static void UseMediaServer(this WebApplication app)
    {
        app.UseMiddleware<MediaServerMiddleware>();
    }
}


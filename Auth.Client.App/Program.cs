using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Sila2.Org.Silastandard.Examples.Greetingprovider.V1;
using SiLA2.Client;
using Sila2.Org.Silastandard.Core.Silaservice.V1;
using SiLA2.Utils.Extensions;
using SiLA2.Utils.Network;
using SiLA2Framework = Sila2.Org.Silastandard;

namespace Auth.Client.App;

internal class Program
{
    private static IConfigurationRoot _configuration;

    // Entry point for the application. Using an async Main allows awaiting asynchronous operations without blocking.
    static async Task Main(string[] args)
    {
        // Build configuration from appsettings.json and environment variables.
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
        _configuration = configBuilder.Build();

        // Configure dependency injection and services.
        var clientSetup = new Configurator(_configuration, args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        clientSetup.Container.AddLogging(x =>
        {
            x.ClearProviders();
            x.AddSerilog(dispose: true);
        });
        clientSetup.UpdateServiceProvider();

        var logger = clientSetup.ServiceProvider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("Starting Server Discovery...");

        // Discover available SiLA2 servers.
        var serverMap = await clientSetup.SearchForServers();

        GrpcChannel channel;
        const string serverType = "SiLA2AuthenticationServer";
        var server = serverMap.Values.FirstOrDefault(x => x.ServerType == serverType);

        if (server != null)
        {
            logger.LogInformation("Found Server");
            logger.LogInformation(server.ServerInfo);
            logger.LogInformation($"Connecting to {server}");
            channel = await clientSetup.GetChannel(
                server.Address,
                server.Port,
                acceptAnyServerCertificate: false,
                server.SilaCA.GetCaFromFormattedCa());
        }
        else
        {
            var clientConfig = clientSetup.ServiceProvider.GetService<IClientConfig>();
            logger.LogInformation(
                $"No connection automatically discovered. Using Server-URI '{clientConfig.IpOrCdirOrFullyQualifiedHostName}:{clientConfig.Port}' from ClientConfig");
            channel = await clientSetup.GetChannel(acceptAnyServerCertificate: true);
        }

        var siLaServiceClient = new SiLAService.SiLAServiceClient(channel);
        var silaServiceNameResponse = await siLaServiceClient.Get_ServerNameAsync(new());
        logger.LogInformation(
            $"Connected to SiLA2 Server '{silaServiceNameResponse.ServerName.Value}'");

        var silaServiceDescriptionResponse = await siLaServiceClient.Get_ServerDescriptionAsync(new());
        logger.LogInformation(
            $"SiLA2 Server Description: {silaServiceDescriptionResponse.ServerDescription.Value}");
        
        //var greeterClient = new GreetingProvider.GreetingProviderClient(channel);

        //var getStartYearResponse = await greeterClient.Get_StartYearAsync(new ());
        
        //logger.LogInformation($"This is year {getStartYearResponse.StartYear.Value}");
        
        //Console.WriteLine("Please enter name: ");
        //string name = Console.ReadLine();
        //name = string.IsNullOrEmpty(name) ? "Dave Lombardo" : name;
        
        //var sayHelloResponse = await greeterClient.SayHelloAsync(new SayHello_Parameters { Name = new SiLA2Framework.String { Value = name }});

        //logger.LogInformation($"Response from Server : {sayHelloResponse.Greeting.Value}");
        
        // var authenticationClient = new AuthenticationService.AuthenticationServiceClient(channel);
        // var loginRequest = new Login_Parameters
        // {
        //     UserIdentification = new SiLA2Framework.String { Value = "User" },
        //     Password = new SiLA2Framework.String { Value = "User" },
        //     RequestedServer = new SiLA2Framework.String { Value = "FB5BE0BF-42DB-4A5D-A325-789556E22825" }
        // };

        // Login_Responses loginResponse;
        // try
        // {
        //     loginResponse = authenticationClient.Login(loginRequest);
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ErrorHandling.HandleException(ex));
        //
        //     Console.WriteLine();
        //     Console.WriteLine("Press any key to exit...");
        //
        //     Console.ReadKey();
        //     return;
        // }

        // logger.LogInformation("Calling 'SayHello' with valid access token");
        //
        // var key = SilaClientMetadata.ConvertMetadataIdentifierToWireFormat("org.silastandard/core/AuthorizationService/v1/MetaData/AccessToken");
        // var authToken = new Metadata_AccessToken { AccessToken = loginResponse.AccessToken };
        // var metadata = new Metadata { new Metadata.Entry(key, authToken.ToByteArray()) };
        //
        // var greetingProviderClient = new GreetingProvider.GreetingProviderClient(channel);
        // var response = greetingProviderClient.SayHello(new SayHello_Parameters { Name = new SiLA2Framework.String { Value = "SiLA2" } }, metadata);
        //
        // logger.LogInformation($"Response from Server => '{response.Greeting.Value}'");
        // logger.LogInformation("Calling 'SayHello' with valid access token succeeded...");
        //
        // Console.WriteLine();
        // Console.BackgroundColor = ConsoleColor.DarkRed;
        // Console.Write("Now testing errors...");
        // Console.ResetColor();
        // Console.WriteLine();
        //
        // logger.LogInformation("Calling 'SayHello' without access token...");
        // try
        // {
        //     greetingProviderClient.SayHello(new SayHello_Parameters());
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ErrorHandling.HandleException(ex));
        // }
        //
        // var invalidAuthToken = new Metadata_AccessToken { AccessToken = new SiLA2Framework.String { Value = "InvalidToken" } };
        // metadata = new Metadata { new Metadata.Entry(key, invalidAuthToken.ToByteArray()) };
        //
        // Console.WriteLine();
        // logger.LogInformation("Calling 'SayHello' with invalid access token...");
        // try
        // {
        //     greetingProviderClient.SayHello(new SayHello_Parameters(), metadata);
        // }
        // catch (Exception ex)
        // {
        //     logger.LogError(ErrorHandling.HandleException(ex));
        // }

        // Wait for user input before exiting.
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
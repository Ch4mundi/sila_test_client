using Grpc.Core;
using SiLA2;
using Sila2.Org.Silastandard;
using Sila2.Org.Silastandard.Core.Authorizationservice.V1;
using Sila2.Org.Silastandard.Examples.Greetingprovider.V1;
using SiLA2.Server;
using SiLA2.Server.Utils;
using String = Sila2.Org.Silastandard.String;

public class GreeterService : GreetingProvider.GreetingProviderBase
{
    private readonly Feature _siLA2Feature;

    public GreeterService(ISiLA2Server siLA2Server)
    {
        _siLA2Feature = siLA2Server.ReadFeature(Path.Combine("Features", "GreetingProvider-v1_0.sila.xml"));
    }

    public override Task<SayHello_Responses> SayHello(SayHello_Parameters request, ServerCallContext context)
    {
        try
        {
            return Task.FromResult(new SayHello_Responses { Greeting = new String { Value = "Hello " + request.Name.Value } });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public override Task<Get_StartYear_Responses> Get_StartYear(Get_StartYear_Parameters request, ServerCallContext context)
    {
        try
        {
            return Task.FromResult(new Get_StartYear_Responses { StartYear = new Integer { Value = DateTime.Now.Year } });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}
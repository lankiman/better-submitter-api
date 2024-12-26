namespace better_submitter_api;


using System.Threading;
using System.Threading.Tasks;
using Makaretu.Dns;

public class CustomMDnsService : IHostedService
{
    private MulticastService mdns;
    private readonly DomainName backendServiceName = new DomainName("better-submitter-api.local");
    private readonly DomainName frontendServiceName = new DomainName("better-submitter.local");

    public  Task StartAsync(CancellationToken token)
    {
        mdns = new MulticastService();
        var sd = new ServiceDiscovery(mdns);

        // Announce backend service
        var backendProfile = new ServiceProfile(backendServiceName, "_http._tcp", 5239);
        sd.Advertise(backendProfile);

        // Announce frontend service
        var frontendProfile = new ServiceProfile(frontendServiceName, "_http._tcp", 5500);
        sd.Advertise(frontendProfile);

        // Start mDNS asynchronously
        mdns.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken token)
    {
        mdns.Stop();
        return Task.CompletedTask;
    }
}


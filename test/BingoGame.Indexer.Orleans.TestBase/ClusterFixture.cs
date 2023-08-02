using Microsoft.Extensions.Configuration;
using Orleans;
using Orleans.Hosting;
using Orleans.TestingHost;
using Volo.Abp.DependencyInjection;

namespace BingoGame.Indexer.Orleans.TestBase;

public class ClusterFixture:IDisposable,ISingletonDependency
{
    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<TestSiloConfigurations>();
        builder.AddClientBuilderConfigurator<TestClientBuilderConfigurator>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose()
    {
        Cluster.StopAllSilos();
    }

    public TestCluster Cluster { get; private set; }
    
    private class TestSiloConfigurations : ISiloConfigurator
    {
        
        public void Configure(ISiloBuilder siloBuilder)
        {
           
        siloBuilder.ConfigureServices(_ => {
                // services.AddSingleton<ITestGrain,TestGrain>();
            })
            .AddSimpleMessageStreamProvider(BingoGameIndexerOrleansConsts.MessageStreamName)
            .AddMemoryGrainStorage("PubSubStore")
            .AddMemoryGrainStorageAsDefault();
            }
        
    }
    
    private class TestClientBuilderConfigurator : IClientBuilderConfigurator
    {
        public void Configure(IConfiguration configuration, IClientBuilder clientBuilder) => clientBuilder
            .AddSimpleMessageStreamProvider(BingoGameIndexerOrleansConsts.MessageStreamName);
    }
}
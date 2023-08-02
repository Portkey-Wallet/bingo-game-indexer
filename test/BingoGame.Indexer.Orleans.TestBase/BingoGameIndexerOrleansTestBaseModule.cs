using Microsoft.Extensions.DependencyInjection;
using Orleans;
using BingoGame.Indexer.TestBase;
using Volo.Abp;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace BingoGame.Indexer.Orleans.TestBase;

[DependsOn(typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(BingoGameIndexerTestBaseModule)
    )]
public class BingoGameIndexerOrleansTestBaseModule:AbpModule
{
    private readonly ClusterFixture _fixture = new();
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // var fixture = new ClusterFixture();
        context.Services.AddSingleton(_fixture);
        context.Services.AddSingleton<IClusterClient>(_ => _fixture.Cluster.Client);
    }
}
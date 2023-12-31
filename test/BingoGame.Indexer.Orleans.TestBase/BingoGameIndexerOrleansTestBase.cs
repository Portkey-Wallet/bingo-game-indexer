using Orleans.TestingHost;
using BingoGame.Indexer.TestBase;
using Volo.Abp.Modularity;

namespace BingoGame.Indexer.Orleans.TestBase;

public abstract class BingoGameIndexerOrleansTestBase<TStartupModule>:BingoGameIndexerTestBase<TStartupModule> 
    where TStartupModule : IAbpModule
{
    protected readonly TestCluster Cluster;

    public BingoGameIndexerOrleansTestBase()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        Cluster = GetRequiredService<ClusterFixture>().Cluster;
    }
}
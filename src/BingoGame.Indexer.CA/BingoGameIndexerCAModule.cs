using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.DependencyInjection;
using BingoGame.Indexer.CA.GraphQL;
using BingoGame.Indexer.CA.Handlers;
using BingoGame.Indexer.CA.Processors;
using Volo.Abp.Modularity;

namespace BingoGame.Indexer.CA;


[DependsOn(typeof(AElfIndexerClientModule))]
public class BingoGameIndexerCAModule:AElfIndexerClientPluginBaseModule<BingoGameIndexerCAModule, BingoGameIndexerCASchema, Query>
{
    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        var configuration = serviceCollection.GetConfiguration();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, BingoedProcessor>();
        serviceCollection.AddSingleton<IAElfLogEventProcessor<TransactionInfo>, PlayedProcessor>();
        serviceCollection.AddTransient<IBlockChainDataHandler, BingoGameHandler>();

        Configure<ContractInfoOptions>(configuration.GetSection("ContractInfo"));
    }

    protected override string ClientId => "";
    protected override string Version => "";

}
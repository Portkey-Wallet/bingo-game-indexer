using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Microsoft.Extensions.DependencyInjection;
using BingoGame.Indexer.CA.GraphQL;
using BingoGame.Indexer.CA.Processors;
using Volo.Abp.Modularity;
using Portkey.Indexer.CA.Handlers;

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

    protected override string ClientId => "BingoGame_DApp";
    protected override string Version => "9ee98e991aa448f5972c55f44ae35013";

}
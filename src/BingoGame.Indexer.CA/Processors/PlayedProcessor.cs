using AElfIndexer.Client;
using AElfIndexer.Client.Handlers;
using AElfIndexer.Grains.State.Client;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portkey.Contracts.BingoGameContract;
using BingoGame.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace BingoGame.Indexer.CA.Processors;

public class PlayedProcessor : BingoGameProcessorBase<Played>
{
    private readonly IAElfIndexerClientEntityRepository<BingoGameIndexEntry, TransactionInfo> _bingoIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<BingoGamestatsIndex, TransactionInfo> _bingostatsIndexRepository;
    public PlayedProcessor(ILogger<PlayedProcessor> logger,
        IAElfIndexerClientEntityRepository<BingoGameIndexEntry, TransactionInfo> bingoIndexRepository,
        IAElfIndexerClientEntityRepository<BingoGamestatsIndex, TransactionInfo> bingostatsIndexRepository,
        IOptionsSnapshot<ContractInfoOptions> contractInfoOptions,
        IObjectMapper objectMapper) :
        base(logger,objectMapper,contractInfoOptions)
    {
        _bingoIndexRepository = bingoIndexRepository;
        _bingostatsIndexRepository = bingostatsIndexRepository;
    }

    public override string GetContractAddress(string chainId)
    {
        return ContractInfoOptions.ContractInfos.First(c=>c.ChainId == chainId).BingoGameContractAddress;
    }
    public class PlayEventAlreadyHandledException : Exception
    {
    public PlayEventAlreadyHandledException() : base("This event is already handled.")
    {
        return;
    }
    }
    

    protected override async Task HandleEventAsync(Played eventValue, LogEventContext context)
    {   
        if (eventValue.PlayerAddress == null || eventValue.PlayerAddress.Value == null)
        {
            return;
        }

        var index = await _bingoIndexRepository.GetFromBlockStateSetAsync(eventValue.PlayId.ToHex(), context.ChainId);
        // we will throw exception if index is not null, because we should not have handled this event before
        if (index != null)
        {
            throw new PlayEventAlreadyHandledException();
        }
        var feeMap = GetTransactionFee(context.ExtraProperties);
        List<TransactionFee> feeList;
        if (!feeMap.IsNullOrEmpty())
        {
            feeList = feeMap.Select(pair => new TransactionFee
            {
                Symbol = pair.Key,
                Amount = pair.Value
            }).ToList();
        }
        else
        {
            feeList = new List<TransactionFee>
            {
                new ()
                {
                    Symbol = null,
                    Amount = 0
                }
            };
        }
        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        var bingoIndex = new BingoGameIndexEntry
        {
            Id = eventValue.PlayId.ToHex(),
            PlayBlockHeight = eventValue.PlayBlockHeight,
            Amount = eventValue.Amount,
            IsComplete = false,
            PlayId = context.TransactionId,
            BingoType = (int)eventValue.Type,
            Dices = new List<int>{},
            PlayerAddress = eventValue.PlayerAddress.ToBase58(),
            PlayTime = context.BlockTime.ToTimestamp().Seconds,
            PlayTransactionFee = feeList,
            PlayBlockHash = context.BlockHash
        };
        ObjectMapper.Map<LogEventContext, BingoGameIndexEntry>(context, bingoIndex);
        await _bingoIndexRepository.AddOrUpdateAsync(bingoIndex);
    }
}
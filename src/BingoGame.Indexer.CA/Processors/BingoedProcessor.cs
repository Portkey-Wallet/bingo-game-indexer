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

public class BingoedProcessor : BingoGameProcessorBase<Bingoed>
{   

    private readonly IAElfIndexerClientEntityRepository<BingoGameIndexEntry, TransactionInfo> _bingoIndexRepository;
    private readonly IAElfIndexerClientEntityRepository<BingoGamestatsIndexEntry, TransactionInfo> _bingostatsIndexRepository;
    public BingoedProcessor(ILogger<BingoedProcessor> logger,
        IAElfIndexerClientEntityRepository<BingoGameIndexEntry, TransactionInfo> bingoIndexRepository,
        IAElfIndexerClientEntityRepository<BingoGamestatsIndexEntry, TransactionInfo> bingostatsIndexRepository,
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

    public class BingoGameIndexEntryNotFoundException : Exception
    {
    public BingoGameIndexEntryNotFoundException() : base("Bingo index not found.")
    {
        return;
    }
    }

    protected override async Task HandleEventAsync(Bingoed eventValue, LogEventContext context)
    {
        if (eventValue.PlayerAddress == null || eventValue.PlayerAddress.Value == null)
        {
            return;
        }
        //update bingoIndex
        var index = await _bingoIndexRepository.GetFromBlockStateSetAsync(eventValue.PlayId.ToHex(), context.ChainId);
        // we will throw exception if index is null, because we should have played event after bingoed event
        if (index == null)
        {
            throw new BingoGameIndexEntryNotFoundException();
        }
        // _objectMapper.Map<LogEventContext, CAHolderIndex>(context, caHolderIndex);
        index.BingoBlockHeight = context.BlockHeight;
        index.BingoId = context.TransactionId;
        index.BingoTime = context.BlockTime.ToTimestamp().Seconds;
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
        index.BingoTransactionFee = feeList;
        index.IsComplete = true;
        index.Dices = eventValue.Dices.Dices.ToList();
        index.Award = eventValue.Award;
        index.BingoBlockHash = context.BlockHash;
        ObjectMapper.Map<LogEventContext, BingoGameIndexEntry>(context, index);
        await _bingoIndexRepository.AddOrUpdateAsync(index);
        
        //update bingostatsIndex
        var statsId= IdGenerateHelper.GetId(context.ChainId, eventValue.PlayerAddress.ToBase58());
        var bingostatsIndex = await _bingostatsIndexRepository.GetFromBlockStateSetAsync(statsId, context.ChainId);
        if (bingostatsIndex == null)
        {
            bingostatsIndex = new BingoGamestatsIndexEntry
            {
                Id = statsId,
                PlayerAddress = eventValue.PlayerAddress.ToBase58(),
                Amount = eventValue.Amount,
                Award = eventValue.Award,
                TotalWins = eventValue.Award > 0 ? 1 : 0,
                TotalPlays = 1
            };
        }
        else
        {
            bingostatsIndex.Amount += eventValue.Amount;
            bingostatsIndex.Award += eventValue.Award;
            bingostatsIndex.TotalPlays += 1;
            bingostatsIndex.TotalWins += eventValue.Award > 0 ? 1 : 0;
        }
        ObjectMapper.Map<LogEventContext, BingoGamestatsIndexEntry>(context, bingostatsIndex);
        await _bingostatsIndexRepository.AddOrUpdateAsync(bingostatsIndex); 
    }
}

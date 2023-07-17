using AElfIndexer.Client;
using AElfIndexer.Grains.State.Client;
using GraphQL;
using Nest;
using BingoGame.Indexer.CA.Entities;
using Volo.Abp.ObjectMapping;

namespace BingoGame.Indexer.CA.GraphQL;

public class Query
{
    public static async Task<BingoResultDto> BingoGameInfo(
        [FromServices] IAElfIndexerClientEntityRepository<BingoGameIndexEntry, LogEventInfo> repository,
        [FromServices] IAElfIndexerClientEntityRepository<BingoGamestatsIndexEntry, LogEventInfo> statsrepository,
        [FromServices] IObjectMapper objectMapper,  GetBingoDto dto)
    {
        var infoQuery = new List<Func<QueryContainerDescriptor<BingoGameIndexEntry>, QueryContainer>>();

        infoQuery.Add(q => q.Term(i => i.Field(f => f.PlayId).Value(dto.PlayId)));
        infoQuery.Add(q => q.Term(i => i.Field(f => f.IsComplete).Value(true)));

        if (dto.CAAddresses != null)
        {
            infoQuery.Add(q => q.Terms(i => i.Field(f => f.PlayerAddress).Terms(dto.CAAddresses)));
        }

        

        var result = await repository.GetSortListAsync(
            f => f.Bool(b => b.Must(infoQuery)), 
            sortFunc: s => s.Descending(a => a.BingoBlockHeight), 
            skip: dto.SkipCount,
            limit: dto.MaxResultCount);
        var dataList = objectMapper.Map<List<BingoGameIndexEntry>, List<BingoInfo>>(result.Item2);
        
        var statsQuery = new List<Func<QueryContainerDescriptor<BingoGamestatsIndexEntry>, QueryContainer>>();
        if (dto.CAAddresses != null)
        {
            statsQuery.Add(q => q.Terms(i => i.Field(f => f.PlayerAddress).Terms(dto.CAAddresses)));
        }

        QueryContainer statsFilter(QueryContainerDescriptor<BingoGamestatsIndexEntry> f) => f.Bool(b => b.Must(statsQuery));
        var statsResult = await statsrepository.GetListAsync(statsFilter);
        var statsDataList = objectMapper.Map<List<BingoGamestatsIndexEntry>, List<Bingostats>>(statsResult.Item2);

        var pageResult = new BingoResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList,
            stats = statsDataList,
        };
        return pageResult;
    }
}
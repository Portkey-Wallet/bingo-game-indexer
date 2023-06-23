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
        [FromServices] IAElfIndexerClientEntityRepository<BingoGamestatsIndex, LogEventInfo> statsrepository,
        [FromServices] IObjectMapper objectMapper,  GetBingoDto dto)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<BingoGameIndexEntry>, QueryContainer>>();

        mustQuery.Add(q => q.Term(i => i.Field(f => f.PlayId).Value(dto.PlayId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.IsComplete).Value(true)));

        if (dto.CAAddresses != null)
        {
            var shouldQuery = new List<Func<QueryContainerDescriptor<BingoGameIndexEntry>, QueryContainer>>();
            foreach (var address in dto.CAAddresses)
            {
                var mustQueryAddressInfo = new List<Func<QueryContainerDescriptor<BingoGameIndexEntry>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.PlayerAddress).Value(address))
                };
                shouldQuery.Add(q => q.Bool(b => b.Must(mustQueryAddressInfo)));
            }

            mustQuery.Add(q => q.Bool(b => b.Should(shouldQuery)));
        }

        QueryContainer Filter(QueryContainerDescriptor<BingoGameIndexEntry> f) => f.Bool(b => b.Must(mustQuery));

        Func<SortDescriptor<BingoGameIndexEntry>, IPromise<IList<ISort>>> sort = s =>
            s.Descending(a => a.BingoBlockHeight);
        var result = await repository.GetSortListAsync(Filter, sortFunc: sort, skip: dto.SkipCount,
            limit: dto.MaxResultCount);
        var dataList = objectMapper.Map<List<BingoGameIndexEntry>, List<BingoInfo>>(result.Item2);
        
        var statsMustQuery = new List<Func<QueryContainerDescriptor<BingoGamestatsIndex>, QueryContainer>>();
        if (dto.CAAddresses != null)
        {
            var statsShouldQuery = new List<Func<QueryContainerDescriptor<BingoGamestatsIndex>, QueryContainer>>();
            foreach (var address in dto.CAAddresses)
            {
                var statsMustQueryAddressInfo = new List<Func<QueryContainerDescriptor<BingoGamestatsIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.PlayerAddress).Value(address))
                };
                statsShouldQuery.Add(q => q.Bool(b => b.Must(statsMustQueryAddressInfo)));
            }

            statsMustQuery.Add(q => q.Bool(b => b.Should(statsShouldQuery)));
        }

        QueryContainer statsFilter(QueryContainerDescriptor<BingoGamestatsIndex> f) => f.Bool(b => b.Must(statsMustQuery));
        var statsResult = await statsrepository.GetListAsync(statsFilter);
        var statsDataList = objectMapper.Map<List<BingoGamestatsIndex>, List<Bingostats>>(statsResult.Item2);

        var pageResult = new BingoResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList,
            stats = statsDataList,
        };
        return pageResult;
    }
}
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
        [FromServices] IAElfIndexerClientEntityRepository<BingoGameStaticsIndex, LogEventInfo> staticsrepository,
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
        
        var staticsMustQuery = new List<Func<QueryContainerDescriptor<BingoGameStaticsIndex>, QueryContainer>>();
        if (dto.CAAddresses != null)
        {
            var staticsShouldQuery = new List<Func<QueryContainerDescriptor<BingoGameStaticsIndex>, QueryContainer>>();
            foreach (var address in dto.CAAddresses)
            {
                var staticsMustQueryAddressInfo = new List<Func<QueryContainerDescriptor<BingoGameStaticsIndex>, QueryContainer>>
                {
                    q => q.Term(i => i.Field(f => f.PlayerAddress).Value(address))
                };
                staticsShouldQuery.Add(q => q.Bool(b => b.Must(staticsMustQueryAddressInfo)));
            }

            staticsMustQuery.Add(q => q.Bool(b => b.Should(staticsShouldQuery)));
        }

        QueryContainer staticsFilter(QueryContainerDescriptor<BingoGameStaticsIndex> f) => f.Bool(b => b.Must(staticsMustQuery));
        var staticsResult = await staticsrepository.GetListAsync(staticsFilter);
        var staticsDataList = objectMapper.Map<List<BingoGameStaticsIndex>, List<BingoStatics>>(staticsResult.Item2);

        var pageResult = new BingoResultDto
        {
            TotalRecordCount = result.Item1,
            Data = dataList,
            Statics = staticsDataList,
        };
        return pageResult;
    }
}
using AElfIndexer.Client.Handlers;
using AutoMapper;
using BingoGame.Indexer.CA.Entities;
using BingoGame.Indexer.CA.GraphQL;

namespace BingoGame.Indexer.CA;

public class TestGraphQLAutoMapperProfile : Profile
{
    public TestGraphQLAutoMapperProfile()
    {
        CreateMap<BingoGameIndexEntry, BingoInfo>();
        CreateMap<BingoGameStaticsIndex, BingoStatics>();
        CreateMap<LogEventContext, BingoGameIndexEntry>();
        CreateMap<LogEventContext, BingoGameStaticsIndex>();
    }
}
using GraphQL;
using Volo.Abp.Application.Dtos;

namespace BingoGame.Indexer.CA.GraphQL;

public class GetBingoDto: PagedResultRequestDto
{
    
    [Name("caAddresses")]
    public List<string> CAAddresses { get; set; }
    
    public string PlayId => null;
}

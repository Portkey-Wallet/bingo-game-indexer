using System.Diagnostics.CodeAnalysis;
using AElfIndexer.Client;
using Nest;

namespace BingoGame.Indexer.CA.Entities;

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class TokenInfoBase: AElfIndexerClientEntity<string>
{
    [Keyword] public override string Id { get; set; }
    
    [Keyword] public string Symbol { get; set; }

    /// <summary>
    /// token contract address
    /// </summary>
    [Keyword] public string TokenContractAddress { get; set; }
    
    public int Decimals { get; set; }

    public long TotalSupply { get; set; }

    [Keyword] public string TokenName { get; set; }

    [Keyword] public string Issuer { get; set; }

    public bool IsBurnable { get; set; }

    public int IssueChainId { get; set; }
    
    // public TokenExternalInfo TokenExternalInfo { get; set; }
}
using BingoGame.Indexer.CA.Entities;

namespace BingoGame.Indexer.CA;

public class InitialInfoOptions
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public List<TokenInfo> TokenInfoList { get; set; } = new();
}


public class TokenInfo : TokenInfoBase
{
    
}
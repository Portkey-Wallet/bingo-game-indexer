namespace BingoGame.Indexer.CA;

public class ContractInfoOptions
{
    public List<ContractInfo> ContractInfos { get; set; }
}

public class ContractInfo
{
    public string ChainId { get; set; }
    // ReSharper disable once UnassignedGetOnlyAutoProperty
    public string BingoGameContractAddress { get; set;}
}
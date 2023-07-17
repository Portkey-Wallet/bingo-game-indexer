namespace BingoGame.Indexer.CA;

public static class IdGenerateHelper
{
    public static string GenerateId(params object[] inputs)
    {
        return inputs.JoinAsString("-");
    }
}
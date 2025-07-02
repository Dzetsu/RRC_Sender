namespace RRC_Sender.Services;

public static class TokenGenerator
{
    public static string Generate()
    {
        return Guid.NewGuid().ToString();
    }
}
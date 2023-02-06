
public class SessionGenerator
{
    public static string CreateSessionID()
    {
        return Guid.NewGuid().ToString();
    }
}

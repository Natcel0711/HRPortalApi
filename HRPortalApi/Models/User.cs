using LiteDB;

public class User
{
    public ObjectId Id { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Username { get; set; }
}

public class Session
{
    public ObjectId Id { get; set; }
    public string SessionId { get; set; }
    public ObjectId UserId { get; set; }
}
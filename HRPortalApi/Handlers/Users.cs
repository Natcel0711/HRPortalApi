using static Microsoft.AspNetCore.Http.Results;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Cryp = BCrypt.Net;

public static class Users
{
    internal static async Task<IResult> SignUpAsync([FromBody] User user)
    {
        try
        {
            if (user is null) return BadRequest(new {error = "user is empty."});
            using (var db = new LiteDatabase(@"C:\Temp\HRPortal.db"))
            {
                var col = db.GetCollection<User>("users");
                var result = col.Query().Where(u => u.Username == user.Username || u.Email == user.Email).ToList();
                if (result.Count() > 0) return BadRequest(new { error = "user exists." });
                var hashed = Cryp.BCrypt.HashPassword(user.Password);
                user.Password = hashed;
                user.Id = ObjectId.NewObjectId();
                col.Insert(user);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        return Ok(new { user });
    }

    internal static async Task<IResult> SignInAsync([FromBody] User user)
    {
        try
        {
            if (user is null) return BadRequest();
            using (var db = new LiteDatabase(@"C:\Temp\HRPortal.db"))
            {
                //Find user
                var userCol = db.GetCollection<User>("users");
                var result = userCol.FindOne(x => x.Username == user.Username || x.Email == user.Email);
                if (result is null) return BadRequest(new { error = "user not found" });
                if (!Cryp.BCrypt.Verify(user.Password, result.Password))
                    return BadRequest(new { error = "wrong credentials" });
                //Create session id
                var sessionid = SessionGenerator.CreateSessionID();
                var sessionCol = db.GetCollection<Session>("sessions");
                var session = new Session
                {
                    SessionId = sessionid,
                    UserId = result.Id,
                };
                sessionCol.Insert(session);
                return Ok(new { session });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    internal static async Task<IResult> GetUserBySession([FromBody] Session session)
    {
        try
        {
            if (session is null) return BadRequest(new { error = "session not available" });
            using (var db = new LiteDatabase(@"C:\Temp\HRPortal.db"))
            {
                var sessionsCol = db.GetCollection<Session>("sessions");
                var res = sessionsCol.FindOne(x => x.SessionId == session.SessionId);
                if (res is null) return BadRequest(new { error = "no results" });
                var userCol = db.GetCollection<User>("users");
                var user = userCol.FindOne(x => x.Id == res.UserId);
                if (user is null) return BadRequest(new { error = "user with that id not found" });
                return Ok(new { user });
            }
        }
        catch (Exception e)
        {
            return BadRequest(new { error = e.Message });
        }
    }
}


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
            if (user is null) return BadRequest();
            using (var db = new LiteDatabase(@"C:\Temp\HRPortal.db"))
            {
                var col = db.GetCollection<User>("users");
                var result = col.Query().Where(u => u.Username == user.Username || u.Email == user.Email).ToList();
                if (result.Count() > 0) return StatusCode(409);
                var hashed = Cryp.BCrypt.HashPassword(user.Password);
                user.Password = hashed;
                col.Insert(user);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        return Ok();
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
                if (result is null) return BadRequest(new { message = "user not found" });
                if (!Cryp.BCrypt.Verify(user.Password, result.Password)) 
                    return BadRequest(new { message = "wrong credentials" });
                //Create session id
                var sessionid = SessionGenerator.CreateSessionID();
                var sessionCol = db.GetCollection<Session>("sessions");
                var session = new Session
                {
                    SessionId = sessionid,
                    UserId = user.Id,
                };
                sessionCol.Insert(session);
                return Ok(sessionid);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
}


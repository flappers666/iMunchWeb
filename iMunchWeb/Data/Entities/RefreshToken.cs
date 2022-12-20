using System.ComponentModel.DataAnnotations;

namespace iMunchWeb.Data.Entities;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
        
    public string UserId { get; set; }
        
    public string Token { get; set; }
    public DateTime Expires { get; set; }

    public DateTime Created { get; set; }
        
    public bool Revoked { get; set; }
        
    public RefreshToken(string token, string userId, DateTime expires)
    {
        UserId = userId;
        Token = token;
        Expires = expires;
        Created = DateTime.UtcNow;
        Revoked = false;
    }
}
namespace FootballAI.Application.Interfaces.AuthInterfaces;

public interface IPasswordHasher
{
    (string Hash, string Salt) HashPassword(string password);
    bool VerifyPassword(string password, string hash, string salt);
}

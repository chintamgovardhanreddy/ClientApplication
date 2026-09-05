using ClientApplication.model.Entity;

namespace ClientApplication.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
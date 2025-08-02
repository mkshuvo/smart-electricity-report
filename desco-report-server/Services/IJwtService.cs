using desco_report_server.Models;

namespace desco_report_server.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user, IList<string> roles);
        bool ValidateToken(string token);
    }
}
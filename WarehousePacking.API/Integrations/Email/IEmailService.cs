using WarehousePacking.Shared.DTOs;

namespace WarehousePacking.API.Integrations.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendPackageFailureEmail(PackageData package, string errorMessage);
    }
}

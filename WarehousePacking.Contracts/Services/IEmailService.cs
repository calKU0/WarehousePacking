using WarehousePacking.Contracts.DTOs;

namespace WarehousePacking.Contracts.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);

        Task SendPackageFailureEmail(PackageData package, string errorMessage);
    }
}
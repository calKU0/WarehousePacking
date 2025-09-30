using KontrolaPakowania.Shared.DTOs;

namespace KontrolaPakowania.API.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendPackageFailureEmail(PackageData package, string errorMessage);
    }
}

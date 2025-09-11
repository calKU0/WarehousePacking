using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Services;
using KontrolaPakowania.API.Services.Interfaces;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace KontrolaPakowania.API.Tests.AuthServiceTests
{
    public class AuthServiceIntegrationTests
    {
        #region Setup

        private readonly IAuthService _service;

        public AuthServiceIntegrationTests()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(config);
            services.AddScoped<IDbExecutor, DapperDbExecutor>();
            services.AddScoped<IAuthService, AuthService>();

            var provider = services.BuildServiceProvider();
            _service = provider.GetRequiredService<IAuthService>();
        }

        #endregion Setup

        #region Login Tests

        [Theory]
        [InlineData("BABJER", "14789632")]
        [InlineData("FEDYUL", "123456789")]
        [Trait("Category", "Integration")]
        public async Task Login_ReturnsTrue_ForValidCredentials(string username, string password)
        {
            // Act
            var result = await _service.Login(username, password);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("admin", "wrong-password")]
        [InlineData("user", "123456")]
        [Trait("Category", "Integration")]
        public async Task Login_ReturnsFalse_ForInvalidCredentials(string username, string password)
        {
            var result = await _service.Login(username, password);

            // Assert
            Assert.False(result);
        }

        #endregion Login Tests

        #region ValidatePassword Tests

        [Theory]
        [InlineData("112")]
        [InlineData("8/;23jWsMa-")]
        [Trait("Category", "Integration")]
        public async Task ValidatePasswordAsync_ReturnsTrue_ForValidPasswords(string password)
        {
            // Act
            var result = await _service.ValidatePasswordAsync(password);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("wrong-password")]
        [InlineData("123456")]
        [Trait("Category", "Integration")]
        public async Task ValidatePasswordAsync_ReturnsFalse_ForInvalidPasswords(string password)
        {
            // Act
            var result = await _service.ValidatePasswordAsync(password);

            // Assert
            Assert.False(result);
        }

        #endregion ValidatePassword Tests
    }
}
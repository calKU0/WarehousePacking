using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WarehousePacking.API.Data;
using WarehousePacking.API.Services.Auth;
using WarehousePacking.Shared.DTOs;

namespace WarehousePacking.API.Tests.AuthServiceTests
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
        [InlineData("BABJER", "14789632", "1120")]
        [InlineData("FEDYUL", "123456789", "1121")]
        [Trait("Category", "Integration")]
        public async Task LoginFlow_Works(string username, string password, string stationNumber)
        {
            // Arrange & Act
            var loginDto = new LoginDto
            {
                Username = username,
                Password = password,
                StationNumber = stationNumber
            };
            var loginResult = await _service.Login(loginDto);

            // Assert
            Assert.True(loginResult);

            // Act
            var operators = await _service.GetLoggedUsersAsync();

            // Assert
            Assert.NotNull(operators);
            Assert.All(operators, op =>
            {
                Assert.False(string.IsNullOrEmpty(op.Username));
                Assert.False(string.IsNullOrEmpty(op.StationNumber));
                Assert.NotEqual(default(DateTime), op.LoginDate);
            });

            // Arrange & Act
            var logoutResult = await _service.LogoutAsync(loginDto.Username);

            // Assert
            Assert.True(logoutResult);
        }

        [Theory]
        [InlineData("admin", "wrong-password", "1120")]
        [InlineData("user", "123456", "1131")]
        [Trait("Category", "Integration")]
        public async Task Login_ReturnsFalse_ForInvalidCredentials(string username, string password, string stationNumber)
        {
            // Arrange & Act
            var loginDto = new LoginDto
            {
                Username = username,
                Password = password,
                StationNumber = stationNumber
            };

            var result = await _service.Login(loginDto);

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
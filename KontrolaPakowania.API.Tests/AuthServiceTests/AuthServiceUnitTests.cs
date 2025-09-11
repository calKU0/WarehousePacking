using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services;
using KontrolaPakowania.API.Services.Interfaces;
using KontrolaPakowania.Shared.DTOs;
using Moq;
using System;
using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace KontrolaPakowania.API.Tests.AuthServiceTests
{
    public class AuthServiceUnitTests
    {
        private readonly Mock<IDbExecutor> _dbExecutorMock;
        private readonly AuthService _service;

        public AuthServiceUnitTests()
        {
            _dbExecutorMock = new Mock<IDbExecutor>();
            _service = new AuthService(_dbExecutorMock.Object);
        }

        #region Login Tests

        [Fact, Trait("Category", "Unit")]
        public async Task Login_ReturnsTrue_WhenCredentialsValid()
        {
            // Arrange
            string username = "admin";
            string password = "1234";
            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.Login(username, password);

            // Assert
            Assert.True(result);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task Login_ReturnsFalse_WhenCredentialsInvalid()
        {
            // Arrange
            string username = "admin";
            string password = "wrong";
            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                .ReturnsAsync(0);

            // Act
            var result = await _service.Login(username, password);

            // Assert
            Assert.False(result);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task Login_ThrowsException_WhenDbExecutorThrows()
        {
            // Arrange
            string username = "admin";
            string password = "1234";
            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.Login(username, password));
        }

        #endregion Login Tests

        #region ValidatePassword Tests

        [Fact, Trait("Category", "Unit")]
        public async Task ValidatePasswordAsync_ReturnsTrue_WhenPasswordValid()
        {
            // Arrange
            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.ValidatePasswordAsync("112");

            // Assert
            Assert.True(result);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task ValidatePasswordAsync_ReturnsFalse_WhenPasswordInvalid()
        {
            // Arrange
            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                .ReturnsAsync(0);

            // Act
            var result = await _service.ValidatePasswordAsync("997");

            // Assert
            Assert.False(result);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task ValidatePasswordAsync_ThrowsException_WhenDbExecutorThrows()
        {
            // Arrange
            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.ValidatePasswordAsync("112"));
        }

        #endregion ValidatePassword Tests
    }
}
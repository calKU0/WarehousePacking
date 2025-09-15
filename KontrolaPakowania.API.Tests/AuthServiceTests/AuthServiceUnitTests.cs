using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Auth;
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
            LoginDto loginDto = new LoginDto
            {
                Username = "admin",
                Password = "1234",
                StationNumber = "1120"
            };
            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.Login(loginDto);

            // Assert
            Assert.True(result);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task Login_ReturnsFalse_WhenCredentialsInvalid()
        {
            // Arrange
            LoginDto loginDto = new LoginDto
            {
                Username = "admin",
                Password = "wrong-password",
                StationNumber = "1120"
            };
            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                .ReturnsAsync(0);

            // Act
            var result = await _service.Login(loginDto);

            // Assert
            Assert.False(result);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task Login_ThrowsException_WhenDbExecutorThrows()
        {
            // Arrange
            LoginDto loginDto = new LoginDto
            {
                Username = "admin",
                Password = "1234",
                StationNumber = "1120"
            };
            _dbExecutorMock.Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    It.IsAny<CommandType>(),
                    It.IsAny<Connection>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.Login(loginDto));
        }

        [Fact, Trait("Category", "Unit")]
        public async Task LogoutOperatorAsync_ShouldReturnTrue_WhenRowsAffectedGreaterThanZero()
        {
            // Arrange
            string username = "admin";

            _dbExecutorMock
                .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    CommandType.StoredProcedure,
                    Connection.ERPConnection))
                .ReturnsAsync(1); // 1 row deleted

            // Act
            var result = await _service.LogoutAsync(username);

            // Assert
            Assert.True(result);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task LogoutOperatorAsync_ShouldReturnFalse_WhenNoRowsAffected()
        {
            // Arrange
            string username = "admin";

            _dbExecutorMock
                .Setup(db => db.QuerySingleOrDefaultAsync<int>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    CommandType.StoredProcedure,
                    Connection.ERPConnection))
                .ReturnsAsync(0); // No rows deleted

            // Act
            var result = await _service.LogoutAsync(username);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetLoggedUsersAsync_ShouldReturnList_WhenDbReturnsData()
        {
            // Arrange
            var expected = new List<LoginDto>
        {
            new LoginDto { Username = "user1", StationNumber = "S1" },
            new LoginDto { Username = "user2", StationNumber = "S2" }
        };

            _dbExecutorMock
                .Setup(db => db.QueryAsync<LoginDto>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    CommandType.StoredProcedure,
                    Connection.ERPConnection))
                .ReturnsAsync(expected);

            // Act
            var result = await _service.GetLoggedUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, x => x.Username == "user1" && x.StationNumber == "S1");
            Assert.Contains(result, x => x.Username == "user2" && x.StationNumber == "S2");
        }

        [Fact]
        public async Task GetLoggedUsersAsync_ShouldReturnEmpty_WhenDbReturnsEmpty()
        {
            // Arrange
            _dbExecutorMock
                .Setup(db => db.QueryAsync<LoginDto>(
                    It.IsAny<string>(),
                    It.IsAny<object?>(),
                    CommandType.StoredProcedure,
                    Connection.ERPConnection))
                .ReturnsAsync(new List<LoginDto>());

            // Act
            var result = await _service.GetLoggedUsersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
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
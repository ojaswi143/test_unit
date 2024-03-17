using ESG_Survey_Automation.Domain;
using ESG_Survey_Automation.Controller;
using ESG_Survey_Automation.Infrastructure.EntityFramework;
using ESG_Survey_Automation.Infrastructure.EntityFramework.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ESG_Survey_AutomationTest
{
    public class AccountsControllerTests
    {
        [Fact]
        public async Task SignIn_WithValidCredentials_ShouldReturnOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<DbContext>(); // Mock your database context
            var mockConfiguration = new Mock<IConfiguration>(); // Mock your configuration
            var mockLogger = new Mock<ILogger<AccountController>>(); // Mock your logger

            var user = new User
            {
                UserId = 1,
                Email = "test@example.com",
                FullName = "Test User",
                EncryptedPassword = "hashed_password" // Assuming you have hashed password stored in the database
            };

            var mockDbSet = new Mock<DbSet<User>>();
            mockDbSet.Setup(x => x.FindAsync(user.UserId)).ReturnsAsync(user);

            mockContext.Setup(x => x.Users).Returns(mockDbSet.Object);

            var controller = new AccountController(mockContext.Object, mockConfiguration.Object, mockLogger.Object);

            var model = new LoginModel
            {
                Email = "test@example.com",
                Password = "password" // Assuming this is the correct password for the user
            };

            // Act
            var result = await controller.SignIn(model);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var loginResponse = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.Equal("Bearer", loginResponse.TokenType);
            Assert.NotEmpty(loginResponse.AccessToken);
            Assert.Equal("Test User", loginResponse.UserName);
        }

        [Fact]
        public async Task Registration_WithValidModel_ShouldReturnOkResult()
        {
            // Arrange
            var mockContext = new Mock<DbContext>(); // Mock your database context
            var mockLogger = new Mock<ILogger<AccountController>>(); // Mock your logger

            var model = new UserRegistrationModel
            {
                Email = "test@example.com",
                Password = "password",
                FullName = "Test User"
            };

            var usersDbSet = new Mock<DbSet<User>>();
            usersDbSet.Setup(x => x.AnyAsync(u => u.Email == model.Email)).ReturnsAsync(false);

            mockContext.Setup(x => x.Users).Returns(usersDbSet.Object);

            var controller = new AccountController(mockContext.Object, null, mockLogger.Object);

            // Act
            var result = await controller.Registration(model);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task Registration_WithExistingEmail_ShouldReturnBadRequestResult()
        {
            // Arrange
            var mockContext = new Mock<DbContext>(); // Mock your database context
            var mockLogger = new Mock<ILogger<AccountController>>(); // Mock your logger

            var model = new UserRegistrationModel
            {
                Email = "existing@example.com",
                Password = "password",
                FullName = "Test User"
            };

            var usersDbSet = new Mock<DbSet<User>>();
            usersDbSet.Setup(x => x.AnyAsync(u => u.Email == model.Email)).ReturnsAsync(true);

            mockContext.Setup(x => x.Users).Returns(usersDbSet.Object);

            var controller = new AccountController(mockContext.Object, null, mockLogger.Object);

            // Act
            var result = await controller.Registration(model);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User with email existing@example.com already exist", badRequestResult.Value);
        }

    }
}
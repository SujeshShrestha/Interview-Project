using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Interview_Project.Controllers;
using Interview_Project.Models;
using Interview_Project.Models.DTOs.Account;
using Interview_Project.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Xunit;

namespace Interview_UnitTests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<JwtServices> _jwtServicesMock = new Mock<JwtServices>();
        private readonly Mock<SignInManager<User>> _signInManagerMock = new Mock<SignInManager<User>>();
        private readonly Mock<UserManager<User>> _userManagerMock = new Mock<UserManager<User>>();

        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _controller = new AccountController(
                _jwtServicesMock.Object,
                _signInManagerMock.Object,
                _userManagerMock.Object
            );
        }

        [Fact]
        public async Task RefreshUserToken_ReturnsActionResult()
        {
            // Arrange
            var userDto = new UserDto();
            var user = new User { UserName = "test@example.com" };
            _userManagerMock.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            _jwtServicesMock.Setup(m => m.CreateJwt(user)).Returns("test_jwt");

            // Act
            var result = await _controller.RefreshUserToken();

            // Assert
            var actionResult = Xunit.Assert.IsType<ActionResult<UserDto>>(result);
            Xunit.Assert.NotNull(actionResult.Value);
            Xunit.Assert.Same(userDto, actionResult.Value);
        }

        [Fact]
        public async Task Login_WithValidModel_ReturnsUserDto()
        {
            // Arrange
            var userDto = new UserDto();
            var user = new User { UserName = "test@example.com", EmailConfirmed = true };
            var model = new LoginDto { UserName = "test@example.com", Password = "password" };
            _userManagerMock.Setup(m => m.FindByNameAsync(model.UserName)).ReturnsAsync(user);
            _signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, model.Password, false)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _jwtServicesMock.Setup(m => m.CreateJwt(user)).Returns("test_jwt");

            // Act
            var result = await _controller.Login(model);

            // Assert
            var actionResult = Xunit.Assert.IsType<ActionResult<UserDto>>(result);
            Xunit.Assert.NotNull(actionResult.Value);
            Xunit.Assert.Same(userDto, actionResult.Value);
        }

        [Fact]
        public async Task Login_WithInvalidModel_ReturnsUnauthorized()
        {
            // Arrange
            var model = new LoginDto { UserName = "test@example.com", Password = "password" };
            _userManagerMock.Setup(m => m.FindByNameAsync(model.UserName)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var actionResult = Xunit.Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Login_WithUnconfirmedEmail_ReturnsUnauthorized()
        {
            // Arrange
            var user = new User { UserName = "test@example.com", EmailConfirmed = false };
            var model = new LoginDto { UserName = "test@example.com", Password = "password" };
            _userManagerMock.Setup(m => m.FindByNameAsync(model.UserName)).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var actionResult = Xunit.Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task Register_WithUniqueEmail_ReturnsOkResult()
        {
            // Arrange
            var registerDto = new RegisterDto { Email = "test@example.com", Password = "password" };
            _userManagerMock.Setup(m => m.Users).Returns(new List<User>().AsQueryable());
            _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var actionResult = Xunit.Assert.IsType<OkObjectResult>(result);
            Xunit.Assert.Equal("Account Created", ((dynamic)actionResult.Value).title);
        }

        [Fact]
        public async Task Register_WithExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var existingUser = new User { UserName = "test@example.com" };
            var registerDto = new RegisterDto { Email = "test@example.com", Password = "password" };
            _userManagerMock.Setup(m => m.Users).Returns(new List<User> { existingUser }.AsQueryable());

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var actionResult = Xunit.Assert.IsType<BadRequestObjectResult>(result);
            Xunit.Assert.Equal("An existing account is using test@example.com, email address. Please try with another email address", actionResult.Value);
        }
    }
}

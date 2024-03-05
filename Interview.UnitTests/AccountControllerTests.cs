using Microsoft.AspNetCore.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Interview.UnitTests
{
    [TestClass]
    public class AccountControllerTests
    {
        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsUserDto()
        {
            // Arrange
            var jwtServiceMock = new Mock<JwtServices>();
            var signInManagerMock = new Mock<SignInManager<User>>();
            var userManagerMock = new Mock<UserManager<User>>();

            var controller = new AccountController(jwtServiceMock.Object, signInManagerMock.Object, userManagerMock.Object);

            var loginDto = new LoginDto { UserName = "test@example.com", Password = "password" };
            var user = new User { UserName = "test@example.com", EmailConfirmed = true }; // Assuming user exists and email is confirmed

            signInManagerMock.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
            signInManagerMock.Setup(m => m.CheckPasswordSignInAsync(user, It.IsAny<string>(), false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsInstanceOf<ActionResult<UserDto>>(result);
            Assert.IsNotNull(result.Value);
            Assert.AreEqual(user.FirstName, result.Value.FirstName); // Assuming userDto includes first name
            Assert.AreEqual(user.LastName, result.Value.LastName); // Assuming userDto includes last name
        }

    }
}

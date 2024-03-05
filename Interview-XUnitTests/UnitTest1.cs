using Interview_Project.Controllers;
using Interview_Project.Models;
using Interview_Project.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;

using Xunit;

namespace Interview_UnitTests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;

        public UserControllerTests()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null, null, null, null, null, null, null, null);
        }
        //[Fact]
        //public async Task UpdateUser_WithValidId_ReturnsOkResult()
        //{
        //    // Arrange
        //    var controller = new UserController(null, null, null); // Assuming all dependencies are not relevant for this test

        //    // Act
        //    var result = await controller.UpdateUser("1", new User());

        //    // Assert
        //    Assert.IsType<OkResult>(result);
        //}

        
       

        [Fact]
        public async Task UpdateUser_WithInvalidId_ReturnsNotFoundResult()
        {
            // Arrange
            var controller = new UserController(null, null, _userManagerMock.Object);
            var id = "1";
            var userDto = new User { FirstName = "John", LastName = "john", PhotoPath = "path/to/photo", Address = "123 Street", PhoneNumber = "1234567890" };

            _userManagerMock.Setup(m => m.FindByIdAsync(id)).ReturnsAsync((User)null);

            // Act
            var result = await controller.UpdateUser(id, userDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateUser_WithUpdateFailure_ReturnsBadRequestResult()
        {
            // Arrange
            var controller = new UserController(null, null, _userManagerMock.Object);
            var id = "1";
            var userDto = new User { FirstName = "John", LastName = "John", PhotoPath = "path/to/photo", Address = "123 Street", PhoneNumber = "1234567890" };
            var user = new User { Id = id };
            var errorDescription = "Update failed";
            var error = IdentityResult.Failed(new IdentityError { Description = errorDescription });

            _userManagerMock.Setup(m => m.FindByIdAsync(id)).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(error);

            // Act
            var result = await controller.UpdateUser(id, userDto);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result);
            var errors = Assert.IsType<List<IdentityError>>(actionResult.Value);
            var firstError = errors.FirstOrDefault();

            Assert.NotNull(firstError);
            Assert.Equal(errorDescription, firstError.Description);
        }


    }
}

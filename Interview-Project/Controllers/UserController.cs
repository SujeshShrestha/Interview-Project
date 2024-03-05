using Interview_Project.Models;
using Interview_Project.Models.DTOs.Account;
using Interview_Project.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Interview_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly JwtServices _jwtService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        public UserController(JwtServices jwtServices, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _jwtService = jwtServices;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, User userDto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update user properties with data from userDto
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.PhotoPath = userDto.PhotoPath;// Assuming PhotoPath can be updated
            user.Address = userDto.Address;
            user.PhoneNumber= userDto.PhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Ok(new JsonResult(new
                {
                    title = "",
                    message = "Your information has been updated",
                    user = user // Include the updated user object
                }));
            }
            else
            {
                // Handle errors
                return BadRequest(result.Errors);
            }
        }
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentLoggedInUser()
        {
            // Get the current user's ID from the claims
            var user = await _userManager.FindByIdAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return Ok(user);
            }
            else
            {
                // Handle errors
                return BadRequest(result.Errors);
            }
        }

    }
}

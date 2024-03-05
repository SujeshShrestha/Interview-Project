using Google.Apis.Auth;
using Interview_Project.Data;
using Interview_Project.Models;
using Interview_Project.Models.DTOs.Account;
using Interview_Project.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Interview_Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly JwtServices _jwtService;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly Context _dbContext;
        public AccountController(JwtServices jwtServices,SignInManager<User> signInManager ,UserManager<User> userManager, Context dbcontext)
        {
            _dbContext = dbcontext;
            _jwtService = jwtServices;
            _signInManager  = signInManager;
            _userManager = userManager;
        }
        
        [HttpGet("refresh-user-token")]
        public async Task<ActionResult<UserDto>> RefreshUserToken()
        {
            
            var user = await _userManager.FindByNameAsync(User.FindFirst(ClaimTypes.Email)?.Value);
            return CreateApplicationUserDto(user);
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return Unauthorized("Invalid UserName or Password");
            }
            if(user.EmailConfirmed== false)
            {
                return Unauthorized("Please confirm your email.");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid username or password");
            }
            return CreateApplicationUserDto(user);
        }
        [HttpPost("LoginWithGoogle")]
        public async Task<ActionResult> LogInWithGoogle([FromBody] GoogleLoginDto model)
        {
            var idToken = model.IdToken;
            var setting = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new string[] { "275291437589-0pc7m649v4c7n4lp9o84vhadlsnfkjht.apps.googleusercontent.com" }
            };

            var result = await GoogleJsonWebSignature.ValidateAsync(idToken, setting);
            if (result is null)
            {
                return BadRequest();
            }
            //var userDtoo = await CheckIfUserExistsWithGoogleInfo(result);
            var user = await FindOrCreateUserFromGoogleInfo(result);

            // If user creation or retrieval fails, handle accordingly
            if (user == null)
            {
                return Unauthorized("Failed to create or find user.");
            }

            // Generate JWT token for the user
            var jwtToken = _jwtService.CreateJwt(user);

            // Return the JWT token along with any other relevant user information
            var userDto = CreateApplicationUserDto(user);
            userDto.Jwt = jwtToken;

            return Ok(userDto);
        }
        [HttpGet("Authenticate")]
        public async Task<bool> Authenticate()        
        {
            return User.Identity.IsAuthenticated;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
           
            var userToAdd = new User
            {
                FirstName = registerDto.FirstName.ToLower(),
                LastName = registerDto.LastName.ToLower(),
                UserName = registerDto.Email.ToLower(),
                EmailConfirmed = true,
                Address = string.Empty,
                PhotoPath = string.Empty,
                PhoneNumber = string.Empty,

            };
            var result = await _userManager.CreateAsync(userToAdd, registerDto.Password);
            if (!result.Succeeded) 
            {
                return BadRequest(result.Errors);
            }
            return Ok(new JsonResult(new { title = "Account Created", message = "Your Account has been created.You can Login" }));
        }
        #region Private Helper Methods
        private UserDto CreateApplicationUserDto(User user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Jwt = _jwtService.CreateJwt(user),

            };
        }
        private async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.NormalizedEmail == email.ToUpper());
        }

        private async Task<User> FindOrCreateUserFromGoogleInfo(GoogleJsonWebSignature.Payload googlePayload)
        {
            
            if (!string.IsNullOrEmpty(googlePayload.Email))
            {
                var loginUser = googlePayload.Email.Trim().ToUpper();
                var existingUser = await _userManager.FindByNameAsync(loginUser);

                if (existingUser != null)
                {
                    // If the user exists, try to sign them in traditionally
                    var traditionalSignInResult = await _signInManager.PasswordSignInAsync(existingUser.UserName, "GoogleSignIn", false, false);
                    if (traditionalSignInResult.Succeeded)
                    {
                        // If the user has logged in traditionally, return the existing user
                        return existingUser;
                    }
                    else
                    {
                        // If traditional sign-in fails, you might want to handle it differently
                        // For example, generate a JWT token and return it
                        var jwtToken = _jwtService.CreateJwt(existingUser);
                        var userDto = CreateApplicationUserDto(existingUser);
                        userDto.Jwt = jwtToken;
                        return existingUser;
                    }

                }

                else
                {
                    // If the user does not exist, create a new user based on Google information
                    var newUser = new User
                    {
                        UserName = googlePayload.Email.Trim(),
                        Email = googlePayload.Email.Trim(),
                        FirstName = googlePayload.GivenName,
                        LastName = googlePayload.FamilyName,
                        PhotoPath = googlePayload.Picture,
                        Address = string.Empty,
                        NormalizedEmail = googlePayload.Email.Trim(),
                        NormalizedUserName = googlePayload.Email.Trim(),
                        LockoutEnabled = true,
                        EmailConfirmed = true,

                        // You might need to map other properties from the Google payload to your user model
                    };

                    // Save the new user to your database
                    var result = await _userManager.CreateAsync(newUser);

                    if (result.Succeeded)
                    {
                        // User created successfully, return the new user
                        return newUser;
                    }
                    else
                    {
                        // If user creation failed, handle the error (e.g., log, return null, etc.)
                        return null;
                    }
                }

            }
            throw new Exception();


        }


        #endregion
    }
}

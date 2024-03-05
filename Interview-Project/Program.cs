
using Interview_Project.Data;
using Interview_Project.Models;
using Interview_Project.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Interview_Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<Context>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

            });
            var googleClientId = builder.Configuration["Google:ClientId"];
            //injecting JWTServices class inside our controller
            builder.Services.AddScoped<JwtServices>();
            //defining identity core services
            builder.Services.AddIdentityCore<User>(options =>
            {
                //password Configuration
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                //for email Confirmation
                options.SignIn.RequireConfirmedEmail = true;


            }).AddRoles<IdentityRole>()// add roles
                .AddRoleManager<RoleManager<IdentityRole>>()//role manager
                .AddSignInManager<SignInManager<User>>()//make use of sign in user
                .AddEntityFrameworkStores<Context>()//providing context
                .AddUserManager<UserManager<User>>()//make use of usermanager to create user
                .AddDefaultTokenProviders();//able to create tokens for email confirmation

            //Authenticte user using JWT
            _ = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

            .AddJwtBearer(options =>
            {


                options.TokenValidationParameters = new TokenValidationParameters
                {
                    //Validate the token based of the key we have provided inside appsetting
                    ValidateIssuerSigningKey = true,
                    //issuer sign in key based JWT:key
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    //validate the issuer 
                    ValidateIssuer = true,
                    //don't validate the audience
                    ValidateAudience = false,

                };

            });
            builder.Services.AddCors();

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors)
                    .Select(x => x.ErrorMessage).ToArray();

                    var toReturn = new
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(toReturn);
                };
            });
            var app = builder.Build();

            //Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            //adding useAuthentication into our pipelines
            //Authenticate verfies the idetify of a user or services, and authorization determines their access rights
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            
            app.UseCors(Options =>
            {
                Options.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials().AllowAnyOrigin()
                .WithOrigins(builder.Configuration["JWT:ClientUrl"]);

            });
            app.Run();
        }
    }
}
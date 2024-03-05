using Interview_Project.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Interview_Project.Data
{
    public class Context:IdentityDbContext<User>
    {
        public Context(DbContextOptions<Context> options):base(options)
        {

        }
    }
}

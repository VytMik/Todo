using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Todo.Models
{
    public class Context : IdentityDbContext
    {
        public Context(DbContextOptions opt) : base(opt)
        {
        }

        public DbSet<IdentityUser> users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Item> Items { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Todo.Models
{
    public class Context : DbContext
    {
        public Context(DbContextOptions opt) : base(opt)
        {

        }

        public DbSet<User> users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Item> Items { get; set; }
    }
}

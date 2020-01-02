using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Todo.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public IdentityUser User { get; set; }
    }
}

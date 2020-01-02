using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Todo.Models;
using Todo.Resources;

namespace Todo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly Context _context;
        private readonly IConfiguration _configuration;
        private readonly SignInManager<IdentityUser> _signManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(Context context, IConfiguration configuration, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _configuration = configuration;
            _userManager = userManager;
            _signManager = signInManager;
            _roleManager = roleManager;
        }

        // GET: api/Users
        [Authorize(Roles = "adm")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResource>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            var returnList = new List<UserResource>();
            foreach (var user in users)
            {
                returnList.Add(
                    new UserResource
                    {
                        Id = user.Id,
                        Name = user.NormalizedUserName,
                        Role = (await _userManager.GetRolesAsync(user)).First()
                    });
            }
            return returnList;
        }

        // GET: api/Users/5
        [Authorize(Roles = "Administrator,User")]
        [Route("[action]")]
        [HttpGet]
        public async Task<IActionResult> GetUser()
        {
            string id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.Users.FindAsync(id);
            if (user == null) { return NotFound(); }

            var role = (await _userManager.GetRolesAsync(user)).First();
            return Ok(
                new UserResource
                {
                    Id = user.Id,
                    Name = user.NormalizedUserName,
                    Role = role
                });
        }

        // Put: api/Users/user
        [HttpPut]
        [Route("[action]/{username}")]
        public async Task<ActionResult<string>> RequestToken([FromBody] string password, string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);

            var key = Encoding.ASCII.GetBytes("labai-ilgas-raktas");

            var JWToken = new JwtSecurityToken(
                issuer: "http://localhost:5000/",
                audience: "http://localhost:5000/",
                claims: new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Role, roles[0])
            },
        notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                expires: new DateTimeOffset(DateTime.Now.AddDays(1)).DateTime,
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            );
            var token = new JwtSecurityTokenHandler().WriteToken(JWToken);
            await _userManager.SetAuthenticationTokenAsync(user, TokenOptions.DefaultProvider, "token", token);
            return new JsonResult(new { token = token });
        }

        // POST: api/Users
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<IdentityUser>> PostUser(IdentityUser user)
        {
            _context.users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UserExists(user.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<IdentityUser>> DeleteUser(string id)
        {
            var user = await _context.users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(string id)
        {
            return _context.users.Any(e => e.Id == id);
        }
    }
}

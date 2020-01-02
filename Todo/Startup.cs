using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Todo.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Identity;

namespace Todo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));
            services.AddDbContext<Context>(opt => opt.UseInMemoryDatabase("todo"));
           // services.AddDbContext<Context>(opt => opt.UseSqlServer(Configuration["ConnectionStrings:DefaultConnection"]));
            services.AddIdentity<IdentityUser, IdentityRole>(opt =>
            {
                opt.Password.RequireDigit = false;
                opt.Password.RequiredLength = 3;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequireUppercase = false;
                opt.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultProvider;
            }).AddEntityFrameworkStores<Context>().AddDefaultTokenProviders();
            services.TryAddScoped<RoleManager<IdentityRole>>();
            services.AddControllers();

            var SecretKey = Encoding.ASCII.GetBytes("labai-ilgas-raktas");
           //Configure JWT Token Authentication
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(token =>
            {
                token.RequireHttpsMetadata = false;
                token.SaveToken = true;
                token.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(SecretKey),
                    ValidateIssuer = true,
                    ValidIssuer = "http://localhost:5000/",
                    ValidateAudience = true,
                    ValidAudience = "http://localhost:5000/",
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Context context, IServiceProvider isp)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseCors("MyPolicy");
            context.Database.EnsureCreated();
            SeedRoles(isp).Wait();
        }
        private async Task SeedRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roleExist = await roleManager.RoleExistsAsync("adm");

            if (roleExist) return;

            await roleManager.CreateAsync(new IdentityRole("adm"));
            await roleManager.CreateAsync(new IdentityRole("usr"));

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var userExists = await userManager.FindByNameAsync("admin");

            if (userExists != null) return;

            await userManager.CreateAsync(new IdentityUser { UserName = "admin" });
            var user = await userManager.FindByNameAsync("admin");
            var res = await userManager.AddPasswordAsync(user, "admin");
            var last = await userManager.AddToRoleAsync(user, "adm");
            await userManager.UpdateAsync(user);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Rex.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Rex
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
            services.AddDbContext<RexContext>(optionsAction => optionsAction.UseInMemoryDatabase("Rex").UseLazyLoadingProxies());
            services.AddControllers();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.Authority = "https://login.microsoftonline.com/a26571f1-22b3-4756-ac7b-39ca684fab48";
                    o.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidAudiences = new List<string>
                        {
                            "https://rex.sierrasoftworks.com",
                            "e284d597-f080-406d-a3fc-91f18eca6baa"
                        }
                    };
                });

            services.AddAuthorization(o =>
            {
                o.AddPolicy("default", p =>
                    p.RequireAuthenticatedUser()
                        .RequireClaim("http://schemas.microsoft.com/identity/claims/objectidentifier"));

                foreach (var name in new[] { "Collections.Read", "Collections.Write", "Ideas.Read", "Ideas.Write", "RoleAssignments.Write" })
                    o.AddPolicy(name, p =>
                    p.RequireAuthenticatedUser()
                        .RequireClaim("http://schemas.microsoft.com/identity/claims/objectidentifier")
                        .RequireClaim("http://schemas.microsoft.com/identity/claims/scope")
                        .RequireAssertion(s => s.User.FindAll(c => c.Type == "http://schemas.microsoft.com/identity/claims/scope" && c.Value.Split(' ').Contains(name)).Any()));

                o.DefaultPolicy = o.GetPolicy("default");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}

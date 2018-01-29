using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using WebServis.DAL.Entities;

namespace WebServis
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,

                            ValidIssuer = Configuration["Auth:ValidIssuer"],
                            ValidAudience = Configuration["Auth:ValidAudience"],
                            IssuerSigningKey =
        new SymmetricSecurityKey(
        Encoding.ASCII.GetBytes(Configuration["Auth:JwtSecurityKey"]))
                        };
                    });

            services.AddMvc().AddJsonOptions(options => {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;                       // Kod odgovora slati informaciju o kojoj verziji API-a se radi
                options.AssumeDefaultVersionWhenUnspecified = true;     // Ukoliko verzija nije ekplicitno odabrana, korisiti zadanu verziju
                options.DefaultApiVersion = new ApiVersion(1, 1);       // Postavljanje zadane verzije API-a. U ovom slucaju je odabrana zadnja verzija v1.1
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");   // Odabir verzije API-a pomocu header-a
            });

            services.AddDbContext<AdventureWorks2014Context>(options=>options.UseSqlServer(Configuration.GetConnectionString("lab3database")));

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1.0", new Info { Title = "JWT API", Version = "v1.0" });   // Dodavanje swagger dokumenta za verziju 1.0
                options.SwaggerDoc("v1.1", new Info { Title = "JWT API", Version = "v1.1" });   // Dodavanje swagger dokumenta za verziju 1.0

                // Ovime se implementira logika za odlucivanje u koji dokument ce ici koja verzija servisa
                options.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var versions = apiDesc.ControllerAttributes()
                                        .OfType<ApiVersionAttribute>()
                                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v.ToString()}" == docName);
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "JWT API v1.0");
                options.SwaggerEndpoint("/swagger/v1.1/swagger.json", "JWT API v1.1");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

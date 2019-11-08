using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogAPI.CustomFormatters;
using CatalogAPI.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace CatalogAPI
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
            services.AddScoped<CatalogContext>();
            services.AddCors(c =>
            {
                //c.AddDefaultPolicy(x => x.AllowAnyOrigin()
                //.AllowAnyMethod()
                //.AllowAnyHeader());
                c.AddPolicy("AllowPartners", x =>
                {
                    x.WithOrigins("http://microsoft.com", "http://synergetics.com")
                    .WithMethods("GET", "POST")
                    .AllowAnyHeader();
                });

                c.AddPolicy("AllowAll", x =>               {
                    x.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });
            });
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("V1", new Info
                {
                    Title = "Catalog API",
                    Description = "Catalog management API methods for EShop application",
                    Version = "1.0",
                    Contact = new Contact()
                    {
                        Name = "abhilipsa",
                        Email = "a@d.com",
                        Url = "https://github.com"

                    }
                });
            });
            services.AddAuthentication(c =>
            {
                c.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                c.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(b =>
            {
                b.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration.GetValue<string>("Jwt:issuer"),
                    ValidAudience = Configuration.GetValue<string>("Jwt:audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:secret")))
                };
            });
            services.AddMvc(options=> {
                options.OutputFormatters.Add(new CsvOutPutFormatter());
            }
                )
                .AddXmlDataContractSerializerFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseSwagger();
            if (env.IsDevelopment()){
                app.UseSwaggerUI(config =>
                {
                    config.SwaggerEndpoint("/swagger/V1/swagger.json", "index.html");
                    config.RoutePrefix = "";
                });
            }
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}

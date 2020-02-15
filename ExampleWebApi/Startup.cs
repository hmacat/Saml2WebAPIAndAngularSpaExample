using ExampleWebApi.Constants;
using ExampleWebApi.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Metadata;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ExampleWebApi
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // SameSiteMode.None is required to support SAML SSO.
                options.MinimumSameSitePolicy = SameSiteMode.None;

                options.CheckConsentNeeded = context => false;

                // Some older browsers don't support SameSiteMode.None.
                options.OnAppendCookie = cookieContext => SameSite.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext => SameSite.CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info() { Title = "Default API", Version = "v1" });
                c.DescribeAllEnumsAsStrings();

                var security = new Dictionary<string, IEnumerable<string>>
                    {
                        { "Bearer", Array.Empty<string>() },
                    };

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = "header",
                    Name = "Authorization",
                    Type = "apiKey",
                });

                c.AddSecurityRequirement(security);
            });

            services.AddCors(x =>
            {
                x.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowCredentials()
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                });
            });

            services.AddAuthentication(o =>
            {
                o.DefaultScheme = ApplicationSamlConstants.Application;
                o.DefaultSignInScheme = ApplicationSamlConstants.External;
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(Convert.ToDouble(this.Configuration["Jwt:ExpireInMinutes"])),
                    ValidIssuer = this.Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["Jwt:Key"])),
                };
            })
            .AddCookie(ApplicationSamlConstants.Application)
            .AddCookie(ApplicationSamlConstants.External)
            .AddSaml2(options =>
            {
                options.SPOptions.EntityId = new EntityId(this.Configuration["Saml:SPEntityId"]);
                options.IdentityProviders.Add(
                    new IdentityProvider(
                        new EntityId(this.Configuration["Saml:IDPEntityId"]), options.SPOptions)
                    {
                        LoadMetadata = true
                    });
                options.SPOptions.ServiceCertificates.Add(new X509Certificate2(this.Configuration["Saml:CertificateFileName"]));
            });

            services.AddSession(options =>
            {
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.None;
            });

            services.AddMvc((options) =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.ReturnHttpNotAcceptable = true;
                options.InputFormatters.Add(new XmlSerializerInputFormatter());
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                options.FormatterMappings.SetMediaTypeMappingForFormat("json", "application/json");
                options.FormatterMappings.SetMediaTypeMappingForFormat("xml", "application/xml");
            })
            .AddDataAnnotationsLocalization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
                c.InjectStylesheet("/swagger-ui/custom.css");
            });

            app.UseCors("AllowAll");

            app.UseAuthentication();

            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}",
                    defaults: new { format = "json" });
            });
        }
    }
}

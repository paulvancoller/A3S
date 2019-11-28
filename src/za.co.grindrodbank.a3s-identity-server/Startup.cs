/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */

using za.co.grindrodbank.a3sidentityserver.Extensions;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using za.co.grindrodbank.a3s.Managers;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3sidentityserver.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;
using za.co.grindrodbank.a3s.Stores;

namespace za.co.grindrodbank.a3sidentityserver
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        private const string CONFIG_SCHEMA = "_ids4";

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<A3SContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<UserModel, IdentityRole>()
                .AddEntityFrameworkStores<A3SContext>()
                .AddUserManager<CustomUserManager>()
                .AddUserStore<CustomUserStore>()
                .AddDefaultTokenProviders();

            // Register own SignInManager to handle Just-In-Time LDAP Auth
            services.AddScoped<SignInManager<UserModel>, CustomSignInManager<UserModel>>();

            services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddConfigurationStore(options =>
            {
                options.ConfigureDbContext = configurationBuilder =>
                    configurationBuilder.UseNpgsql(
                        Configuration.GetConnectionString("DefaultConnection")
                        );

                options.DefaultSchema = CONFIG_SCHEMA;
            })
            .AddAspNetIdentity<UserModel>()
            .AddProfileService<IdentityWithAdditionalClaimsProfileService>()
            .LoadSigningCredentialFrom(Configuration["certificates:signing"], Configuration["certificates:signingPassword"], Environment);

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ILdapAuthenticationModeRepository, LdapAuthenticationModeRepository>();
            services.AddScoped<ITermsOfServiceRepository, TermsOfServiceRepository>();

            // Register services
            services.AddScoped<ILdapConnectionService, LdapConnectionService>();
            services.AddScoped<ISafeRandomizerService, SafeRandomizerService>();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

                // Initialiaze the configuration database and seed entries from the Config file.
                InitializeConfigurationDatabase(app);

            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }

        private void InitializeConfigurationDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();

                if (!context.Clients.Any())
                {
                    foreach (var client in Config.GetClients())
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.GetIdentityResources())
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in Config.GetApis())
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}

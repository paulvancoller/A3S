/**
 * *************************************************
 * Copyright (c) 2019, Grindrod Bank Limited
 * License MIT: https://opensource.org/licenses/MIT
 * **************************************************
 */
using System.Collections.Generic;
using System.Linq;
using za.co.grindrodbank.a3s.AuthorisationPolicies;
using za.co.grindrodbank.a3s.ContentFormatters;
using za.co.grindrodbank.a3s.MediaTypeHeaders;
using za.co.grindrodbank.a3s.Models;
using za.co.grindrodbank.a3s.Repositories;
using za.co.grindrodbank.a3s.Services;
using AutoMapper;
using GlobalErrorHandling.Extensions;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Steeltoe.Management.Endpoint.Health;
using Steeltoe.Management.Endpoint.Info;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using za.co.grindrodbank.a3s.Managers;
using za.co.grindrodbank.a3s.Stores;
using za.co.grindrodbank.a3s.Helpers;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Mvc;

namespace za.co.grindrodbank.a3s
{
    public class Startup
    {
        private const string CONFIG_SCHEMA = "_ids4";

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            CurrentEnvironment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment CurrentEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // kestrel
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // IIS
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            // Register Steeltoes actuator endpoint services
            services.AddHealthActuator(Configuration); // Add general health checks actuator
            services.AddInfoActuator(Configuration); // Add Info Actuator

            // Add a database context for operating on the A3S data.
            services.AddDbContextPool<A3SContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("IdentityServerConnection")
                    ));

            // NOTE! Attempting to pool the configuration DB context results in the following exception.
            // The DbContext of type 'ConfigurationDbContext' cannot be pooled because it does not have a single public constructor accepting a single parameter of type DbContextOptions.
            services.AddDbContext<ConfigurationDbContext>(options =>
                options.UseNpgsql(
                    Configuration.GetConnectionString("IdentityServerConnection")
                    ));

            // We need to add this so we can correctly utilise the underlying user manager.
            services.AddIdentity<UserModel, IdentityRole>()
                .AddEntityFrameworkStores<A3SContext>()
                .AddDefaultTokenProviders()
                .AddUserManager<CustomUserManager>()
                .AddUserStore<CustomUserStore>();


            // Add an indentity service to use the configuration context, as the configuration store options are a dependent service of the ID Server configuration DB context.
            // We don't need to actually run the ID server and inject it into the pipeline, just configure it.
            services.AddIdentityServer()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = configurationBuilder =>
                        configurationBuilder.UseNpgsql(Configuration.GetConnectionString("IdentityServerConnection"));

                    options.DefaultSchema = CONFIG_SCHEMA;
                });

            services.AddAutoMapper(typeof(Startup));

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                // base-address of the identityserver
                options.Authority = Configuration["Jwt:Issuer"];
                // name of the API resource
                options.Audience = Configuration["Jwt:Audience"];

                if (CurrentEnvironment.IsDevelopment())
                {
                    options.RequireHttpsMetadata = false;
                }
            });


            services
                .AddMvc(options =>
                {
                    options.InputFormatters.Add(new YamlInputFormatter((Deserializer)new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build()));
                    options.OutputFormatters.Add(new YamlOutputFormatter((Serializer)new SerializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
                        .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
                        .Build()));

                    options.FormatterMappings.SetMediaTypeMappingForFormat("yaml", MediaTypeHeaderValues.ApplicationYaml);
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("permission:a3s.securityContracts.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.securityContracts.read")));
                options.AddPolicy("permission:a3s.securityContracts.update", policy => policy.Requirements.Add(new PermissionRequirement("a3s.securityContracts.update")));
                options.AddPolicy("permission:a3s.applications.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.applications.read")));
                options.AddPolicy("permission:a3s.clientRegistration.update", policy => policy.Requirements.Add(new PermissionRequirement("a3s.clientRegistration.update")));
                options.AddPolicy("permission:a3s.functions.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.functions.read")));
                options.AddPolicy("permission:a3s.functions.create", policy => policy.Requirements.Add(new PermissionRequirement("a3s.functions.create")));
                options.AddPolicy("permission:a3s.functions.update", policy => policy.Requirements.Add(new PermissionRequirement("a3s.functions.update")));
                options.AddPolicy("permission:a3s.functions.delete", policy => policy.Requirements.Add(new PermissionRequirement("a3s.functions.delete")));
                options.AddPolicy("permission:a3s.applicationFunctions.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.applicationFunctions.read")));
                options.AddPolicy("permission:a3s.permissions.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.permissions.read")));
                options.AddPolicy("permission:a3s.roles.create", policy => policy.Requirements.Add(new PermissionRequirement("a3s.roles.create")));
                options.AddPolicy("permission:a3s.roles.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.roles.read")));
                options.AddPolicy("permission:a3s.roles.update", policy => policy.Requirements.Add(new PermissionRequirement("a3s.roles.update")));
                options.AddPolicy("permission:a3s.roles.delete", policy => policy.Requirements.Add(new PermissionRequirement("a3s.roles.delete")));
                options.AddPolicy("permission:a3s.teams.create", policy => policy.Requirements.Add(new PermissionRequirement("a3s.teams.create")));
                options.AddPolicy("permission:a3s.teams.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.teams.read")));
                options.AddPolicy("permission:a3s.teams.update", policy => policy.Requirements.Add(new PermissionRequirement("a3s.teams.update")));
                options.AddPolicy("permission:a3s.teams.delete", policy => policy.Requirements.Add(new PermissionRequirement("a3s.teams.delete")));
                options.AddPolicy("permission:a3s.users.create", policy => policy.Requirements.Add(new PermissionRequirement("a3s.users.create")));
                options.AddPolicy("permission:a3s.users.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.users.read")));
                options.AddPolicy("permission:a3s.users.update", policy => policy.Requirements.Add(new PermissionRequirement("a3s.users.update")));
                options.AddPolicy("permission:a3s.users.delete", policy => policy.Requirements.Add(new PermissionRequirement("a3s.users.delete")));
                options.AddPolicy("permission:a3s.ldapAuthenticationModes.create", policy => policy.Requirements.Add(new PermissionRequirement("a3s.ldapAuthenticationModes.create")));
                options.AddPolicy("permission:a3s.ldapAuthenticationModes.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.ldapAuthenticationModes.read")));
                options.AddPolicy("permission:a3s.ldapAuthenticationModes.update", policy => policy.Requirements.Add(new PermissionRequirement("a3s.ldapAuthenticationModes.update")));
                options.AddPolicy("permission:a3s.ldapAuthenticationModes.delete", policy => policy.Requirements.Add(new PermissionRequirement("a3s.ldapAuthenticationModes.delete")));
                options.AddPolicy("permission:a3s.twoFactorAuth.remove", policy => policy.Requirements.Add(new PermissionRequirement("a3s.twoFactorAuth.remove")));
                options.AddPolicy("permission:a3s.twoFactorAuth.validateOtp", policy => policy.Requirements.Add(new PermissionRequirement("a3s.twoFactorAuth.validateOtp")));
                options.AddPolicy("permission:a3s.termsOfService.read", policy => policy.Requirements.Add(new PermissionRequirement("a3s.termsOfService.read")));
                options.AddPolicy("permission:a3s.termsOfService.create", policy => policy.Requirements.Add(new PermissionRequirement("a3s.termsOfService.create")));
                options.AddPolicy("permission:a3s.termsOfService.delete", policy => policy.Requirements.Add(new PermissionRequirement("a3s.termsOfService.delete")));
            });

            // Add policy handler services
            services.AddSingleton<IAuthorizationHandler, PermissionsAuthorisationHandler>();

            // Register all the repositories
            services.AddScoped<IIdentityApiResourceRepository, IdentityApiResouceRepository>();
            services.AddScoped<IApplicationRepository, ApplicationRepository>();
            services.AddScoped<ILdapAuthenticationModeRepository, LdapAuthenticationModeRepository>();
            services.AddScoped<IPermissionRepository, PermissionRepository>();
            services.AddScoped<IFunctionRepository, FunctionRepository>();
            services.AddScoped<IApplicationFunctionRepository, ApplicationFunctionRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<IIdentityClientRepository, IdentityClientRepository>();
            services.AddScoped<IApplicationDataPolicyRepository, ApplicationDataPolicyRepository>();
            services.AddScoped<ITermsOfServiceRepository, TermsOfServiceRepository>();

            // Resgister all the services.
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IFunctionService, FunctionService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            services.AddScoped<ILdapAuthenticationModeService, LdapAuthenticationModeService>();
            services.AddScoped<ISecurityContractService, SecurityContractService>();
            services.AddScoped<ISecurityContractApplicationService, SecurityContractApplicationService>();
            services.AddScoped<ISecurityContractClientService, SecurityContractClientService>();
            services.AddScoped<ISecurityContractDefaultConfigurationService, SecurityContractDefaultConfigurationService>();
            services.AddScoped<IApplicationFunctionService, ApplicationFunctionService>();
            services.AddScoped<ISafeRandomizerService, SafeRandomizerService>();
            services.AddScoped<ILdapConnectionService, LdapConnectionService>();
            services.AddScoped<ITwoFactorAuthService, TwoFactorAuthService>();
            services.AddScoped<ITermsOfServiceService, TermsOfServiceService>();

            // Register Helpers
            services.AddScoped<IArchiveHelper, ArchiveHelper>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.ConfigureExceptionHandler();

            if (!env.IsDevelopment())
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            // Expose the actual endpoints added by the Steeltoe services.
            app.UseHealthActuator();
            app.UseInfoActuator();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            // Bootstrap an admin user.
            BootstrapAdminUserWithRolesAndPermissions(app);

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/openapi-original.json", "A3S API");
            });
        }

        private void BootstrapAdminUserWithRolesAndPermissions(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<A3SContext>();

                var writePermission = context.Permission.Where(p => p.Name == "a3s.securityContracts.update").FirstOrDefault();

                if(writePermission == null)
                {
                    writePermission = new PermissionModel
                    {
                        Name = "a3s.securityContracts.update",
                        Description = "Enables idempotently applying (creating or updating) a security contract definition. This includes creation or updating of permissions, functions, applications and the relationships between them.",
                        ChangedBy = Guid.Empty
                    };
                }

                var readPermission = context.Permission.Where(p => p.Name == "a3s.securityContracts.read").FirstOrDefault();

                if(readPermission == null)
                {
                    readPermission = new PermissionModel
                    {
                        Name = "a3s.securityContracts.read",
                        Description = "Enables fetching of a security contract definition.",
                        ChangedBy = Guid.Empty
                    };
                }
                
                // Ensure that the A3S application is createdd.
                var application = context.Application.Where(a => a.Name == "a3s").FirstOrDefault();

                if (application == null)
                {
                    application = new ApplicationModel
                    {
                        Name = "a3s",
                        ApplicationFunctions = new List<ApplicationFunctionModel>()
                        {
                            new ApplicationFunctionModel
                            {
                                Application = application,
                                Name = "a3s.securityContracts",
                                ApplicationFunctionPermissions = new List<ApplicationFunctionPermissionModel>
                                {
                                    new ApplicationFunctionPermissionModel
                                    {
                                        Permission = writePermission
                                    },
                                    new ApplicationFunctionPermissionModel
                                    {
                                        Permission = readPermission
                                    }
                                }
                            }
                        }
                    };

                    context.Application.Add(application);
                    context.SaveChanges();
                }

                // Create the 'a3s.securityContractMaintenance' function.
                var function = context.Function.Where(f => f.Name == "a3s.securityContractMaintenance").FirstOrDefault();

                if(function == null)
                {
                    function = new FunctionModel
                    {
                        FunctionPermissions = new List<FunctionPermissionModel>(),
                        Name = "a3s.securityContractMaintenance",
                        Description = "Functionality to apply security contracts for micro-services.",
                        Application = application,
                        ChangedBy = Guid.Empty
                    };
                    function.FunctionPermissions.Add(new FunctionPermissionModel
                    {
                        Function = function,
                        Permission = writePermission,
                        ChangedBy = Guid.Empty
                    });
                    function.FunctionPermissions.Add(new FunctionPermissionModel
                    {
                        Function = function,
                        Permission = readPermission,
                        ChangedBy = Guid.Empty
                    });

                    context.Function.Add(function);
                    context.SaveChanges();
                }


                // Create the bootsrap Role.
                var bootstrapRole = context.Role.Where(r => r.Name == "a3s-bootstrap").FirstOrDefault();

                if(bootstrapRole == null)
                {
                    bootstrapRole = new RoleModel();
                    bootstrapRole.RoleFunctions = new List<RoleFunctionModel>();
                    bootstrapRole.Name = "a3s-bootstrap";
                    bootstrapRole.Description = "A3S bootstrap role for applying security contracts.";
                    bootstrapRole.ChangedBy = Guid.Empty;
                    bootstrapRole.RoleFunctions.Add(new RoleFunctionModel
                    {
                        Role = bootstrapRole,
                        Function = function,
                        ChangedBy = Guid.Empty
                    });

                    context.Role.Add(bootstrapRole);
                    context.SaveChanges();
                }

                // Check to see if the admin user is already present.
                var adminUser = context.User.Where(u => u.UserName == "a3s-bootstrap-admin").FirstOrDefault();

                if (adminUser != null)
                {
                    return;
                }
                
                adminUser = new UserModel();
                adminUser.FirstName = "Bootstrap";
                adminUser.Surname = "Admin";
                adminUser.UserName = "a3s-bootstrap-admin";
                adminUser.NormalizedUserName = "A3S-BOOTSTRAP-ADMIN";
                adminUser.Email = "a3s-bootstrap-admin@localhost";
                adminUser.NormalizedEmail = "A3S-BOOTSTRAP-ADMIN@LOCALHOST";
                adminUser.EmailConfirmed = true;
                adminUser.LdapAuthenticationModeId = null;
                adminUser.ChangedBy = Guid.Empty;

                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<UserModel>>();

                IdentityResult result = userManager.CreateAsync
                (adminUser, "Password1#").Result;

                if (result.Succeeded)
                {
                    adminUser = context.User.Where(u => u.UserName == "a3s-bootstrap-admin")
                                            .Include(u => u.UserRoles)
                                              .ThenInclude(ur => ur.Role)
                                            .FirstOrDefault();

                    adminUser.UserRoles.Add(new UserRoleModel {
                        User = adminUser,
                        Role = bootstrapRole,
                        ChangedBy = Guid.Empty
                    });

                    context.Entry(adminUser).State = EntityState.Modified;
                    context.SaveChanges();
                }
            }
        }
    }
}

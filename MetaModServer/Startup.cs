
using System.Collections.Generic;
using System.Text;
using AspNetCore.Identity.LiteDB;
using AspNetCore.Identity.LiteDB.Data;
using AspNetCore.Identity.LiteDB.Models;
using LiteDB.Async;
using MetaModFramework.DTOs;
using MetaModFramework.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using IdentityRole = AspNetCore.Identity.LiteDB.IdentityRole;

namespace MetaModFramework
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
            services.AddControllers();
            
            services.AddSingleton<ILiteDbContext, LiteDbInstance>()
                    .AddSingleton(provider => ((LiteDbInstance)provider.GetService<ILiteDbContext>())?.Database)
                    .AddSingleton<ItemTranslationLayer>();
            
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                                                                {
                                                                    options.Password.RequireDigit           = false;
                                                                    options.Password.RequireUppercase       = false;
                                                                    options.Password.RequireLowercase       = false;
                                                                    options.Password.RequireNonAlphanumeric = false;
                                                                    options.Password.RequiredLength         = 1;
                                                                })
                    .AddUserStore<LiteDbUserStore<ApplicationUser>>()
                    .AddRoleStore<LiteDbRoleStore<IdentityRole>>()
                    .AddDefaultTokenProviders();
            
            services.AddAuthentication(options =>
                                       {
                                           options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                                           options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
                                           options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
                                       })
                    .AddJwtBearer(options =>  
                                  {
                                      options.SaveToken            = true;  
                                      options.RequireHttpsMetadata = true;
                                      options.TokenValidationParameters = new TokenValidationParameters 
                                                                          {  
                                                                              ValidateIssuer = true,  
                                                                              ValidateAudience = true,
                                                                              ValidAudiences = Configuration.GetSection("JWT:ValidAudiences").Get<List<string>>(),
                                                                              ValidIssuer = Configuration["JWT:ValidIssuer"],  
                                                                              IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))  
                                                                          };
                                  }); 
            services.AddSwaggerGen(c =>
                                   {
                                       c.SwaggerDoc("v1",
                                                    new OpenApiInfo { Title = "MetaModServer", Version = "v1" });
                                   });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseHttpsRedirection()
               .UseSwagger()
               .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaModServer v1"))
               .UseRouting()
               .UseAuthentication()
               .UseAuthorization()
               .UseEndpoints(endpoints =>
                             {
                                 endpoints.MapControllers();
                             });

            var db = app.ApplicationServices.GetService<LiteDatabaseAsync>(); 
            
            if (await db!.GetCollection<ApiReference>().CountAsync() == 0)
            {
                await db.GetCollection<ApiReference>().InsertAsync(new ApiReference());
            }

            await app.ApplicationServices.GetService<ItemTranslationLayer>()!.DisposeAsync();
        }
    }
}
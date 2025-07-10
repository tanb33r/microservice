using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PlatformService.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PlatformService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            if (builder.Environment.IsProduction())
            {
                Console.WriteLine("--> Using SqlServer Db");
                builder.Services.AddDbContext<AppDbContext>(opt =>
                    opt.UseSqlServer(builder.Configuration.GetConnectionString("PlatformsConn")));
            }
            else
            {
                Console.WriteLine("--> Using InMem Db");
                builder.Services.AddDbContext<AppDbContext>(opt =>
                    opt.UseInMemoryDatabase("InMem"));
            }

            builder.Services.AddScoped<IPlatformRepo, PlatformRepo>();
            // AddHttpClient, AddSingleton, AddGrpc, AddControllers, AddAutoMapper, AddSwaggerGen
            builder.Services.AddHttpClient(); // Add specific interfaces/implementations as needed
            builder.Services.AddGrpc();
            builder.Services.AddControllers();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlatformService", Version = "v1" });
            });

            Console.WriteLine($"--> CommandService Endpoint {builder.Configuration["CommandService"]}");

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlatformService v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                // endpoints.MapGrpcService<GrpcPlatformService>(); // Uncomment and add using if needed
                endpoints.MapGet("/protos/platforms.proto", async context =>
                {
                    await context.Response.WriteAsync(File.ReadAllText("Protos/platforms.proto"));
                });
            });

            PrepDb.PrepPopulation(app, app.Environment.IsProduction());

            app.Run();
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyMeetUp.FunctionApp.EventQueueReceiver.Data;
using MyMeetUp.Web.Models;
using System;

[assembly: FunctionsStartup(typeof(MyMeetUp.FunctionApp.EventQueueReceiver.Startup))]
namespace MyMeetUp.FunctionApp.EventQueueReceiver
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder) {
            //Dependency Injection
            //SQL Server service
            var connectionString = Environment.GetEnvironmentVariable("MyMeetUpDb_Dev");
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            //Identity service
            builder.Services.AddIdentityCore<ApplicationUser>()
              .AddSignInManager()
              .AddEntityFrameworkStores<ApplicationDbContext>()
              .AddDefaultTokenProviders();
        }
    }
}

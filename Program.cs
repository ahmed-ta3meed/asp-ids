using duende;
using duende.Models;
using Serilog;
using Microsoft.EntityFrameworkCore;
using duende.Data;
using Microsoft.AspNetCore.Identity;
using IdentityModel;
using System.Security.Claims;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using IdentityModel.Client;

Log.Logger = new LoggerConfiguration()
.WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{

    var builder = WebApplication.CreateBuilder(args);
    

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();

    var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var signinMgr = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();


    app.MapPost("/", async (LoginRequest loginRequest) =>
    {
        var user = userMgr.FindByEmailAsync(loginRequest.Email).Result;
        Console.WriteLine(user.Id);
        //var result = await signinMgr.PasswordSignInAsync(user.UserName, loginRequest.Password, true, false);
        //await eventService.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));
        //var client = new HttpClient();

        //var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
        //{
        //    Address = "https://localhost:5001/connect/token",
        //    ClientId = "m2m.client",
        //    ClientSecret = "secret",
        //    Scope = "scope1"
        //});
        //Console.WriteLine(response.IsError);
        //Console.WriteLine(response.ErrorDescription);
        //Console.WriteLine(response.ErrorType);
        //Console.WriteLine(response.IsError);
        //return response.AccessToken;
    });

    app.MapPost("/sign-up", async (CreateUserRequest createUserDto) =>
    {
        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = userMgr.FindByNameAsync(createUserDto.Email).Result;
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = createUserDto.FirstName + createUserDto.LastName,
                Email = createUserDto.Email,
                EmailConfirmed = true,
                TemporaryPassword = createUserDto.TemporaryPassword
            };
            var result = userMgr.CreateAsync(user, createUserDto.TemporaryPassword).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = userMgr.AddClaimsAsync(user, new Claim[]{
                            new Claim(JwtClaimTypes.Name, createUserDto.FirstName + createUserDto.LastName),
                            new Claim(JwtClaimTypes.GivenName, createUserDto.FirstName),
                            new Claim(JwtClaimTypes.FamilyName, createUserDto.LastName),
                            new Claim(JwtClaimTypes.Email, createUserDto.Email),
                        }).Result;
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
            Log.Debug("user created");
        }
        else
        {
            Log.Debug($"user with email {createUserDto.Email} already exists");
        }

    });

    // this seeding is only for the template to bootstrap the DB and users.
    // in production you will likely want a different approach.
    if (args.Contains("/seed"))
    {
         
        Log.Information("Seeding database...");
        SeedData.EnsureSeedData(app);
        Log.Information("Done seeding database. Exiting.");
        return;
    }

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException") // https://github.com/dotnet/runtime/issues/60600
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
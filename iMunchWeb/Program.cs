using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using iMunchWeb.Data;
using iMunchWeb.Data.Entities;
using iMunchWeb.Feature.Auth.Services;
using iMunchWeb.Feature.Project.Commands;
using iMunchWeb.Feature.Project.Const;
using iMunchWeb.Feature.Project.Helpers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

var appSettingsSection = builder.Configuration.GetSection("AppSettings");
var appSettings = appSettingsSection.Get<AppSettings>();
var key = Encoding.ASCII.GetBytes(appSettings.JwtSecret);

builder.Services.AddAuthentication(co =>
    {
        co.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        co.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddIdentityServerJwt().AddJwtBearer(jbo =>
    {
        jbo.RequireHttpsMetadata = false;
        jbo.SaveToken = true;
        jbo.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAuthorization();
builder.Services.AddMediatR(typeof(BaseResult));
builder.Services.AddSingleton<AppSettings>(appSettings);
builder.Services.AddTransient<ITokenService, TokenService>();
builder.Services.AddCors();

var app = builder.Build();

void CreateRoles()
{
    using var scope = app.Services.CreateScope();
    var roleManager = scope.ServiceProvider.GetService(typeof(RoleManager<IdentityRole>)) as RoleManager<IdentityRole>;
    var roles = new List<string>
    {
        Roles.Customer,
        Roles.Supplier
    };
    foreach (var r in roles)
    {
        if (roleManager == null) continue;
        var hasRole = roleManager.RoleExistsAsync(r);
        hasRole.Wait();
        if (!hasRole.Result)
        {
            roleManager.CreateAsync(new IdentityRole(r)).Wait();
        }
    }
}

CreateRoles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UsePathBase(new PathString("/api"));
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller}/{action=Index}/{id?}"
    );
});

if (app.Environment.IsDevelopment())
{
    app.UseSpa(spa => { spa.UseProxyToSpaDevelopmentServer("http://localhost:5173"); });
}
else
{
    app.MapFallbackToFile("index.html");
}

app.Run();
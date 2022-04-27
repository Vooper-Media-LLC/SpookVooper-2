global using SV2.Database;
global using SV2.Database.Models.Districts;
global using SV2.Database.Models.Economy;
global using SV2.Database.Models.Entities;
global using SV2.Database.Models.Factories;
global using SV2.Database.Models.Forums;
global using SV2.Database.Models.Government;
global using SV2.Database.Models.Groups;
global using SV2.Database.Models.Items;
global using SV2.Database.Models.Military;
global using SV2.Database.Models.Permissions;
global using SV2.Database.Models.Buildings;
global using SV2.Database.Models.Users;
global using SV2.Database.Models.OAuth2;
global using SV2.Models.Districts;
global using SV2.Managers;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SV2.API;
using SV2.Workers;
using SV2.Managers;
using SV2.VoopAI;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;

await VoopAI.Main();

var builder = WebApplication.CreateBuilder(args);
string CONF_LOC = "SV2Config/";
string DBCONF_FILE = "DBConfig.json";

// Add services to the container.
builder.Services.AddMvc()
    .AddRazorRuntimeCompilation();

// Create directory if it doesn't exist
if (!Directory.Exists(CONF_LOC))
{
    Directory.CreateDirectory(CONF_LOC);
}

// Load database settings
DBConfig dbconfig;
if (File.Exists(CONF_LOC + DBCONF_FILE))
{
    // If there is a config, read it
    dbconfig = await JsonSerializer.DeserializeAsync<DBConfig>(File.OpenRead(CONF_LOC + DBCONF_FILE));
}
else
{
    // Otherwise create a config with default values and write it to the location
    dbconfig = new DBConfig()
    {
        Database = "database",
        Host = "host",
        Password = "password",
        Username = "user"
    };

    File.WriteAllText(CONF_LOC + DBCONF_FILE, JsonSerializer.Serialize(dbconfig));
    Console.WriteLine("Error: No DB config was found. Creating file...");
}

builder.Services.AddDbContextPool<VooperDB>(options =>
{
    options.UseNpgsql(VooperDB.ConnectionString, options => options.EnableRetryOnFailure());
});

builder.Services.AddHostedService<EconomyWorker>();
builder.Services.AddHostedService<TransactionWorker>();

builder.Services.AddDataProtection().PersistKeysToDbContext<VooperDB>();

builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromDays(90);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//BaseAPI       .AddRoutes(app);
ItemAPI        .AddRoutes(app);
EcoAPI         .AddRoutes(app);
EntityAPI      .AddRoutes(app);
DevAPI         .AddRoutes(app);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// ensure districts & Vooperia are created
await VooperDB.Startup();
await ResourceManager.Load();

app.Run();

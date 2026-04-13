using BBScoreboard.Infrastructure;
using BBScoreboard.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BBScoreboardDbContext>(name: "database");

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }

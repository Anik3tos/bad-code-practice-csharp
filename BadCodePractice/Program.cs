using BadCodePractice.Components;
using BadCodePractice.Data;
using BadCodePractice.Features.EfCoreChallenge;
using BadCodePractice.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<QueryMetrics>();
builder.Services.AddSingleton<QueryCountingInterceptor>();

var databaseProvider = builder.Configuration["Database:Provider"]?.Trim().ToLowerInvariant() ?? "postgres";
var sqliteConnectionString = builder.Configuration.GetConnectionString("ChallengeDbSqlite")
    ?? "Data Source=bad-code-practice.db";
var postgresConnectionString = builder.Configuration.GetConnectionString("ChallengeDbPostgres")
    ?? "Host=localhost;Port=5433;Database=bad_code_practice;Username=postgres;Password=postgres";

builder.Services.AddDbContext<ChallengeDbContext>((serviceProvider, options) =>
{
    switch (databaseProvider)
    {
        case "postgres":
        case "postgresql":
            options.UseNpgsql(postgresConnectionString, npgsql => npgsql.EnableRetryOnFailure(3));
            break;
        case "sqlite":
            options.UseSqlite(sqliteConnectionString);
            break;
        default:
            throw new InvalidOperationException(
                $"Unsupported Database:Provider '{databaseProvider}'. Use 'postgres' or 'sqlite'.");
    }

    options.AddInterceptors(serviceProvider.GetRequiredService<QueryCountingInterceptor>());
});

builder.Services.AddScoped<BadOrderReportService>();
builder.Services.AddScoped<RefactoredOrderReportService>();
builder.Services.AddScoped<EfCoreChallengeRunner>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ChallengeDbContext>();
    await ChallengeSeeder.SeedAsync(dbContext);
}

app.Run();

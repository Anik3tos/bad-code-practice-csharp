using BadCodePractice.Components;
using BadCodePractice.Features.CachingChallenge;
using BadCodePractice.Data;
using BadCodePractice.Features.EfCoreChallenge;
using BadCodePractice.Features.MemoryLeakChallenge;
using BadCodePractice.Features.AsyncMisuseChallenge;
using BadCodePractice.Features.ConcurrencyChallenge;
using BadCodePractice.Features.DiLifetimeChallenge;
using BadCodePractice.Features.AllocationChallenge;
using BadCodePractice.Features.ResiliencyRetryChallenge;
using BadCodePractice.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Disable scope validation so we can demonstrate the captive dependency anti-pattern
// By default, ASP.NET Core checks for scoped services injected into singletons during development
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = false;
    options.ValidateOnBuild = false;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<QueryMetrics>();
builder.Services.AddSingleton<QueryCountingInterceptor>();

var databaseProvider = builder.Configuration["Database:Provider"]?.Trim().ToLowerInvariant() ?? "postgres";
var sqliteConnectionString = builder.Configuration.GetConnectionString("ChallengeDbSqlite")
                             ?? "Data Source=bad-code-practice.db";
var postgresConnectionString = builder.Configuration.GetConnectionString("ChallengeDbPostgres")
                               ??
                               "Host=localhost;Port=5433;Database=bad_code_practice;Username=postgres;Password=postgres";

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
builder.Services.AddScoped<PracticeOrderReportService>();
builder.Services.AddScoped<RefactoredOrderReportService>();
builder.Services.AddScoped<EfCoreChallengeRunner>();
builder.Services.AddScoped<BadCachingChallengeService>();
builder.Services.AddScoped<PracticeCachingChallengeService>();
builder.Services.AddScoped<RefactoredCachingChallengeService>();
builder.Services.AddScoped<CachingChallengeRunner>();
builder.Services.AddScoped<BadMemoryLeakService>();
builder.Services.AddScoped<PracticeMemoryLeakService>();
builder.Services.AddScoped<RefactoredMemoryLeakService>();
builder.Services.AddScoped<MemoryLeakChallengeRunner>();
builder.Services.AddScoped<BadAsyncMisuseService>();
builder.Services.AddScoped<PracticeAsyncMisuseService>();
builder.Services.AddScoped<RefactoredAsyncMisuseService>();
builder.Services.AddScoped<AsyncMisuseChallengeRunner>();
builder.Services.AddScoped<BadConcurrencyService>();
builder.Services.AddScoped<PracticeConcurrencyService>();
builder.Services.AddScoped<RefactoredConcurrencyService>();
builder.Services.AddScoped<ConcurrencyChallengeRunner>();
builder.Services.AddScoped<BadAllocationService>();
builder.Services.AddScoped<PracticeAllocationService>();
builder.Services.AddScoped<RefactoredAllocationService>();
builder.Services.AddScoped<AllocationChallengeRunner>();
builder.Services.AddScoped<BadResiliencyRetryService>();
builder.Services.AddScoped<PracticeResiliencyRetryService>();
builder.Services.AddScoped<RefactoredResiliencyRetryService>();
builder.Services.AddScoped<ResiliencyRetryChallengeRunner>();

// DI Lifetime Challenge Services
builder.Services.AddScoped<IScopedState, ScopedState>();
builder.Services.AddTransient<ITransientOperation, TransientOperation>();

// Crucially, these services must be registered as Singletons to demonstrate the captive dependency
builder.Services.AddSingleton<BadDiLifetimeService>();
builder.Services.AddSingleton<PracticeDiLifetimeService>();
builder.Services.AddSingleton<RefactoredDiLifetimeService>();
builder.Services.AddSingleton<DiLifetimeChallengeRunner>();

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

using Microsoft.EntityFrameworkCore;
using SmartShop.Data;
using SmartShop.Infrastructure;
using SmartShop.Services;
using SmartShop.Services.Gemini;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SmartShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SmartShop"),
        sql => sql.EnableRetryOnFailure(5)));

builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.SectionName));
builder.Services.AddHttpClient<IGeminiClient, GeminiClient>((sp, http) =>
{
    var baseUrl = builder.Configuration[$"{GeminiOptions.SectionName}:BaseUrl"]
                  ?? "https://generativelanguage.googleapis.com/v1beta/";
    http.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
    http.Timeout = TimeSpan.FromSeconds(60);
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ChatHistoryStore>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<ShoppingAssistantService>();
builder.Services.AddScoped<ReportService>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddControllers();
builder.Services.AddRazorPages();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SmartShopDbContext>();
    await db.Database.MigrateAsync();
}

if (string.IsNullOrWhiteSpace(app.Configuration[$"{GeminiOptions.SectionName}:ApiKey"]))
    app.Logger.LogWarning("Gemini:ApiKey is not set. AI endpoints will return 503 until it is configured (see README.md).");

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapRazorPages();

app.Run();

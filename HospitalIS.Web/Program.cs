using HospitalIS.Web.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<HospitalContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HospitalContext>();

    const int maxRetries = 10;
    var retryDelay = TimeSpan.FromSeconds(2);

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            DbInitializer.Initialize(dbContext);
            break;
        }
        catch (Exception exception) when (attempt < maxRetries)
        {
            app.Logger.LogWarning(
                exception,
                "Ошибка инициализации БД. Повтор {Attempt}/{MaxRetries} через {DelaySeconds} сек.",
                attempt,
                maxRetries,
                retryDelay.TotalSeconds);

            Thread.Sleep(retryDelay);
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/health", async (HospitalContext dbContext) =>
{
    try
    {
        await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
        return Results.Ok(new { status = "healthy", utcTime = DateTime.UtcNow });
    }
    catch
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});

app.Run();


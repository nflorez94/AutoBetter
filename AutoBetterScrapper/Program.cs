using AutoBetterScrapper.Models.Settings;
using AutoBetterScrapper.Repositories;
using AutoBetterScrapper.Servicios;
using AutoBetterScrapper.Transversals.Mappers;
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddTransient<IScrapperService, ScrapperService>();
builder.Services.AddTransient<IScrapperRepository, ScrapperRepository>();
builder.Services.AddTransient<IJobsService, JobsService>();
builder.Services.AddAutoMapper(typeof(ScrappingMappers)); ;
builder.Services.Configure<LoginInformation>(builder.Configuration.GetSection("LoginInformation"));
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangFire"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));
builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseHangfireDashboard("/hangfire");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var jobsService= builder.Services.BuildServiceProvider().GetRequiredService<IJobsService>();

//BackgroundJob.Schedule(() => jobsService.UpdateBalance(null), TimeSpan.FromMilliseconds(600000));
//BackgroundJob.Schedule(() => jobsService.UpdateBalance(null), TimeSpan.FromMilliseconds(0));

app.Run();



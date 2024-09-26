using Microsoft.FeatureManagement;
using WebDemoNet6;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from Azure App Configuration
builder.Configuration.AddAzureAppConfiguration(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("AppConfigurationConnectionString");
    options.Connect(connectionString)
           // Load all keys that start with `WebDemo:` and have no label
           .Select("WebDemo:*")
           // Configure to reload configuration if any of the selected keys change
           .ConfigureRefresh(refreshOptions =>
           {
               refreshOptions.Register("WebDemo:Settings:BackgroundColor", refreshAll: true)
                             .Register("WebDemo:Settings:FontSize", refreshAll: true)
                             .Register("WebDemo:Settings:Messages", refreshAll: true)
                             .SetCacheExpiration(TimeSpan.FromSeconds(30));
           })
           // Load all feature flags with no label
           .UseFeatureFlags(featureFlagOptions =>
           {
               featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(30);
           });
});

// Add services to the container.
builder.Services.AddRazorPages();

// Add Azure App Configuration and feature management services to the container.
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();

// Bind configuration to the Settings object
builder.Services.Configure<Settings>(builder.Configuration.GetSection("WebDemo:Settings"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Use Azure App Configuration middleware for dynamic configuration refresh.
app.UseAzureAppConfiguration();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

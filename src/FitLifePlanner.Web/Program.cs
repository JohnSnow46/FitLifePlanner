using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FitLifePlanner.Web;
using FitLifePlanner.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TokenStore>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddTransient<BearerTokenHandler>();

builder.Services.AddHttpClient<UsersApiClient>(c => c.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!))
    .AddHttpMessageHandler<BearerTokenHandler>();

await builder.Build().RunAsync();

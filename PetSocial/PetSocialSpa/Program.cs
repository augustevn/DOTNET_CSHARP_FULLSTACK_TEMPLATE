using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PetSocialSpa;
using SharedSpa;
using SharedSpa.Config;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSharedSpa(builder.Configuration);

var backendConfig = builder.Configuration.GetSection(nameof(BackendConfig)).Get<BackendConfig>();
builder.Services.AddScoped(_ => new HttpClient {BaseAddress = new Uri(backendConfig.BaseUrl)});

await builder.Build().RunAsync();
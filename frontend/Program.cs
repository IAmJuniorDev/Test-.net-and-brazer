using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services; 
using frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register MudBlazor services
builder.Services.AddMudServices();

// Register the ApiService
builder.Services.AddScoped<ApiService>();

// Configure the HttpClient with the base address of your backend API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5251") });

await builder.Build().RunAsync();

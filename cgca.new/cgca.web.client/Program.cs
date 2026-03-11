using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using cgca.web.client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
    Timeout = TimeSpan.FromSeconds(30)
});
builder.Services.AddScoped<IChatService, ChatService>();

await builder.Build().RunAsync();

using Blazored.Modal;
using Constellation.Portal.Parents.Client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("Constellation.Portal.Parents.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Constellation.Portal.Parents.ServerAPI"));

builder.Services.AddApiAuthorization();

builder.Services.AddBlazoredModal();

// Update culture to always run in en-AU location to ensure that dates are formatted correctly
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-AU");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-AU");

await builder.Build().RunAsync();

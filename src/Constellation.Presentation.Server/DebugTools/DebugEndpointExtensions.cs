namespace Constellation.Presentation.Server.DebugTools;

using System.Globalization;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

public static class DebugEndpointExtensions
{
    public static WebApplication MapDebugAuth(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.Map("/debug/auth", async context =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";

            var user = context.User;
            StringBuilder sb = new();

            sb.Append("""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <title>Auth Debug</title>
                <style>
                    body { font-family: system-ui, sans-serif; padding: 2rem; background: #f5f5f5; }
                    h1, h2 { margin-bottom: 0.25rem; }
                    .card {
                        background: white; border: 1px solid #ddd; border-radius: 8px;
                        padding: 1.25rem 1.5rem; margin-bottom: 1.5rem;
                    }
                    .badge {
                        display: inline-block; padding: 0.2rem 0.6rem; border-radius: 4px;
                        font-size: 0.8rem; font-weight: 600;
                    }
                    .badge-green { background: #d4edda; color: #1a7f37; }
                    .badge-red   { background: #fde8e8; color: #9a3e0b; }
                    table { width: 100%; border-collapse: collapse; font-size: 0.875rem; margin-top: 0.75rem; }
                    thead { background: #2d2d2d; color: white; }
                    th, td { padding: 0.5rem 1rem; text-align: left; border-top: 1px solid #eee; }
                    th { border-top: none; }
                    td:first-child { width: 35%; color: #555; word-break: break-all; }
                    td:last-child  { word-break: break-all; }
                    tr:hover td { background: #f0f7ff; }
                </style>
            </head>
            <body>
                <h1>Auth Debug</h1>
            """);

            // Authentication status
            sb.Append("<div class=\"card\">");
            sb.Append("<h2>Status</h2>");

            if (user.Identity?.IsAuthenticated == true)
            {
                sb.Append("<p><span class=\"badge badge-green\">Authenticated</span></p>");
                sb.Append(CultureInfo.InvariantCulture, $"<p><strong>Name:</strong> {user.Identity.Name}</p>");
                sb.Append(CultureInfo.InvariantCulture, $"<p><strong>Auth Type:</strong> {user.Identity.AuthenticationType}</p>");
            }
            else
            {
                sb.Append("<p><span class=\"badge badge-red\">Not Authenticated</span></p>");
            }
            sb.Append("</div>");

            // Claims
            sb.Append("<div class=\"card\">");
            sb.Append("<h2>Claims</h2>");
            sb.Append("""
            <table>
                <thead><tr><th>Type</th><th>Value</th></tr></thead>
                <tbody>
            """);

            foreach (var claim in user.Claims)
            {
                // Shorten the long schema URIs for readability
                string claimType = claim.Type
                    .Replace("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/", "xmlsoap: ")
                    .Replace("http://schemas.microsoft.com/ws/2008/06/identity/claims/", "ms: ");

                sb.Append(CultureInfo.InvariantCulture,
                    $"<tr><td title=\"{claim.Type}\">{claimType}</td><td>{claim.Value}</td></tr>");
            }

            sb.Append("</tbody></table></div>");

            // Identities
            sb.Append("<div class=\"card\">");
            sb.Append("<h2>Identities</h2>");
            sb.Append("""
            <table>
                <thead><tr><th>Auth Type</th><th>Is Authenticated</th><th>Name</th></tr></thead>
                <tbody>
            """);

            foreach (var identity in user.Identities)
            {
                sb.Append(CultureInfo.InvariantCulture,
                    $"<tr><td>{identity.AuthenticationType}</td><td>{identity.IsAuthenticated}</td><td>{identity.Name}</td></tr>");
            }

            sb.Append("</tbody></table></div>");

            sb.Append("</body></html>");
            await context.Response.WriteAsync(sb.ToString());
        });

        return app;
    }

    public static WebApplication MapDebugServices(this WebApplication app, IServiceCollection services)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.Map("/debug/services", hostBuilder => hostBuilder.Run(async context =>
        {
            context.Response.ContentType = "text/html; charset=utf-8";

            StringBuilder sb = new();
            sb.Append("""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                    <meta charset="utf-8" />
                    <meta name="viewport" content="width=device-width, initial-scale=1" />
                    <title>Registered Services</title>
                    <style>
                        body {
                            font-family: system-ui, sans-serif;
                            padding: 2rem;
                            background: #f5f5f5;
                            color: #1a1a1a;
                        }
                        h1 { margin-bottom: 0.25rem; }
                        #summary {
                            color: #555;
                            margin-bottom: 1rem;
                            font-size: 0.9rem;
                        }
                        #search {
                            width: 100%;
                            max-width: 500px;
                            padding: 0.5rem 0.75rem;
                            font-size: 1rem;
                            border: 1px solid #ccc;
                            border-radius: 6px;
                            margin-bottom: 1rem;
                            box-sizing: border-box;
                        }
                        .table-wrapper {
                            overflow-x: auto;
                            border-radius: 8px;
                            border: 1px solid #ddd;
                            background: white;
                        }
                        table {
                            width: 100%;
                            border-collapse: collapse;
                            font-size: 0.875rem;
                        }
                        thead {
                            background: #2d2d2d;
                            color: white;
                            position: sticky;
                            top: 0;
                        }
                        th {
                            padding: 0.65rem 1rem;
                            text-align: left;
                            white-space: nowrap;
                        }
                        td {
                            padding: 0.5rem 1rem;
                            border-top: 1px solid #eee;
                            word-break: break-all;
                            vertical-align: top;
                        }
                        tr:hover td { background: #f0f7ff; }
                        .lifetime-Singleton { color: #1a7f37; font-weight: 600; }
                        .lifetime-Scoped    { color: #0969da; font-weight: 600; }
                        .lifetime-Transient { color: #9a3e0b; font-weight: 600; }
                        #no-results {
                            display: none;
                            padding: 2rem;
                            text-align: center;
                            color: #888;
                        }
                    </style>
                </head>
                <body>
                    <h1>Registered Services</h1>
                """);

            var serviceList = services.ToList();

            sb.Append(CultureInfo.InvariantCulture,
                $"<p id=\"summary\">Showing <span id=\"visible-count\">{serviceList.Count}</span> of {serviceList.Count} registered services</p>");

            sb.Append("""
                    <input type="search" id="search" placeholder="Search by type, lifetime, or implementation..." autocomplete="off" />
                    <div class="table-wrapper">
                        <table id="services-table">
                            <thead>
                                <tr>
                                    <th style="width:45%">Service Type</th>
                                    <th style="width:10%">Lifetime</th>
                                    <th style="width:45%">Implementation</th>
                                </tr>
                            </thead>
                            <tbody>
                """);

            foreach (ServiceDescriptor svc in serviceList)
            {
                string lifetime = svc.Lifetime.ToString();
                sb.Append(CultureInfo.InvariantCulture, $"""
                                <tr>
                                    <td>{svc.ServiceType.FullName}</td>
                                    <td><span class="lifetime-{lifetime}">{lifetime}</span></td>
                                    <td>{svc.ImplementationType?.FullName ?? "<em>factory / instance</em>"}</td>
                                </tr>
                    """);
            }

            sb.Append("""
                            </tbody>
                        </table>
                        <p id="no-results">No services match your search.</p>
                    </div>
                    <script>
                        const search   = document.getElementById('search');
                        const rows     = document.querySelectorAll('#services-table tbody tr');
                        const noRes    = document.getElementById('no-results');
                        const countEl  = document.getElementById('visible-count');

                        search.addEventListener('input', () => {
                            const term = search.value.toLowerCase();
                            let visible = 0;
                            rows.forEach(row => {
                                const match = row.textContent.toLowerCase().includes(term);
                                row.style.display = match ? '' : 'none';
                                if (match) visible++;
                            });
                            countEl.textContent = visible;
                            noRes.style.display = visible === 0 ? 'block' : 'none';
                        });
                    </script>
                </body>
                </html>
                """);

            await context.Response.WriteAsync(sb.ToString());
        }));

        return app;
    }
}
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System.Text;
using System.Text.RegularExpressions;

namespace Constellation.Infrastructure.Templates.Services
{
    // Code from: https://github.com/aspnet/Entropy/blob/dev/samples/Mvc.RenderViewToString/RazorViewToStringRenderer.cs

    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        private IRazorViewEngine _viewEngine;
        private ITempDataProvider _tempDataProvider;
        private IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var actionContext = GetActionContext();
            var view = FindView(actionContext, viewName);

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary()) { Model = model },
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        _tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                return output.ToString();
            }
        }

        public async Task<RenderedEmail> RenderEmail<TModel>(string viewName, TModel model)
        {
            string html = await RenderViewToStringAsync(viewName, model);

            string sanitisedHtml = SafeLinksSanitiser.UnwrapAllInHtml(html);

            string plainText = Convert(sanitisedHtml);

            return new RenderedEmail(sanitisedHtml, plainText);
        }

        private IView FindView(ActionContext actionContext, string viewName)
        {
            var getViewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = _viewEngine.FindView(actionContext, viewName, isMainPage: true);
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
            var errorMessage = string.Join(
                Environment.NewLine,
                new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(
                    searchedLocations));
            ;

            throw new InvalidOperationException(errorMessage);
        }

        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = _serviceProvider;
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }

        private string Convert(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            HtmlDocument doc = new();
            doc.LoadHtml(html);

            // Remove non-visible elements entirely
            RemoveNodes(doc, "//script");
            RemoveNodes(doc, "//style");
            RemoveNodes(doc, "//head");

            StringBuilder sb = new();
            ProcessNode(doc.DocumentNode, sb);

            // Collapse multiple blank lines into a single blank line
            string result = Regex.Replace(sb.ToString(), @"\n{3,}", "\n\n");

            return result.Trim();
        }

        private static void RemoveNodes(HtmlDocument doc, string xpath)
        {
            foreach (var node in doc.DocumentNode.SelectNodes(xpath) ?? Enumerable.Empty<HtmlNode>())
                node.Remove();
        }

        private static void ProcessNode(HtmlNode node, StringBuilder sb)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Text:
                    string text = HtmlEntity.DeEntitize(node.InnerText);
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.Append(text);
                    break;

                case HtmlNodeType.Element:
                    ProcessElement(node, sb);
                    break;

                case HtmlNodeType.Document:
                    foreach (var child in node.ChildNodes)
                        ProcessNode(child, sb);
                    break;
            }
        }

        private static void ProcessElement(HtmlNode node, StringBuilder sb)
        {
            switch (node.Name.ToLowerInvariant())
            {
                // Block elements — add newlines around content
                case "p":
                case "div":
                case "section":
                case "article":
                    sb.AppendLine();
                    foreach (var child in node.ChildNodes)
                        ProcessNode(child, sb);
                    sb.AppendLine();
                    break;

                // Headings — uppercase and surround with newlines
                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    sb.AppendLine();
                    string heading = HtmlEntity.DeEntitize(node.InnerText).ToUpperInvariant();
                    sb.AppendLine(heading);
                    sb.AppendLine(new string('-', heading.Length));
                    break;

                // Line breaks
                case "br":
                    sb.AppendLine();
                    break;

                // Horizontal rule
                case "hr":
                    sb.AppendLine();
                    sb.AppendLine(new string('-', 40));
                    break;

                // List items
                case "li":
                    sb.Append("  • ");
                    foreach (var child in node.ChildNodes)
                        ProcessNode(child, sb);
                    sb.AppendLine();
                    break;

                // Links — preserve the URL
                case "a":
                    string linkText = HtmlEntity.DeEntitize(node.InnerText).Trim();
                    string? href = node.GetAttributeValue("href", null);

                    if (!string.IsNullOrWhiteSpace(href)
                        && !href.StartsWith("#")
                        && href != linkText)
                        sb.Append($"{linkText} ({href})");
                    else
                        sb.Append(linkText);
                    break;

                // Images — use alt text if available
                case "img":
                    string? alt = node.GetAttributeValue("alt", null);
                    if (!string.IsNullOrWhiteSpace(alt))
                        sb.Append($"[{alt}]");
                    break;

                // Skip tracking pixel — 1x1 images with no meaningful alt text
                // (already handled by the img case above returning nothing for empty alt)

                // Table structure — treat rows as lines, cells as tab-separated
                case "tr":
                    foreach (var child in node.ChildNodes)
                        ProcessNode(child, sb);
                    sb.AppendLine();
                    break;

                case "td":
                case "th":
                    foreach (var child in node.ChildNodes)
                        ProcessNode(child, sb);
                    sb.Append('\t');
                    break;

                // Everything else — just recurse into children
                default:
                    foreach (var child in node.ChildNodes)
                        ProcessNode(child, sb);
                    break;
            }
        }

        private static class SafeLinksSanitiser
        {
            private const string SafeLinksHost = "safelinks.protection.outlook.com";

            /// <summary>
            /// If the provided URL is an O365 Safe Links redirect, returns the original
            /// destination URL. Otherwise returns the input unchanged.
            /// </summary>
            public static string Unwrap(string url)
            {
                if (string.IsNullOrWhiteSpace(url))
                    return url;

                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                    return url;

                if (!uri.Host.EndsWith(SafeLinksHost, StringComparison.OrdinalIgnoreCase))
                    return url;

                // Extract the inner 'url' query parameter
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var inner = query["url"];

                if (string.IsNullOrWhiteSpace(inner))
                    return url;

                // Validate the extracted URL before returning it
                if (!Uri.TryCreate(inner, UriKind.Absolute, out var innerUri)
                    || (innerUri.Scheme != Uri.UriSchemeHttp
                        && innerUri.Scheme != Uri.UriSchemeHttps))
                    return url;

                return inner;
            }

            ///// <summary>
            ///// Unwraps all O365 Safe Links redirects found in a block of HTML content.
            ///// </summary>
            //public static string UnwrapAllInHtml(string html)
            //{
            //    if (string.IsNullOrWhiteSpace(html))
            //        return html;

            //    return System.Text.RegularExpressions.Regex.Replace(
            //        html,
            //        @"https://[a-z0-9]+\.safelinks\.protection\.outlook\.com/\?[^\s""'<>]+",
            //        match => Unwrap(match.Value),
            //        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            //}

            /// <summary>
            /// Unwraps all O365 Safe Links redirects found in anchor tags in an HTML document.
            /// </summary>
            public static string UnwrapAllInHtml(string html)
            {
                if (string.IsNullOrWhiteSpace(html))
                    return html;

                var document = new HtmlDocument();
                document.LoadHtml(html);

                var anchors = document.DocumentNode
                    .SelectNodes("//a[@href]");

                if (anchors is null)
                    return html;

                foreach (var anchor in anchors)
                {
                    var href = anchor.GetAttributeValue("href", string.Empty);
                    var unwrapped = Unwrap(href);

                    if (unwrapped != href)
                        anchor.SetAttributeValue("href", unwrapped);
                }

                return document.DocumentNode.OuterHtml;
            }
        }
    }
}

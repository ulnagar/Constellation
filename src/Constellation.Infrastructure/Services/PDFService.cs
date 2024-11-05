using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.DependencyInjection;
using SelectPdf;
using System.IO;
using System.Net.Mail;

namespace Constellation.Infrastructure.Services
{
    using System.Drawing;

    // Reviewed for ASYNC Operations
    public class PDFService : IPDFService, IScopedService
    {
        public Attachment PageToPdfAttachment(string url, string filename)
        {
            var converter = new HtmlToPdf();
            converter.Options.PdfPageSize = PdfPageSize.A4;
            converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;

            var doc = converter.ConvertUrl(url);
            var pdfStream = new MemoryStream();
            doc.Save(pdfStream);
            pdfStream.Position = 0;
            var attachment = new Attachment(pdfStream, filename);
            doc.Close();

            return attachment;
        }

        public Attachment StringToPdfAttachment(string input, string filename)
        {
            HtmlToPdf converter = new() { 
                Options =
                {
                    PdfPageSize = PdfPageSize.A4, 
                    MarginTop = 30, 
                    MarginLeft = 20, 
                    MarginRight = 20,
                    MarginBottom = 20
                }
            };

            converter.Options.DisplayFooter = true;
            converter.Footer.DisplayOnEvenPages = true;
            converter.Footer.DisplayOnOddPages = true;
            converter.Footer.DisplayOnFirstPage = true;
            converter.Footer.Height = 12;
            PdfTextSection pageNumber = new(500, 0, "Page: {page_number} of {total_pages}", new("Open Sans", 8));
            converter.Footer.Add(pageNumber);
            
            PdfDocument doc = converter.ConvertHtmlString(input);
            MemoryStream pdfStream = new();

            doc.Save(pdfStream);
            pdfStream.Position = 0;
            
            Attachment attachment = new(pdfStream, filename);
            doc.Close();

            return attachment;
        }

        public Attachment StringToPdfAttachment(string input, string header, string filename)
        {
            var converter = new HtmlToPdf();

            if (!string.IsNullOrEmpty(header))
            {
                converter.Options.DisplayHeader = true;
                converter.Header.DisplayOnFirstPage = true;
                converter.Header.DisplayOnEvenPages = true;
                converter.Header.DisplayOnOddPages = true;
                converter.Header.Height = 100;
                var headerSection = new PdfHtmlSection(header, "");
                headerSection.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerSection);
            }

            var doc = converter.ConvertHtmlString(input);
            var pdfStream = new MemoryStream();
            doc.Save(pdfStream);
            pdfStream.Position = 0;
            var attachment = new Attachment(pdfStream, filename);
            doc.Close();

            return attachment;
        }

        public MemoryStream StringToPdfStream(string input, string header)
        {
            var converter = new HtmlToPdf();

            if (!string.IsNullOrEmpty(header))
            {
                converter.Options.DisplayHeader = true;
                converter.Header.DisplayOnFirstPage = true;
                converter.Header.DisplayOnEvenPages = true;
                converter.Header.DisplayOnOddPages = true;
                converter.Header.Height = 110;
                var headerSection = new PdfHtmlSection(header, "");
                headerSection.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;
                converter.Header.Add(headerSection);
            }

            var doc = converter.ConvertHtmlString(input);
            var pdfStream = new MemoryStream();
            doc.Save(pdfStream);
            pdfStream.Position = 0;
            doc.Close();

            return pdfStream;
        }
    }
}

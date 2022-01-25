using Constellation.Application.Interfaces.Services;
using Constellation.Infrastructure.DependencyInjection;
using SelectPdf;
using System.IO;
using System.Net.Mail;

namespace Constellation.Infrastructure.Services
{
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
            var converter = new HtmlToPdf();
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
                converter.Header.Height = 100;
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

namespace Constellation.Infrastructure.Services;

using Constellation.Application.Interfaces.Services;
using SelectPdf;
using System.IO;
using System.Net.Mail;

public class PDFService : IPDFService
{
    public Attachment PageToPdfAttachment(string url, string filename)
    {
        HtmlToPdf converter = new()
        {
            Options =
            {
                PdfPageSize = PdfPageSize.A4, 
                PdfPageOrientation = PdfPageOrientation.Portrait
            }
        };

        PdfDocument doc = converter.ConvertUrl(url);
        MemoryStream pdfStream = new();
        doc.Save(pdfStream);
        pdfStream.Position = 0;
        Attachment attachment = new(pdfStream, filename);
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
                MarginBottom = 20,
                DisplayFooter = true
            },
            Footer =
            {
                DisplayOnEvenPages = true, 
                DisplayOnOddPages = true, 
                DisplayOnFirstPage = true, 
                Height = 12
            }
        };

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

    public Attachment StringToPdfAttachment(string input, string header, int headerHeight, string filename)
    {
        HtmlToPdf converter = new();

        if (!string.IsNullOrEmpty(header))
        {
            converter.Options.DisplayHeader = true;
            converter.Header.DisplayOnFirstPage = true;
            converter.Header.DisplayOnEvenPages = true;
            converter.Header.DisplayOnOddPages = true;
            converter.Header.Height = headerHeight;
            PdfHtmlSection headerSection = new(header, "")
            {
                AutoFitHeight = HtmlToPdfPageFitMode.AutoFit
            };
            converter.Header.Add(headerSection);
        }

        PdfDocument doc = converter.ConvertHtmlString(input);
        MemoryStream pdfStream = new();
        doc.Save(pdfStream);
        pdfStream.Position = 0;
        Attachment attachment = new(pdfStream, filename);
        doc.Close();

        return attachment;
    }

    public MemoryStream StringToPdfStream(string input, string header, int headerHeight)
    {
        HtmlToPdf converter = new();

        if (!string.IsNullOrEmpty(header))
        {
            converter.Options.DisplayHeader = true;
            converter.Header.DisplayOnFirstPage = true;
            converter.Header.DisplayOnEvenPages = true;
            converter.Header.DisplayOnOddPages = true;
            converter.Header.Height = headerHeight;
            PdfHtmlSection headerSection = new(header, "")
            {
                AutoFitHeight = HtmlToPdfPageFitMode.AutoFit
            };
            converter.Header.Add(headerSection);
        }

        PdfDocument doc = converter.ConvertHtmlString(input);
        MemoryStream pdfStream = new();
        doc.Save(pdfStream);
        pdfStream.Position = 0;
        doc.Close();

        return pdfStream;
    }
}
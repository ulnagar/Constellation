namespace Constellation.Application.Interfaces.Services;

using System.IO;
using System.Net.Mail;

public interface IPDFService
{
    Attachment PageToPdfAttachment(string url, string filename);
    Attachment StringToPdfAttachment(string input, string filename);
    Attachment StringToPdfAttachment(string input, string header, int headerHeight, string filename);
    MemoryStream StringToPdfStream(string input, string header, int headerHeight);
}
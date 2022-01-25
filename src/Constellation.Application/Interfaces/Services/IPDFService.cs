using System.IO;
using System.Net.Mail;

namespace Constellation.Application.Interfaces.Services
{
    public interface IPDFService
    {
        Attachment PageToPdfAttachment(string url, string filename);
        Attachment StringToPdfAttachment(string input, string filename);
        MemoryStream StringToPdfStream(string input, string header);
    }
}

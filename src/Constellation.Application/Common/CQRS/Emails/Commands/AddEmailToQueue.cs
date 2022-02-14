using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models;
using FluentValidation;
using MediatR;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Common.CQRS.Emails.Commands
{
    public class AddEmailToQueue : IRequest<Guid>
    {
        public Dictionary<string, string> ToAddresses { get; set; }
        public Dictionary<string, string> CcAddresses { get; set; }
        public Dictionary<string, string> BccAddresses { get; set; }
        public Tuple<string, string> FromAddress { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public ICollection<Attachment> Attachments { get; set; }
    }

    public class AddEmailToQueueValidator : AbstractValidator<AddEmailToQueue>
    {
        public AddEmailToQueueValidator()
        {
            RuleFor(request => request.ToAddresses).NotEmpty();
            RuleFor(request => request.Subject).NotEmpty();
            RuleFor(request => request.Body).NotEmpty();
        }
    }

    public class AddEmailToQueueHandler : IRequestHandler<AddEmailToQueue, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AddEmailToQueueHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Guid> Handle(AddEmailToQueue request, CancellationToken cancellationToken)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(request.FromAddress.Item1, request.FromAddress.Item2));

            foreach (var recipient in request.ToAddresses)
                message.To.Add(new MailboxAddress(recipient.Key, recipient.Value));

            if (request.CcAddresses != null)
                foreach (var recipient in request.CcAddresses)
                    message.Cc.Add(new MailboxAddress(recipient.Key, recipient.Value));

            if (request.BccAddresses != null)
                foreach (var recipient in request.BccAddresses)
                    message.Bcc.Add(new MailboxAddress(recipient.Key, recipient.Value));

            message.Subject = request.Subject;

            var textPartBody = new TextPart(TextFormat.Html)
            {
                Text = request.Body
            };

            if (request.Attachments != null)
            {
                var multipart = new Multipart("mixed");
                multipart.Add(textPartBody);

                foreach (var item in request.Attachments)
                {
                    var attachment = new MimePart
                    {
                        Content = new MimeContent(item.ContentStream, ContentEncoding.Default),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Default,
                        FileName = item.Name
                    };

                    multipart.Add(attachment);
                }

                message.Body = multipart;
            }
            else
            {
                message.Body = textPartBody;
            }

            var id = Guid.NewGuid();
            Directory.CreateDirectory("emails");
            var filePath = $"emails/{id}.eml";
            await message.WriteToAsync(new FileStream(filePath, FileMode.Create), cancellationToken);

            var entry = new EmailToProcess
            {
                Id = id,
                FilePath = filePath,
                Status = EmailToProcess.EmailStatus.Ready
            };

            _unitOfWork.Add(entry);
            await _unitOfWork.CompleteAsync();

            return id;
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using Newtonsoft.Json;
using System.Net;
using PersonalStaticApp.Client.Models;

namespace API
{
    public class ContactFunction
    {
        private readonly ILogger<ContactFunction> _logger;

        public ContactFunction(ILogger<ContactFunction> logger)
        {
            _logger = logger;
        }

        [Function("Post")]
        public async Task<IActionResult> RunPost([HttpTrigger(AuthorizationLevel.Function, "post", Route = "contact")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var emailRequest = JsonConvert.DeserializeObject<EmailRequest>(requestBody);

            if (emailRequest == null)
            {
                return new BadRequestObjectResult("Please pass a valid email request in the request body.");
            }

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(emailRequest.Email));
            email.To.Add(MailboxAddress.Parse("gwcgreen3@gmail.com"));
            email.To.Add(MailboxAddress.Parse("georgecarrwork@gmail.com"));
            email.Subject = $"Contact from {emailRequest.Name} via TheLemic.co.uk";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = emailRequest.Body
            };

            using (var smtp = new SmtpClient())
            {
                smtp.Connect("", 587, MailKit.Security.SecureSocketOptions.StartTls);
                smtp.Authenticate("", "");
                smtp.Send(email);
                smtp.Disconnect(true);
            }

            return new OkObjectResult("Message sent successfully");
        }

        [Function("FunctionHttpResponseData")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "responsedata")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                Name = "Azure Function",
                CurrentTime = DateTime.Now
            });

            return response;
        }
    }
}

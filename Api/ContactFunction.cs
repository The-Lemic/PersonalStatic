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

namespace Api
{
    public class ContactFunction
    {
        private readonly ILogger<ContactFunction> _logger;

        public ContactFunction(ILogger<ContactFunction> logger)
        {
            _logger = logger;
        }

        [Function("Post")]
        public async Task<IActionResult> RunPost([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "contact")] HttpRequest req)
        {
            try
            {
                _logger.LogInformation("Contact form submission received");

                // Read and parse request body
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Request body: {RequestBody}", requestBody);

                var emailRequest = JsonConvert.DeserializeObject<EmailRequest>(requestBody);

                if (emailRequest == null)
                {
                    _logger.LogWarning("Email request is null");
                    return new BadRequestObjectResult("Please pass a valid email request in the request body.");
                }

                // Validate environment variables
                var smtpHost = Environment.GetEnvironmentVariable("SMTPHost");
                var smtpUsername = Environment.GetEnvironmentVariable("SMTPUsername");
                var smtpPassword = Environment.GetEnvironmentVariable("SMTPPassword");

                if (string.IsNullOrEmpty(smtpHost))
                {
                    _logger.LogError("SMTPHost environment variable is not set");
                    return new ObjectResult("SMTP configuration error: SMTPHost not configured") { StatusCode = 500 };
                }

                if (string.IsNullOrEmpty(smtpUsername))
                {
                    _logger.LogError("SMTPUsername environment variable is not set");
                    return new ObjectResult("SMTP configuration error: SMTPUsername not configured") { StatusCode = 500 };
                }

                if (string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogError("SMTPPassword environment variable is not set");
                    return new ObjectResult("SMTP configuration error: SMTPPassword not configured") { StatusCode = 500 };
                }

                _logger.LogInformation("SMTP Config - Host: {SmtpHost}, Username: {SmtpUsername}", smtpHost, smtpUsername);

                // Create email message
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(emailRequest.Email));
                email.To.Add(MailboxAddress.Parse("gwcgreen3@gmail.com"));
                email.To.Add(MailboxAddress.Parse("georgecarrwork@gmail.com"));
                email.Subject = $"Contact from {emailRequest.Name} via TheLemic.co.uk";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = emailRequest.Body
                };

                _logger.LogInformation("Email message created, attempting to send...");

                // Send email
                using (var smtp = new SmtpClient())
                {
                    try
                    {
                        _logger.LogInformation("Connecting to SMTP server: {SmtpHost}:587", smtpHost);
                        await smtp.ConnectAsync(smtpHost, 587, MailKit.Security.SecureSocketOptions.StartTls);
                        _logger.LogInformation("Connected to SMTP server");

                        _logger.LogInformation("Authenticating...");
                        await smtp.AuthenticateAsync(smtpUsername, smtpPassword);
                        _logger.LogInformation("Authentication successful");

                        _logger.LogInformation("Sending email...");
                        await smtp.SendAsync(email);
                        _logger.LogInformation("Email sent successfully");

                        await smtp.DisconnectAsync(true);
                        _logger.LogInformation("Disconnected from SMTP server");
                    }
                    catch (Exception smtpEx)
                    {
                        _logger.LogError("SMTP error: {ExceptionType} - {Message}", smtpEx.GetType().Name, smtpEx.Message);
                        _logger.LogError("Stack trace: {StackTrace}", smtpEx.StackTrace);
                        return new ObjectResult($"Failed to send email: {smtpEx.Message}") { StatusCode = 500 };
                    }
                }

                _logger.LogInformation("Contact form processed successfully");
                return new OkObjectResult("Message sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error in ContactFunction: {ExceptionType} - {Message}", ex.GetType().Name, ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                return new ObjectResult($"Server error: {ex.Message}") { StatusCode = 500 };
            }
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

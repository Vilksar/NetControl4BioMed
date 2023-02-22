using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Implements an e-mail sender using MailKit.
    /// </summary>
    public class MailKitEmailSender : IEmailSender
    {
        /// <summary>
        /// Represents the web host environment.
        /// </summary>
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Represents the configuration.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Represents the partial view renderer.
        /// </summary>
        private readonly IPartialViewRenderer _renderer;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="configuration">Represents the application configuration options.</param>
        /// <param name="renderer">Represents the partial view renderer.</param>
        public MailKitEmailSender(IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IPartialViewRenderer renderer)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _renderer = renderer;
        }

        /// <summary>
        /// Sends an e-mail confirmation e-mail to the specified user.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendEmailConfirmationEmailAsync(EmailEmailConfirmationViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.Email));
            email.Subject = "NetControl4BioMed - Confirm your e-mail address";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailEmailConfirmationPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with instructions on changing the user e-mail.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendEmailChangeEmailAsync(EmailEmailChangeViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.NewEmail));
            email.Subject = "NetControl4BioMed - Change your e-mail address";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailEmailChangePartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with instructions on resetting the user password.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendPasswordResetEmailAsync(EmailPasswordResetViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.Email));
            email.Subject = "NetControl4BioMed - Reset your password";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailPasswordResetPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with a notification that the user e-mail has changed.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendEmailChangedEmailAsync(EmailEmailChangedViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.OldEmail));
            email.Subject = "NetControl4BioMed - Your e-mail address has been changed";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailEmailChangedPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with a notification that the user password has changed.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendPasswordChangedEmailAsync(EmailPasswordChangedViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.Email));
            email.Subject = "NetControl4BioMed - Your password has been changed";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailPasswordChangedPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends a contact e-mail to the administrator users specified in the configuration file.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendContactEmailAsync(EmailContactViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(_configuration.GetSection("Administrator:Email").Value));
            email.Subject = "NetControl4BioMed - You have received a new message";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailContactPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with a notification that the user has given access to a generic network.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendAddedToNetworkEmailAsync(EmailAddedToNetworkViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.Email));
            email.Subject = "NetControl4BioMed - You shared a network";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailAddedToNetworkPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with a notification that the user has been given access to a generic network.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendWasAddedToNetworkEmailAsync(EmailWasAddedToNetworkViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.Email));
            email.Subject = "NetControl4BioMed - Someone shared a network with you";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailWasAddedToNetworkPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with a notification that the user has given access to a generic analysis.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendAddedToAnalysisEmailAsync(EmailAddedToAnalysisViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.Email));
            email.Subject = "NetControl4BioMed - You shared an analysis";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailAddedToAnalysisPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with a notification that the user was given access to a generic analysis.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendWasAddedToAnalysisEmailAsync(EmailWasAddedToAnalysisViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.Email));
            email.Subject = "NetControl4BioMed - Someone shared an analysis with you";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailWasAddedToAnalysisPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with a notification that a network has ended.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendNetworkEndedEmailAsync(EmailNetworkEndedViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.Email));
            email.Subject = "NetControl4BioMed - A network has ended";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailNetworkEndedPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with a notification that an analysis has ended.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendAnalysisEndedEmailAsync(EmailAnalysisEndedViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.Email));
            email.Subject = "NetControl4BioMed - An analysis has ended";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailAnalysisEndedPartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends an e-mail with an alert that one or more networks or analyses will be deleted.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendAlertDeleteEmailAsync(EmailAlertDeleteViewModel viewModel)
        {
            // Check the web host environment.
            if (_webHostEnvironment.EnvironmentName != "Production")
            {
                // Return.
                return;
            }
            // Define the e-mail.
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("EmailSender:Email").Value));
            email.To.Add(MailboxAddress.Parse(viewModel.Email));
            email.Subject = "NetControl4BioMed - Data will soon be deleted";
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = await _renderer.RenderPartialToStringAsync("_EmailAlertDeletePartial", viewModel)
            };
            // Send the e-mail.
            await SendEmailAsync(email);
        }

        /// <summary>
        /// Sends the provided e-mail
        /// </summary>
        /// <param name="email">Represents the e-mail to send.</param>
        private async Task SendEmailAsync(MimeMessage email)
        {
            // Use a new SMTP client.
            using var smtpClient = new SmtpClient();
            await smtpClient.ConnectAsync(_configuration.GetSection("Authentication:Smtp:Address").Value, 587, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(_configuration.GetSection("Authentication:Smtp:UserName").Value, _configuration.GetSection("Authentication:Smtp:Password").Value);
            await smtpClient.SendAsync(email);
            await smtpClient.DisconnectAsync(true);
        }
    }
}

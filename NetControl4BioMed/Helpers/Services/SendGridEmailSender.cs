using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Implements an e-mail sender using SendGrid.
    /// </summary>
    public class SendGridEmailSender : ISendGridEmailSender
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
        public SendGridEmailSender(IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IPartialViewRenderer renderer)
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - Confirm your e-mail address";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailEmailConfirmationPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail containing the URL.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.OldEmail, viewModel.OldEmail);
            var subject = "NetControl4BioMed - Change your e-mail address";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailEmailChangePartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail containing the URL.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - Reset your password";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailPasswordResetPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail containing the URL.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.OldEmail, viewModel.OldEmail);
            var subject = "NetControl4BioMed - Your e-mail address has been changed";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailEmailChangedPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail containing the URL.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - Your password has been changed";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailPasswordChangedPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail containing the URL.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(_configuration.GetSection("Administrator:Email").Value, _configuration.GetSection("Administrator:Email").Value);
            var subject = "NetControl4BioMed - You have received a new message";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailContactPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail containing the message.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - You shared a network";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailAddedToNetworkPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - Someone shared a network with you";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailWasAddedToNetworkPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - You shared an analysis";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailAddedToAnalysisPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - Someone shared an analysis with you";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailWasAddedToAnalysisPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - A network has ended";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailNetworkEndedPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - An analysis has ended";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailAnalysisEndedPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail.
            await client.SendEmailAsync(msg);
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - Data will soon be deleted";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailAlertDeletePartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail.
            await client.SendEmailAsync(msg);
        }
    }
}

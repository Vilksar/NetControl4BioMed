using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.Interfaces;
using NetControl4BioMed.Helpers.ViewModels;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Services
{
    /// <summary>
    /// Implements an e-mail sender using SendGrid.
    /// </summary>
    public class SendGridEmailSender : ISendGridEmailSender
    {
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
        public SendGridEmailSender(IConfiguration configuration, IPartialViewRenderer renderer)
        {
            _configuration = configuration;
            _renderer = renderer;
        }

        /// <summary>
        /// Sends an e-mail confirmation e-mail to the specified user.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendEmailConfirmationEmailAsync(EmailEmailConfirmationViewModel viewModel)
        {
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - Confirm your e-mail";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailEmailConfirmationPartial", viewModel);
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
        /// Sends an e-mail with a notification that the user e-mail has changed.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendEmailChangedEmailAsync(EmailEmailChangedViewModel viewModel)
        {
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.OldEmail, viewModel.OldEmail);
            var subject = "NetControl4BioMed - Your e-mail has been changed";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailEmailChangedPartial", viewModel);
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
        /// Sends a contact e-mail to the administrator users specified in the configuration file.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendContactEmailAsync(EmailContactViewModel viewModel)
        {
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var tos = _configuration.GetSection("Administrators").GetChildren().Select(item => new EmailAddress(item.GetSection("Email").Value, item.GetSection("Email").Value));
            var subject = "Message from NetControl4BioMed";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailContactPartial", viewModel);
            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos.ToList(), subject, string.Empty, htmlContent);
            // Send the e-mail containing the message.
            await client.SendEmailAsync(msg);
        }

        /// <summary>
        /// Sends an e-mail with a notification that the user has given access to a generic network.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendAddedToNetworkEmailAsync(EmailAddedToNetworkViewModel viewModel)
        {
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - A network was shared with you";
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
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - An analysis was shared with you";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailWasAddedToAnalysisPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail.
            await client.SendEmailAsync(msg);
        }

        /// <summary>
        /// Sends an e-mail with a notification that a generic analysis has ended.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendAnalysisEndedEmailAsync(EmailAnalysisEndedViewModel viewModel)
        {
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - Analysis ended";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailAnalysisEndedPartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail.
            await client.SendEmailAsync(msg);
        }

        /// <summary>
        /// Sends an e-mail with an alert that one or more analyses will be deleted.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        public async Task SendAlertDeleteEmailAsync(EmailAlertDeleteViewModel viewModel)
        {
            // Define the variables for the e-mail.
            var apiKey = _configuration.GetSection("Authentication:SendGrid:AppKey").Value;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration.GetSection("EmailSender:Email").Value, _configuration.GetSection("EmailSender:Name").Value);
            var to = new EmailAddress(viewModel.Email, viewModel.Email);
            var subject = "NetControl4BioMed - Analyses to be deleted";
            var htmlContent = await _renderer.RenderPartialToStringAsync("_EmailAlertDeletePartial", viewModel);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            // Send the e-mail.
            await client.SendEmailAsync(msg);
        }
    }
}

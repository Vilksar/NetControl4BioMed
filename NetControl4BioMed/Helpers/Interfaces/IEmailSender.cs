﻿using NetControl4BioMed.Helpers.ViewModels;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the e-mail sender.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an e-mail with instructions on confirming the user e-mail address.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendEmailConfirmationEmailAsync(EmailEmailConfirmationViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with instructions on changing the user e-mail.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendEmailChangeEmailAsync(EmailEmailChangeViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with instructions on resetting the user password.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendPasswordResetEmailAsync(EmailPasswordResetViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user e-mail has changed.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendEmailChangedEmailAsync(EmailEmailChangedViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user password has changed.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendPasswordChangedEmailAsync(EmailPasswordChangedViewModel viewModel);

        /// <summary>
        /// Sends a contact e-mail to the administrator users specified in the configuration file.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendContactEmailAsync(EmailContactViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user has given access to a network.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendAddedToNetworkEmailAsync(EmailAddedToNetworkViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user has been given access to a network.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendWasAddedToNetworkEmailAsync(EmailWasAddedToNetworkViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user has given access to an analysis.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendAddedToAnalysisEmailAsync(EmailAddedToAnalysisViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user was given access to an analysis.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendWasAddedToAnalysisEmailAsync(EmailWasAddedToAnalysisViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that a network has ended.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendNetworkEndedEmailAsync(EmailNetworkEndedViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that an analysis has ended.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendAnalysisEndedEmailAsync(EmailAnalysisEndedViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with an alert that one or more analyses will be deleted.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendAlertDeleteEmailAsync(EmailAlertDeleteViewModel viewModel);
    }
}

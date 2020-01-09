using NetControl4BioMed.Data.Models;
using NetControl4BioMed.Helpers.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Helpers.Interfaces
{
    /// <summary>
    /// Provides an abstraction for the SendGrid e-mail sender.
    /// </summary>
    public interface ISendGridEmailSender
    {
        /// <summary>
        /// Sends an e-mail with instructions on confirming the user e-mail address.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendEmailConfirmationEmailAsync(EmailEmailConfirmationViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user password has changed.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendPasswordChangedEmailAsync(EmailPasswordChangedViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user e-mail has changed.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendEmailChangedEmailAsync(EmailEmailChangedViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with instructions on resetting the user password.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendPasswordResetEmailAsync(EmailPasswordResetViewModel viewModel);

        /// <summary>
        /// Sends a contact e-mail to the administrator users specified in the configuration file.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendContactEmailAsync(EmailContactViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user has given access to a generic network.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendAddedToNetworkEmailAsync(EmailAddedToNetworkViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user has been given access to a generic network.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendWasAddedToNetworkEmailAsync(EmailWasAddedToNetworkViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user has given access to a generic analysis.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendAddedToAnalysisEmailAsync(EmailAddedToAnalysisViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that the user was given access to a generic analysis.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendWasAddedToAnalysisEmailAsync(EmailWasAddedToAnalysisViewModel viewModel);

        /// <summary>
        /// Sends an e-mail with a notification that a generic analysis has ended.
        /// </summary>
        /// <param name="viewModel">Represents the view model of the e-mail.</param>
        Task SendAnalysisEndedEmailAsync(EmailAnalysisEndedViewModel viewModel);
    }
}

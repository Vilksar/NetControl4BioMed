// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Wait for the window to load.
$(window).on('load', () => {

    // Check if there is a cookie notification alert on the page.
    if ($('.cookie-consent-alert').length !== 0) {
        // Get the cookie acceptance button.
        const button = $('.cookie-consent-alert').first().find('.cookie-consent-alert-dismiss');
        // Add a listener for clicking the button.
        $(button).on('click', () => {
            document.cookie = button.data('cookie-consent-string');
        });
    }

    // Check if there is a UTC date on the page.
    if ($('.utc-date').length !== 0) {
        // Define a function to update an element containing a UTC date.
        const updateDate = (element) => {
            // Get the date in UTC format.
            const date = new Date($(element).data('date'));
            // Update the UTC date to the local date.
            $(element).find('.utc-date-date').attr('title', date.toLocaleDateString());
            $(element).find('.utc-date-date').text(date.toLocaleDateString());
            $(element).find('.utc-date-time').attr('title', date.toLocaleTimeString());
            $(element).find('.utc-date-time').text(date.toLocaleTimeString());
        };
        // Execute the function on page load.
        (() => {
            // Update all elements containing UTC dates.
            $('.utc-date').each((index, element) => updateDate(element));
        })();
    }

});

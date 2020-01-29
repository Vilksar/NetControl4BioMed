// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.
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

    // Check if there is a QR code on the page.
    if ($('.qr-code').length !== 0) {
        // Load the QR generation script.
        $.getScript('/lib/jquery-qrcode/jquery.qrcode.min.js', () => {
            // Go over all of the QR codes on the page.
            $('.qr-code').each((index, element) => {
                // And generate the QR code based on the given uri.
                $(element).qrcode($(element).data('uri'));
            });
        });
    }

});

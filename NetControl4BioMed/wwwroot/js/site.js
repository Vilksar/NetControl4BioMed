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

    // Check if there is a datatable on the page.
    if ($('.table-datatable').length !== 0) {
        // Format the table as datatable.
        //$('.table-datatable').DataTable();
        $('.table-datatable').each((index, element) => {
            // Format the table as datatable.
            const table = $(element).DataTable();
            // Get the index of the index column.
            const columnIndex = table.column('index:name').index();
            // Check if there is any index column.
            if (typeof (columnIndex) !== 'undefined') {
                // Add a listener for ordering or searching in the table.
                table.on('order.dt search.dt', () => {
                    // Update the corresponding column on searching or ordering.
                    table.column(columnIndex, { search: 'applied', order: 'applied' }).nodes().each((cell, index) => {
                        $(cell).find('span').html(index + 1);
                    });
                }).draw();
            }
        });
    }

    // Check if there is a list group of items on the page.
    if ($('.item-group').length !== 0) {
        // Define a function which gets all of the selected items and creates a JSON string array with their IDs.
        const updateSelectedItems = (groupElement) => {
            // Get all of the list group items.
            const items = $(groupElement).find('.item-group-item');
            // Remove the active class from all list items.
            $(items).removeClass('table-active');
            // Go over all of the checked elements and get the corresponding list group items.
            const selectedItems = $(groupElement).find('input[type="checkbox"]:checked').closest('.item-group-item');
            // Go over each of the selected items and mark them as active.
            $(selectedItems).addClass('table-active');
            // Check how many elements are selected.
            if (selectedItems.length === 0) {
                // Disable the group buttons.
                $('.item-group-button').prop('disabled', true);
                // Unmark the checkbox as indeterminate.
                $(groupElement).find('.item-group-select').prop('indeterminate', false);
                // Uncheck the checkbox.
                $(groupElement).find('.item-group-select').prop('checked', false);
            } else {
                // Enable the group buttons.
                $('.item-group-button').prop('disabled', false);
                // Check if not all elements are selected.
                if (selectedItems.length !== items.length) {
                    // Mark the checkbox as indeterminate.
                    $(groupElement).find('.item-group-select').prop('indeterminate', true);
                } else {
                    // Unmark the checkbox as indeterminate.
                    $(groupElement).find('.item-group-select').prop('indeterminate', false);
                    // Check the checkbox.
                    $(groupElement).find('.item-group-select').prop('checked', true);
                }
            }
        };
        // Add a listener for when a checkbox gets checked or unchecked.
        $('.item-group').on('change', 'input[type="checkbox"]', (event) => {
            // Get the current list group.
            const groupElement = $(event.target).closest('.item-group');
            // Update the selected items.
            updateSelectedItems(groupElement);
        });
        // Add a listener for the select checkbox.
        $('.item-group-select').on('change', (event) => {
            // Get the current list group.
            const groupElement = $(event.target).closest('.item-group');
            // Check if the checkbox is currently checked.
            if ($(event.target).prop('checked')) {
                // Check all of the checkboxes on the page.
                $(groupElement).find('input[type="checkbox"]:not(:checked)').prop('checked', true);
            } else {
                // Uncheck all of the checkboxes on the page.
                $(groupElement).find('input[type="checkbox"]:checked').prop('checked', false);
            }
            // Update the selected items.
            updateSelectedItems(groupElement);
        });
        // On page load, parse the input and check the group items.
        (() => {
            // Go over all of the groups.
            $('.item-group').each((index, element) => {
                // Update the selected items.
                updateSelectedItems(element);
            });
        })();
    }

});

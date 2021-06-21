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
        // Go over each datatable.
        $('.table-datatable').each((index, element) => {
            // Format the table as datatable.
            const table = $(element).DataTable({
                'autoWidth': false
            });
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

    // Check if there is an item group on the page.
    if ($('.item-group').length !== 0) {
        // Define a function which gets all of the selected items and creates a JSON string array with their IDs.
        const updateSelectedItems = (groupElement) => {
            // Get all of the list group items.
            const items = $(groupElement).find('.item-group-item');
            // Go over all of the checked elements and get the corresponding list group items.
            const selectedItems = $(groupElement).find('.item-group-item-checkbox:checked').closest('.item-group-item');
            // Go over all of the unchecked elements and get the corresponding list group items.
            const unselectedItems = $(groupElement).find('.item-group-item-checkbox:not(:checked)').closest('.item-group-item');
            // Go over all of the selected items and check all of the checkboxes.
            $(selectedItems).find('input[type="checkbox"]:not(:checked)').prop('checked', true);
            // Go over all of the unselected items and uncheck all of the checkboxes.
            $(unselectedItems).find('input[type="checkbox"]:checked').prop('checked', false);
            // Go over all of the selected items and mark them as active.
            $(selectedItems).addClass('table-active');
            // Go over all of the unselected items and mark them as not active.
            $(unselectedItems).removeClass('table-active');
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
        $('.item-group').on('change', '.item-group-item-checkbox', (event) => {
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
                $(groupElement).find('.item-group-item-checkbox').prop('checked', true);
            } else {
                // Uncheck all of the checkboxes on the page.
                $(groupElement).find('.item-group-item-checkbox').prop('checked', false);
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

    // Check if there is a Cytoscape area on the page.
    if ($('.cytoscape-area').length !== 0) {
        // Get the Cytoscape configuration JSON.
        const cytoscapeJson = JSON.parse($('.cytoscape-configuration').first().text());
        // Define the Cytoscape variable.
        const cy = cytoscape({
            container: $('.cytoscape-container').first().get(0),
            elements: cytoscapeJson.elements,
            layout: cytoscapeJson.layout,
            style: cytoscapeJson.style
        });
        // Add listener for when a node is clicked.
        cy.on('tap', 'node', (event) => {
            // Check if there is a link.
            if (event.target.data('href') && event.target.data('href').length !== 0) {
                // Open a new link.
                window.location.href = event.target.data('href');
            }
        });
        // Hide the loading message.
        $('.cytoscape-loading').prop('hidden', true);
    }

    // Check if there is a refreshable item on the page.
    if ($('.item-refresh').length !== 0) {
        // Define a function to refresh the details.
        const refresh = (element) => {
            // Get the ID of the item.
            const id = $(element).data('id');
            // Get the status of the item.
            const status = $(element).data('status');
            // Get the data for the item with the provided ID.
            const ajaxCall = $.ajax({
                url: `${window.location.pathname}?handler=Refresh&id=${id}`,
                dataType: 'json',
                success: (data) => {
                    // Check if the status has changed.
                    if (status !== data.status) {
                        // Reload the page.
                        location.reload(true);
                    }
                    // Go over each JSON property.
                    $.each(data, (key, value) => {
                        // Update the corresponding fields.
                        $(element).find(`.item-refresh-item[data-type=${key}]`).attr('title', value);
                        $(element).find(`.item-refresh-item[data-type=${key}]`).text(value);
                    });
                },
                error: () => { }
            });
        };
        // Execute the function on page load.
        (() => {
            // Refresh all items once.
            $('.item-refresh').each((index, element) => refresh(element));
            // Check if the items need to be refreshed.
            if ($('.item-refresh[data-refresh="True"]').length !== 0) {
                // Repeat the function every few seconds.
                setInterval(() => {
                    // Go over all elements in the page.
                    $('.item-refresh[data-refresh="True"]').each((index, element) => refresh(element));
                }, _refreshInterval);
            }
        })();
    }

});

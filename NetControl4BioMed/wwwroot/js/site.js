// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Wait for the window to load.
$(window).on('load', () => {

    // Define the time interval in which refreshing takes place, in miliseconds.
    const _refreshInterval = 5000;

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
        // Go over each datatable.
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
            // Get the enclosing form.
            const form = $(element).closest('form');
            // Check if there is an enclosing form.
            if (form.length !== 0) {
                // Add a listener for the form submission.
                $(form).on('submit', (event) => {
                    // Iterate over all the checkboxes in the table that are checked, but don't exist in DOM.
                    table.$('input[type="checkbox"]').filter((index, element) => !$.contains(document, element)).filter((index, element) => element.checked).each((index, element) => {
                        // Create a hidden element and append it to the form.
                        $(form).append($('<input>').attr('type', 'hidden').attr('id', $(element).attr('id')).attr('name', $(element).attr('name')).val($(element).val()))
                    });
                });
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

    // Check if there is a file group on the page.
    if ($('.form-file-group').length !== 0 || $('.file-group').length !== 0 || $('.heuristics-group').length !== 0) {
        // Define the HTML of an optgroup.
        const heuristicGroupOptgroupHTML = `<optgroup></optgroup>`;
        // Define a function which shows the alert of the specified type, with the specified text.
        const formFileGroupShowAlert = (groupElement, alertType, alertIcon, alertText) => {
            // Get the alert.
            const alertElement = $(groupElement).find('.form-file-group-alert').first();
            // Toggle the corresponding classes.
            $(alertElement).removeClass((index, classes) => (classes.match(/(^|\s)alert-\S+/g) || []).join(' ')).addClass(`alert-${alertType}`);
            $(alertElement).find('.form-file-group-alert-icon').html('').append($('<i>').addClass(`fas fa-${alertIcon}`));
            $(alertElement).find(`.form-file-group-alert-text`).text(alertText);
            // Show the alert.
            $(alertElement).removeAttr('hidden');
        };
        // Define a function which parses the data and fills in the form.
        const formFileGroupUpdateInputs = (groupElement) => {
            // Define a variable for the input data.
            let data = undefined;
            // Try to parse the input data.
            try {
                // Get the input data.
                data = JSON.parse($(groupElement).find('.form-file-group-text').first().val());
            }
            catch (error) {
                // Show an error.
                formFileGroupShowAlert(groupElement, 'danger', 'exclamation-circle', `The error \"${error}\" occurred while reading the content of the file. Please make sure that the file has the correct format.`);
                // Return from the function.
                return;
            }
            // Get the type of the form file group.
            const type = $(groupElement).data('type');
            // Check if the provided type doesn't match the data.
            if (!data.hasOwnProperty('Type') || typeof(data['Type']) !== 'string' || data['Type'].toLowerCase() !== type) {
                // Show an error.
                formFileGroupShowAlert(groupElement, 'danger', 'exclamation-circle', `The file does not have the required type \"${data['Type']}\". Please make sure that the right file was selected, and that the file has the correct format.`);
                // Return from the function.
                return;
            }
            // Get the database type of the form file group.
            const databaseType = $(groupElement).data('database-type');
            // Check if the provided database type doesn't match the data.
            if (!data.hasOwnProperty('DatabaseType') || typeof (data['DatabaseType']) !== 'string' || data['DatabaseType'].toLowerCase() !== databaseType) {
                // Show an error.
                formFileGroupShowAlert(groupElement, 'danger', 'exclamation-circle', `The file does not have the required database type \"${data['DatabaseType']}\". Please make sure that the right file was selected, and that the file has the correct format.`);
                // Return from the function.
                return;
            }
            // Get the form.
            const formElement = $(groupElement).closest('form');
            // Check if we have a network.
            if (type === 'network') {
                // Define the required fields.
                const requiredFields = ['Name', 'Description', 'IsPublic', 'Algorithm', 'NodeDatabaseData', 'EdgeDatabaseData', 'SeedNodeData', 'SeedNodeCollectionData', 'SeedEdgeData'];
                // Get the fields that do not appear in the data.
                const missingFields = requiredFields.filter(item => !Object.keys(data).Contains(item));
                // Check if there are any missing fields.
                if (missingFields.length !== 0) {
                    // Show an error.
                    formFileGroupShowAlert(groupElement, 'danger', 'exclamation-circle', `The file is not in a valid format. The following required fields are missing: \"${missingFields.join('\", \"')}\".`);
                    // Return from the function.
                    return;
                }
                // Go over each required field.
                for (let field of requiredFields) {
                    // Get the value of the data.
                    const value = typeof (data[field]) !== 'object' ? data[field].toString() : JSON.stringify(data[field]);
                    // Get the current input.
                    const inputElement = $(formElement).find(`[name="Input.${field}"]`);
                    // Update its value.
                    $(inputElement).val(value);
                    // Try to get its file group.
                    const fileGroup = $(inputElement).closest('.file-group');
                    // Check if the file group exists.
                    if ($(fileGroup).length !== 0) {
                        // Update its text.
                        fileGroupUpdateText(fileGroup);
                        fileGroupUpdateInput(fileGroup);
                    }
                }
            } else if (type === 'analysis') {
                // Get the algorithm of the form file group.
                const algorithm = $(groupElement).data('algorithm');
                // Check if the provided database type doesn't match the data.
                if (!data.hasOwnProperty('Algorithm') || typeof (data['Algorithm']) !== 'string' || data['Algorithm'].toLowerCase() !== algorithm) {
                    // Show an error.
                    formFileGroupShowAlert(groupElement, 'danger', 'exclamation-circle', `The file does not have the required algorithm \"${data['Algorithm']}\". Please make sure that the right file was selected, and that the file has the correct format.`);
                    // Return from the function.
                    return;
                }
                // Define the required fields.
                const requiredFields = ['Name', 'Description', 'IsPublic', 'Algorithm', 'NetworkData', 'SourceNodeData', 'SourceNodeCollectionData', 'TargetNodeData', 'TargetNodeCollectionData', 'MaximumIterations', 'MaximumIterationsWithoutImprovement', 'AlgorithmParameters'];
                // Get the fields that do not appear in the data.
                const missingFields = requiredFields.filter(item => !Object.keys(data).includes(item));
                // Check if there are any missing fields.
                if (missingFields.length !== 0) {
                    // Show an error.
                    formFileGroupShowAlert(groupElement, 'danger', 'exclamation-circle', `The file is not in a valid format. The following required fields are missing: \"${missingFields.join('\", \"')}\".`);
                    // Return from the function.
                    return;
                }
                // Go over each key in the data.
                for (let field of requiredFields.slice(0, -1)) {
                    // Get the value of the data.
                    const value = typeof (data[field]) !== 'object' ? data[field].toString() : JSON.stringify(data[field]);
                    // Get the current input.
                    const inputElement = $(formElement).find(`[name="Input.${field}"]`);
                    // Update its value.
                    $(inputElement).val(value);
                    // Try to get its file group.
                    const fileGroup = $(inputElement).closest('.file-group');
                    // Check if the file group exists.
                    if ($(fileGroup).length !== 0) {
                        // Update its text.
                        fileGroupUpdateText(fileGroup);
                        fileGroupUpdateInput(fileGroup);
                    }
                }
                // Get the algorithm parameters.
                const algorithmParameters = JSON.parse(data['AlgorithmParameters']);
                // Go over each parameter.
                for (let parameterKey of Object.keys(algorithmParameters)) {
                    // Get the value of the data.
                    const value = typeof (algorithmParameters[parameterKey]) !== 'object' ? algorithmParameters[parameterKey].toString() : JSON.stringify(algorithmParameters[parameterKey]);
                    // Get the current input.
                    const inputElement = $(formElement).find(`[name="Input.${data['Algorithm']}AlgorithmParameters.${parameterKey}"]`);
                    // Update its value.
                    $(inputElement).val(value);
                    // Try to get its heuristic group.
                    const heuristicGroup = $(inputElement).closest('.heuristics-group');
                    // Check if the heuristic group exists.
                    if ($(heuristicGroup).length !== 0) {
                        // Update the heuristics.
                        heuristicGroupUpdateHeuristics(heuristicGroup);
                        // Update the current heuristics.
                        heuristicGroupUpdateCurrentHeuristics(heuristicGroup);
                        // Update the input text.
                        heuristicGroupUpdateText(heuristicGroup);
                    }
                }
            }
            // Show a success message.
            formFileGroupShowAlert(groupElement, 'success', 'check-circle', `The file has been read successfully. Please check carefully all of the fields before continuing.`);
        };
        // Define a function which updates the data to be submitted.
        const fileGroupUpdateInput = (groupElement) => {
            // Check if the text is empty.
            if (!$.trim($(groupElement).find('.file-group-text').first().val())) {
                // Update the value of the count.
                $(groupElement).find('.file-group-count').first().text(0);
                // Update the data to be submitted.
                $(groupElement).find('.file-group-input').first().val(JSON.stringify([]));
                // Return from the function.
                return;
            }
            // Get the in-line separator.
            const inlineSeparator = $(groupElement).find('.file-group-in-line-separator').first().val();
            // Get the line separator.
            const lineSeparator = $(groupElement).find('.file-group-line-separator').first().val();
            // Get the type of the file group.
            const type = $(groupElement).data('type');
            // Check if we have simple items.
            if (type === 'items') {
                // Split the text into different lines.
                const rows = $(groupElement).find('.file-group-text').first().val().split(new RegExp(lineSeparator)).filter((element) => {
                    // Select only the non empty elements.
                    return $.trim(element);
                });
                // Update the value of the count.
                $(groupElement).find('.file-group-count').first().text(rows.length);
                // Update the data to be submitted.
                $(groupElement).find('.file-group-input').first().val(JSON.stringify(rows));
            }
            // Check if we have edges.
            else if (type === 'edges') {
                // Split the text into different lines.
                const rows = $(groupElement).find('.file-group-text').first().val().split(new RegExp(lineSeparator)).filter((element) => {
                    // Split the row into its composing items.
                    const row = element.split(new RegExp(inlineSeparator)).filter((el) => {
                        // Select only the non-empty items.
                        return el !== '';
                    });
                    // Select only elements with more than two items.
                    return row.length === 2;
                });
                // Go over each row.
                const items = $.map(rows, (element, index) => {
                    // Split the row into its composing items.
                    const row = element.split(new RegExp(inlineSeparator));
                    // Check if we don't have both source and target nodes.
                    if (!row[0] || !row[1]) {
                        // Don't return anything.
                        return;
                    }
                    // Split the element into an array of items.
                    return { 'SourceNode': row[0], 'TargetNode': row[1] };
                });
                // Update the value of the count.
                $(groupElement).find('.file-group-count').first().text(rows.length);
                // Update the data to be submitted.
                $(groupElement).find('.file-group-input').first().val(JSON.stringify(items));
            }
        };
        // Define a function which updates the displayed text.
        const fileGroupUpdateText = (groupElement) => {
            // Get the type of the file group.
            const type = $(groupElement).data('type');
            // Define a variable for the input data.
            let data = undefined;
            // Try to parse the input data.
            try {
                // Get the input data.
                data = JSON.parse($(groupElement).find('.file-group-input').first().val());
            }
            catch (error) {
                // Return from the function.
                return;
            }
            // Check if there isn't any data.
            if (typeof data === 'undefined') {
                // Return from the function.
                return;
            }
            // Check if we have a proper array.
            if (!Array.isArray(data)) {
                // Return from the function.
                return;
            }
            // Check if we have simple items.
            if (type === 'items') {
                // Go over all of the elements.
                data = data.filter((element) => {
                    // Keep only the ones which are of the proper type.
                    return typeof element === 'string';
                });
                // Add the elements to the text.
                $(groupElement).find('.file-group-text').first().val(data.join('\n'));
            }
            // Check if we have edges.
            if (type === 'edges') {
                // Go over all of the elements.
                data = data.filter((element) => {
                    // Keep only the ones which are of the proper type.
                    return element['SourceNode'] && typeof element['SourceNode'] === 'string' && element['TargetNode'] && typeof element['TargetNode'] === 'string';
                });
                // Go over all of the elements.
                data = $.map(data, (element, index) => {
                    // Check if we have more than one of each.
                    if (!element['SourceNode'] || !element['TargetNode']) {
                        // Return an undefined value.
                        return undefined;
                    } else {
                        // Return a simplified object.
                        return `${element['SourceNode']};${element['TargetNode']}`;
                    }
                });
                // Add the elements to the text.
                $(groupElement).find('.file-group-text').first().val(data.join('\n'));
            }
        };
        // Define a function which updates the heuristics based on the input data.
        const heuristicGroupUpdateHeuristics = (groupElement) => {
            // Define a variable for the input data.
            let data = undefined;
            // Try to parse the input data.
            try {
                // Get the input data.
                data = JSON.parse($(groupElement).find('.heuristics-group-input').first().val());
            }
            catch (error) {
                // Return from the function.
                return;
            }
            // Check if there isn't any data.
            if (typeof data === 'undefined') {
                // Return from the function.
                return;
            }
            // Get the possible heuristics element.
            const possibleHeuristicsElement = $(groupElement).find('.heuristics-group-possible');
            // Get the current heuristics element.
            const currentHeuristicsElement = $(groupElement).find('.heuristics-group-current');
            // Clear the current heuristics element.
            $(currentHeuristicsElement).html('');
            // Go over each optgroup in the input data.
            jQuery.each(data, (index1, item1) => {
                // Add a new optgroup to the current heuristics.
                $(currentHeuristicsElement).append(heuristicGroupOptgroupHTML);
                // Go over each option within the optgroup.
                jQuery.each(item1, (index2, item2) => {
                    // Append a clone of the corresponding possible option element to the current heuristics.
                    $(currentHeuristicsElement).children().last().append($(possibleHeuristicsElement).children(`option[value="${item2}"]`).clone());
                });
            });
        };
        // Define a function which updates the data to be submitted.
        const heuristicGroupUpdateText = (groupElement) => {
            // Parse the current heuristics into a JSON object and add it to the input data.
            $(groupElement).find('.heuristics-group-input').val(JSON.stringify($.map($(groupElement).find('.heuristics-group-current').children(), (element1, index1) => [$.map($(element1).children(), (element2, index2) => $(element2).val())])))
        };
        // Define a function which updates the current heuristics, by updating the group index numbers.
        const heuristicGroupUpdateCurrentHeuristics = (groupElement) => {
            // Remove the empty optgroups.
            $(groupElement).find('.heuristics-group-current').children().filter((index, element) => $(element).children().length === 0).remove();
            // Go over each optgroup in the current heuristics of the group element.
            $(groupElement).find('.heuristics-group-current').children().each((index1, element1) => {
                // Get the new text of the element.
                const text = `Group ${index1 + 1}`;
                // Update the label of the element.
                $(element1).prop('label', text);
                // Update the title of the element.
                $(element1).prop('title', text);
                // Define an array to store the unique values.
                let unique = [];
                // Go over each option in the optgroup.
                $(element1).children().each((index2, element2) => {
                    // Get the value of the option.
                    const value = $(element2).val();
                    // Check if the value already appears in the array.
                    if (unique.includes(value)) {
                        // Remove the element.
                        $(element2).remove();
                    } else {
                        // Add the value to the array.
                        unique.push(value)
                    }
                });
            });
        };
        // Add a listener for if the upload button was clicked.
        $('.form-file-group-file-upload').on('change', (event) => {
            // Get the current file.
            const file = event.target.files[0];
            // Set the filename in the label.
            $(event.target).siblings('.form-file-group-file-label').html(file.name);
            // Define the file reader and the variable for storing its content.
            let fileReader = new FileReader();
            // Define what happens when we read the file.
            fileReader.onload = (e) => {
                // Get the actual group which was clicked.
                const groupElement = $(event.target).closest('.form-file-group');
                // Write the content of the file to the text area.
                $(groupElement).find('.form-file-group-text').first().val(e.target.result);
                // Parse the uploaded text.
                formFileGroupUpdateInputs(groupElement);
            };
            // Read the file.
            fileReader.readAsText(file);
        });
        // Add a listener for typing into the text area.
        $('.file-group-text').on('keyup', (event) => {
            // Get the actual group which was clicked.
            const groupElement = $(event.target).closest('.file-group');
            // Update the selected items.
            fileGroupUpdateInput(groupElement);
        });
        // Add a listener for changing one of the separators.
        $('.file-group').on('change', '.file-group-separator', (event) => {
            // Get the actual group which was clicked.
            const groupElement = $(event.target).closest('.file-group');
            // Update the selected items.
            fileGroupUpdateInput(groupElement);
        });
        // Add a listener for if the upload button was clicked.
        $('.file-group-file-upload').on('change', (event) => {
            // Get the current file.
            const file = event.target.files[0];
            // Set the filename in the label.
            $(event.target).siblings('.file-group-file-label').html(file.name);
            // Define the file reader and the variable for storing its content.
            let fileReader = new FileReader();
            // Define what happens when we read the file.
            fileReader.onload = (e) => {
                // Write the content of the file to the text area.
                $(event.target).closest('.file-group').find('.file-group-text').first().val(e.target.result);
                // Get the actual group which was clicked.
                const groupElement = $(event.target).closest('.file-group');
                // Update the selected items.
                fileGroupUpdateInput(groupElement);
            };
            // Read the file.
            fileReader.readAsText(file);
        });
        // Add a listener for if the add button was clicked.
        $('.heuristics-group-add').on('click', (event) => {
            // Get the actual group which was clicked.
            const groupElement = $(event.target).closest('.heuristics-group');
            // Get the current heuristics element.
            const currentHeuristicsElement = $(groupElement).find('.heuristics-group-current');
            // Get a clone of the selected possible heuristics options.
            const options = $(groupElement).find('.heuristics-group-possible').children('option:selected').clone();
            // Check if the option to add a new group is selected.
            if ($(options).filter((index, element) => $(element).val() === '').length !== 0) {
                // Define the optgroups to be updated.
                let optgroups;
                // Check if there are any selected current heuristics optgroups.
                if ($(currentHeuristicsElement).find('option:selected').length !== 0) {
                    // Add a new optgroup after each selected current heuristics optgroup.
                    $(currentHeuristicsElement).children().filter((index, element) => $(element).children('option:selected').length !== 0).after(heuristicGroupOptgroupHTML);
                    // Update the selected optgroups.
                    optgroups = $(currentHeuristicsElement).children().filter((index, element) => $(element).children().length === 0);
                } else {
                    // Add a new optgroup to the current heuristics.
                    $(currentHeuristicsElement).append(heuristicGroupOptgroupHTML);
                    // Update the selected optgroups.
                    optgroups = $(currentHeuristicsElement).children().last();
                }
                // Append to them the clones of the selected possible heurstics options, except for the new group.
                $(optgroups).append($(options).filter((index, element) => $(element).val() !== ''));

            } else {
                // Define the optgroups to be updated.
                let optgroups;
                // Check if there doesn't exist any optgroup.
                if ($(currentHeuristicsElement).children().length === 0) {
                    // Add a new optgroup to the current heuristics.
                    $(currentHeuristicsElement).append(heuristicGroupOptgroupHTML);
                }
                // Check if there are any selected current heuristics optgroups.
                if ($(currentHeuristicsElement).find('option:selected').length !== 0) {
                    // Update the selected optgroups.
                    optgroups = $(currentHeuristicsElement).children().filter((index, element) => $(element).children('option:selected').length !== 0);
                } else {
                    // Update the selected optgroups.
                    optgroups = $(currentHeuristicsElement).children().last();
                }
                // Append to them the clones of the selected possible heurstics options.
                $(optgroups).append(options);
            }
            // Update the current heuristics.
            heuristicGroupUpdateCurrentHeuristics(groupElement);
            // Update the input text.
            heuristicGroupUpdateText(groupElement);
        });
        // Add a listener for if the remove button was clicked.
        $('.heuristics-group-remove').on('click', (event) => {
            // Get the actual group which was clicked.
            const groupElement = $(event.target).closest('.heuristics-group');
            // Remove the selected current heuristics options.
            $(groupElement).find('.heuristics-group-current').find('option:selected').remove();
            // Update the current heuristics.
            heuristicGroupUpdateCurrentHeuristics(groupElement);
            // Update the input text.
            heuristicGroupUpdateText(groupElement);
        });
        // Execute the function on page load.
        (() => {
            // Go over all of the form groups.
            $('.form-file-group').each((index, groupElement) => {
                // Clear any existing text.
                $(groupElement).find('.form-file-group-text').val('');
            });
            // Go over all of the groups.
            $('.file-group').each((index, groupElement) => {
                // Update the text.
                fileGroupUpdateText(groupElement);
                // Update the selected items.
                fileGroupUpdateInput(groupElement);
            });
            // Go over all of the groups.
            $('.heuristics-group').each((index, groupElement) => {
                // Update the heuristics.
                heuristicGroupUpdateHeuristics(groupElement);
                // Update the current heuristics.
                heuristicGroupUpdateCurrentHeuristics(groupElement);
                // Update the input text.
                heuristicGroupUpdateText(groupElement);
            });
        })();
    }

    // Check if there is a copy group on the page.
    if ($('.copy-group').length !== 0) {
        // Add a listener for clicking the button.
        $('.copy-group-button').on('click', (event) => {
            // Get the actual group which was clicked.
            const groupElement = $(event.target).closest('.copy-group');
            // Select all of the corresponding data.
            $(groupElement).find('.copy-group-data').first().select();
        });
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

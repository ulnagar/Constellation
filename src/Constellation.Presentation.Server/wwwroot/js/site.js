// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    // Activate any comboboxes
    $(".combo").select2({ theme: 'bootstrap' });

    // Activate any comboboxes with free-text entry
    $(".combo-with-tag").select2({
        theme: 'bootstrap',
        tags: true
    });

    // Format dates in datatables for AUS
    $.fn.dataTable.moment('D/M/YYYY');

    // Activate any datatables
    $('.data-table').DataTable({ "order": [[0, 'asc']] });

    $('.data-table-25')
        .DataTable({
            "order": [],
            "pageLength": 25
        });

    $('.grouped-data-table')
        .DataTable({
            "order": [],
            "rowGroup": {
                "dataSrc": 0
            },
            "columnDefs": [
                { "visible": false, "targets": 0 }
            ]
        });

    // Create a datatable with the first column hidden, and the first column used as the sort data for the second column
    $('.hidden-sort-data-table')
        .DataTable({
            "order": [ [ 1, 'asc'] ],
            "columnDefs": [
                { "visible": false, "targets": 0 },
                { "orderData": 0, "targets": 1 }
            ]
        });
});
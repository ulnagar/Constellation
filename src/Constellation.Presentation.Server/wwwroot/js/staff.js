﻿$(document).ready(function () {
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

    $('.data-table-sort-1').DataTable({ "order": [[1, 'asc']] });

    $('.data-table-no-sort').DataTable({ "order": [] });

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

    // Add the showLoader function to any nav links
    $('a.showLoader').on('click', showLoader);
    $('a.nav-link:not(.dropdown-toggle):not([role="tab"])').on('click', showLoader);
    $('a.dropdown-item').on('click', showLoader);
    $('a.btn').on('click', showLoader);
});

function showLoader() {
    if ($('.spinner-overlay').length > 0) {
        $('.spinner-overlay').removeClass('d-none');
        setTimeout(function () {
            $('.spinner-overlay').addClass('d-none');
        }, 10000);
    }
};
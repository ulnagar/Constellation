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
    $('.data-table').DataTable({ "order": [] });
});
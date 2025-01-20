$(function () {
    // Add the showLoader function to any nav links
    $('a.showLoader:not(.no-loader):not(.show-loader):not(.show-loader-5):not(.show-loader-10)').on('click', showLoader);
    $('input.showLoader:not(.no-loader):not(.show-loader):not(.show-loader-5):not(.show-loader-10)').on('click', showLoader);
    $('a.nav-link:not(.no-loader):not(.dropdown-toggle):not([role="tab"]):not(.show-loader):not(.show-loader-5):not(.show-loader-10)').on('click', showLoader);
    $('a.dropdown-item:not(.no-loader):not(.show-loader):not(.show-loader-5):not(.show-loader-10)').on('click', showLoader);
    $('a.btn:not(.no-loader):not(.show-loader):not(.show-loader-5):not(.show-loader-10)').on('click', showLoader);

    // Start to replace the above automatic registration of loader display with explicit calls to show the loader in particular lengths
    // Show loader until page refresh
    // Use the delegated selector $(document).on('click', 'target', function()) to ensure that dynamic content is also able to trigger the function.
    $(document).on('click', '.show-loader', function () {
        showLoader()
    });
    // Show loader for 5 seconds
    $(document).on('click', '.show-loader-5', function () {
        showLoader(5000)
    });
    // Show loader for 10 seconds
    $(document).on('click', '.show-loader-10', function () {
        showLoader(10000)
    }); 

    // Activate any comboboxes
    $(".combo").select2({ theme: 'bootstrap' });

    // Activate any comboboxes with free-text entry
    $(".combo-with-tag").select2({
        theme: 'bootstrap',
        tags: true
    });

    // Format dates in datatables for AUS
    $.fn.dataTable.moment('D/M/YYYY');

    // Set datatables defaults
    Object.assign(DataTable.defaults, {
        stateSave: true,
        stateDuration: 60 * 10
    });

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
});

function showLoader(delay = 0) {
    $('.spinner-overlay').removeClass('d-none');

    if (delay > 0) {
        setTimeout(function () {
            $('.spinner-overlay').addClass('d-none');
        }, delay);
    }
}

function toggleLoader() {
    var overlay = $('.spinner-overlay');

    if (overlay.hasClass('d-none'))
        $('.spinner-overlay').removeClass('d-none');
    else
        $('.spinner-overlay').addClass('d-none');
}
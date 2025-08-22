$(function () {
    // Activate any comboboxes
    $(".combo").select2({ theme: 'bootstrap-5' });

    // Activate any comboboxes with free-text entry
    $(".combo-with-tag").select2({
        theme: 'bootstrap-5',
        tags: true
    });

    $(document).on('click', '.show-loader', function () {
        showLoader()
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
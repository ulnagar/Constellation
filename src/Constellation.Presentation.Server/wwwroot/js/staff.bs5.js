$(function () {
    // Activate any comboboxes
    $(".combo").each(function () {
        $(this).select2({
            theme: 'bootstrap-5',
            dropdownParent: $(this).parent()
        });
    });

    // Activate any comboboxes with free-text entry
    $(".combo-with-tag").each(function () {
        $(this).select2({
            theme: 'bootstrap-5',
            tags: true,
            dropdownParent: $(this).parent()
        });
    });

    $(document).on('focusin', function (e) {
        if ($(e.target).closest('.select2-container').length) {
            e.stopImmediatePropagation();
        }
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

// Active any Summernote text areas
$('.summernote').summernote({
    height: 300,
    toolbar: [
        // [groupName, [list of button]]
        ['style', ['bold', 'italic', 'underline', 'clear']],
        ['font', ['strikethrough', 'superscript', 'subscript']],
        ['fontsize', ['fontsize']],
        ['color', ['color']],
        ['para', ['ul', 'ol', 'paragraph']],
        ['insert', ['link']],
        ['height', ['height']]
    ]
});

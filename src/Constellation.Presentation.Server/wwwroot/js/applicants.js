function showLoader(delay = 0) {
    $('.spinner-overlay').removeClass('d-none');

    if (delay > 0) {
        setTimeout(function () {
            $('.spinner-overlay').addClass('d-none');
        }, delay);
    }
}


/* Code for the tri-state switch */

document.addEventListener('click', function (e) {
    if (e.target.closest('[data-unset]')) e.preventDefault();
}, true);

function updateShowWhen(name, val) {
    document.querySelectorAll('[data-show-when^="' + name + '="]').forEach(function (el) {
        el.classList.toggle('tri-show', val === el.dataset.showWhen.split('=')[1]);
    });
}

document.addEventListener('change', function (e) {
    if (e.target.classList.contains('btn-check')) updateShowWhen(e.target.name, e.target.value);
});

// Initialise — handles pre-populated edit forms
document.querySelectorAll('[data-show-when]').forEach(function (el) {
    var parts = el.dataset.showWhen.split('=');
    var checked = document.querySelector('[name="' + parts[0] + '"]:checked');
    if (checked && checked.value === parts[1]) el.classList.add('tri-show');
});

/* End of code for the tri-state switch */
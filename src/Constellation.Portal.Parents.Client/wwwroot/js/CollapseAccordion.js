window.collapseAccordion = () => {
    console.log('Collapsing!');
    //$('.accordion-collapse').collapse('hide');
    var collapseElementList = [].slice.call(document.querySelectorAll('.collapse'));
    var collapseList = collapseElementList.map(function (collapseEl) {
        return new bootstrap.Collapse(collapseEl, { hide: false });
    });
}
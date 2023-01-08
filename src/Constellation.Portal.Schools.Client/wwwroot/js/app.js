window.collapseAccordion = () => {
    console.log('Collapsing!');
    const collapseElements = document.querySelectorAll('.collapse');

    collapseElements.forEach(element => {
        element.classList.remove('show');
    });

    const buttonElements = document.querySelectorAll('.accordion-button');

    buttonElements.forEach(element => {
        element.classList.add('collapsed');
    })
}

window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}
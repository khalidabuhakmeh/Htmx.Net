// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("htmx:configRequest", (evt) => {
    let httpVerb = evt.detail.verb.toUpperCase();
    if (httpVerb === 'GET') return;
    
    let antiForgery = htmx.config.antiForgery;
    
    if (antiForgery) {
        if (antiForgery.headerName) {
            evt.detail.headers[antiForgery.headerName]
                = antiForgery.requestToken;
        } else {
            evt.detail.parameters[antiForgery.formFieldName]
                = antiForgery.requestToken;
        }
    }
});
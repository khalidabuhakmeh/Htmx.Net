// Progressive enhancement for the sample:
//  - syntax-highlights each "How it works" code figure (highlight.js)
//  - adds a "copy" button to each code figure
(function () {
    // Map the figcaption's .lang label to a highlight.js language.
    var LANGS = { html: "xml", razor: "cshtml", cshtml: "cshtml", csharp: "csharp", cs: "csharp", js: "javascript", json: "json", css: "css" };

    function highlightCode() {
        if (!window.hljs) return;
        document.querySelectorAll("figure.code").forEach(function (fig) {
            var code = fig.querySelector("pre code");
            if (!code || code.dataset.highlighted) return;
            var langLabel = (fig.querySelector(".lang")?.textContent || "").trim().toLowerCase();
            var lang = LANGS[langLabel];
            if (lang) code.classList.add("language-" + lang);
            try { window.hljs.highlightElement(code); } catch (e) { /* leave plain */ }
        });
    }

    function addCopyButtons() {
        document.querySelectorAll("figure.code > figcaption").forEach(function (cap) {
            if (cap.querySelector(".copy-btn")) return;
            var fig = cap.closest("figure.code");
            var code = fig && fig.querySelector("pre code, pre");
            if (!code) return;
            var btn = document.createElement("button");
            btn.type = "button";
            btn.className = "copy-btn";
            btn.textContent = "Copy";
            btn.addEventListener("click", function () {
                navigator.clipboard.writeText(code.textContent).then(function () {
                    btn.textContent = "Copied";
                    btn.classList.add("copied");
                    setTimeout(function () { btn.textContent = "Copy"; btn.classList.remove("copied"); }, 1200);
                });
            });
            cap.appendChild(btn);
        });
    }

    function enhance() { highlightCode(); addCopyButtons(); }

    if (document.readyState !== "loading") enhance();
    else document.addEventListener("DOMContentLoaded", enhance);
})();

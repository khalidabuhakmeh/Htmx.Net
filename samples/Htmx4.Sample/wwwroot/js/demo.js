// Progressive enhancement for the sample: adds a "copy" button to each code figure.
(function () {
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
    if (document.readyState !== "loading") addCopyButtons();
    else document.addEventListener("DOMContentLoaded", addCopyButtons);
})();

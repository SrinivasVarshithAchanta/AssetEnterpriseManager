// AssetOps - minimal progressive enhancement only.
// No frameworks. Everything degrades gracefully if JavaScript is off.

(function () {
    "use strict";

    // 1) Confirm before destructive actions (retire / deactivate / cancel).
    //    Any form with data-confirm shows a prompt before it submits.
    document.addEventListener("submit", function (e) {
        var form = e.target;
        if (form.matches("form[data-confirm]")) {
            var message = form.getAttribute("data-confirm") || "Are you sure?";
            if (!window.confirm(message)) {
                e.preventDefault();
            }
        }
    });

    // 2) Live "available assets" count on the request creation page.
    //    Calls the internal API when the category dropdown changes.
    var categorySelect = document.getElementById("categorySelect");
    var availabilityHint = document.getElementById("availabilityHint");

    if (categorySelect && availabilityHint) {
        var updateAvailability = function () {
            var categoryId = categorySelect.value;
            if (!categoryId) {
                availabilityHint.textContent = "";
                return;
            }
            fetch("/api/assets/availability?categoryId=" + encodeURIComponent(categoryId), {
                headers: { "Accept": "application/json" }
            })
                .then(function (r) { return r.ok ? r.json() : null; })
                .then(function (data) {
                    if (data) {
                        availabilityHint.textContent =
                            data.available + " asset(s) currently available in this category.";
                    }
                })
                .catch(function () {
                    availabilityHint.textContent = "";
                });
        };

        categorySelect.addEventListener("change", updateAvailability);
        updateAvailability();
    }

    // 3) Optional client side helper: quick-filter rows of a table by text.
    //    Used where a table has a [data-quick-filter] input pointing at it.
    var quickFilter = document.querySelector("[data-quick-filter]");
    if (quickFilter) {
        var targetSelector = quickFilter.getAttribute("data-quick-filter");
        var table = document.querySelector(targetSelector);
        if (table) {
            quickFilter.addEventListener("input", function () {
                var term = quickFilter.value.toLowerCase();
                table.querySelectorAll("tbody tr").forEach(function (row) {
                    row.style.display = row.textContent.toLowerCase().indexOf(term) > -1 ? "" : "none";
                });
            });
        }
    }
})();

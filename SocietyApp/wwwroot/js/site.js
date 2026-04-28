// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ── Bootstrap Confirm Modal ──────────────────────────────────────────────────
(function () {
    var confirmModal = null;
    var pendingForm = null;

    document.addEventListener('DOMContentLoaded', function () {
        confirmModal = new bootstrap.Modal(document.getElementById('confirmModal'));

        document.getElementById('confirmModalOk').addEventListener('click', function () {
            confirmModal.hide();
            if (pendingForm) {
                pendingForm.removeAttribute('data-confirm-active');
                pendingForm.submit();
                pendingForm = null;
            }
        });

        // Intercept any form with data-confirm or any submit button with data-confirm
        document.body.addEventListener('click', function (e) {
            var btn = e.target.closest('button[type="submit"][data-confirm]');
            if (btn) {
                e.preventDefault();
                e.stopPropagation();
                var form = btn.closest('form');
                pendingForm = form;
                document.getElementById('confirmModalMessage').textContent = btn.getAttribute('data-confirm');
                confirmModal.show();
                return;
            }
        }, true);

        document.body.addEventListener('submit', function (e) {
            var form = e.target;
            var msg = form.getAttribute('data-confirm');
            if (msg && !form.hasAttribute('data-confirm-active')) {
                e.preventDefault();
                pendingForm = form;
                document.getElementById('confirmModalMessage').textContent = msg;
                confirmModal.show();
            }
        });
    });
}());

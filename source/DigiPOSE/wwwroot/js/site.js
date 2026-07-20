/**
 * Global Dynamic AJAX Modal Form Engine for ERP Architecture
 * Address: Unobtrusive Validation Parsing, Anti-Forgery Tokens, & DRY Event Delegation
 */
$(document).ready(function () {

    // 1. GLOBAL GET ENGINE: Triggered by any element with class '.btn-show-modal'
    $(document).on('click', '.btn-show-modal', function (e) {
        e.preventDefault();
        var $btn = $(this);
        var url = $btn.data('url') || $btn.attr('href');

        if (!url) return;

        $.ajax({
            url: url,
            type: 'GET',
            headers: { "X-Requested-With": "XMLHttpRequest" },
            success: function (htmlContent) {
                var $container = $('#globalModalContainer');
                $container.html(htmlContent);

                // RE-PARSE UNOBTRUSIVE VALIDATION FOR DYNAMIC HTML IF AVAILABLE
                if ($.validator && $.validator.unobtrusive) {
                    $.validator.unobtrusive.parse($container);
                }

                var modalEl = document.getElementById('globalModal');
                if (modalEl) {
                    var modal = bootstrap.Modal.getInstance(modalEl) || new bootstrap.Modal(modalEl);
                    modal.show();
                }
            },
            error: function (xhr) {
                var errorMsg = (xhr.responseJSON && xhr.responseJSON.message) 
                    ? xhr.responseJSON.message 
                    : "Failed to load view (" + xhr.status + " " + xhr.statusText + ").";

                if (typeof showCyberNotify === 'function') {
                    showCyberNotify('System Fault', errorMsg, 'error');
                } else {
                    alert(errorMsg);
                }
            }
        });
    });

    // 2. GLOBAL POST ENGINE: Triggered by any form with class '.ajax-modal-form'
    $(document).on('submit', '.ajax-modal-form', function (e) {
        e.preventDefault();
        var $form = $(this);

        // CLIENT-SIDE VALIDATION CHECK IF AVAILABLE
        if ($form.valid && !$form.valid()) {
            return false;
        }

        $.ajax({
            url: $form.attr('action'),
            type: 'POST',
            data: $form.serialize(), // AUTOMATICALLY INCLUDES __RequestVerificationToken & MODEL DATA
            headers: { "X-Requested-With": "XMLHttpRequest" },
            success: function (response) {
                // CASE A: SERVER RETURNS JSON (SUCCESSFUL TRANSACTION)
                if (typeof response === 'object' && response.success) {
                    var modalEl = document.getElementById('globalModal');
                    if (modalEl) {
                        var modalInstance = bootstrap.Modal.getInstance(modalEl);
                        if (modalInstance) modalInstance.hide();
                    }

                    if (typeof showCyberNotify === 'function') {
                        showCyberNotify('Success', response.message || 'Action executed successfully.', 'success');
                    }
                    
                    setTimeout(function () {
                        location.reload();
                    }, 600);
                } 
                // CASE B: SERVER RETURNS JSON (BUSINESS RULE FAILURE)
                else if (typeof response === 'object' && !response.success) {
                    if (typeof showCyberNotify === 'function') {
                        showCyberNotify('Validation Fault', response.message, 'warning');
                    }
                }
                // CASE C: SERVER RETURNS HTML PARTIAL VIEW (MODELSTATE INVALID WITH INLINE ERRORS)
                else {
                    var $container = $('#globalModalContainer');
                    $container.html(response);

                    // RE-PARSE UNOBTRUSIVE VALIDATION FOR RETURNED ERROR PARTIAL VIEW
                    if ($.validator && $.validator.unobtrusive) {
                        $.validator.unobtrusive.parse($container);
                    }
                }
            },
            error: function (xhr) {
                var errorMsg = (xhr.responseJSON && xhr.responseJSON.message) 
                    ? xhr.responseJSON.message 
                    : "An error occurred during submission (" + xhr.status + ").";

                if (typeof showCyberNotify === 'function') {
                    showCyberNotify('Execution Error', errorMsg, 'error');
                } else {
                    alert(errorMsg);
                }
            }
        });
    });

});

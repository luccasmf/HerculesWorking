/// <reference path="Util.js" />
$(document).ready(function () {
    // Util
    bindShowConfirmacao();
    bindDatatable();
    bindFormFilter();
    bindCancelar();
    bindCadastro();

    bindAlterarStatus();
    bindFormSubmitAlterarStatus();
});

function bindAlterarStatus() {
    $(document).on("click", ".btnAlterarStatus", function () {
        $.ajax({
            url: $(this).attr("url"),
            type: 'GET',
            beforeSend: function () {
                showLoader();
            },
            success: function (result) {
                if (result.Status == true)
                    showNotificationRefresh(result, false, true);
                else
                    showNotification(result)
            },
            error: function () {
                var obj = {
                    Type: "error",
                    Message: "Ocorreu um erro ao realizar esta operação"
                }
                showNotification(obj);
            },
            complete: function () {
                hideLoader();
            },
        });
    });
}

function bindFormSubmitAlterarStatus() {
    $(document).on("submit", ".frm-submit-modal", function (e) {
        e.preventDefault();
        var frm = $('.frm-submit-modal');
        $.ajax({
            url: frm.attr("action"),
            type: frm.attr("method"),
            data: frm.serialize(),
            beforeSend: function () {
                showLoader();
            },
            success: function (result) {
                if (result.Status == true)
                    showNotificationRefresh(result, true, true);
                else
                    showNotification(result);
            },
            error: function (result) {
                var obj = {
                    Type: "error",
                    Message: "Ocorreu um erro ao realizar esta operação"
                }
                showNotification(obj);
            },
            complete: function () {
                hideLoader();
            },
        });
        return false;
    });
}
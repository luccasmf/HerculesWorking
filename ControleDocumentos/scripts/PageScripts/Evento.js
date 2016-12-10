/// <reference path="Util.js" />
$(document).ready(function () {
    // Util
    bindShowConfirmacao();
    bindDatatable();
    bindFormFilter();
    bindCancelar();
    bindFormSubmit();
    bindCadastro();
    bindFormSubmitModal();

    bindChamada();
    bindCancelarChamada();
    bindGerarCertificados();
    bindFormSubmitChamada();
});

function bindChamada() {
    $(document).on("click", ".btnChamada", function () {
        $.ajax({
            url: $(this).attr("url"),
            type: 'GET',
            beforeSend: function () {
                showLoader();
            },
            success: function (data) {
                if (data == "") {
                    var obj = {
                        Type: "error",
                        Message: "Não autorizado!"
                    }
                    showNotification(obj);
                }
                else {
                    $(".formChamada").html(data);
                    $(".divFormChamada").show();
                    $(".divList").hide();
                }
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
            }
        });
    });

}

function bindCancelarChamada() {
    $(document).on("click", ".btnCancelarChamada", function () {
        $(".divFormChamada").hide();
        $(".divList").show();
    });
}

function bindGerarCertificados() {
    $(document).on("click", ".btnGerarCertificados", function () {
        $.ajax({
            url: $(this).attr("url"),
            type: 'GET',
            beforeSend: function () {
                showLoader();
            },
            success: function (data) {
                showNotification(data);
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
            }
        });
    });

}

function bindFormSubmitChamada() {
    $(document).on("submit", ".frm-submit-chamada", function (e) {
        e.preventDefault();
        var frm = $('.frm-submit-chamada');
        $.ajax({
            url: frm.attr("action"),
            type: frm.attr("method"),
            data: frm.serialize(),
            beforeSend: function () {
                showLoader();
            },
            success: function (result) {
                if (result.Status == true) {
                    $(".divFormChamada").hide();
                    $(".divList").show();

                    $('.form-filter').trigger('submit', function () {
                        bindFormFilter();
                    });

                    var obj = {
                        Type: result.Type,
                        Message: result.Message
                    }
                    showNotification(obj);
                }
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
            }
        });
        return false;
    });
}
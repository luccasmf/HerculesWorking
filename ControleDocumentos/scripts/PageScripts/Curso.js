/// <reference path="Util.js" />
$(document).ready(function () {
    bindCadastro();
    bindCamposModal();
    bindGetCoordenadores();

    //Util
    bindFormFilter();
    bindDatatable();
    bindShowConfirmacao();
    bindFormSubmitModal();
});

function bindCadastro() {
    $(document).on("click", ".btnCadastro", function () {
        $.ajax({
            url: $(this).attr("url"),
            type: 'GET',
            beforeSend: function () {
                showLoader();
            },
            success: function (data) {
                $("#divModalGlobalBody").html(data);
                $("#divModalGlobal").modal("show");
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

function bindCamposModal() {
    $('.modal').on('shown.bs.modal', function () {
        $("#Nome").focus();
    })
}

function bindGetCoordenadores() {
    $(document).on("click", ".refreshCoordenadores", function () {
        var url = $(this).attr("url");
        $.ajax({
            url: url,
            type: 'post',
            beforeSend: function () {
                showLoader();
            },
            success: function (data) {
                var options = $("#ddlCoordenador");
                options.empty();
                $.each(data, function () {
                    options.append($("<option />").val(this.Value).text(this.Text));
                });
            },
            error: function () {
                var obj = {
                    Type: "error",
                    Message: "Ocorreu um erro ao realizar esta operação"
                };
                showNotification(obj);
            },
            complete: function () {
                hideLoader();
            },
        });
    });
}
/// <reference path="Util.js" />
$(document).ready(function () {
    bindCadastro();
    bindCamposModal();

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
        $("#TipoDocumento1").focus();
    })
}
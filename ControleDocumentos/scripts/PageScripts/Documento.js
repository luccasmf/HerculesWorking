/// <reference path="Util.js" />
$(document).ready(function () {
    // Util
    bindShowConfirmacao();
    bindDatatable();
    bindFormFilter();
    bindFormSubmitModal();
    bindCancelar();

    bindCadastro();
    bindGetAlunos();
    bindSubmitDocumento();
});

function bindGetAlunos() {
    $(document).on("change", "#ddlCurso", function () {
        var idCurso = $(this).val();
        if (idCurso > 0) {
            var url = $(this).attr("url");
            $.ajax({
                url: url,
                type: 'post',
                data: { "idCurso": idCurso },
                beforeSend: function () {
                    showLoader();
                },
                success: function (data) {
                    var options = $("#ddlAluno");
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
        }
        else {
            var options = $("#ddlAluno");
            options.empty();
            options.append($("<option />").val("").text("Selecione um curso"))
        }
    });
}

function bindSubmitDocumento() {
    $(document).on("submit", '.frm-submit', function () {
        var frm = $('.frm-submit');
        $.ajax({
            url: frm.attr("action"),
            cache: false,
            contentType: false,
            processData: false,
            type: frm.attr("method"),
            data: new FormData(this),
            beforeSend: function () {
                showLoader();
            },
            success: function (result) {
                if (result.Status == true)
                    showNotificationRefresh(result, false, true);
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
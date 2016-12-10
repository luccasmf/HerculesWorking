$(document).ready(function () {
    $.datepicker.setDefaults(
        $.extend(
        { 'dateFormat': 'dd-mm-yyyy' },
        $.datepicker.regional['pt-BR']
        )
    );
})

function inicializaTooltip() {
    $('[rel="tooltip"],[data-rel="tooltip"]').tooltip({ "placement": "bottom", delay: { show: 400, hide: 200 } });
}

// Início Loader

function showLoader() {
    $(".hiddenLoader").show()
}

function hideLoader() {
    $(".hiddenLoader").hide()
}

// Fim Loader

// Início Notification

function showNotificationRefresh(notification, hideModal, hideForm) {
    if (hideModal == true)
        $("#divModalGlobal").modal("hide");

    if (hideForm) {
        $(".divFormSave").hide();
        $(".divList").show();
    }

    $('.form-filter').trigger('submit', function () {
        bindFormFilter();
    });

    var obj = {
        Type: notification.Type,
        Message: notification.Message
    }
    showNotification(obj);
}

function showNotificationRedirect(notification) {
    $.ajax({
        url: notification.ReturnUrl,
        type: 'GET',
        beforeSend: function () {
            showLoader();
        },
        success: function (data) {
            $("body").html(data);
            var obj = {
                Type: notification.Type,
                Message: notification.Message
            }
            showNotification(obj);
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
}

function showNotification(notification) {
    var n = noty({
        text: notification.Message,
        layout: "topCenter",
        type: notification.Type
    });
};

// Fim Notification

/*--------------------------------------------------------*/

// Início Submit

function bindFormSubmitModal() {
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
                    showNotificationRefresh(result, true, false);
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

function bindFormSubmit() {
    $(document).on("submit", ".frm-submit", function (e) {
        e.preventDefault();
        var frm = $('.frm-submit');
        $.ajax({
            url: frm.attr("action"),
            type: frm.attr("method"),
            data: frm.serialize(),
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

function bindFormSubmitRedirect() {
    $(document).on("submit", ".frm-submit-redirect", function (e) {
        e.preventDefault();
        var frm = $('.frm-submit-redirect');
        $.ajax({
            url: frm.attr("action"),
            type: frm.attr("method"),
            data: frm.serialize(),
            beforeSend: function () {
                showLoader();
            },
            success: function (result) {
                if (result.Status == true) {
                    showLoader();
                    window.location = result.ReturnUrl;
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


// Fim Submit

/*--------------------------------------------------------*/

// Início List

function bindFormFilter() {
    $(document).on('submit', '.form-filter', function (e) {
        e.preventDefault();
        var frm = $('.form-filter');
        $.ajax({
            url: frm.attr("action"),
            type: frm.attr("method"),
            data: frm.serialize(),
            beforeSend: function () {
                showLoader();
            },
            success: function (result) {
                $(".partialList").html(result);
                bindDatatable();
                inicializaTooltip();
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

function bindDatatable() {
    $(".commmon-datatable").DataTable({
        "language": {
            "sEmptyTable": "Nenhum registro encontrado",
            "sInfo": "Mostrando de _START_ até _END_ de _TOTAL_ registros",
            "sInfoEmpty": "Mostrando 0 até 0 de 0 registros",
            "sInfoFiltered": "(Filtrados de _MAX_ registros)",
            "sInfoPostFix": "",
            "sInfoThousands": ".",
            "sLengthMenu": "_MENU_ resultados por página",
            "sLoadingRecords": "Carregando...",
            "sProcessing": "Processando...",
            "sZeroRecords": "Nenhum registro encontrado",
            "sSearch": "Pesquisar",
            "oPaginate": {
                "sNext": "Próximo",
                "sPrevious": "Anterior",
                "sFirst": "Primeiro",
                "sLast": "Último"
            },
            "oAria": {
                "sSortAscending": ": Ordenar colunas de forma ascendente",
                "sSortDescending": ": Ordenar colunas de forma descendente"
            }
        }
    });
}

// Fim List

/*--------------------------------------------------------*/

// Início Form Binds

function bindShowConfirmacao() {
    $(document).on("click", ".btnConfirm", function (e) {
        e.preventDefault();
        $.ajax({
            url: $(this).attr("url"),
            type: 'GET',
            dataType: "html",
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

function bindCadastro() {
    $(document).on("click", ".btnCadastro", function () {
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
                    $(".formSave").html(data);
                    $(".divFormSave").show();
                    $(".divList").hide();

                    $('.datepicker').datepicker();
                    $('.datetimepicker').datetimepicker();
                    $(":file").filestyle({
                        buttonText: "Procurar",
                        buttonName: "btn-primary",
                        iconName: "fa fa-folder-open",
                        buttonBefore: true,
                    });
                    $('[data-rel="chosen"],[rel="chosen"]').chosen({
                        placeholder_text_multiple:"Selecione pelo menos uma opção"
                    });
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

function bindCancelar() {
    $(document).on("click", ".btnCancelar", function () {
        $(".divFormSave").hide();
        $(".divList").show();
    });
}

// Fim Form Binds
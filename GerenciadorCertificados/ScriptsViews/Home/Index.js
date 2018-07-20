$(document).ready(function () {

    // #region CRIAR MODELO 

    $('#btn-undo').on('click', function () {
        document.execCommand('undo');
    });

    $('#btn-redo').on('click', function () {
        document.execCommand('redo');
    });

    $('#btn-negrito').on('click', function () {
        document.execCommand('bold');
    });

    $('#btn-italico').on('click', function () {
        document.execCommand('italic');
    });

    $('#btn-sublinhado').on('click', function () {
        document.execCommand('underline');
    });

    $('#btn-aumentar-texto').on('click', function (e) {
        var size = parseInt(document.queryCommandValue('FontSize'));
        document.execCommand('fontSize',false,size + 1)
    });

    $('#btn-diminuir-texto').on('click', function () {
        var size = parseInt(document.queryCommandValue('FontSize'));
        document.execCommand('fontSize', false, size - 1)
    });

    $('#btn-alinhar-esquerda').on('click', function () {
        document.execCommand('justifyLeft');
    });

    $('#btn-centralizar').on('click', function () {
        document.execCommand('justifyCenter');
    });

    $('#btn-alinhar-direita').on('click', function () {
        document.execCommand('justifyRight');
    });

    $('#btn-justificar').on('click', function () {
        document.execCommand('justifyFull');
    });

    $('#input-imagem-fundo').on('change', function () {
        var file = this.files[0];
        var reader = new FileReader();
        reader.onloadend = function () {
            $('#criar-modelo-page').css('background-image', 'url("' + reader.result + '")');
        }
        if (file) {
            reader.readAsDataURL(file);
        }
    });

    // #endregion 

    // #region ENVIAR CERTIFICADOS

    $('#btn-enviar-certificados').on('click', function () {

        var participantes = $('#importar-participantes-textarea').val();
        var textoCertificado = $('#criar-modelo-page')[0].innerText;
        var backgroundCertificado = $('#criar-modelo-page').css('background-image');
        var htmlCertificado = $('#criar-modelo-page')[0].innerHTML;

        $.ajax({
            type: "POST",
            url: '../Home/EnviarCertificados',
            data: { Participantes: participantes, Texto: textoCertificado, Background: backgroundCertificado, Html: htmlCertificado },
            dataType: 'text',
            success: function (data) {
                var retorno = JSON.parse(data);
                if (retorno.Sucesso) {
                    $('#modal-sucesso-texto').text(retorno.Mensagem);
                    $('#modal-sucesso').modal('show');    
                } else {
                    $('#modal-erro-texto').text(retorno.Mensagem);
                    $('#modal-erro').modal('show');
                }
            }
        });
    });

    // #endregion
});
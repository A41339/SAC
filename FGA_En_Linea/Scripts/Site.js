
function preventBack() {
    window.history.forward();
}

setTimeout("preventBack()", 0);

window.onunload = function () { null };


$(function () {

    $('.ui-state-disabled').removeClass('ui-state-disabled');

    // Configuración moderna de idioma español para MonthPicker
    if (window.jQuery && jQuery.MonthPicker) {
        jQuery.MonthPicker.i18n = {
            year: 'A\u00f1o',
            prevYear: 'A\u00f1o Previo',
            nextYear: 'A\u00f1o Siguiente',
            next12Years: 'Saltar 12 a\u00f1os',
            prev12Years: 'Ir 12 a\u00f1os atr\u00e1s',
            nextLabel: 'Sig',
            prevLabel: 'Ant',
            buttonText: 'Abrir',
            jumpYears: 'Ver a\u00f1os',
            backTo: 'Regresar a',
            months: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Set', 'Oct', 'Nov', 'Dic']
        };
    }

    // Corrección de posicionamiento para que MonthPicker se muestre siempre directamente debajo del input
    if (window.jQuery && jQuery.KidSysco && jQuery.KidSysco.MonthPicker) {
        jQuery.KidSysco.MonthPicker.prototype._position = function ($menu) {
            var _el = this.element;
            var offset = _el.offset();
            if (offset) {
                var menuWidth = 215;
                var leftPos = offset.left;
                var winWidth = $(window).width();
                if (leftPos + menuWidth > winWidth - 10) {
                    leftPos = Math.max(10, winWidth - menuWidth - 10);
                }
                return $menu.css({
                    top: (offset.top + _el.outerHeight() + 6) + 'px',
                    left: leftPos + 'px',
                    position: 'absolute',
                    zIndex: 99999
                });
            }
        };
    }

    function posicionarMenu($input) {
        var offset = $input.offset();
        if (offset) {
            var menuWidth = 215;
            var leftPos = offset.left;
            var winWidth = $(window).width();
            if (leftPos + menuWidth > winWidth - 10) {
                leftPos = Math.max(10, winWidth - menuWidth - 10);
            }
            var $menu = $('#MonthPicker_' + $input.attr('id'));
            $menu.css({
                top: (offset.top + $input.outerHeight() + 6) + 'px',
                left: leftPos + 'px',
                position: 'absolute',
                zIndex: 99999
            });
            $menu.find('.ui-state-highlight').removeClass('ui-state-highlight');
        }
    }

    $('.datecontrol').each(function () {
        var $input = $(this);
        if (!$input.parent().hasClass('modern-input-wrapper')) {
            $input.wrap('<div class="modern-input-wrapper" style="position: relative; display: inline-flex; align-items: center; width: 100%;"></div>');
        }
    });

    $('.datecontrol').MonthPicker({
        Button: '<img src="https://www.ffc.co.cr/FFC/Content/images/calendar.png" />',
        OnAfterMenuOpen: function () {
            posicionarMenu($(this));
            $('#MonthPicker_' + $(this).attr('id')).find('.ui-state-highlight').removeClass('ui-state-highlight');
        }
    });

    // Abrir también al hacer clic o foco en el campo de texto
    $(document).on('click', '.datecontrol', function () {
        var $btn = $(this).siblings('img, .month-picker-open-button');
        if ($btn.length) {
            $btn.trigger('click');
        }
    });

    $(".date").datepicker({
        showOn: "button",
        buttonImage: 'https://www.ffc.co.cr/FFC/Content/images/calendar.png',
        dateFormat: 'dd-mm-yy'
    });
});


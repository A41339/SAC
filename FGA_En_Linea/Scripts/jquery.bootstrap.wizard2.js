(function ($) {

    // Plugin declaration
    $.fn.jqueryBootstrapWizard = function (options) {
        // Parameters
        options = $.extend({}, $.fn.jqueryBootstrapWizard.defaultOptions, options);

        // Prepare header
        var header = $.fn.jqueryBootstrapWizard.headerTemplate.replace('{HEADER}', '');

        // Prepare body
        var body = $.fn.jqueryBootstrapWizard.bodyTemplate.replace('{BODY}', '');

        // Prepare footer
        var footer = $.fn.jqueryBootstrapWizard.footerTemplate.replace('{PREV_LABEL}', $.fn.jqueryBootstrapWizard.labels.btn_back);
        footer = footer.replace('{NEXT_LABEL}', $.fn.jqueryBootstrapWizard.labels.btn_next);
        footer = footer.replace('{FINISH_LABEL}', $.fn.jqueryBootstrapWizard.labels.btn_finish);
        if (options.cancel_url != null) {
            footer = footer.replace('{CANCEL_PANEL}', '<div class="float-left"><a id="jqb-btn-cancel" class="btn btn-danger text-white mr-1" href="{CANCEL_URL}">{CANCEL_LABEL}</a></div>');
            footer = footer.replace('{CANCEL_LABEL}', $.fn.jqueryBootstrapWizard.labels.btn_cancel);
            footer = footer.replace('{CANCEL_URL}', options.cancel_url);
        } else {
            footer = footer.replace('{CANCEL_PANEL}', '');
        }

        // Prepare template
        var template = $.fn.jqueryBootstrapWizard.wizardTemplate.replace('{HEADER}', header);
        template = template.replace('{BODY}', body);
        template = template.replace('{FOOTER}', footer);

        // Insert wizard
        $(template).insertBefore(this);

        // Move nav to wizard
        $(this).detach().appendTo("#jqb-body");
        
        // Bind show tab event
        $('#navWizardSteps li > a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
            //e.target // newly activated tab
            //e.relatedTarget // previous active tab

            // Update wizard title
            $.fn.jqueryBootstrapWizard.updateHeader(e.target.text);

            // Update wizard buttons
            $.fn.jqueryBootstrapWizard.updateButtons();

            if (typeof (options.onShownTab) == 'function') {
                e.targetIndex = $('#navWizardSteps li > a').index(e.target);
                e.relatedTargetIndex = $('#navWizardSteps li > a').index(e.relatedTarget);
                options.onShownTab(e);
            }
        });

        $('#navWizardSteps li > a[data-toggle="tab"]').on('show.bs.tab', function (e) {
            if (typeof (options.onBeforeShowTab) == 'function') {
                e.targetIndex = $('#navWizardSteps li > a').index(e.target);
                e.relatedTargetIndex = $('#navWizardSteps li > a').index(e.relatedTarget);
                options.onBeforeShowTab(e);
            }
        });
        

        // Bind navigation wizard buttons
        $('#jqb-btn-prev').click(function () {
            var current = $.fn.jqueryBootstrapWizard.currentTabIndex();
            if (current > 0) {
                $.fn.jqueryBootstrapWizard.showTabByIndex(current - 1)
            }
        });
        $('#jqb-btn-next').click(function () {
            var current = $.fn.jqueryBootstrapWizard.currentTabIndex();
            if (current < $.fn.jqueryBootstrapWizard.tabCount()) {
                $.fn.jqueryBootstrapWizard.showTabByIndex(current + 1)
            }

            if (current == $.fn.jqueryBootstrapWizard.tabCount() - 1) {
                if (typeof (options.onFinish) == 'function') {
                    options.onFinish();
                }
            }
        });

        // Initialize UI
        $('#navWizardSteps').addClass('hidden');
        $.fn.jqueryBootstrapWizard.updateButtons();
        $.fn.jqueryBootstrapWizard.updateHeader($.fn.jqueryBootstrapWizard.currentTabNode().text);

        return this;
    }

    $.fn.jqueryBootstrapWizard.updateHeader = function (header) {
        $('#jqb-header').html(header);
    }

    $.fn.jqueryBootstrapWizard.updateButtons = function (options) {
        var current = $.fn.jqueryBootstrapWizard.currentTabIndex();
        document.getElementById('evalSpeaker').pause();

        $('#jqb-btn-next').on('click', function () {
            return true;
        });

        if (current == 0) {
            $('#jqb-btn-prev').addClass('hidden'); 
        } else {
            $('#jqb-btn-prev').removeClass('hidden');
        }

        if (current == $.fn.jqueryBootstrapWizard.tabCount() - 1) {
            $('#cant_responsables').val(5);
            $('#jqb-btn-next').addClass('hidden');
            $('#jqb-btn-prev').addClass('hidden');
        } else {
            $('#jqb-btn-next').removeClass('hidden');
        }

        if (current == 3 && $.fn.jqueryBootstrapWizard.tabCount() == 5) {
            $('#jqb-btn-next').html("Guardar y seguir");
            var cantidad = $('#cant_responsables').val();
            if (cantidad == 5) {
                $('#jqb-btn-next').removeClass("hidden");
            }
            else {
                $('#jqb-btn-next').addClass("hidden");
            }
        }
        else if (current == 2) {
            $('#jqb-btn-next').html("Aceptar");
            $('#jqb-btn-next').addClass("hidden");
            document.getElementById('evalSpeaker').play();
        }else {
            $('#jqb-btn-next').html($.fn.jqueryBootstrapWizard.labels.btn_next);
        }
    }

    $.fn.jqueryBootstrapWizard.tabCount = function () {
        return $('#navWizardSteps li > a').length;
    }

    $.fn.jqueryBootstrapWizard.currentTabNode = function () {
        return $($('#navWizardSteps li > a')[$.fn.jqueryBootstrapWizard.currentTabIndex()])[0];
    }

    $.fn.jqueryBootstrapWizard.currentTabIndex = function () {
        var selectedTab = -1;
        $('#navWizardSteps li > a').each(function (index, node) {          
            if ($(node).hasClass('active')) {
                selectedTab = index;
            }
        });
        return selectedTab;
    }

    $.fn.jqueryBootstrapWizard.showTabByIndex = function (index) {
        $($('#navWizardSteps li > a')[index]).tab('show');
    }

    $.fn.jqueryBootstrapWizard.wizardTemplate = '<div id="jqb-wizard" class="card border" style="width: 100%;">' +
        '    {HEADER}' +
        '    {BODY}' +
        '    {FOOTER}' +
        '</div>';
    $.fn.jqueryBootstrapWizard.headerTemplate = '<div id="jqb-header" class="card-header">{HEADER}</div>';
    $.fn.jqueryBootstrapWizard.bodyTemplate = '<div id="jqb-body" class="card-body">{BODY}</div>';
    $.fn.jqueryBootstrapWizard.footerTemplate = '<div id="jqb-footer" class="card-footer text-center">' +
        '{CANCEL_PANEL}<div class="float-right">' +
        '<a id="jqb-btn-prev" class="btn btn-danger text-white mr-1">{PREV_LABEL}</a>' +
        '<a id="jqb-btn-next" class="btn btn-success text-white" >{NEXT_LABEL}</a>' +
        '</div></div>';

    // Default options
    $.fn.jqueryBootstrapWizard.defaultOptions = {
        cancel_url: null,
		onBeforeShowTab: null,
        onShownTab: null,
        onFinish: null
    }

    $.fn.jqueryBootstrapWizard.labels = {
        btn_back: 'Back',
        btn_next: 'Next',
        btn_finish: 'Finish',
        btn_cancel: 'Cancel'
    }

})(jQuery);

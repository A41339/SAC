
function preventBack() {
    window.history.forward();
}

setTimeout("preventBack()", 0);

window.onunload = function () { null };


$(function () {

    $('.ui-state-disabled').removeClass('ui-state-disabled')


    $('.datecontrol').MonthPicker({
        Button: '<img src="https://www.ffc.co.cr/FFC/Content/images/calendar.png" />',
    });

    $(".date").datepicker({
        showOn: "button",
        buttonImage: 'https://www.ffc.co.cr/FFC/Content/images/calendar.png',
        dateFormat: 'dd-mm-yy'
    });

});


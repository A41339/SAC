$(document).ready(function () {
    try {
        $("input[type='text']").each(function () {
            $(this).attr("autocomplete", "off");
        });
    }
    catch (e) { }

    try {
        $("input[type='password']").each(function () {
            $(this).attr("autocomplete", "off");
        });
    }
    catch (e) { }
});

var step = Number($("#numStep").val());

$(document).ready(function () {
    hideButtons(step);
});

$('.action.next').click(function () {
    var currentStep = $('.card-body.step:visible');
    var anyOptionSelected = currentStep.find('input[type="radio"]:checked').length > 0;
    if (anyOptionSelected) {
        var nextStep = currentStep.next('.card-body.step');
        if (nextStep.length > 0) {
            step = step + 1;
            currentStep.hide();
            nextStep.show();
            updateButtonVisibility();
        }
    }

    hideButtons(step);

});

$('.action.back').click(function () {
    var currentStep = $('.card-body.step:visible');
    var previousStep = currentStep.prev('.card-body.step');
    if (previousStep.length > 0) {
        step = step - 1;
        currentStep.hide();
        previousStep.show();
        updateButtonVisibility();
    }

    hideButtons(step);
});

hideButtons = function (step) {
    var limit = parseInt($(".step").length);
    $(".action").hide();
    if (step < limit) {
        $(".next").show();
    }
    if (step > 1) {
        $(".back").show();
    }
    if (step == limit) {
        $(".next").hide();
        $(".submit").show();
    }

    updateButtonVisibility();
};
function onBeginPageChange() {
    setWaitWhileAjaxProcessing(true);
}

function onPageChangedSuccess() {
    $("#mainLayoutBottom div").html("");
    setWaitWhileAjaxProcessing(false);
}

function onPageChangedFailture(result) {
    var response = JSON.parse(result.responseText);
    $("#mainLayoutBottom div").html("<span style='color: " + response.message.Color.Name + "'>" + response.message.Text + "</span>");
    setWaitWhileAjaxProcessing(false);
}

// set wait and disable on body while ajax processing
function setWaitWhileAjaxProcessing(wait) {
    $('html, body').css("cursor", wait ? "wait" : "auto");
    $('html, body').css("pointer-events", wait ? "none" : "auto");
}
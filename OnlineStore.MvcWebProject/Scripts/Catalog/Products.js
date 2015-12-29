function onEditOrderBegin() {
    setWaitWhileAjaxProcessing(true);
}

function onEditToOrderSuccess(result) {
    var resultEntity = JSON.parse(result);
    $("#count" + resultEntity.id).text(resultEntity.count);
    setWaitWhileAjaxProcessing(false);
}

function onAjaxFailture(result) {
    var response = JSON.parse(result.responseText);
    $("#mainLayoutBottom div").html("<span style='color: " + response.message.Color.Name + "'>" + response.message.Text + "</span>");
    setWaitWhileAjaxProcessing(false);
}
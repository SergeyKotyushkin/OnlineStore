function onPasswordChangeBegin() {
    setWaitWhileAjaxProcessing(true);
}

function onProfileChangeBegin() {
    setWaitWhileAjaxProcessing(true);
}

function onChangeImageClick() {
    $("#fileUploader").click();
}

function onChosenImageChanged() {
    setWaitWhileAjaxProcessing(true);
    $("#profileButtonImageChange").css("display", "none");
    $("#profileImage").css("display", "none");
    $("#profileLoader").css("display", "");
    var formData = new FormData();
    var file = document.getElementById("fileUploader").files[0];
    formData.append("file", file);

    $.ajax({
        type: "POST",
        url: $("#urlToUpload").val(),
        data: formData,
        dataType: "json",
        contentType: false,
        processData: false,
        success: function (data) {
            if (data != null) {
                $("#profileImage").attr("src", data.urlToImage);
                $("#profileHiddenImageSrc").attr("value", data.urlToImage);
            }

            $("#profileImage").css("display", "");
            $("#profileLoader").css("display", "none");
            $("#profileButtonImageChange").css("display", "");
            setWaitWhileAjaxProcessing(false);
        },
        error: function (result) {
            var response = JSON.parse(result.responseText);
            $("#mainLayoutBottom div").html("<span style='color: " + response.message.Color + "'>" + response.message.Text + "</span>");

            $("#profileImage").css("display", "");
            $("#profileLoader").css("display", "none");
            $("#profileButtonImageChange").css("display", "");
            setWaitWhileAjaxProcessing(false);
        }
    });
}

function onProfileChangedSuccessfully(data) {
    var response = JSON.parse(data);
    $("#mainLayoutBottom div").html("<span style='color: " + response.color + "'>" + response.text + "</span>");
    setWaitWhileAjaxProcessing(false);
}

function onPasswordChanged(data) {
    var response = JSON.parse(data);
    $("#profilePasswordForm input[type=text]").each(function() {
        $(this).val("");
    });

    $("#mainLayoutBottom div").html("<span style='color: " + response.color + "'>" + response.text + "</span>");
    setWaitWhileAjaxProcessing(false);
}
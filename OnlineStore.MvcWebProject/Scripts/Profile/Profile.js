function onChangeImageClick() {
    $("#fileUploader").click();
}

function onChosenImageChanged() {
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
            } else
                alert("It was error during image loading");

            $("#profileImage").css("display", "");
            $("#profileLoader").css("display", "none");
            $("#profileButtonImageChange").css("display", "");
        },
        error: function () {
            alert("It was error during image loading");

            $("#profileImage").css("display", "");
            $("#profileLoader").css("display", "none");
            $("#profileButtonImageChange").css("display", "");
        }
    });
}

function onProfileChangedSuccessfully(data) {
    var response = JSON.parse(data);
    $("#mainLayoutBottom div").html("");
    $("#mainLayoutBottom div").append("<span style='color: " + response.color + "'>" + response.text + "</span>");
}

function onFileChanged() {
    if ($("#fileUploader").val() !== "")
        $("#profileButtonImageChange").css("display", "");
    else 
        $("#profileButtonImageChange").css("display", "none");
}

function onPasswordChanged(data) {
    var response = JSON.parse(data);
    $("#profilePasswordForm input[type=text]").each(function() {
        $(this).val("");
    });

    $("#mainLayoutBottom div").html("");
    $("#mainLayoutBottom div").append("<span style='color: " + response.color + "'>" + response.text + "</span>");
}
function onDeleteSuccess(data) {
    var response = JSON.parse(data);
    if (response.result) {
        $("#mainLayoutBottom div").html("");
        $("#mainLayoutBottom div").append("<span style='color: " + response.message.Color + "'>" + response.message.Text + "</span>");
        refreshTable();
    }
}

function onAddSuccess(data) {
    var response = JSON.parse(data);
    if (response.result) {
        $("#mainLayoutBottom div").html("");
        $("#mainLayoutBottom div").append("<span style='color: " + response.message.Color + "'>" + response.message.Text + "</span>");
        refreshTable();
    }
}

function startEdit(el) {
    $("#hiddenEditId").val($(el).attr("name"));
    $(".managementDeleteButtons a, .managementEditButtons input").each(function () {
        $(this).css("display", "none");
    });

    $(".managementAddInputs, #addButton").each(function () {
        $(this).attr("disabled", true);
    });

    $(el).closest("tr").find(".editAfter").each(function () {
        $(this).css("display", "");
    });

    var cells = $(el).closest("tr").find("td");
    var id = cells[2].innerText;
    var name = cells[3].innerText;
    var category = cells[4].innerText;
    var price = cells[5].innerText;

    cells.each(function (index) {
        if (index < 2)
            return;

        var value = "";
        switch (index) {
        case 2:
            value = id;
            break;
        case 3:
            value = name;
            break;
        case 4:
            value = category;
            break;
        case 5:
            value = price;
            break;
        default:
            break;
        }

        $(this).html("<input type=\"text\" value=\"" + value + "\" class=\"managementEditInputs\" />");
        if (index === 2)
            $(this).find("input").first().attr("disabled", true);
    });
}

function editAfterClick(el, result) {
    if (result) addOrUpdate(el, false, "#urlToEdit");
    else refreshTable();

}

function addClick(el) {
    addOrUpdate(el, true, "#urlToAdd");
}

function onEditSuccess(data) {
    var response = JSON.parse(data);
    if (response.result) {
        $("#mainLayoutBottom div").html("");
        $("#mainLayoutBottom div").append("<span style='color: " + response.message.Color + "'>" + response.message.Text + "</span>");
        refreshTable();
    }
}

function refreshTable() {
    $.ajax({
        type: "POST",
        url: $("#urlToRefresh").val(),
        contentType: "html",
        processData: false,
        success: function(data) {
            if (data != null) {
                $("#managementTableContainer").html(data);
            } else
                alert("It was error during table refresh");
        },
        error: function() {
            alert("It was error during table refresh");
        }
    });
}

function addOrUpdate(el, isAdd, urlAction) {
    var product = [];
    $(el).closest("tr").find("input[type=text]").each(function (index) {
        product[index] = $(this).val();
    });

    var formData = new FormData();
    var jsonProduct = isAdd ?
        { Id: -1, Name: product[0], Category: product[1], Price: product[2] } :
        { Id: product[0], Name: product[1], Category: product[2], Price: product[3] };

    formData.append("jsonProduct", JSON.stringify(jsonProduct));

    $.ajax({
        type: "POST",
        url: $(urlAction).val(),
        data: formData,
        dataType: "json",
        contentType: false,
        processData: false,
        complete: function (data) {
            var response = JSON.parse(data.responseText);
            $("#mainLayoutBottom div").html("");
            $("#mainLayoutBottom div").append("<span style='color: " + response.message.Color + "'>" + response.message.Text + "</span>");

            refreshTable();
        }
    });
}
function onIsSearchChange() {
    var isSearch = document.getElementById('IsSearch');
    var searchPanel = document.getElementById('productCatalogSearchPanel');
    searchPanel.style.display = isSearch.checked ? "" : "none";
}

function onEditToOrderSuccess(result) {
    var resultEntity = JSON.parse(result);
    $("#count" + resultEntity.id).text(resultEntity.count);
}
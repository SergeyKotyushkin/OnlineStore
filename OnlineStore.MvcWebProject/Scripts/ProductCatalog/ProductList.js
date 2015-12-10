function onIsSearchChange() {
    var isSearch = document.getElementById('IsSearch');
    var searchPanel = document.getElementById('productCatalogSearchPanel');
    searchPanel.style.display = isSearch.checked ? "" : "none";
}
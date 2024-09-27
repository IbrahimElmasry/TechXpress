var dtble;
$(document).ready(function () {
    loaddata();
});


function loaddata() {
    dtble = $("#mytable").DataTable({
        "ajax": {
            "url": "/Admin/Product/GetData",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "name" },         // Matches 'name' from JSON
            { "data": "description" },  // Matches 'description' from JSON
            { "data": "price" },        // Matches 'price' from JSON
            { "data": "categoryName" }  // Matches 'categoryName' from JSON
        ]
    });
}

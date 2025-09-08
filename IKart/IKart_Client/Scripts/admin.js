// Basic admin JS helpers
$(function () {
    $('#sidebarToggle').click(function (e) {
        e.preventDefault();
        $('#admin-sidebar').toggleClass('show');
    });



    // Generic modal confirm (used by delete buttons etc.)
    window.confirmAction = function (message, callback) {
        if (confirm(message)) callback();
    };


    // DataTables auto-init for tables with id ending with Table
    $('table[id$="Table"]').each(function () {
        if (!$.fn.DataTable.isDataTable(this)) {
            $(this).DataTable();
        }
    });
});
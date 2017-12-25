angular.module('SCE', ['textAngular']);
$(document).ready(function () {
    var table = $('#table');
    if(table!=null){
        table.dataTable({
            bLengthChange: true,
            searching:true,
            oLanguage: {
                "sLengthMenu": "每页显示 _MENU_ 条记录",
                "sZeroRecords": "抱歉， 没有找到",
                "sInfo": "当页显示从 _START_ 项到 _END_ 项 - 共 _TOTAL_ 项数据",
                "sInfoEmpty": "没有数据",
                "sInfoFiltered": "(从 _MAX_ 条数据中检索)",
                "oPaginate": {
                    "sFirst": "首页",
                    "sPrevious": "前一页",
                    "sNext": "后一页",
                    "sLast": "尾页"
                },
                "sSearch":"搜索:"
            }
        });
    }
});
// EpgDataCap_Bon Reserve.js
function updateReserve() {
    //<a href="#!/recordinfo/1" class="list-group-item" data-toggle="collapse" data-target="#records .list-group-item-text:eq(0)">
    //<h4 class="list-group-item-heading">
    //    録画01
    //</h4>
    //<p class="list-group-item-text in">...</p>
    //</a>
    var i = 0;
    for (var x in reserveList) {
        var info = reserveList[x];
        var link = $("<a>")
            .attr("href", "javascript:void(0)")
            .addClass("list-group-item")
            .attr("data-toggle", "collapse")
            .attr("data-target", "#reserves .list-group-item-text:eq(" + i + ")");
        var h4 = $("<h4>")
            .addClass("list-group-item-heading")
            .text(info.EventName)
            .appendTo(link);
        if (i == 0) h4.append($("<span class='label label-info pull-right'>次の予約</span>"));
        var text = info.TextView;
        $("<p>")
            .addClass("list-group-item-text")
            .addClass(i == 0 ? "in" : "collapse")
            .append($("<a>")
                .addClass("btn btn-warning")
                .text("詳細")
                .attr("data-resID", x)
                .click(function (e) {
                    resProc(Number($(this).attr("data-resID")));
                    return false;
                })
                )
            .append($("<p>").html(text.split("\n").join("<br />")))
            .appendTo(link);
        $("#reserves .list-group").append(link);
        i++;
    }
}
function isReserve(ev) {
    for (var res in reserveList) {
        res = reserveList[res];
        if (res.TSID == ev.TSID && res.ONID == ev.ONID && res.SID == ev.SID && res.EventID == ev.EventID) return true;
    }
    return false;
}
function resProc(id) {
    console.log(reserveList[id].KeyS);
    $("#recChange").modal("show");
}
// JavaScript source code
var lastStart = 0;
var updated = false;
var oldScroll = 0;
function showOverline() {
    if (new Date().getMinutes() == 0 && !updated) {
        startDate += 3600;
        updated = true;
        reloadEpg();
        return;
    } else if(new Date().getMinutes() != 0){
        updated = false;
    }
    var o = getUnixTime(new Date()); o -= o % 3600;
    o += 3600;
    if (getUnixTime(new Date(startDate * 1000)) > o) {
        return;
    }
    var left = $("#epg #body #timeline").position().left;
    var top = $("#epg #body #timeline").position().top + (minSize * new Date().getMinutes());
    var elem = $("<div>")
        .attr("id", "overline")
        .append($("<div>").text(getDStr(new Date())))
        .width($("#epg").width())
        .offset({ top: top, left: left });
    $("#overline").parent().remove();
    elem.appendTo($("<li>").appendTo($("#epg #body")));
    setTimeout(showOverline, 1000 * 10);
    //console.log("Overline writed");
}
function updateEpgView(target, targetService, serviceList, serviceEvent) {
    console.time("updateEpg");
    var elem = $(target).empty();
    var srvButtons = $("<div>").addClass("btn-group");
    for (var p in SrvList) {
        var srv = $("<a>").addClass("btn btn-default");
        if (SrvList[p] == targetService) {
            srv.addClass("active");
        }
        srv.text(SrvList[p]).attr("data-service", SrvList[p]).click(function () {
            changeSrv($(this).attr("data-service"));
        }).appendTo(srvButtons);
        srv.appendTo(srvButtons);
    }
    elem.append(srvButtons);
    var dateButtons = $("<div>").addClass("btn-group");
    var epgMain = $("<div>").attr("id", "epg");
    var epgHeader = $("<div>").addClass("header").attr("id", "header").appendTo(epgMain);
    var epgBody = $("<div>").attr("id", "body").appendTo(epgMain);
    var timeline = $("<div>").attr("id", "timeline").addClass("list").appendTo(epgBody);
    var ListUL = $("<ul>").addClass("epg");
    var now = new Date(startDate * 1000);
    $("<div>").addClass("item time").text("TIME").appendTo(epgHeader);
    $("<div>").text(("00" + now.getHours()).substr(-2)).height(60 * minSize).addClass("item").appendTo(timeline);
    for (var i = 0; i < maxHours - 1; i++) {
        var t = now.getHours() + i + 1;
        if (t > 23) t %= 24;
        $("<div>").text(("00" + t).substr(-2)).height(60 * minSize).addClass("item").appendTo(timeline);
    }

    now = getUnixTime(now);
    var nowTmp = now;
    nowTmp -= nowTmp % 3600;
    var lastTime = nowTmp + (maxHours * 60 * 60);
    for (var svTmp in serviceList) {
        var sv = serviceList[svTmp];
        if (getSrvType(sv.ONID) != targetService) continue;
        $("<div>").text(sv.ServiceName).addClass("item").appendTo(epgHeader);
        var body = $("<div>").addClass("list").appendTo(epgBody);
        var oldTime = 0;
        if (serviceEvent[sv.Key] == undefined) {
            $("<div class='item no-epg' >")
                .css("height", maxHours * 60 * minSize)
                .text("EPGなし")
                .appendTo(body);
        }
        for (var evTmp in serviceEvent[sv.Key]) {
            var ev = serviceEvent[sv.Key][evTmp];
            var start = getUnixTime(new Date(ev.StartTime));
            var end = getUnixTime(new Date(ev.EndTime));
            if (oldTime + 1 < start && oldTime != 0) {
                var clSize = (start - oldTime) / 60;
                $("<div class='item no-epg'>")
                    .css("height", clSize * minSize)
                    .text("放送休止 EPGなし")
                    .appendTo(body);
                console.warn("EPG not found");
            }
            oldTime = end;
            if (start < nowTmp) {
                start = nowTmp;
            }
            if (end > lastTime) {
                end = lastTime;
            }

            var size = Math.floor(minSize * ((end - start) / 60));
            var title = ev.ShortInfo.event_name;
            if (size <= 0) continue;
            $("<div class='item' data-key='" + ev.EventID + "' data-dkey='" + ev.ServiceKeyS + "' style='background:" + getNibbleColor(ev) + ";' >" + getDStr(new Date(ev.StartTime)) + " " + title + "</div>")
                .attr({
                    "title": title,
                })
                .click(function () {
                    epgProc(Number($(this).attr("data-dkey")), Number($(this).attr("data-key")));
                })
                .css({
                    "height": size,
                    //"border": (isReserve(ev) ? "2px solid rgba(75, 255, 75, 0.8)" : "1px solid gray"),
                })
                .appendTo(body);
        }
    }
    elem.append(epgMain);
    $.each($("#epg #header"), function (i, val) {
        $(val).clone().css("left", $(val).offset().left).attr("id", "").addClass("header-copy header-fixed").appendTo($("#epg #header"));
    });
    
    var scrollFunc = function () {
        var nav = $("nav:eq(0)");
        var s = $(window).scrollTop();
        var ts = (oldScroll - $(window).scrollLeft());
        var t = $("#epg #header .item").first().offset().top + ($("#epg #header .item").first().height() / 2);
        if (s > t) {
            $("#epg #header .header-copy")
                .css("top", nav.height())
                .css("left", $(window).scrollLeft() + $("#epg #header *").first().offset().left).removeClass("hide");
        } else {
            $("#epg #header .header-copy").addClass("hide");
        }
    }
    $(window).scroll(scrollFunc);
    $(window).resize(scrollFunc);
    scrollFunc();
    $("[data-toggle=tooltip]").tooltip();
    $("[data-toggle=popover]").popover();

    showOverline();
    console.timeEnd("updateEpg");
}
function viewEpgDetails(ev) {
    var html = "<div class='modal fade'>" +
                  "<div class='modal-dialog'>" +
                    "<div class='modal-content epg-detail'>" +
                      "<div class='modal-header'>" +
                        "<button type='button' class='close' data-dismiss='modal' >" +
                            "<span aria-hidden='true'>" +
                            "&times;</span><span class='sr-only'>Close</span></button>" +
                        "<h4 class='modal-title'></h4>" +
                      "</div>" +
                      "<div class='modal-body'>" +
                        "<p></p>" +
                      "</div>" +
                      "<div class='modal-footer'>" +
                        "<button type='button' class='addres btn " + (!isReserve(ev) ? "btn-danger' >録画予約" : "btn-success' >予約変更") + "</button>" +
                        "<button type='button' class='btn btn-default' data-dismiss='modal'>閉じる</button>" +
                      "</div>" +
                    "</div>" +
                  "</div>" +
                  "</div>";
    var elem = $(html).appendTo($("body"));
    $(".addres", elem).click(function(){
        var tsid = $(".epg-detail", elem).attr("data-tsid");
        var sid = $(".epg-detail", elem).attr("data-sid");
        var onid = $(".epg-detail", elem).attr("data-onid");
        var eid = $(".epg-detail", elem).attr("data-eid");
        WebAPI.call("AddReserve", {tsid: tsid,sid:sid,onid:onid,eid:eid}, function(e){
            console.log(e);
            elem.modal("hide");
        });
    });
    $(".epg-detail", elem).attr({
        "data-tsid": ev.TSID,
        "data-onid": ev.ONID,
        "data-sid": ev.SID,
        "data-eid": ev.EventID
    });
    $("h4", elem).text(ev.ShortInfo.event_name);
    $("p", elem).html(ev.TextViewAll.split("\n").join("<br />"));
    elem.modal("show");
    $(".epg-detail").focus();
}
function epgProc(ServiceKey, EventID) {
    for (var i in eventList[ServiceKey]) {
        if (eventList[ServiceKey][i].EventID == EventID) {
            viewEpgDetails(eventList[ServiceKey][i]);
        }
    }
}
function reloadEpg() {
    var time = new Date(startDate * 1000);
    time.setMinutes(0, 10);
    WebAPI.call("EnumServiceEvent", { maxHour: String(maxHours), unixStart: getUnixTime(time) }, function (data) {
        for (var num in data) {
            if (serviceListEpg[data[num].Key] == undefined) continue;
            //console.log("add eventsvr: " + data[num].Key);
            eventList[data[num].Key] = data[num].Value;
        }
        updateEpgView($("#inner div").empty(), epgSrv, serviceListEpg, eventList);
        $("#loading").css("display", "none");
    });
    $("#loading").css("display", "block");
}
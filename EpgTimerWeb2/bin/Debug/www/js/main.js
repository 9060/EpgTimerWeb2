//EpgDataCap_Bon
var serviceList = {};
var serviceListEpg = {};
var reserveListEpg = {};
var eventList = {};
var contentKind = {};
var contentKind2 = {};
var componentDict = {};
var maxHours = 7;
var maxDay = 7;
var timeSize = 4; //Hour
var epgSrv = "地デジ";
var SrvList = ["地デジ", "BS", "CS", "Other"];
var EpgColor = {
    0: "#ffffe0",
    1: "#e0e0ff",
    2: "#ffe0f0",
    3: "#ffe0e0",
    4: "#e0ffe0",
    5: "#e0ffff",
    6: "#fff0e0",
    7: "#ffe0ff",
    8: "#ffffe0",
    9: "#fff0e0",
    10: "#e0f0ff",
    11: "#e0f0ff",
    15: "#f0f0f0"
};

var timeList = [];
var startDate = getUnixTime(new Date());// - (new Date().getTimezoneOffset() * 60);
var isNow = true;
var reserveList = {};
var recFileInfo = {};
var minSize = 4;

function getUnixTime(date) {
    return parseInt(date / 1000);
}
function getServiceKey(ONID, TSID, SID) {
    return ONID << 32 | TSID << 16 | SID;
}
function getSrvType(ONID) {
    var name = "Other";
    if (ONID == 0x0004) {
        name = "BS";
    }
    else if (ONID == 0x0006 || ONID == 0x0007) {
        name = "CS";
    }
    else if (0x7880 <= ONID && ONID <= 0x7FE8) {
        name = "地デジ";
    }
    return name;
}

function getNibbleStr(un1, un2, lv1, lv2) {
    var n1 = lv1, n2 = lv2;
    var res = "";
    if (n1 == 0x0E && n2 == 0x01) {
        n1 = un1;
        n2 = un2;
        var key1 = (n1 << 8 | 0xFF);
        var key2 = (n1 << 8 | n2);
        if (contentKind2[key1] != undefined) {
            res += contentKind2[key1].ContentName;
        }
        if (contentKind2[key2] != undefined) {
            res += " - " + contentKind2[key2].SubName;
        }
    } else {
        var key1 = (n1 << 8 | 0xFF);
        var key2 = (n1 << 8 | n2);
        if (contentKind[key1] != undefined) {
            res += contentKind[key1].ContentName;
        }
        if (contentKind[key2] != undefined) {
            res += " - " + contentKind[key2].SubName;
        }
    }
    return res;
}
function getNibbleColor(epg) {
    if (!epg.ContentInfo) return "#fff";
    var nl = epg.ContentInfo.nibbleList[0];
    var n1 = nl.content_nibble_level_1;
    return (EpgColor[n1] || "#fff");
}
function getDStr(d) {
    return ("00" + d.getHours()).substr(-2) + ":" + ("00" + d.getMinutes()).substr(-2)
}
function getUDStr(d) {
    return ("00" + d.getUTCHours()).substr(-2) + ":" + ("00" + d.getUTCMinutes()).substr(-2)
}
function getDayStr(d) {
    return ("00" + (d.getMonth() + 1)).substr(-2) + "/" + ("00" + (d.getDate())).substr(-2)
        + " " + ("00" + d.getHours()).substr(-2) + ":" + "00";
}


function changeSrv(srv) {
    epgSrv = srv;
    updateEpgView($("#inner div").empty(), epgSrv, serviceListEpg, eventList);
}

function disconnect() {
    console.error("error: disconnected");
    $("#overline").parent().remove();
    $("#loading").css("display", "none");
    var html = "<div class='modal fade'>" +
                  "<div class='modal-dialog'>" +
                    "<div class='modal-content'>" +
                      "<div class='modal-header'>" +
                        "<h4 class='modal-title'>切断されました。</h4>" +
                      "</div>" +
                      "<div class='modal-body'>" +
                        "<p>切断されました。再接続しますか？</p>" +
                      "</div>" +
                      "<div class='modal-footer'>" +
                        "<button type='button' class='btn btn-default' data-dismiss='modal'>いいえ</button>" +
                        "<button type='button' class='btn btn-primary'>はい</button>" +
                      "</div>" +
                    "</div>" +
                  "</div>" +
                  "</div>";
    var elem = $(html).appendTo($("body"));
    $("button.btn-primary").click(function () {
        connect();
        elem.modal('hide');
    })
    elem.modal({
        backdrop: 'static',
        keyboard: false
    });

}
function login() {
    $("#loading").css("display", "none");
    $("#formLogin").modal('hide');
    $("#formLogin").modal();
    $("#formLogin input").first().focus();
    $("#authsubmit").click(function () {
        if ($("#sess").hasClass("active")) {
            WebAPI.login($("#sess_code").val());
        } else {
            WebAPI.login($("#username").val(), $("#password").val());
        }
        $("#sess_code").val("");
        $("#username").val("");
        $("#password").val("");
    });
}
var sessKey = "";
function connect() {
    $("#loading").css("display", "block");
    WebAPI.onboot = function (e) {
        $("#formLogin").modal("hide");
        init();
    }
    WebAPI.onreqauth = function () {
        login();
    }
    WebAPI.onclose = function (e) {
        disconnect();
    }
    WebAPI.open();
}
function updateRec() {
    //<a href="#!/recordinfo/1" class="list-group-item" data-toggle="collapse" data-target="#records .list-group-item-text:eq(0)">
    //<h4 class="list-group-item-heading">
    //    録画01
    //</h4>
    //<p class="list-group-item-text in">...</p>
    //</a>
    var i = 0;
    for (var x in recFileInfo) {
        var info = recFileInfo[x];
        var link = $("<a>")
            .attr("href", "javascript:void(0)")
            .addClass("list-group-item")
            .attr("data-toggle", "collapse")
            .attr("data-target", "#records .list-group-item-text:eq(" + i + ")");
        $("<h4>")
            .addClass("list-group-item-heading")
            .text(info.EventName)
            .appendTo(link);
        var text = info.ProgramInfo.length > 8 ? (function () {
            var p = info.TextView.split("\n");
            p.splice(0, 4);
            return p.join("\n");
        })() : info.TextView;
        var prog = info.ProgramInfo.length > 90 ? info.ProgramInfo.substring(0, 90) + "...\n" : info.ProgramInfo;
        var details = $("<p class='list-group-item-text'>")
            .addClass(i == 0 ? "in" : "collapse")
            .append($("<a class='btn btn-success'>")
                .text("詳細")
                .attr("data-recID", x)
                .click(function () {
                    recProc(Number($(this).attr("data-recID")));
                    return false;
                })
                )
            .append($("<p>").html((prog + text).split("\n").join("<br />")))
            .appendTo(link);
        $("#records .list-group").append(link);
        i++;
    }
}

function recProc(id) {
    console.log(recFileInfo[id].KeyS);
}

function init() {
    WebAPI.call("GetContentColorTable", {}, function(data){
        EpgColor = {};
        for(var i in data){
            EpgColor[data[i].ContentLevel1] = data[i].Color;
        }
    });
    WebAPI.call("GetCommonManager", {}, function (data) {
        for (var x in data.ContentKindDictionary) {
            contentKind[data.ContentKindDictionary[x].Key] = data.ContentKindDictionary[x].Value;
        }
        for (var x in data.ContentKindDictionary2) {
            contentKind2[data.ContentKindDictionary2[x].Key] = data.ContentKindDictionary2[x].Value;
        }
        for (var x in data.ComponentKindDictionary) {
            componentDict[data.ComponentKindDictionary[x].Key] = data.ComponentKindDictionary[x].Value;
        }
    });
    WebAPI.call("EnumService", {}, function (data) {
        for (var num in data) {
            //console.log("add service: " + data[num].ServiceName);
            serviceList[data[num].Key] = data[num];
            if (data[num]["EpgCapFlag"] != 0) {
                serviceListEpg[data[num].Key] = data[num];
                //console.log("add epg: " + data[num].ServiceName)
            }
        }
    });
    WebAPI.call("EnumReserve", {}, function (data) {
        for (var x in data) {
            reserveListEpg[data[x].Key] = 1;
            reserveList[x] = data[x];
        }

        var time = new Date(startDate * 1000);
        time.setMinutes(0, 10);
    });
    var time = new Date(startDate * 1000);
    time.setMinutes(0, 10);
    WebAPI.call("EnumServiceEvent", { maxHour: String(maxHours), unixStart: getUnixTime(time) }, function (data) {
        for (var num in data) {
            if (serviceListEpg[data[num].Key] == undefined) continue;
            //console.log("add eventsvr: " + data[num].Key);
            eventList[data[num].Key] = data[num].Value;
        }
    });

    WebAPI.call("EnumRecFileInfo", {}, function (data) {
        for (var x in data) {
            recFileInfo[data[x].ID] = data[x];
        }
        updateRec();
        updateReserve();

        updateEpgView($("#inner div").empty(), epgSrv, serviceListEpg, eventList);

        $("#loading").css("display", "none");
    });
}

$(document).ready(function () {
    $("#loading").css("display", "block");
    console.log("--- started ---");
    connect();
    $("button[data-toggle='tooltip']").tooltip();
    $("button[data-target='#infoInner']").click(function () {
        $("#infoInner").collapse($("#inner").hasClass("col-sm-12") ? "show" : "hide");
        if ($("#inner").hasClass("col-sm-12")) {
            $("#inner").removeClass("col-sm-12").addClass("col-sm-8");
            $("button[data-target='#infoInner'] span").removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
            return;
        }
        $("#inner").removeClass("col-sm-8").addClass("col-sm-12");
        $("button[data-target='#infoInner'] span").removeClass("glyphicon-chevron-up").addClass("glyphicon-chevron-down");
    });
    $("a[href='#/epg']").click(function () {
        $("#infoInner").collapse("hide");
        $("#inner").removeClass("col-sm-8").addClass("col-sm-12");
        $("button[data-target='#infoInner'] span").removeClass("glyphicon-chevron-up").addClass("glyphicon-chevron-down");
    });
    $("#refresh").click(function () {
        reloadEpg();
    });
    $("#epgcap").click(function () {
        WebAPI.call("EpgCapNow", {}, function () { });
    });
    $("#epgreload").click(function () {
        WebAPI.call("EpgReload", {}, function () { reloadEpg(); });
    });
    $("#searchBtn").click(function () {
        var text = $("#searchForm input").first().val();

    });
    $("a[href='#/about']").click(function () {
        var html = "<div class='modal fade'>" +
                        "<div class='modal-dialog'>" +
                        "<div class='modal-content'>" +
                            "<div class='modal-header'>" +
                            "<h4 class='modal-title'>このソフトについて</h4>" +
                            "</div>" +
                            "<div class='modal-body'>" +
                            "<p>開発: yuki (Trip:0X7hT.k8kU <a href='http://yuki.0am.jp/'>Web</a>)</p>" +
                            "<p>このソフトのライセンスは、GNU General Public Licenseとします</p>" +
                            "<p>バグ報告 => 2ch「【開発】 TS関連ソフトウェア総合スレ」へ</p>" +
                            "</div>" +
                            "<div class='modal-footer'>" +
                            "<button type='button' class='btn btn-default' data-dismiss='modal'>閉じる</button>" +
                            "</div>" +
                        "</div>" +
                        "</div>" +
                        "</div>";
        var elem = $(html).appendTo($("body"));
        elem.modal({
            backdrop: 'static',
            keyboard: false
        });
        return false;
    });
});

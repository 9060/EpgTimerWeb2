//EpgDataCap_Bon
$(document).ready(function () {
    $("button[data-toggle='tooltip']").tooltip();
    $("button[data-target='#infoInner']").click(function () {
        $("#infoInner").collapse($("#epgInner").hasClass("col-sm-12") ? "show" : "hide");
        if ($("#epgInner").hasClass("col-sm-12")) {
            $("#epgInner").removeClass("col-sm-12").addClass("col-sm-8");
            $("button[data-target='#infoInner'] span").removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-up");
            return;
        }
        $("#epgInner").removeClass("col-sm-8").addClass("col-sm-12");
        $("button[data-target='#infoInner'] span").removeClass("glyphicon-chevron-up").addClass("glyphicon-chevron-down");
    });
    $("a[data-target='/epg']").click(function () {
        $("#infoInner").collapse("hide");
        $("#epgInner").removeClass("col-sm-8").addClass("col-sm-12");
        $("button[data-target='#infoInner'] span").removeClass("glyphicon-chevron-up").addClass("glyphicon-chevron-down");
    });
    $.each($("#epgInner table"), function (i, val) {
        var target = val;
        var fixed = false;
        var head = $("thead.header", target);
        var headTop = head.offset().top - 50 + (head.height() / 2);
        var scrollFunc = function () {
            var i, scrollTop = $(window).scrollTop();
            var t = head.offset().top - 50 + (head.height() / 2);
            if (headTop != t) { headTop = t; }
            if (scrollTop >= headTop) { fixed = 1; }
            else if (scrollTop <= headTop) { fixed = 0; }
            if(fixed){
                $('thead.header-copy', target).removeClass('hide');
                $("thead.header-copy", target).offset(
                    { left: head.offset().left }
                );
            }else{
                $('thead.header-copy', target).addClass('hide');
            }
        }
        var copy = head.clone().removeClass('header').addClass('header-copy header-fixed');
        $("tr > th", copy).width($("tr:first > th", head).innerWidth()).css("max-height",$("tr:first > th", head).innerHeight());
        copy.appendTo(target);
        $(window).on("scroll", scrollFunc);
        scrollFunc();
    });
});
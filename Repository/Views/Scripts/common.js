﻿//d3 formatting
String.prototype.replaceAll = function (find, replace) {
    var str = this;
    return str.replace(new RegExp(find.replace(/[-\/\\^$*+?.()|[\]{}]/g, '\\$&'), 'g'), replace);
};
String.prototype.valueFormat = function (value) {
    var str = this;
    return str.replaceAll(',', thousandSeparator).replaceAll('$', currencySymbol).replace('.', decimalSeparator);
};
String.prototype.normalizeNumeric = function (valueStr) {
    return parseFloat(this.replaceAll(',', '').replace(/\s+/g, ''));
};


function restrictionSelectChange(source) {
    var idSelect = "#" + $(source).attr('id');
    if ($(source).attr('opid') != null) {
        idSelect = "#" + $(source).attr('opid');
    }
    var idValue = idSelect.replace("Operator", "Value");

    var op = $(idSelect).val().toLowerCase();
    var display1 = "inline", display2 = "inline", display3 = "inline", display4 = "inline";
    if (op == 'isnull' || op == 'isnotnull') {
        display1 = "none", display2 = "none"; display3 = "none"; display4 = "none";
    }
    else if (op == 'greater' || op == 'greaterequal' || op == 'smaller' || op == 'smallerequal' || op == 'valueonly') {
        display2 = "none"; display3 = "none"; display4 = "none";
    }
    else if (op == 'between' || op == 'notbetween' || op == 'valueonly') {
        display3 = "none"; display4 = "none";
    }
    else {
        if ($(idValue + "_3").val() == "" && $(idValue + "_4").val() == "") display4 = "none";
        if ($(idValue + "_2").val() == "" && display4 == "none") display3 = "none";
        if ($(idValue + "_1").val() == "" && display3 == "none" && display4 == "none") display2 = "none";
    }

    if ($(idValue + "_1").parent().hasClass("date")) { //date
        $(idValue + "_1").parent().parent().css("display", display1);
        $(idValue + "_2").parent().parent().css("display", display2);
        $(idValue + "_3").parent().parent().css("display", display3);
        $(idValue + "_4").parent().parent().css("display", display4);
    }
    else { //numeric or text
        $(idValue + "_1").css("display", display1);
        $(idValue + "_2").css("display", display2);
        $(idValue + "_3").css("display", display3);
        $(idValue + "_4").css("display", display4);
    }
    /*
    var restr = "";
    $(".operator_select").each(function (key, value) {
        var idSel = $(value).attr("id");
        if (idSel) {
            var idVal = "#" + idSel.replace("Operator", "Value");
            var val = "";
            if ($(idVal + "_1").val()) val += $(idVal + "_1").val() + ";";
            if ($(idVal + "_2").val()) val += $(idVal + "_2").val() + ";";
            if ($(idVal + "_3").val()) val += $(idVal + "_3").val() + ";";
            if ($(idVal + "_4").val()) val += $(idVal + "_4").val() + ";";

            if (val == "") { //Enum
                idVal = idSel.replace("Operator", "Option_Value");
                if ($("#" + idVal).length) {
                    var selText = $("button[data-id=" + idVal + "]").attr("title");
                    if (selText != "") val = selText + ";";
                }
            }
            if (val) restr += $("#" + idSel + " option:selected").text() + " " + val + " " + "\r\n";
        }
    });*/
 //   $("#restrictions_button").attr('title', restr).tooltip();
}

function initNavMenu() {
    var $menu = $('#nav_menu');
    if (!$menu.length) {
        $menu = $("<ul id='nav_menu' class='dropdown-menu' role='menu'/>");
        $("body").append($menu);
        $menu
            .mouseenter(function () {
                $menu.show();
            })
            .mouseleave(function () {
                $menu.hide();
            });
    }
}

function scrollMessages() {
    var $messages = $("#execution_messages");
    if ($messages) {
        setTimeout(function () { $messages.scrollTop($messages[0].scrollHeight); }, 200);
    }
}

function showNavMenu() {
    $("#nav_menu")
        .show()
        .css({
            position: "absolute",
            "z-index": "1040",
            left: $("#nav_button").offset().left,
            top: $("#nav_button").offset().top + 2 * $("#nav_button").height()
        });
}

function showPopupNavMenu(source, content) {
    var $popup = $('#nav_popupmenu');
    if (!$popup.length) {
        $popup = $("<ul id='nav_popupmenu' class='dropdown-menu' role='menu'/>");
        $("body").append($popup);
        $popup
            .mouseenter(function () {
                $popup.show();
            })
            .mouseleave(function () {
                $popup.hide();
            });
    }
    $popup.html(content);
    $('#nav_popupmenu li').click(function (e) {
        executeReport($(this).attr("nav"));
        $popup.hide();
    });
    $popup
        .show()
        .css({
            position: "absolute",
            left: source.offset().left,
            top: source.offset().top + source.height() + 3
        });
}


function initNavCells() {
    $(".cell_value").mouseenter(function (e) {
        if ($(this).attr("navigation")) {
            showPopupNavMenu($(this), $(this).attr("navigation"));
        }
    });

    $(".cell_value").mouseleave(function () {
        $("#nav_popupmenu").hide();
    });
}

function executeTimer() {
    if (executionTimer != null) {
        var $messages = $("#execution_messages");
        var $form = $("#header_form");
        $form.attr("action", urlPrefix + "ActionRefreshReport");
        if (urlPrefix != "") {
            $.post(urlPrefix + "ActionRefreshReport", { execution_guid: $("#execution_guid").val() })
                .done(function (data) {
                    if (data.result_ready) {
                        clearInterval(executionTimer);
                        if ($("#execution_guid").val() != null) {
                            $form.attr("action", urlPrefix + "Result");
                            $form.submit();
                        }
                    }
                    else if (data.progression_message != null) {
                        setProgressBarMessage("#progress_bar", data.progression, data.progression_message, "progress-bar-success");
                        setProgressBarMessage("#progress_bar_tasks", data.progression_tasks, data.progression_tasks_message, "progress-bar-primary");
                        setProgressBarMessage("#progress_bar_models", data.progression_models, data.progression_models_message, "progress-bar-info");
                        if (data.execution_messages != null && $("#execution_messages").length) {
                            $messages.removeClass('hidden');
                            $messages.html(data.execution_messages);
                            scrollMessages();
                        }
                    }
                    else if (data.error != null) {
                        setProgressBarMessage("#progress_bar", 100, data.error, "progress-bar-error");
                        clearInterval(executionTimer);
                        $("#execute_button").css("display", "none");
                    }
                });
        }
        else {
            $messages.removeClass('hidden');
            $form.submit();
        }
    }
}


function setProgressBarMessage(selector, progression, message, classname) {
    $(selector).css('width', progression + '%').css('min-width', '120px').attr('aria-valuenow', progression);
    $(selector).html(message);
    $(selector).removeClass("progress-bar-error").removeClass("progress-bar-warning").removeClass("progress-bar-success").removeClass("progress-bar-primary");
    $(selector).addClass(classname);
}

function executeReport(nav) {
    if (refreshTimer) clearInterval(refreshTimer);

    var url = "";
    if (executionTimer == null) {
        $("#information_div").html("");

        $("#execution_messages").addClass('hidden');
        $("#execution_messages").html("");

        $("#alert_status").addClass('hidden');
        $("#alert_status").html("");

        $("#execute_button").text(cancelText);
        $("#execute_button").removeClass("btn-success").addClass("btn-warning");

        $("#progress_panel").removeClass('hidden');
        $(".progress").removeClass('hidden');
        setProgressBarMessage("#progress_bar", 5, startingExecText, "progress-bar-success");

        url = urlPrefix + (nav == null ? "ActionExecuteReport" : "ActionNavigate");
        if (nav == null || urlPrefix == "") executionTimer = setInterval(function () { executeTimer() }, 1200);
    }
    else {
        url = urlPrefix + "ActionCancelReport";
    }

    $("#navigation_id").val(nav);
    $("#header_form").attr("target", "");
    if (urlPrefix != "") {
        $.post(url, $("#header_form").serialize()).done(function (data) {
            if (nav != null) $('body').html(data);
        });
    }
    else {
        $("#header_form").attr("action", url);
        $("#header_form").submit();
    }
    //disable controls during execution
    $('#restrictions_div input').prop("disabled", true);
    $('#restrictions_div select').attr("disabled", true);
    $('#restrictions_div select').selectpicker('refresh');
    $('.view').css("display", "none");
    $("#nav_button").attr("disabled", "disabled");
}

function submitViewParameter(viewId, parameterName, parameterValue) {
    if (generateHTMLDisplay) {
        if (urlPrefix != "") {
            $.post(urlPrefix + "ActionUpdateViewParameter", { execution_guid: $("#execution_guid").val(), parameter_view_id: viewId, parameter_view_name: parameterName, parameter_view_value: parameterValue });
        }
        else {
            $("#parameter_view_id").val(viewId);
            $("#parameter_view_name").val(parameterName);
            $("#parameter_view_value").val(parameterValue);
            $("#header_form").attr("action", "ActionUpdateViewParameter");
            $("#header_form").submit();
        }
    }
}


function getTableData(datatable, guid, viewid, pageid, data, callback, settings) {
    try {
        var params = data.draw + "§" + settings.aaSorting + "§" + settings.oPreviousSearch.sSearch + "§" + settings._iDisplayLength + "§" + settings._iDisplayStart
        if (urlPrefix != "") {
            $.post(urlPrefix + "ActionGetTableData", { execution_guid: guid, viewid: viewid, pageid: pageid, parameters: params })
                .done(function (data) {
                    try {
                        var json = jQuery.parseJSON(data);
                        callback(json);
                        initNavCells();
                    }
                    catch (ex) {
                        datatable[0].innerHTML = "Error loading data..." + "<br>" + ex.message;
                    }
                });
        }
        else {
            $("#header_form").attr("action", "ActionGetTableData");
            $("#parameter_tableload").html(params);
            $("#viewid_tableload").val(viewid);
            $("#pageid_tableload").val(pageid);
            $("#header_form").submit();
            var json = jQuery.parseJSON($("#parameter_tableload").text());
            callback(json);
            $("#parameter_tableload").html("");
            initNavCells();
        }
    }
    catch (ex2) {
        datatable[0].innerHTML = "Error loading data..." + "<br>" + ex2.message;
    }
}

function resize()
{
    if (!printLayout) $("body").css("padding-top", $("#bar_top").height() + 15);
}


function mainInit() {
    //force execute
    $("input").keydown(function (event) {
        if (event.keyCode == 13) $("#execute_button").focus()
    });

    $("#execute_button").click(function () {
        executeReport();
    });

    $("#restrictions_button").click(function () {
        submitViewParameter(rootViewId, "restriction_button", !$("#restrictions_div").hasClass("in"));
        $("#restrictions_button").toggleClass("active");
        //Collapse navbar
        if ($('.navbar-toggle').css('display') != 'none') $('.navbar-toggle').click();
    });

    //print layout
    if (printLayout) {
        $("nav").removeClass("navbar-fixed-top");
        $("body").css("padding-top", "0px");
    }

    //tabs buttons
    $(".sr_tab").click(function () {
        submitViewParameter(rootViewId, "information_button", false);
        if ($("#message_button").length) {
            submitViewParameter(rootViewId, "message_button", false);
            scrollMessages();
        }
        submitViewParameter(rootViewId, $(this).attr("id"), true);

        //Collapse navbar
        if ($('.navbar-toggle').css('display') != 'none') $('.navbar-toggle').click();
    });

    if (showInformation) $('[href="#information_div"]').tab('show');
    else if (showMessage) $('[href="#message_div"]').tab('show');
    else $('[href="#' + rootViewId + '"]').tab('show');

    //result links
    $(".sr_result").click(function () {
        $("#header_form").attr("target", urlPrefix != "" ? "_blank" : "");
        $("#header_form").attr("action", urlPrefix + $(this).attr("id"));
        $("#header_form").submit();
        //Collapse navbar
        if ($('.navbar-toggle').css('display') != 'none') $('.navbar-toggle').click();
    });

    //operator change
    $(".form-control").change(function () {
        restrictionSelectChange(this);
    }).change();

    $(".form-control").keyup(function () {
        restrictionSelectChange(this);
    });

    //validation
    $(".numeric_input").keyup(function () {
        var v = this.value;
        if (!$.isNumeric(v)) {
            this.value = this.value.slice(0, -1);
        }
    });

    //navigation
    if (hasNavigation) {
        $("#nav_button")
            .mouseenter(function () {
                if (urlPrefix != "") {
                    $.post(urlPrefix + "ActionGetNavigationLinks", { execution_guid: $("#execution_guid").val() })
                        .done(function (data) {
                            if (data.links != null && data.links != "") {
                                initNavMenu();
                                $("#nav_menu").html(data.links);
                                showNavMenu();
                            }
                        });
                }
                else {
                    $("#header_form").attr("action", "ActionGetNavigationLinks");
                    initNavMenu();
                    $("#header_form").submit();
                    showNavMenu();
                }
            })
            .mouseleave(function () {
                $("#nav_menu").hide();
            });

        $("#nav_badge").removeClass("hidden");
    }
}


$(document).ready(function () {
    mainInit();

    if ((forceExecution || !hasRestrictions) && !isExecuting && !isCancel) executeReport();

    //Select Picker
    $(".operator_select").selectpicker('refresh');
    $(".enum").selectpicker({
        "liveSearch": true,
        "actionsBox": true
    });

    //Date Picker
    $(".datepicker_datetime").datetimepicker({
        showClose: true,
        showClear: true,
        format: shortDateTimeFormat,
    });

    $(".datepicker_date").datetimepicker({
        showClose: true,
        showClear: true,
        format: shortDateFormat
    });

    $('.datepicker_date,.datepicker_datetime').datetimepicker({
        locale: languageName
    });

    //resize handler
    $(window).on('resize', function () {
        resize();
    });
    resize();
    
    if (!executionTimer && refreshRate > 0) refreshTimer = setInterval(executeReport, refreshRate * 1000);

    //back to top
    $(window).scroll(function () {
        if ($(this).scrollTop() > 50) $('#back-to-top').fadeIn();
        else $('#back-to-top').fadeOut();
    });
    // scroll body to 0px on click
    $('#back-to-top').click(function () {
        $('#back-to-top').tooltip('hide');
        $('body,html').animate({
            scrollTop: 0
        }, 800);
        return false;
    });
    $('#back-to-top').tooltip('show');

    scrollMessages();
});

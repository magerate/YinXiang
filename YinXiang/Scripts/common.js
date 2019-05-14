var domin = "http://" + document.location.host;
$.ajaxSetup({ cache: false });
jQuery.cookie = function (name, value, options) {
    if (typeof value != 'undefined') {
        var cookieValue = null;
        var tempKey = "";
        if (document.cookie && document.cookie != '') {
            var cookies = document.cookie.split(';');
            for (var iI = 0; iI < cookies.length; iI++) {
                var cookie = jQuery.trim(cookies[iI]);
                if (cookie.indexOf(domin + "=") == 0) { tempKey = cookie.substring((domin + "=").length); break; };
            }
        };
        var teveKeyArr = new Array();
        if (tempKey != "") { teveKeyArr = tempKey.split('&'); };
        var isExist = false;
        for (var ki = 0; ki < teveKeyArr.length; ki++) {
            var cookie = teveKeyArr[ki];
            if (cookie.substring(0, name.length + 1) == (name + '=')) {
                teveKeyArr[ki] = (name + '=') + encodeURIComponent(value); isExist = true;
            };
        };
        if (!isExist) {
            teveKeyArr[teveKeyArr.length] = (name + '=') + encodeURIComponent(value);
        };
        var tempValue = teveKeyArr.join("&");
        var tempS = [domin, '=', tempValue, '', '', '', ''].join('');

        var date = new Date();
        date.setFullYear(date.getFullYear() + 10);
        document.cookie = [domin + '=' + tempValue, ";expires=" + date.toGMTString(), ';path=/'].join('');
    }
    else {
        var cookieValue = null;
        if (!document.cookie && document.cookie == '') {
            return "";
        }
        var tempKey = "";
        var cookies = document.cookie.split(';');
        for (var iI = 0; iI < cookies.length; iI++) {
            var cookie = jQuery.trim(cookies[iI]);
            if (cookie.indexOf(domin + "=") == 0) {
                tempKey = cookie.substring((domin + "=").length);
                break;
            }
        }
        var teveKeyArr = tempKey.split('&');
        for (var ki = 0; ki < teveKeyArr.length; ki++) {
            var cookie = teveKeyArr[ki];
            if (cookie.substring(0, name.length + 1) == (name + '=')) {
                return decodeURIComponent(cookie.substring(name.length + 1));
            };
        };
        return "";
    };
};
(function ($) {
    $.fn.trimSerialize = function (selector) {
        var clonedObject = $(this);
        selector = (selector === undefined) ? 'input' : selector;
        clonedObject.find(selector).each(function () {
            $(this).val($.trim($(this).val()));
        });
        return clonedObject.serialize();
    };
})(jQuery);
function Post(url, fn, frmId, data) {
    var tempData = null;
    if (frmId != "") {
        tempData = $("#" + frmId).trimSerialize(); 
    }
    else {
        tempData = data;
    }
    $.post(url, tempData, fn);
};
function Get(url, fn) {
    $.get(url, function (data) {
        if (fn) { fn(data); };
    });
};
function boostrapDialog(title, tWidth, tHeight, url, ctId, btId, objId, closefn, isFull) {
    var dWidth, dHeight, dialogId;
    var winWidth = $(window).width();
    var winHeight = $(window).height();
    btId = btId == "" ? null : btId;
    btns = btId ? btId : "";
    ctId = ctId ? ctId : "";
    dialogId = objId ? objId : "muShowDialog";
    if (isFull) {
        dWidth = winWidth;
        dHeight = winHeight;
    }
    else {
        dWidth = (tWidth == null || tWidth == "") ? winWidth : (tWidth > winWidth ? winWidth : tWidth);
        dHeight = (tHeight == null || tHeight == "") ? winHeight : (tHeight > winHeight ? winHeight : tHeight);
    };
    var modalBodyHeight = dHeight - 72;
    if ($("#" + dialogId).length == 0) {
        var modalDiv = "<div id='" + dialogId + "' class='modal fade bd-example-modal-lg' tabindex='-1'"
            + " role='dialog' aria-labelledby='myLargeModalLabel' aria-hidden='true'>"
            + "<div class='modal-dialog' style='width:" + dWidth + "px;height:" + dHeight + "px;max-width:" + dWidth + "px;max-height:" + dHeight + "px;'>"
            + " <div class='modal-content'  style='height:100%;'>"
            + "  <div class='modal-header'>"
            + "   <h5 class='modal-title'>" + title + "</h5>"
            + "   <button type='button' class='close' data-dismiss='modal' aria-label='Close'><span aria-hidden='true'>&times;</span></button>"
            + "  </div>"
            + "<div class='modal-body' style='width:100%;height:" + modalBodyHeight + "px;overflow-y: auto;overflow-x: hidden;'></div>"
            + "<div class='modal-footer'></div>"
            + "</div>";
        $(document.body).append(modalDiv);
    };
    var DivObj = $("#" + dialogId);
    var isClose = false;
    DivObj.addClass("scroll-y");
    DivObj.on('hidden.bs.modal', function (e) {
        DivObj.remove();
        if (closefn) { closefn(); }
        isClose = true;
    })
    Get(url, function (data) {
        if (!isClose) {
            var dialogHeader = DivObj.find('.modal-header:first');
            if (data.indexOf("modal-body") > 0) {
                DivObj.find('.modal-content').html(dialogHeader[0].outerHTML + data);
            }
            else {
                DivObj.find('.modal-body').html(data);
            }
            DivObj.find('.modal-body').addClass('modalBodyClass');
            DivObj.find('.modal-body').css({ 'width': '100%', 'height': modalBodyHeight + 'px', 'overflow-y': 'auto', 'overflow-x': 'hidden' });
            DivObj.modal('show');
            if (DivObj.find(".datagrid").length == 0 || DivObj.find('div:first').height() < DivObj.height()) {
                DivObj.removeClass("scroll-y");
            }
        }
    });
};
function boostrapAlert(alertShowToId, msg, alertClass) {
    $('#' + alertShowToId).html('<div class="alert ' + alertClass + ' alert-dismissible fade show" role="alert">'
        + msg
        + '  <button type="button" class="close" data-dismiss="alert" aria-label="Close">'
        + '    <span aria-hidden="true">&times;</span>'
        + '  </button>'
        + '</div>');
};
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
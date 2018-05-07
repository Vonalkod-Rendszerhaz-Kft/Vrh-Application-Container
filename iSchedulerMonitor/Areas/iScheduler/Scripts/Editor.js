// Az Editor.js számára szükséges konstansok, melyek a cshtml-ben kapnak értéket
// Azért a prototype, hogy gépelés közben legyen intellisense
function ImportVariables() {
    // Üzenetek és labelek
    this.WAIT_MESSAGE = '';
    this.CONFIRM_HEADER = '';
    this.CONFIRM_DELETE = '';
    this.CONFIRM_FROMSERIES = '';
    this.CONFIRM_DELETESERIES = '';
    this.BUTTON_LABEL_YES = '';
    this.BUTTON_LABEL_NO = '';
    this.NUMBER_OF_LABEL = '';
    this.EXECUTE_WAIT_MESSAGE = '';

    // Url-ek
    this.URL_DELETE = '';
    this.URL_FOR_OBJECTID = '';
    this.URL_FOR_OPERATIONID = '';
    this.URL_OBJECTEDITOR = '';
    this.URL_SHOWSTATE = '';

    // Egyéb állandók
    this.EDITOR_DIALOG_ID = '';
    this.EDITOR_FORM_ID = '';
    this.VARIABLE_OBJECTID = '';
    this.VARIABLE_GROUPID = '';
    this.CONNECTION_STRING = '';

    this.VIEWMODE_LIST = '';
    this.VIEWMODE_CALENDAR = '';
}
var ivSch = new ImportVariables();    //!!! ennek az objektumnak kell értéket adni a cshtml-ben !!!

function EditorSelectors() {
    this.$EditorBootboxId = $('#' + ivSch.EDITOR_DIALOG_ID);
    this.$EditorFormId = $('#' + ivSch.EDITOR_FORM_ID);
    this.$ObjectIdValue = $('#ObjectIdValue');
    this.$ObjectGroupId = $('#ObjectGroupId');
    this.$inputNumberOf = $('input[name=NumberOf]');
    this.$labelNumberOf = $('label[for=NumberOf]');
}
var selEditor;


/*##### EVENTS #####*/

$(document).ready(function () {
    'use strict'
        
    console.log('iScheduler.Editor.document.ready: PING');

    EditorConstantsInit(); //AutoCompInit előtt kell lennie !!!
    selEditor = new EditorSelectors();
    console.log('iScheduler.Editor.document.ready: ivSch selEditor', ivSch, selEditor);

    //Autocomplete inicializálás
    AutoCompInit('ObjectIdText', 'ObjectIdValue', ivSch.URL_FOR_OBJECTID);
    AutoCompInit('OperationIdText', 'OperationIdValue', ivSch.URL_FOR_OPERATIONID);

    $(document).find('.editor-htmlattributes').children(':first-child').addClass('form-control');

    var $schType = $('input[name=ScheduleType]:checked');
    var $preType = $('#PreviousScheduleType');
    var stype = $schType.val();
    var ptype = $preType.val();
    //console.log('iScheduler.Editor.document.ready: ScheduleType, PreviousScheduleType', stype, ptype);
    $preType.val(stype);

    numberOfChange();

    $('input[type=radio][name=ScheduleType]').change(function () {
        console.log('ScheduleType change event occured.');
        $('#' + ivSch.EDITOR_FORM_ID).submit();
    });

    selEditor.$EditorFormId.on("submit", function (e) {
        //e.preventDefault(); // cancel the actual submit
        var checked = document.getElementById('IsScheduleExecute').checked;
        console.log("checked", checked);
        if (checked) {
            console.log("Ott a pipa!");
            var html = '<td colspan="3" style="text-align:center;"><h3><i class="glyphicon glyphicon-cog spin-right"></i> ' + ivSch.EXECUTE_WAIT_MESSAGE + '<h3></td>'
            document.getElementById('buttoncontainer').innerHTML = html;
        }
        //return false; //nem folytatja a submit-ot
    });
})// $(document).ready event VÉGE

/*##### EVENTS END #####*/


/*##### METHODS #####*/
function numberOfChange() {
    var vl = selEditor.$inputNumberOf.val();
    console.log('numberOfChange() PING. value', vl);
    selEditor.$labelNumberOf.text(ivSch.NUMBER_OF_LABEL + ': ' + vl);
    setSeriesName();
}

function setSeriesName() {
    var seriesid = $('#SeriesId').val();
    if (!seriesid) { //csak akkor írjuk felül, ha nincs még azonosítója
        var start = $('#OperationTime').val();
        var vl = selEditor.$inputNumberOf.val();
        $('#SeriesName').val(start + '-W' + vl);
    }
}

function waitMessageDialog(message) {
    'use strict'
    if (!message) message = ivSch.WAIT_MESSAGE;
    return bootbox.dialog({ message: '<div class="text-center"><i class="glyphicon glyphicon-cog spin-right"></i> ' + message + '</div>' });
}

function showStateMessageFromEditor(id) {
    'use strict';
    var thisfn = 'iScheduler.Editor.showStateMessageFromEditor: ';
    var url = ivSch.URL_SHOWSTATE;
    console.log(thisfn + 'id url', id, url);

    $.ajax({
        url: url,
        type: 'get',
        contenttype: 'application/json',
        datatype: 'json',
        data: { id: id },
        success: function (response) {
            bootbox.alert(response);
        },
        error: function (jqXHR, exception) {
            console.log(thisfn + 'Ajax hívás sikertelen! ', jqXHR.responseText);
        }
    });
}// showStateMessageFromEditor function VÉGE

function openObjectEditor() {
    'use strict'
    var variable = ivSch.VARIABLE_OBJECTID;
    var objectid = selEditor.$ObjectIdValue.val();
    var groupid = selEditor.$ObjectGroupId.val();
    if (objectid && objectid != '-1') {
        if (groupid) {
            var url = ivSch.URL_OBJECTEDITOR;
            console.log('iScheduler.Editor.openObjectEditor: before url', url);
            while (url.includes(ivSch.VARIABLE_OBJECTID)) url = url.replace(ivSch.VARIABLE_OBJECTID, objectid);
            while (url.includes(ivSch.VARIABLE_GROUPID)) url = url.replace(ivSch.VARIABLE_GROUPID, groupid);
            console.log('iScheduler.Editor.openObjectEditor: after url', url);
            bootboxAction(url, false, null, null, 1000); //1000 a dialóg ablak szélessége, ez azértéke van ott is beégetve :S
        } else {
            bootbox.alert('Előbb adjon meg egy csoport!');
        }
    }
    else {
        bootbox.alert('Előbb válasszon ki egy objektumot!');
    }
}// openObjectEditor function VÉGE

function removeScheduleFromSeries() {
    'use strict'
    console.log('iScheduler.Editor.removeScheduleFromSeries: PING');
    selEditor.$EditorBootboxId.addClass('darken');
    var dialog = bootbox.dialog({
        title: ivSch.CONFIRM_HEADER,
        message: ivSch.CONFIRM_FROMSERIES,
        onEscape: function (event) {
            dialog.modal('hide');
            selEditor.$EditorBootboxId.removeClass('darken');
        },
        buttons: {
            ok: {
                label: ivSch.BUTTON_LABEL_YES,
                className: 'btn btn-danger',
                callback: function () {
                    console.log('iScheduler.Editor.removeScheduleFromSeries: biztos benne');
                    dialog.modal('hide');
                    selEditor.$EditorBootboxId.removeClass('darken');
                    $('#SeriesId').val(null);   // kitörlöm a sorozatra vonatkozó jelzést
                    selEditor.$EditorFormId.submit();     // mentés
                }
            },
            cancel: {
                label: ivSch.BUTTON_LABEL_NO,
                className: 'btn btn-primary',
                callback: function () {
                    selEditor.$EditorBootboxId.removeClass('darken');
                }
            }
        }
    });
}// removeScheduleFromSeries function VÉGE

function deleteScheduleFromEditor(id, viewMode, isDeleteSeries) {
    'use strict'
    /// A függvény a törlés rákérdezése után a paraméterben megadott id-val, meghívja
    /// az [AjaxParameters.Delete.AjaxUrl]-t. Amely következő függvényt hívja a controllerben:
    ///     public JsonResult Delete(int id){ ... }
    /// Ha az üres stringgel tér vissza, akkor minden rendben lezajlott, ha nem, akkor az hibaüzenet.
    console.log('iScheduler.Editor.deleteScheduleFromEditor: id viewMode isDeleteSeries', id, viewMode, isDeleteSeries);
    var confirm = ivSch.CONFIRM_DELETE;                       // feltételezzük, hogy sima törlés
    if (isDeleteSeries) confirm = ivSch.CONFIRM_DELETESERIES; // ha sorozat törlés, akkor más a szöveg
    selEditor.$EditorBootboxId.addClass('darken');
    var dialog = bootbox.dialog({
        title: ivSch.CONFIRM_HEADER,
        message: confirm,
        onEscape: function (event) {
            dialog.modal('hide');
            selEditor.$EditorBootboxId.removeClass('darken');
        },
        buttons: {
            ok: {
                label: ivSch.BUTTON_LABEL_YES,
                className: 'btn btn-danger',
                callback: function () {
                    console.log('iScheduler.Editor.deleteScheduleFromEditor: biztos benne Id=' + id);
                    dialog.modal('hide');
                    var diagwait = waitMessageDialog();
                    $.ajax({
                        cache: false,
                        url: ivSch.URL_DELETE,
                        contenttype: 'application/json',
                        datatype: 'json',
                        data: ({
                            id: id,
                            connectionString: ivSch.CONNECTION_STRING,
                            isDeleteSeries: isDeleteSeries
                        }),
                        success: function (errorMessage) {
                            diagwait.modal('hide');
                            if (errorMessage == "") {
                                selEditor.$EditorBootboxId.removeClass('darken');
                                console.log('iScheduler.Editor.deleteScheduleFromEditor: Sikeres törlés. viewMode', viewMode);
                                if (viewMode == ivSch.VIEWMODE_LIST) {
                                    oTable.fnDraw();
                                }
                                if (viewMode == ivSch.VIEWMODE_CALENDAR) {
                                    $calendar.fullCalendar('refetchEvents');
                                }
                                bootboxActionHide(ivSch.EDITOR_DIALOG_ID);
                            } else {
                                bootbox.alert(errorMessage, function () {
                                    selEditor.$EditorBootboxId.removeClass('darken');
                                });
                            }
                        },
                        error: function (jqXHR, exception) {
                            diagwait.modal('hide');
                            console.log('iScheduler.Editor.deleteScheduleFromEditor: Ajax hívás sikertelen! ', jqXHR.responseText);
                        }
                    });
                }
            },
            cancel: {
                label: ivSch.BUTTON_LABEL_NO,
                className: 'btn btn-primary',
                callback: function () {
                    selEditor.$EditorBootboxId.removeClass('darken');
                }
            }
        }
    });
}// deleteScheduleFromEditor function VÉGE

// !!! Nem jó az eredeti AutoCompInit, mert nem tudja a selectListJSON-t. !!!
function AutoCompInit(listid, targetid, url, filterid) {
    'use strict'
    console.log('AutoCompInit listid: ' + listid + ', targetid: ' + targetid + ', url: ' + url);
    var $listid = $('#' + listid);
    var $targetid = $('#' + targetid);
    $listid.autocomplete({
        source: function (request, response) {
            if (filterid === undefined || filterid == '') {
                console.log('AutoCompInit source(search) event: filterid=undefined; url: ' + url);
                $.ajax({
                    url: url,
                    type: "POST",
                    dataType: "json",
                    data: { query: request.term },
                    success: function (data) {
                        response($.map(data, function (item) {
                            return { label: item.Text, value: item.Text, id: item.Value }
                        }));
                    }
                });
            } else {
                console.log('AutoCompInit source(search) event: filterid=' + filterid + '; url: ' + url);
                var _filterid = '#' + filterid;
                $.ajax({
                    url: url,
                    type: "POST",
                    dataType: "json",
                    data: { query: request.term, filter: $(_filterid).val() },
                    success: function (data) {
                        response($.map(data, function (item) {
                            return { label: item.Text, value: item.Text, id: item.Value }
                        }));
                    }
                });
            }

        },
        change: function (event, ui) {
            if (!ui.item) {
                //console.log(_targetid + ' change: null');
                $targetid.val(-1);
            }
            else {
                //console.log(_targetid + ' change:' + ui.item.id);
                $targetid.val(ui.item.id);
            }
        },
        select: function (event, ui) {
            var v = ui.item.id;
            //console.log(_targetid + ' select: ' + v);
            $targetid.val(ui.item.id);
        },
        autoFocus: true,
        open: function () {
            //console.log('open');
            $(this).autocomplete('widget').css('z-index', 99999);
        },
        minLength: 0, //hány karakter esetén nyiljon meg, 0 = akkor is megnyilik, ha nincs karakter beütve
    });

    //a listid után találd meg a következő <i> tagot
    $listid.find(' + i').click(function () {
        console.log('AutoCompInit ' + listid + ' click event.');
        $listid.val("");
        $listid.autocomplete("search", "");
        $listid.trigger("focus");
    });

}// AutoCompInit VÉGE

//function AutoCompClickOnSearch(elem) {
//    'use strict'
//    var item = '#' + elem;
//    console.log('AutoCompClickOnSearch item:' + item);
//    $(item).val("");
//    $(item).autocomplete("search", "");
//    $(item).trigger("focus");
//}
/*##### METHODS END #####*/









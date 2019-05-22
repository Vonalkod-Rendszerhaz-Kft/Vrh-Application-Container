/*!
 * iSchedulerReport area Editor.js v1.0.0
 *
 * Copyright (c) Vonalkód Rendszerház 
*/
// A szükséges konstansok, melyek a cshtml-ben kapnak értéket.
// Azért a prototype, hogy gépelés közben legyen intellisense
function EditorConstants() {
    function CallSourcesType() {
        this.Manager = 'manager';
    }
    this.CallSources = new CallSourcesType();

    // Messages
    this.MESS_WAIT = '';
    this.MESS_WAITFOREXECUTE = '';
    this.MESS_SUCCESSFUL = '';
    this.MESS_UNSUCCESSFUL = '';
    this.MESS_NAMEISEMPTY = '';
    this.MESS_USERSLISTISEMPTY = '';
    this.MESS_REPORTSLISTEMPTY = '';
    this.MESS_FORMATSLISTEMPTY = '';
    this.MESS_NOMOREREPORTSADD = '';

    // Labels and Titles
    this.TITLE_USERSOFROLE = '';
    this.LABEL_USER = '';
    this.LABEL_EMAIL = '';

    // Url-ek
    this.URL_DELETE = '';
    this.URL_USERLIST = '';     //az összes felhasználó választó inputhoz
    this.URL_USERSOFROLE = '';  //a "Riportot megkapják" akcióhoz
    this.URL_ROLEGROUPLIST = '';
    this.URL_REPORTLIST = '';
    this.URL_FORMATLIST = '';

    // Variables
    this.VAR_ROLENAME = '';
    this.VAR_PCKNAME = '';

    // Egyéb állandók
    this.ROLENAMEMASK = '';
    
    // Import változók
    this.EditorDialogId = '';
    this.EditorFormId = '';
    this.SelectedReports = [];
    this.SelectedFormats = [];

    //Törléshez
    this.confirmDelete = '';
    this.labelId = '';
    this.labelName = '';
    this.labelDescription = '';
    this.labelYes = '';
    this.labelNo = '';
}
var constEditor = new EditorConstants();    //!!! ennek az objektumnak kell értéket adni a cshtml-ben !!!

/*##### SELECTORS' CASH #####*/
var selCash;
function EditorSelectors() {
    this.$PackageName = $('#PackageName');
    this.$PackageRoleName = $('#PackageRoleName');
    this.$rolenames = $('#role-names');
    this.$reports = $('#PackageReportsSelected');
}
/*##### SELECTORS' CASH END #####*/

/*##### GLOBAL VARIABLES #####*/
var thisjs = 'iSchedulerReport.Editor.js: ';
/*##### GLOBAL VARIABLES END #####*/

/*##### EVENTS #####*/
$(document).ready(function () {
    'use strict';

    var thisfn = thisjs + 'READY event';
    console.log(thisfn);
    EditorConstantsInit(); //AutoCompInit előtt kell lennie !!!
    selCash = new EditorSelectors();
    //console.log(thisjs + 'constEditor selCash', constEditor, selCash);
    changePackageName(); //rolename előzetes beállítása

    //Autocomplete inicializálás
    var inp1 = 'PackageRolegroups';
    var inp2 = 'PackageUsers';
    var sel = 'Selected';
    vrhct.autocomplete.init({ textName: inp1 + sel, valueName: inp1, isMultiList: true, url: constEditor.URL_ROLEGROUPLIST });
    vrhct.autocomplete.init({ textName: inp2 + sel, valueName: inp2, isMultiList: true, url: constEditor.URL_USERLIST });

    $(document).find('.editor-htmlattributes').children(':first-child').addClass('form-control');

    addReportItem(constEditor.SelectedReports, constEditor.SelectedFormats);

    $('#' + constEditor.EditorFormId).on("submit", function (e) {
        //e.preventDefault(); // cancel the actual submit
        var html = '<td colspan="3" style="text-align:center;"><h3><i class="glyphicon glyphicon-cog spin-right"></i> ' + constEditor.MESS_WAITFOREXECUTE + '<h3></td>'
        //document.getElementById('buttoncontainer').innerHTML = html;
        $('#' + constEditor.EditorDialogId).find('#buttoncontainer').html(html);
        //return false; //nem folytatja a submit-ot
    });
});
/*##### EVENTS  END #####*/


/*##### METHODS #####*/

function waitMessageDialog(message) {
    'use strict';
    if (!message) message = constEditor.MESS_WAIT;
    return bootbox.dialog({ message: '<div class="text-center"><i class="glyphicon glyphicon-cog spin-right"></i> ' + message + '</div>' });
}

function changePackageName() {
    'use strict';
    var pckname = selCash.$PackageName.val();
    var rolename = constEditor.ROLENAMEMASK;
    while (rolename.includes(constEditor.VAR_PCKNAME)) rolename = rolename.replace(constEditor.VAR_PCKNAME, pckname);
    selCash.$PackageRoleName.val(rolename);
}// changePackageName function END

function deleteReportItem(id) {
    'use strict';
    var $item = $('.selector-class-for-reports#' + id);
    console.log(thisjs + 'deleteReportItem: item', $item);
    $('.selector-class-for-reports#' + id).remove();
}

function addReportItem(initReports, initFormats) {
    'use strict';
    //A már meglévő két lista birtokában a sorok beillesztése
    function addRow(reportlist, formatlist)
    {
        function getRow(id, report, format) {
            var ret = '<tr id="' + id + '" class="selector-class-for-reports">'
                    + '<td><select id="SelectedReports" name="SelectedReports" class="form-control selector-reportid">';
            for (var i = 0; i < reportlist.length; i++) {
                var slctd = reportlist[i].Value == report ? 'selected' : '';
                ret += '<option value="' + reportlist[i].Value + '" ' + slctd + '>' + reportlist[i].Text + '</option>';
            }
            ret += '</select></td><td><select id="SelectedFormats" name="SelectedFormats" class="form-control selector-exporttype">';
            for (var i = 0; i < formatlist.length; i++) {
                var slctd = formatlist[i].Value == format ? 'selected' : '';
                ret += '<option value="' + formatlist[i].Value + '" ' + slctd + '>' + formatlist[i].Text + '</option>';
            }
            ret += '</select></td><td style="vertical-align:middle;text-align:center;">'
                 + '<span class="glyphicon glyphicon-trash" style="cursor:pointer;" onclick="deleteReportItem(' + id + ');"></span>'
                 + '</td></tr>';;
            return(ret);
        }// getRow function END

        var id = 0;
        var rows = '';
        var selectedvalues = [];

        if (initReports && initReports.length > 0) {
            //ha van init, akkor ez a kezdeti hívás
            for (var i = 0; i < initReports.length; i++) {
                selectedvalues.push(initReports[i]);
                rows += getRow(id, initReports[i], initFormats[i]);
                id++;
            }
            $('#package-report-items').html(rows);
            diagwait.modal('hide');
        } else {
            //ha nincs init, akkor le kell kérdezni az eddig kiválasztott reportazonosítókat
            var selectedReports = $(".selector-class-for-reports");
            console.log('selectedReports', selectedReports);
            if (selectedReports && selectedReports.length > 0) {
                for (var i = 0; i < selectedReports.length; i++) {
                    selectedvalues.push($(selectedReports[i]).find(".selector-reportid").val());
                    id++;
                 }
            }
            console.log('selectedvalues', selectedvalues);

            var newValue = '';
            for (var i = 0; i < reportlist.length; i++) {
                if (!selectedvalues.includes(reportlist[i].Value)) {
                    newValue = reportlist[i].Value;
                    break;
                }
            }

            console.log('newValue', newValue);
            diagwait.modal('hide');
            if (newValue) {
                rows += getRow(id, newValue, formatlist[0]);
                $('#package-report-items').append(rows);
            } else {
                bootbox.alert(constEditor.MESS_NOMOREREPORTSADD);
            }
        }
    }// addRow function END

    var thisfn = thisjs + 'addReportItem: ';
    console.log(thisfn + 'initReports', initReports);

    //lekérem a két listát
    var diagwait = waitMessageDialog();
    $.ajax({
        url: constEditor.URL_REPORTLIST,
        contenttype: 'application/json',
        datatype: 'json',
        success: function (reportlist) {
            if (!reportlist || reportlist.length == 0) {
                diagwait.modal('hide');
                bootbox.alert(constEditor.MESS_REPORTSLISTEMPTY);
            } else {
                $.ajax({
                    url: constEditor.URL_FORMATLIST,
                    contenttype: 'application/json',
                    datatype: 'json',
                    success: function (formatlist) {
                        if (!formatlist || formatlist.length == 0) {
                            diagwait.modal('hide');
                            bootbox.alert(constEditor.MESS_FORMATSLISTEMPTY);
                        } else {
                            addRow(reportlist, formatlist);
                        }
                    },
                    error: function (jqXHR, exception) {
                        diagwait.modal('hide');
                        console.log(thisfn + 'formatlist: Ajax hívás sikertelen! ', jqXHR.responseText);
                    }
                });
            }
        },
        error: function (jqXHR, exception) {
            diagwait.modal('hide');
            console.log(thisfn + 'reportlist: Ajax hívás sikertelen! ', jqXHR.responseText);
        }
    });
}// addReportItem function END

function viewUsersOfRole() {
    'use strict';

    var thisfn = 'viewUsersOfRole: ';
    var rolename = selCash.$PackageRoleName.val();
    var url = constEditor.URL_USERSOFROLE;
    console.log(thisjs + thisfn + 'variable rolename url', constEditor.VAR_ROLENAME, rolename, url);
    while (url.includes(constEditor.VAR_ROLENAME)) url = url.replace(constEditor.VAR_ROLENAME, rolename);
    console.log(thisjs + thisfn + 'url', url);
    $.ajax({
        type: "GET",
        url: url,
        success: function (response) {
            console.log(thisjs + thisfn + 'response', response);
            if (response != null && response.length > 0) {
                var mess = '<style>'
                            + '#usersoftheroletable td, th { border: 1px solid; vertical-align: top; padding: 3px; }'
                            + '</style>'
                            + '<table id="usersoftheroletable" style="width:100%;">'
                            + '<tr><th>' + constEditor.LABEL_USER + '</th><th>' + constEditor.LABEL_EMAIL + '</th></tr>';
                for (var i = 0; i < response.length; i++) {
                    mess += '<tr><td>' + response[i].Value + '</td><td>' + response[i].Text + '</td></tr>';
                }
                mess != '</table>';
                bootbox.alert({ message: mess, title: constEditor.TITLE_USERSOFROLE });
            } else {
                bootbox.alert(constEditor.MESS_USERSLISTISEMPTY);
            }
        },
        error: function (jqXHR, exception) {
            console.log(thisjs + thisfn + 'Ajax hívás sikertelen!', jqXHR.responseText);
            bootbox.alert(constEditor.MESS_UNSUCCESSFUL);
        },
    });
}//viewUsersOfRole function END

function deleteTheEntirePackage(id, sourceOfCalling) {
    'use strict';

    /// A függvény a törlés rákérdezése után a paraméterben megadott id-val, meghívja
    /// a deletePackageURL,-t. Amely következő függvényt hívja az iSchedulerReport controllerben:
    ///     public JsonResult DeletePackage(int id){ ... }
    /// Ha üres stringgel tér vissza, akkor minden rendben lezajlott, ha nem, akkor az hibaüzenet.
    var thisfn = 'Editor.js-deleteTheEntirePackage: '
    var name = $('#PackageName').val();
    var description = $('#PackageDescription').val();
    var mess = '<table>'
             + '<tr><td>' + constEditor.labelId + ':</td><td style="padding-left:4px;">' + id + '</td></tr>'
             + '<tr><td>' + constEditor.labelName + ':</td><td style="padding-left:4px;">' + name + '</td></tr>'
             + '<tr><td>' + constEditor.labelDescription + ':</td><td style="padding-left:4px;">' + description + '</td></tr>'
             + '</table>'
             + '<br />'
             + '<div style="font-weight:bold;">' + constEditor.confirmDelete + '</div>';
    console.log(thisfn + 'id mess', id, mess);
    var dialog = bootbox.dialog({
        message: mess,
        onEscape: function (event) {
            dialog.modal('hide');
        },
        buttons: {
            ok: {
                label: constEditor.labelYes,
                className: 'btn btn-danger',
                callback: function () {
                    console.log(thisfn + 'biztos benne id=' + id);
                    dialog.modal('hide');
                    $.ajax({
                        cache: false,
                        url: constEditor.URL_DELETE,
                        type: 'post',
                        contenttype: 'application/json',
                        datatype: 'json',
                        data: ({ id: id }),
                        success: function (errorMessage) {
                            console.log(thisfn + 'Ajax hívás sikeres! errorMessage', errorMessage);
                            if (errorMessage) {
                                bootbox.alert(errorMessage);
                            } else {
                                if (sourceOfCalling == constEditor.CallSources.Manager) {
                                    oTable.fnDraw();
                                }
                                bootboxActionHide(constEditor.EditorDialogId);
                            }
                        },
                        error: function (jqXHR, exception) {
                            console.log(thisfn + 'Ajax hívás sikertelen! ', jqXHR.responseText);
                        }
                    });
                }
            },
            cancel: {
                label: constEditor.labelNo,
                className: 'btn btn-primary',
                callback: function () {
                }
            }
        }
    });
}// packageThEntireDelete function END

//function UpdatePackage() {
//    "use strict";

//    if ($("#package-name").val().length < 1) {
//        bootbox.alert(this.MESS_NAMEISEMPTY);
//        return;
//    }

//    var reportItems = new Array();
//    var reports = $(".selector-class-for-reports");

//    if (reports.length > 0) {
//        for (var i = 0; i < reports.length; i++) {
//            reportItems.push({ "ReportId": $(reports[i]).find(".selector-reportid").val(), "ExportType": $(reports[i]).find(".selector-exporttype").val() });
//        }
//    }

//    var data = JSON.stringify({
//        "PackageId": currentPackageId,
//        "PackageName": $("#package-name").val(),
//        "PackageGroupName": $("#package-group").val(),
//        "PackageDescription": $("#package-description").val(),
//        "IsActive": $("#package-active").is(":checked"),
//        "RoleName": $rolenames.val(),
//        "Reports": reportItems
//    });

//    $.ajax({
//        type: "POST",
//        data: { data: data },
//        url: '@Html.Raw(Url.Action("UpdatePackage", "Ajax", new { Area = "iSchedulerReport", connectionString = Model.ConnectionString }))',
//        success: function (response) {
//            console.log('_PackageEditor.cshtml: UpdatePackage: ajax response', response);
//            if (response.error == "null") {
//                bootbox.alert(constEditor.MESS_SUCCESSFUL);
//                isPackageEdited = true;
//                $packmodal.modal('hide');
//            } else {
//                bootbox.alert(response.error);
//            }
//        },
//        error: function () {
//            bootbox.alert(this.MESS_UNSUCCESSFUL);
//        }
//    });
//}

/*##### METHODS END #####*/

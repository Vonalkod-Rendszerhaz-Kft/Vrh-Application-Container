function ManagerVariables() {
    this.EditorUrl = '';
    this.EditorSize = 1000;

    this.labelId = '';
    this.labelName = '';
    this.labelDescription = '';
    this.labelYes = '';
    this.labelNo = '';
    this.confirmDelete = '';
    this.DeletePackageUrl = '';
}
var manvar = new ManagerVariables();

$(document).ready(function () {

    /****** DataTables Initialization ******/
    oTable = dataTableInitialization({
        dataTableSelector: '#scheduler-packages',
        sAjaxSource: dataTableUrl,
        aoColumns: columns,
        languageUrl: languageUrl,
        dateTimePickerLanguage: dateTimePickerLanguage,
        oTableTools: {
            sSwfPath: sSwfPath,
            pdfExport: true,
            exportColumns: exportColumns
        },
    });
    /****** End DataTables Initialization ******/
});

/****** EVENTS ******/

/* /// Create Pack Button Click /// */
$(function () {
    'use strict';
    $('#create-pack').click(function () {
        console.log('#create-pack click event PING');
        updatePackage();
    });
});

/* /// Mark no schedule Button Click /// */
$(function () {
    'use strict';
    $('#mark-pack-noschedule').click(function () {
        alert("Mark no schedule");
    });
});

/* /// Mark Packages Missing Button Click /// */
$(function () {
    'use strict';
    $('#mark-pack-missing').click(function () {
        alert("Mark missing");
    });
});

/* /// Clear Marks Button Click /// */
$(function () {
    'use strict';
    $("#clear-marks").click(function () {
        alert("Clear marks");
    });
});
/****** END EVENTS ******/


/****** METHODS ******/

var isAllowUpdatePackage = true;
var timeoutUpdatePackage;
function updatePackage(groupid, packname) {
    'use strict';
    var thisfn = 'Manager.js-updatePackage: ';
    console.log(thisfn + 'groupid packname', groupid, packname);
    if (isAllowUpdatePackage) {
        isAllowUpdatePackage = false;

        if (!groupid) {
            groupid = packageGroupId;
            packname = '';
        }

        var prefix = (manvar.EditorUrl.indexOf('?') != -1) ? '&' : '?';
        var url = manvar.EditorUrl + prefix + 'GroupId=' + groupid + '&ObjectId=' + packname;
        timeoutUpdatePackage = setTimeout(function () {
            isAllowUpdatePackage = true;
        }, 2000);   // 2 mp-ig vár és újra engedélyezi az update-et (ha netán sokszor nyomták a billentyűt)
        bootboxAction(url, false, null, null, manvar.EditorSize);
    }
}// updatePackage function END

/* /// Call Method which delete package by id /// */
function deletePackage(id, name, description) {
    'use strict';

    /// A függvény a törlés rákérdezése után a paraméterben megadott id-val, meghívja
    /// a deletePackageURL,-t. Amely következő függvényt hívja az iSchedulerReport controllerben:
    ///     public JsonResult DeletePackage(int id){ ... }
    /// Ha üres stringgel tér vissza, akkor minden rendben lezajlott, ha nem, akkor az hibaüzenet.
    var thisfn = 'Manager.js-deletePackage: '
    var mess = '<table>'
             + '<tr><td>' + manvar.labelId + ':</td><td style="padding-left:4px;">' + id + '</td></tr>'
             + '<tr><td>' + manvar.labelName + ':</td><td style="padding-left:4px;">' + name + '</td></tr>'
             + '<tr><td>' + manvar.labelDescription + ':</td><td style="padding-left:4px;">' + description + '</td></tr>'
             + '</table>'
             + '<br />'
             + '<div style="font-weight:bold;">' + manvar.confirmDelete + '</div>';
    console.log(thisfn + 'id mess', id, mess);
    var dialog = bootbox.dialog({
        message: mess,
        onEscape: function (event) {
            dialog.modal('hide');
        },
        buttons: {
            ok: {
                label: manvar.labelYes,
                className: 'btn btn-danger',
                callback: function () {
                    console.log(thisfn + 'biztos benne id=' + id);
                    dialog.modal('hide');
                    $.ajax({
                        cache: false,
                        url: manvar.DeletePackageUrl,
                        type: 'post',
                        contenttype: 'application/json',
                        datatype: 'json',
                        data: ({ id: id }),
                        success: function (errorMessage) {
                            console.log(thisfn + 'Ajax hívás sikeree! errorMessage', errorMessage);
                            if (errorMessage) {
                                bootbox.alert(errorMessage);
                            } else {
                                oTable.fnDraw();
                            }
                        },
                        error: function (jqXHR, exception) {
                            console.log(thisfn + 'Ajax hívás sikertelen! ', jqXHR.responseText);
                        }
                    });
                }
            },
            cancel: {
                label: manvar.labelNo,
                className: 'btn btn-primary',
                callback: function () {
                }
            }
        }
    });
}// packageDelete function END

/****** END METHODS ******/



//Delete selected expression from table
function DeleteCalculatedExpression(selectedExpressionId) {
    $.ajax({
        url: '/Home/Delete',
        type: "POST",
        dataType: 'json',
        data: { 'Id': selectedExpressionId },
        success: function (data) {
            var $a = $('a.deleteRow');// gets all 'A' tags whos class="deleteRow"
            var $activeA = $a.context.activeElement; // gets current active tag 'A' clicked by user
            var $tr = $activeA.closest('tr'); //gets the current 'TR' tag to which belongs current active tag 'A'
            $tr.remove(); // removes selected row
        },
        error: function () {
            window.location.href = '/Home/Index';
            alert("Error occured during caliing function of DeleteCalculatedExpression");
        }
    });
};

// Load calculations history
function LoadCalculationsHistory() {

    $('#update_panel').html('<br><center>Loading calculations history...</center><br>');
    var $loadingAnimation = $('div#progress');
    $.ajax({
        url: '/home/GetCalculationsHistory',
        type: 'GET',
        dataType: 'json',
        success: function (d) {
            if (d.length > 0) {
                var $data = $('<hr><table id="generatedTable"></table>').addClass('table table-responsive table-striped');
                var header = "<thead><tr><th id='clientIP_header'>Visitor's IP address</th><th id='calculatedEpression_header'>Calculated Expressions</th><th id='calculatedDatetime_header'>Calculated Datetime</th><th></th></tr></thead>";
                $data.append(header);

                $.each(d, function (i, row) {
                    var reISO = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2}(?:\.\d*))(?:Z|(\+|-)([\d|:]*))?$/;
                    var reMsAjax = /^\/Date\((d|-|.*)\)[\/|\\]$/;

                    var a = reMsAjax.exec(row.CalculatedDatetime);
                    var date;
                    if (a) {
                        // perform some jujitsu to make use of it:
                        var b = a[1].split(/[-+,.]/);
                        date = new Date(b[0] ? +b[0] : 0 - +b[1]);
                    } else {
                        date = new Date();
                    }

                    var $row = $('<tr class="generatedRow" ></tr>');
                    $row.append($('<td/>').html(row.ClientIP));
                    $row.append($('<td/>').html(row.CalculatedExpression));
                    $row.append($('<td/>').html(date.toLocaleDateString() + " " + date.toLocaleTimeString() ));
                    $row.append($('<td/>').html("<a href='javascript:;' class='deleteRow' onclick='DeleteCalculatedExpression(" + row.Id + ");'>Delete</a>"));
                    $data.append($row);
                });

                $loadingAnimation.hide();
                $('#update_panel').html($data);
            }
            else {

                $loadingAnimation.hide();
                var $noData = $('<div/>').html("<hr><center><p>You don't have any calculations history data now!</p></center>").addClass('noData');
                $('#update_panel').html($noData);
            }
        },
        error: function () {
            alert('Error! Please try again.');
        }
    });

}

//Insert new calculated expression to DB and update table
function insertCalculatedExpressionsToDB(expression) {
    $.ajax({
        url: "/Home/Create",
        type: "POST",
        data: { "expression": expression },
        dataType: "json",
        success: function (row) {
            row = row.lastInsertedRow;

            var reMsAjax = /^\/Date\((d|-|.*)\)[\/|\\]$/;
            var a = reMsAjax.exec(row.CalculatedDatetime);
            var date;
            if (a) {
                // perform some jujitsu to make use of it:
                var b = a[1].split(/[-+,.]/);
                date = new Date(b[0] ? +b[0] : 0 - +b[1]);
            } else {
                date = new Date();
            }

            var $row = $('<tr class="generatedRow" ></tr>');
            $row.append($('<td/>').html(row.ClientIP));
            $row.append($('<td/>').html(row.CalculatedExpression));
            $row.append($('<td/>').html(date.toLocaleDateString() + " " + date.toLocaleTimeString() ));
            $row.append($('<td/>').html("<a href='javascript:;' class='deleteRow' onclick='DeleteCalculatedExpression(" + row.Id + ");'>Delete</a>"));
            var $tbody = $('#generatedTable tbody');
            if ($tbody[0] && $tbody[0].tagName == 'TBODY') {
                $tbody.prepend($row);
            } else {
                var $noDataDiv = $('div .noData');
                if ($noDataDiv[0])
                    $noDataDiv[0].remove();

                var $data = $('<hr><table id="generatedTable"></table>').addClass('table table-responsive table-striped');
                var header = "<thead><tr><th><center>Visitor's IP address</center></th><th><center>Calculated Expressions</center></th><th><center>Calculated Datetime</center></th><th></th></tr></thead>";
                $data.append(header);
                $data.append($row);
                $('#update_panel').html($data);
            }

        },
        error: function (error) {
            alert("Error! Please try again " + error.toString());
        }
    });
};
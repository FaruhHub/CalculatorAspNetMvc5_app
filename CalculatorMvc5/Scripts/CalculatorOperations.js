//Calculator operations
var no1, tx1, v1 = 0, v2, choice = 'e';

function f1(value) {
    no1 = value;
    tx1 = document.getElementById('txtAns').value;
    document.getElementById('txtAns').value = tx1 + no1;

};
function f2(value) {
    if (value == 'p') {
        v1 = document.getElementById('txtAns').value;
        document.getElementById('txtAns').value = "";
        choice = value;

    }
    else if (value == 's') {
        v1 = document.getElementById('txtAns').value;
        document.getElementById('txtAns').value = "";
        choice = value;

    }
    else if (value == 'm') {
        v1 = document.getElementById('txtAns').value;
        document.getElementById('txtAns').value = "";
        choice = value;

    }
    else if (value == 'd') {
        v1 = document.getElementById('txtAns').value;
        document.getElementById('txtAns').value = "";
        choice = value;

    }
    else if (value == 'mo') {
        v1 = document.getElementById('txtAns').value;
        choice = value;

    }
    else//equal operation
    {
        v2 = document.getElementById('txtAns').value;
        document.getElementById('txtAns').value = "";
        var calcExpression;
        switch (choice) {
            case 'p':
                ans = parseInt(v1) + parseInt(v2);
                document.getElementById('txtAns').value = ans;
                calcExpression = v1 + " + " + v2 + " = " + ans
                insertCalculatedExpressionsToDB(calcExpression);
                //LoadCalculationsHistory();
                break;

            case 's':
                ans = parseInt(v1) - parseInt(v2);
                document.getElementById('txtAns').value = ans;
                calcExpression = v1 + " - " + v2 + " = " + ans
                insertCalculatedExpressionsToDB(calcExpression);
                //LoadCalculationsHistory();
                break;

            case 'm':
                ans = parseInt(v1) * parseInt(v2);
                document.getElementById('txtAns').value = ans;
                calcExpression = v1 + " * " + v2 + " = " + ans
                insertCalculatedExpressionsToDB(calcExpression);
                //LoadCalculationsHistory();
                break;

            case 'd':
                ans = parseInt(v1) / parseInt(v2);
                document.getElementById('txtAns').value = ans;
                calcExpression = v1 + " / " + v2 + " = " + ans
                insertCalculatedExpressionsToDB(calcExpression);
                //LoadCalculationsHistory();
                break;

            default:
                alert('please enter valid number');
        }
    }
};
function f3(value) {
    if (value == 'c') {
        document.getElementById('txtAns').value = "";
    }
    if (value == '-') {
        var tempvalue = parseInt(document.getElementById('txtAns').value);
        console.log(tempvalue);
        if (tempvalue > 0)//positive value
        {
            console.log('positive value');
            tempvalue = -Math.abs(tempvalue);
            console.log(tempvalue);
            document.getElementById('txtAns').value = "";
            document.getElementById('txtAns').value = tempvalue;
        }
        else//negative value
        {
            console.log('negative value');
            tempvalue = -Math.abs(tempvalue);
            document.getElementById('txtAns').value = tempvalue;
        }
    }
};
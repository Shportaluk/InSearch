function Complete ()
{
    var nameQuestion = $("#questions p").text();
    var arrQuest = "";
    $(".question").each(function (i, elem) {
        var obj = $(elem);
        arrQuest += obj.find(".ask").text() + ":";
        arrQuest += obj.find(".input_radio input:checked").val() + "|"
    })
    $.ajax({
        url: '/CompleteQuestion',
        type: 'POST',
        contentType: 'application/json;',
        data: JSON.stringify({ nameQuestion: nameQuestion, arrQuest: arrQuest }),
        success: function (res) {
            alert(res);
        }
    });
}
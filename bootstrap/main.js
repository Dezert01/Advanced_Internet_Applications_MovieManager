$(document).ready(function() {
    $('#searchBar').on('keyup', function(e) {
        if (e.keyCode !== 13) {
            return
        }
        let query = $(this).val().toLowerCase();
        $('#movieCards .card-title').each(function() {

            let cardTitle = $(this).text().toLowerCase();
            if (cardTitle.indexOf(query) > -1) {
                $(this).parent().parent().parent().show();
            } else {
                $(this).parent().parent().parent().hide();
            }
        });
    });
});
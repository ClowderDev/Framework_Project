$(function() {
    if ($(".confirmDeletion").length) {
        $(".confirmDeletion").click(function(e) {
            e.preventDefault();
            
            const deleteUrl = $(this).attr('href');

            Swal.fire({
              title: "Are you sure?",
              text: "This action cannot be undone",
              icon: "warning",
              showCancelButton: true,
              confirmButtonColor: "#3085d6",
              cancelButtonColor: "#d33",
              confirmButtonText: "Yes, delete it!",
            }).then((result) => {
              if (result.isConfirmed) {
                $.ajax({
                  url: deleteUrl,
                        type: 'DELETE',
                  success: function (result) {
                    location.reload();
                  },
                  error: function (xhr, status, error) {
                    Swal.fire(
                      "Error!",
                      "There was an error deleting the item.",
                      "error"
                    );
                  },
                });
              }
            });
        });
    }
});
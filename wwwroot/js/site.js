// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).on("click", function (event) {
    var clickover = $(event.target);
    var navbarCollapse = $(".navbar-collapse");
    if (navbarCollapse.hasClass("show") && !clickover.hasClass("navbar-toggler")) {
        navbarCollapse.collapse('hide');
    }
});


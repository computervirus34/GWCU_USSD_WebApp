$(document).ready(function () {
    $('nav.sidebar .nav-link').removeClass('active')
    $('nav.sidebar .nav-link:contains(' + $('title').text() + ')').addClass('active')
})
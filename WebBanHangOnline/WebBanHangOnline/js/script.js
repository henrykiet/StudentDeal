// Tạo chuyển động qua lại trên promo 
var ImgPromo = 1;
showDivs(ImgPromo);

function plusDivs(n) {
    showDivs(ImgPromo += n);
}

function currentDiv(n) {
    showDivs(ImgPromo = n);
}

function showDivs(n) {
    var i;
    var x = document.getElementsByClassName("imgP");
    var dots = document.getElementsByClassName("image-badge");
    if (n > x.length) { ImgPromo = 1 }
    if (n < 1) { ImgPromo = x.length }
    for (i = 0; i < x.length; i++) {
        x[i].style.display = "none";
    }
    for (i = 0; i < dots.length; i++) {
        dots[i].className = dots[i].className.replace(" badge-white", "");
    }
    x[ImgPromo - 1].style.display = "block";
    dots[ImgPromo - 1].className += " badge-white";
}

// Star media query 
const toggle_menu = document.getElementsByClassName('mobile');
const nav = document.getElementsByClassName('navbar1');

toggle_menu.onclick = function () {
    nav.classList.toggle('hide');
}


const button = document.querySelector(".pro_btn"),
    toast = document.querySelector(".toast")
closeIcon = document.querySelector(".close"),
    progress = document.querySelector(".progress");

button.addEventListener("click", () => {
    toast.classList.add("active");
    progress.classList.add("active");

    setTimeout(() => {
        toast.classList.remove("active");
    }, 5000); // 1s = 1000 ms

    setTimeout(() => {
        progress.classList.remove("active");
    }, 5300); // 1s = 1000 ms
});

closeIcon.addEventListener("click", () => {
    toast.classList.remove("active");

    setTimeout(() => {
        progress.classList.remove("active");
    }, 300); // 1s = 1000 ms
});
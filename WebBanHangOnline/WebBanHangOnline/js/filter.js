const btns = document.querySelectorAll(".btn");
const storeVoucher = document.querySelectorAll(".pro");

for (let i = 0; i < btns.length; i++) {

    btns[i].addEventListener("click", (e) => {
        e.preventDefault();
        const filter = e.target.dataset.filter;

        storeVoucher.forEach((voucher) => {
            if (filter == "all") {
                voucher.style.display = "block"
            } else {
                if (voucher.classList.contains(filter)) {
                    voucher.style.display = "block"
                } else {
                    voucher.style.display = "none"
                }
            }
        })
    })

}
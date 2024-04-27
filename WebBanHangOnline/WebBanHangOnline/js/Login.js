const formContainer = document.querySelector('.form-container');
const loginForm = document.querySelector('#sign-in-form');
const registerForm = document.querySelector('#sign-up-form');

const switchForm = (form) => {
    if (form === 'register') {
        formContainer.style.left = '50%';
        loginForm.style.marginLeft = '-150%';
        registerForm.style.marginLeft = '-100%';
    } else {
        formContainer.style.left = '0%';
        loginForm.style.marginLeft = '0%';
        registerForm.style.marginLeft = '50%';
    }
};

document.addEventListener('DOMContentLoaded', () => {
    const registerLink = document.querySelector('#register-link');
    if (registerLink) {
        registerLink.addEventListener('click', () => {
            switchForm('register');
        });
    }
});

document.addEventListener('DOMContentLoaded', function () {
    const recoveryForm = document.getElementById('recoveryForm');
    const emailPolicy = document.getElementById('emailPolicy');
    const userPolicy = document.getElementById('userPolicy');
    const emailField = document.getElementById('emailField');
    const formControls = document.querySelectorAll('.form-control');
    const usernameField = document.getElementById('usernameField');
    const emailInput = document.getElementById('Email');
    const usernameInput = document.getElementById('Username');
    const recoveryBtn = document.querySelector('.register');

    function isInputHidden(elemento) {
        return elemento.tagName.toLowerCase() === 'input' && elemento.type.toLowerCase() === 'hidden';
    }

    function activateEmailPolicy() {
        emailField.classList.remove('d-none');
        usernameField.classList.add('d-none');
        usernameInput.removeAttribute('required');
        emailInput.setAttribute('required', '');
    }

    function activateUserPolicy() {
        usernameField.classList.remove('d-none');
        emailField.classList.add('d-none');
        emailInput.removeAttribute('required');
        usernameInput.setAttribute('required', '');
    }

    // Manejar cambio entre radio buttons
    if (emailPolicy) {
        emailPolicy.addEventListener('change', function (event) {
            if (event.target.checked) {
                activateEmailPolicy();
            }
        });
    }

    if (!isInputHidden(userPolicy)) {
        userPolicy.addEventListener('change', function () {
            if (this.checked) {
                activateUserPolicy();
            }
        });
    }

    // Validar formulario al hacer clic en el botón
    recoveryBtn.addEventListener('click', function (event) {
        // Disable register button
        event.target.disabled = true;
        event.target.querySelector('i').classList.remove('fa-check');
        event.target.querySelector('i').classList.add('fa-pulse', 'fa-spinner');

        // Validate form
        if (!recoveryForm.checkValidity()) {
            // Si el formulario no es válido, se evita el envío
            event.preventDefault();
            event.stopPropagation();
            // Enable register button
            event.target.disabled = false;
            event.target.querySelector('i').classList.remove('fa-pulse', 'fa-spinner');
            event.target.querySelector('i').classList.add('fa-check');
            // Add was-validated class to form
            recoveryForm.classList.add('was-validated');
        }
        else {
            // Si el formulario es válido, se establece readonly en los campos
            formControls.forEach(control => control.readOnly = true);
            // Se envía el formulario
            recoveryForm.submit();
        }
    });

    if (emailPolicy) activateEmailPolicy();
});
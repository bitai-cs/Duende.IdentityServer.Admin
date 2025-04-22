// Validación de formulario
(function () {
    'use strict';

    // Seleccionar el formulario
    const form = document.getElementById('signupForm');
    const formControls = document.querySelectorAll('.form-control');
    const register = document.querySelector('.register');
    const cancel = document.querySelector('.cancel');
    const passwordInput = document.getElementById('@nameof(Model.Password)');
    const confirmPasswordInput = document.getElementById('@nameof(Model.ConfirmPassword)');
    const passwordStrength = document.getElementById('passwordStrength');
    const togglePassword = document.getElementById('togglePassword');
    const toggleConfirmPassword = document.getElementById('toggleConfirmPassword');

    // Validación complejidad de password
    passwordInput.addEventListener('input', function () {
        const password = passwordInput.value;
        let strength = 0;

        // Longitud mínima
        if (password.length >= 8) strength += 1;
        //if (password.length >= 12) strength += 1;

        // Contiene números
        if (password.match(/\d/)) strength += 1;

        // Contiene letras minúsculas
        if (password.match(/[a-z]/)) strength += 1;

        // Contiene letras mayúsculas
        if (password.match(/[A-Z]/)) strength += 1;

        // Contiene caracteres especiales
        if (password.match(/[^a-zA-Z0-9]/)) strength += 1;

        // Actualizar indicador de fuerza
        passwordStrength.className = 'password-strength';
        passwordStrength.classList.add(`strength-${Math.min(strength, 5)}`);
    });

    // Toggle para mostrar/ocultar confirmación de contraseña
    togglePassword.addEventListener('click', function () {
        const icon = this.querySelector('i');
        if (passwordInput.type === 'password') {
            passwordInput.type = 'text';
            icon.classList.replace('fa-eye', 'fa-eye-slash');
        } else {
            passwordInput.type = 'password';
            icon.classList.replace('fa-eye-slash', 'fa-eye');
        }
    });

    // Toggle para mostrar/ocultar confirmación de contraseña
    toggleConfirmPassword.addEventListener('click', function () {
        const icon = this.querySelector('i');
        if (confirmPasswordInput.type === 'password') {
            confirmPasswordInput.type = 'text';
            icon.classList.replace('fa-eye', 'fa-eye-slash');
        } else {
            confirmPasswordInput.type = 'password';
            icon.classList.replace('fa-eye-slash', 'fa-eye');
        }
    });

    // Validar que las contraseñas coincidan
    confirmPasswordInput.addEventListener('input', function () {
        if (confirmPasswordInput.value !== passwordInput.value) {
            confirmPasswordInput.setCustomValidity('Las contraseña y la confirmación de contraseña no coinciden.');
            // Agregar clases de Bootstrap para mostrar el error
            confirmPasswordInput.classList.add('is-invalid');
            document.getElementById('confirmPasswordFeedback').textContent = 'Las contraseña y la confirmación de contraseña no coinciden.';
        } else {
            confirmPasswordInput.setCustomValidity('');
            // Remover clases de error si las contraseñas coinciden
            confirmPasswordInput.classList.remove('is-invalid');
            confirmPasswordInput.classList.add('is-valid');
            document.getElementById('confirmPasswordFeedback').textContent = '';
        }
    });

    // Validación de Bootstrap
    register.addEventListener('click', function (event) {
        // Disable register button
        event.target.disabled = true;
        event.target.querySelector('i').classList.remove('fa-check');
        event.target.querySelector('i').classList.add('fa-pulse', 'fa-spinner');
        // Disable cancel button
        cancel.disabled = true;
        // Validate form
        if (!form.checkValidity()) {
            // Si el formulario no es válido, se evita el envío
            event.preventDefault();
            event.stopPropagation();
            // Enable register button
            event.target.disabled = false;
            event.target.querySelector('i').classList.remove('fa-pulse', 'fa-spinner');
            event.target.querySelector('i').classList.add('fa-check');
            // Enable cancel button
            cancel.disabled = false;
            // Add was-validated class to form
            form.classList.add('was-validated');
        }
        else {
            // Readonly all form-control
            formControls.forEach(control => control.readOnly = true);
            // Send form
            form.submit();
        }
    }, false);
})();
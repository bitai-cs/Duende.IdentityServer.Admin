(() => {
    'use strict'

    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    var btnlogin = document.querySelector('.login');
    if (btnlogin) {
        btnlogin.addEventListener('click', function (event) {
            // Deshabilitar botón login y cambiar icono
            event.target.disabled = true;
            event.target.querySelector('i').classList.remove('fa-sign-in-alt');
            event.target.querySelector('i').classList.add('fa-pulse', 'fa-spinner');
            // Deshabilitar controles cancel
            document.querySelectorAll('.cancel, .configcombo').forEach(control => {
                control.disabled = true;
            });

            var form = document.querySelector('.needs-validation');
            if (form.checkValidity()) {
                // Establecer readonly a form-control y form-check-input
                document.querySelectorAll('.form-control, .form-check-input').forEach(control => {
                    control.readOnly = true;
                });
                // Deshabilitar controles social
                document.querySelectorAll('.social').forEach(control => {
                    control.disabled = true;
                });
                //Asignar el valor del botón oculto y enviar el formulario
                var hiddenButton = document.getElementsByName('button')[0];
                hiddenButton.value = event.target.value;
                form.submit();
            } else {                
                // Remover readonly de form-control y form-check-input
                document.querySelectorAll('.form-control, .form-check-input').forEach(control => {
                    control.readOnly = false;
                });
                // Volver a habilitar botón login
                event.target.querySelector('i').classList.remove('fa-pulse', 'fa-spinner');
                event.target.querySelector('i').classList.add('fa-sign-in-alt');
                event.target.disabled = false;
                // Volver a habilitar controles cancel
                document.querySelectorAll('.cancel, .configcombo').forEach(control => {
                    control.disabled = false;
                });
                // Mostrar validación
                form.classList.add('was-validated');
            }
        }, false);
    }

    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    var btncancel = document.querySelector('.cancel');
    if (btncancel) {
        btncancel.addEventListener('click', function (event) {
            var hiddenButton = document.getElementsByName('button')[0]; //.value = 'cancel';
            hiddenButton.value = event.target.value;
            var form = document.querySelector('.needs-validation');
            form.submit();
        }, false);
    }
})();
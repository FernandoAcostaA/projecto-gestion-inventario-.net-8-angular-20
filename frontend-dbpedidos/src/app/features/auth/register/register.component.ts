import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import Swal from 'sweetalert2';


@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'] // si usas un .scss para estilos
})
export class RegisterComponent implements OnInit {
  form!: FormGroup;
  error = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      userName: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4)]]
    });
  }

  async register() {
  if (this.form.invalid) return;

  this.error = '';
  try {
    await this.authService.register(this.form.value);
    await Swal.fire({
      icon: 'success',
      title: '¡Registro exitoso!',
      text: 'Tu cuenta fue creada correctamente',
      confirmButtonText: 'Ir a login'
    });
    this.router.navigate(['/login']);
  } catch (e: any) {
    this.error = e?.response?.data ?? 'Error al registrar usuario';

    Swal.fire({
      icon: 'error',
      title: 'Error',
      text: this.error,
      confirmButtonText: 'Aceptar'
    });
  }
}


  goToLogin() {
    this.router.navigate(['/login']);
  }
  cancel() {
    this.router.navigate(['/login']);
  }
}

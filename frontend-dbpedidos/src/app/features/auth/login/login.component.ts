import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import Swal from 'sweetalert2';


@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'] // si usas estilos
})
export class LoginComponent implements OnInit {
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
      password: ['', Validators.required]
    });
  }

  async login() {
  if (this.form.invalid) return;

  this.error = '';
  try {
    await this.authService.login(this.form.value);
    await Swal.fire({
      icon: 'success',
      title: '¡Bienvenido!',
      text: 'Inicio de sesión exitoso',
      confirmButtonText: 'Continuar'
    });
    this.router.navigate(['']);
  } catch (e: any) {
    this.error = e?.response?.data ?? 'Error de autenticación';

    Swal.fire({
      icon: 'error',
      title: 'Error',
      text: this.error,
      confirmButtonText: 'Intentar de nuevo'
    });
  }
}


  goToRegister() {
    this.router.navigate(['/register']);
  }
}

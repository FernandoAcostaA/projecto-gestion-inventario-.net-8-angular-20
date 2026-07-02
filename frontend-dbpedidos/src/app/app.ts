import { Component, signal } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AiAssistantComponent } from './features/ai/ai-assistant/ai-assistant.component';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, AiAssistantComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('frontend-dbpedidos');

  public currentYear: number = new Date().getFullYear();

  constructor(private router: Router, public authService: AuthService) {}

  logout() {
  Swal.fire({
    title: '¿Estás seguro?',
    text: '¿Deseas cerrar sesión?',
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: 'Sí, cerrar sesión',
    cancelButtonText: 'Cancelar'
  }).then((result) => {
    if (result.isConfirmed) {
      this.authService.logout();
      this.router.navigate(['/login']);
      Swal.fire('¡Hasta luego!', 'Tu sesión ha sido cerrada.', 'success');
    }
  });
}
}

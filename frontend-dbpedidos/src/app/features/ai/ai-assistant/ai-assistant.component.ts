import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AiService, AiMessage, AiSuggestedAction, AiDataChange } from '../../../core/services/ai.service';
import { RefreshService } from '../../../core/services/refresh.service';

@Component({
  selector: 'app-ai-assistant',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-assistant.component.html',
  styleUrl: './ai-assistant.component.scss'
})
export class AiAssistantComponent {
  isOpen = signal(false);
  message = '';
  messages = signal<{ role: string; content: string; actions?: AiSuggestedAction[] }[]>([]);
  loading = signal(false);

  constructor(
    private aiService: AiService,
    private router: Router,
    private refreshService: RefreshService
  ) {}

  quickActions = [
    { label: '📊 Resumen del negocio', message: 'Dame un resumen completo del negocio' },
    { label: '📦 Stock bajo', message: '¿Qué artículos tienen stock bajo?' },
    { label: '🏆 Más vendidos', message: '¿Cuáles son los artículos más vendidos?' },
    { label: '👤 Insights cliente', message: 'Quiero ver insights de un cliente' },
    { label: '🛒 Recomendar', message: 'Recomiéndame productos para promocionar' },
  ];

  toggleChat() {
    this.isOpen.update(v => !v);
    if (this.isOpen() && this.messages().length === 0) {
      this.addMessage('assistant', '¡Hola! Soy tu asistente IA de **Ventas La Ganga**.\n\nPuedo ayudarte con:\n\n• 📊 **Resumen del negocio** — ventas, stock, tendencias\n• 👤 **Insights de clientes** — historial, frecuencia, sugerencias\n• 🔍 **Búsqueda inteligente** — encontrá artículos en lenguaje natural\n• 📦 **Stock y reportes** — bajos, más vendidos, anomalías\n\nElegí una opción rápida o escribime lo que necesites.');
    }
  }

  sendQuickAction(message: string) {
    this.message = message;
    this.sendMessage();
  }

  async sendMessage() {
    const text = this.message.trim();
    if (!text || this.loading()) return;

    this.addMessage('user', text);
    this.message = '';
    this.loading.set(true);

    try {
      const history: AiMessage[] = this.messages()
        .filter(m => m.role !== 'system')
        .slice(-10)
        .map(m => ({ role: m.role, content: m.content }));

      const response = await this.aiService.chat(text, history.slice(0, -1));
      this.addMessage('assistant', response.reply, response.suggestedActions);
      if (response.dataChanges) {
        for (const change of response.dataChanges) {
          this.refreshService.notify(change.entity, change.action);
        }
      }
    } catch {
      this.addMessage('assistant', 'Lo siento, hubo un error al procesar tu mensaje. Verifica que la IA esté configurada correctamente.');
    } finally {
      this.loading.set(false);
    }
  }

  executeAction(action: AiSuggestedAction) {
    if (action.route) {
      this.router.navigate([action.route]);
    }
  }

  private addMessage(role: string, content: string, actions?: AiSuggestedAction[]) {
    this.messages.update(m => [...m, { role, content, actions }]);
  }
}

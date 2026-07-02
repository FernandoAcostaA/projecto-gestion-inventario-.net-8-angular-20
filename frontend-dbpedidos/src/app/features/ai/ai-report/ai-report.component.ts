import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiService } from '../../../core/services/ai.service';

@Component({
  selector: 'app-ai-report',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-report.component.html',
  styleUrl: './ai-report.component.scss'
})
export class AiReportComponent {
  prompt = '';
  period = 'mes';
  report = signal<string | null>(null);
  loading = signal(false);

  constructor(private aiService: AiService) {}

  async generateReport() {
    if (!this.prompt.trim() || this.loading()) return;

    this.loading.set(true);
    this.report.set(null);

    try {
      const result = await this.aiService.generateReport(this.prompt, this.period);
      this.report.set(result);
    } catch {
      this.report.set('Error al generar el reporte. Verifica que la IA esté configurada.');
    } finally {
      this.loading.set(false);
    }
  }
}

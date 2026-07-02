import { Component, OnInit, OnDestroy } from '@angular/core';
import { RouterModule } from '@angular/router'; 
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { AiService } from '../core/services/ai.service';
import { RefreshService } from '../core/services/refresh.service';

@Component({
  selector: 'app-home',
  imports: [CommonModule, RouterModule], 
  templateUrl: './home.component.html',
  standalone: true, 
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit, OnDestroy {
  resumenIA = '';
  cargandoResumen = false;
  errorResumen = false;
  private refreshSub?: Subscription;

  constructor(
    private aiService: AiService,
    private refreshService: RefreshService
  ) {}

  ngOnInit() {
    this.cargarResumen();
    this.refreshSub = this.refreshService.changes$.subscribe(() => {
      this.cargarResumen();
    });
  }

  ngOnDestroy() {
    this.refreshSub?.unsubscribe();
  }

  async cargarResumen() {
    this.cargandoResumen = true;
    this.errorResumen = false;
    try {
      this.resumenIA = await this.aiService.getDashboardSummary();
    } catch {
      this.errorResumen = true;
      this.resumenIA = '';
    } finally {
      this.cargandoResumen = false;
    }
  }
}

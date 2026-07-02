import { Component, input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiService, AiAutocompleteItem } from '../../core/services/ai.service';

@Component({
  selector: 'app-ai-autocomplete',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="ai-autocomplete-wrapper">
      <input
        type="text"
        [ngModel]="searchQuery()"
        (ngModelChange)="onSearch($event)"
        [placeholder]="placeholder()"
        class="ai-autocomplete-input"
        (blur)="hideDropdown()"
        (focus)="showDropdown()"
      />
      @if (isOpen() && results().length > 0) {
        <ul class="ai-autocomplete-dropdown">
          @for (item of results(); track item.id) {
            <li (mousedown)="selectItem(item)">
              <i class="fas fa-search"></i>
              <div class="item-info">
                <span class="item-name">{{ item.nombre }}</span>
                @if (item.codigo) {
                  <span class="item-detail">Código: {{ item.codigo }}</span>
                }
                @if (item.documento) {
                  <span class="item-detail">Doc: {{ item.documento }}</span>
                }
              </div>
            </li>
          }
        </ul>
      }
    </div>
  `,
  styles: [`
    .ai-autocomplete-wrapper {
      position: relative;
      width: 100%;
    }
    .ai-autocomplete-input {
      width: 100%;
      padding: 8px 12px;
      border: 2px solid #e0e0e0;
      border-radius: 6px;
      font-size: 14px;
      outline: none;
      box-sizing: border-box;
    }
    .ai-autocomplete-input:focus {
      border-color: #3498db;
    }
    .ai-autocomplete-dropdown {
      position: absolute;
      top: 100%;
      left: 0;
      right: 0;
      background: white;
      border: 1px solid #dee2e6;
      border-radius: 0 0 8px 8px;
      box-shadow: 0 4px 12px rgba(0,0,0,0.15);
      z-index: 100;
      list-style: none;
      margin: 0;
      padding: 4px 0;
      max-height: 200px;
      overflow-y: auto;
    }
    .ai-autocomplete-dropdown li {
      padding: 8px 12px;
      display: flex;
      align-items: center;
      gap: 10px;
      cursor: pointer;
      transition: background 0.15s;
    }
    .ai-autocomplete-dropdown li:hover {
      background: #f0f7ff;
    }
    .ai-autocomplete-dropdown li i {
      color: #3498db;
      font-size: 12px;
    }
    .item-info {
      display: flex;
      flex-direction: column;
    }
    .item-name {
      font-weight: 500;
      font-size: 14px;
    }
    .item-detail {
      font-size: 12px;
      color: #666;
    }
  `]
})
export class AiAutocompleteComponent {
  entityType = input.required<string>();
  placeholder = input('Buscar...');
  itemSelected = output<AiAutocompleteItem>();

  searchQuery = signal('');
  results = signal<AiAutocompleteItem[]>([]);
  isOpen = signal(false);
  private debounceTimer: any;

  constructor(private aiService: AiService) {}

  onSearch(value: string) {
    this.searchQuery.set(value);
    clearTimeout(this.debounceTimer);

    if (value.length < 2) {
      this.results.set([]);
      return;
    }

    this.debounceTimer = setTimeout(async () => {
      try {
        const items = await this.aiService.autocomplete(this.entityType(), value);
        this.results.set(items);
        this.isOpen.set(items.length > 0);
      } catch {
        this.results.set([]);
      }
    }, 300);
  }

  selectItem(item: AiAutocompleteItem) {
    this.searchQuery.set(item.nombre);
    this.isOpen.set(false);
    this.itemSelected.emit(item);
  }

  showDropdown() {
    if (this.results().length > 0) this.isOpen.set(true);
  }

  hideDropdown() {
    setTimeout(() => this.isOpen.set(false), 200);
  }
}

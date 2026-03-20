import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService, type ThemeMode, type ColorVariant, type FontChoice } from '../../../core/services/theme.service';

@Component({
  selector: 'app-theme-picker',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (open()) {
      <div class="tp-backdrop" (click)="open.set(false)"></div>
    }
    <div class="tp-wrapper">
      <button class="tp-trigger" (click)="toggle()" aria-label="Aparência">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="5"/>
          <line x1="12" y1="1" x2="12" y2="3"/>
          <line x1="12" y1="21" x2="12" y2="23"/>
          <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/>
          <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/>
          <line x1="1" y1="12" x2="3" y2="12"/>
          <line x1="21" y1="12" x2="23" y2="12"/>
          <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"/>
          <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/>
        </svg>
      </button>

      @if (open()) {
        <div class="tp-dropdown animate-fade-up">
          <span class="tp-title">Aparência</span>

          <span class="tp-label">Tema</span>
          <div class="tp-row">
            <button class="tp-option" [class.tp-option--active]="theme.theme() === 'light'" (click)="theme.toggleTheme()">Claro</button>
            <button class="tp-option" [class.tp-option--active]="theme.theme() === 'dark'" (click)="theme.toggleTheme()">Escuro</button>
          </div>

          <span class="tp-label">Cor de acento</span>
          @for (c of colors; track c.id) {
            <button class="tp-color-row" [class.tp-color-row--active]="theme.color() === c.id" (click)="theme.setColor(c.id)">
              <span class="tp-swatch" [style.background]="c.hex"></span>
              <span>{{ c.name }}</span>
            </button>
          }

          <span class="tp-label">Fonte</span>
          @for (f of fonts; track f.id) {
            <button class="tp-font-row" [class.tp-font-row--active]="theme.font() === f.id" (click)="theme.setFont(f.id)">
              {{ f.name }}
            </button>
          }
        </div>
      }
    </div>
  `,
  styles: [`
    .tp-wrapper { position: relative; }

    .tp-trigger {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 36px;
      height: 36px;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      background: var(--color-surface);
      color: var(--color-muted);
      cursor: pointer;
      transition: border-color 0.15s, color 0.15s;

      &:hover {
        border-color: var(--color-primary-300);
        color: var(--color-primary-300);
      }
    }

    .tp-backdrop {
      position: fixed;
      inset: 0;
      z-index: 90;
    }

    .tp-dropdown {
      position: absolute;
      right: 0;
      top: calc(100% + 8px);
      z-index: 100;
      width: 220px;
      background: var(--color-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-xl);
      padding: 16px;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .tp-title {
      font-size: 15px;
      font-weight: 700;
      color: var(--color-text);
      margin-bottom: 4px;
    }

    .tp-label {
      font-size: 10.5px;
      font-weight: 600;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      color: var(--color-muted);
      margin-top: 4px;
    }

    .tp-row {
      display: flex;
      gap: 6px;
    }

    .tp-option {
      flex: 1;
      padding: 5px 0;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background: var(--color-surface-2);
      color: var(--color-muted);
      font-size: 12px;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.15s;

      &--active {
        background: var(--color-primary-50);
        border-color: var(--color-primary-300);
        color: var(--color-primary-300);
      }
    }

    .tp-color-row {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 5px 8px;
      border: 1px solid transparent;
      border-radius: var(--radius-sm);
      background: transparent;
      color: var(--color-text);
      font-size: 12px;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.15s;

      &:hover { background: var(--color-surface-2); }
      &--active {
        background: var(--color-primary-50);
        border-color: var(--color-primary-300);
      }
    }

    .tp-swatch {
      width: 14px;
      height: 14px;
      border-radius: 50%;
      border: 2px solid var(--color-border);
      flex-shrink: 0;
    }

    .tp-font-row {
      padding: 4px 8px;
      border: 1px solid transparent;
      border-radius: var(--radius-sm);
      background: transparent;
      color: var(--color-text);
      font-size: 12px;
      font-weight: 500;
      cursor: pointer;
      text-align: left;
      transition: all 0.15s;

      &:hover { background: var(--color-surface-2); }
      &--active {
        background: var(--color-primary-50);
        border-color: var(--color-primary-300);
      }
    }
  `],
})
export class ThemePickerComponent {
  readonly theme = inject(ThemeService);
  readonly open = signal(false);

  toggle(): void {
    this.open.update((v) => !v);
  }

  readonly colors: { id: ColorVariant; name: string; hex: string }[] = [
    { id: '1', name: 'Azul Clínica', hex: '#4A86C8' },
    { id: '2', name: 'Teal Saúde', hex: '#0F9B8E' },
    { id: '3', name: 'Violeta Bem-estar', hex: '#7C5CBF' },
    { id: '4', name: 'Slate Premium', hex: '#4A5568' },
  ];

  readonly fonts: { id: FontChoice; name: string }[] = [
    { id: 'jakarta', name: 'Plus Jakarta Sans' },
    { id: 'dm-sans', name: 'DM Sans' },
    { id: 'outfit', name: 'Outfit' },
  ];
}

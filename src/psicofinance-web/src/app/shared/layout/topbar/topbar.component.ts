import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemePickerComponent } from '../../components/theme-picker/theme-picker.component';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, ThemePickerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="topbar">
      <div class="topbar__left">
        <h1 class="heading-lg">{{ pageTitle() }}</h1>
      </div>

      <div class="topbar__right">
        <app-theme-picker />

        <button class="topbar__notification" aria-label="Notificações">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/>
          </svg>
          <span class="topbar__notif-dot"></span>
        </button>

        <div class="topbar__avatar">
          <span class="topbar__avatar-initials">AR</span>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .topbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 16px 24px;
      background: var(--color-bg);
    }

    .topbar__left h1 {
      color: var(--color-text);
    }

    .topbar__right {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .topbar__notification {
      position: relative;
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

    .topbar__notif-dot {
      position: absolute;
      top: 6px;
      right: 6px;
      width: 7px;
      height: 7px;
      background: var(--color-danger);
      border-radius: 50%;
      border: 1.5px solid var(--color-surface);
    }

    .topbar__avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: var(--color-primary-300);
      display: flex;
      align-items: center;
      justify-content: center;
      cursor: pointer;
    }

    .topbar__avatar-initials {
      font-size: 12px;
      font-weight: 700;
      color: #FFFFFF;
    }
  `],
})
export class TopbarComponent {
  readonly pageTitle = input('Dashboard');
}

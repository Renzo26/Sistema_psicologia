import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, TopbarComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="layout">
      <app-sidebar />
      <div class="layout__main">
        <app-topbar />
        <main id="main-content" class="layout__content">
          <router-outlet />
        </main>
      </div>
    </div>
  `,
  styles: [`
    .layout {
      display: flex;
      min-height: 100vh;
    }

    .layout__main {
      flex: 1;
      margin-left: var(--sidebar-width);
      display: flex;
      flex-direction: column;
    }

    .layout__content {
      flex: 1;
      padding: 0 24px 24px;
    }

    @media (max-width: 768px) {
      .layout__main {
        margin-left: 0;
      }
    }
  `],
})
export class MainLayoutComponent {}

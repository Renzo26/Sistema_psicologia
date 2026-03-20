import { Injectable, signal, effect } from '@angular/core';

export type ThemeMode = 'light' | 'dark';
export type ColorVariant = '1' | '2' | '3' | '4';
export type FontChoice = 'jakarta' | 'dm-sans' | 'outfit';

const STORAGE_KEYS = {
  theme: 'psico-theme',
  color: 'psico-color',
  font: 'psico-font',
} as const;

const FONT_MAP: Record<FontChoice, string> = {
  jakarta: "'Plus Jakarta Sans', sans-serif",
  'dm-sans': "'DM Sans', sans-serif",
  outfit: "'Outfit', sans-serif",
};

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly theme = signal<ThemeMode>(this.load(STORAGE_KEYS.theme, 'light') as ThemeMode);
  readonly color = signal<ColorVariant>(this.load(STORAGE_KEYS.color, '1') as ColorVariant);
  readonly font = signal<FontChoice>(this.load(STORAGE_KEYS.font, 'jakarta') as FontChoice);

  constructor() {
    effect(() => {
      const t = this.theme();
      document.documentElement.setAttribute('data-theme', t);
      localStorage.setItem(STORAGE_KEYS.theme, t);
    });

    effect(() => {
      const c = this.color();
      document.documentElement.setAttribute('data-color', c);
      localStorage.setItem(STORAGE_KEYS.color, c);
    });

    effect(() => {
      const f = this.font();
      document.documentElement.style.setProperty('--font-sans', FONT_MAP[f]);
      localStorage.setItem(STORAGE_KEYS.font, f);
    });
  }

  toggleTheme(): void {
    this.theme.update((t) => (t === 'light' ? 'dark' : 'light'));
  }

  setColor(variant: ColorVariant): void {
    this.color.set(variant);
  }

  setFont(font: FontChoice): void {
    this.font.set(font);
  }

  private load(key: string, fallback: string): string {
    if (typeof localStorage === 'undefined') return fallback;
    return localStorage.getItem(key) || fallback;
  }
}

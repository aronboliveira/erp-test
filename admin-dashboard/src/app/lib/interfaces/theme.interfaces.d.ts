import type { ThemeMode } from '../types/theme.types';

export interface ThemeProvider<TTokens> {
  supports: (mode: ThemeMode) => boolean;
  provide: (mode: ThemeMode) => TTokens;
}

export interface AppThemeTokens {
  colors: Readonly<{
    bg: string;
    border: string;
    focus: string;
    mutedText: string;
    surface: string;
    text: string;
  }>;

  chart: Readonly<{
    axis: string;
    grid: string;
    series: readonly string[];
    tooltipBg: string;
    tooltipBorder: string;
    tooltipText: string;
  }>;

  table: Readonly<{
    headBg: string;
    rowAltBg: string;
    rowHoverBg: string;
  }>;
}

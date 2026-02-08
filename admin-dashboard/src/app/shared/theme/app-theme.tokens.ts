import { ObjectMapper } from '../utils/object-mapper.adapter';
import type { AppThemeTokens } from '../../lib/interfaces/theme.interfaces';

export const LIGHT_APP_THEME: Readonly<AppThemeTokens> = ObjectMapper.deepFreeze({
  colors: {
    bg: '#0b1220',
    border: 'rgba(255,255,255,0.10)',
    focus: 'rgba(99,102,241,0.55)',
    mutedText: 'rgba(255,255,255,0.70)',
    surface: 'rgba(255,255,255,0.06)',
    text: 'rgba(255,255,255,0.92)',
  },
  chart: {
    axis: 'rgba(255,255,255,0.78)',
    grid: 'rgba(255,255,255,0.12)',
    series: ['#60a5fa', '#34d399', '#fbbf24', '#f472b6', '#a78bfa', '#fb7185', '#22d3ee'],
    tooltipBg: 'rgba(15,23,42,0.96)',
    tooltipBorder: 'rgba(255,255,255,0.16)',
    tooltipText: 'rgba(255,255,255,0.92)',
  },
  table: {
    headBg: 'rgba(255,255,255,0.08)',
    rowAltBg: 'rgba(255,255,255,0.03)',
    rowHoverBg: 'rgba(99,102,241,0.14)',
  },
});

export const DARK_APP_THEME: Readonly<AppThemeTokens> = ObjectMapper.deepFreeze({
  colors: {
    bg: '#050a14',
    border: 'rgba(255,255,255,0.12)',
    focus: 'rgba(129,140,248,0.62)',
    mutedText: 'rgba(255,255,255,0.74)',
    surface: 'rgba(255,255,255,0.07)',
    text: 'rgba(255,255,255,0.94)',
  },
  chart: {
    axis: 'rgba(255,255,255,0.82)',
    grid: 'rgba(255,255,255,0.14)',
    series: ['#93c5fd', '#6ee7b7', '#fde68a', '#f9a8d4', '#c4b5fd', '#fda4af', '#67e8f9'],
    tooltipBg: 'rgba(2,6,23,0.96)',
    tooltipBorder: 'rgba(255,255,255,0.18)',
    tooltipText: 'rgba(255,255,255,0.94)',
  },
  table: {
    headBg: 'rgba(255,255,255,0.09)',
    rowAltBg: 'rgba(255,255,255,0.035)',
    rowHoverBg: 'rgba(129,140,248,0.18)',
  },
});

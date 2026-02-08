export type ChartKind = 'revenueLine' | 'expenseBars' | 'budgetDonut';

export type NgxSeriesPoint = Readonly<{ name: string; value: number }>;

export type NgxLineSeries = Readonly<{
  name: string;
  series: readonly NgxSeriesPoint[];
}>;

export type ChartTileSpec = Readonly<{
  id: string;
  title: string;
  kind: ChartKind;
  ariaLabel: string;
}>;

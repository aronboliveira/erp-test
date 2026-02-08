import { Component, Input } from '@angular/core';
import type { ColumnDef, ValueAtPath } from './data-table.types';

@Component({
  standalone: true,
  selector: 'app-data-table',
  template: `
    <div class="tableWrap">
      <table class="table" role="table" [attr.aria-label]="ariaLabel">
        @if (caption) {
          <caption class="a11y-sr-only">
            {{
              caption
            }}
          </caption>
        }
        <thead>
          <tr role="row">
            @for (c of columns; track c.header) {
              <th
                class="table__cell table__cell--head"
                role="columnheader"
                [attr.aria-label]="c.ariaLabel || c.header"
                scope="col"
              >
                {{ c.header }}
              </th>
            }
          </tr>
        </thead>

        <tbody>
          @for (row of rows; track $index) {
            <tr class="table__row" role="row">
              @for (c of columns; track c.header) {
                <td class="table__cell" role="cell">
                  {{ cell(row, c) }}
                </td>
              }
            </tr>
          } @empty {
            <tr role="row">
              <td
                class="table__cell table__cell--empty"
                role="cell"
                [attr.colspan]="columns.length"
              >
                No data
              </td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
})
export class DataTableComponent<T extends Record<string, any>> {
  @Input() ariaLabel = 'Data table';
  @Input() caption: string | null = null;
  @Input({ required: true }) columns!: readonly ColumnDef<T>[];
  @Input({ required: true }) rows!: readonly T[];

  trackByIndex = (i: number) => i;

  cell(row: T, c: ColumnDef<T>): string {
    if (c.kind === 'render') return c.render(row);
    return String(this.#readPath<ValueAtPath<T, typeof c.path>>(row, c.path) ?? '');
  }

  #readPath<V>(o: T, p: string): V | null {
    try {
      const parts = p.split('.').filter(Boolean);
      let cur: any = o;
      for (const k of parts) cur = cur?.[k];
      return (cur ?? null) as V | null;
    } catch (e) {
      console.error('DataTable readPath failed', e);
      return null;
    }
  }
}

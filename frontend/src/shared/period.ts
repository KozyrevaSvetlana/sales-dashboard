import { useCallback, useEffect, useState } from 'react';
import type { PeriodPreset } from '../api/types';
import type { QueryParams } from '../api/client';

export interface Period {
  preset: PeriodPreset;
  /** yyyy-MM-dd, только для Custom */
  from?: string;
  to?: string;
}

export const DEFAULT_PERIOD: Period = { preset: 'Last30Days' };

export const PERIOD_PRESETS: ReadonlyArray<{ value: PeriodPreset; label: string }> = [
  { value: 'Today', label: 'Сегодня' },
  { value: 'Last7Days', label: '7 дней' },
  { value: 'Last30Days', label: '30 дней' },
  { value: 'CurrentMonth', label: 'Этот месяц' },
  { value: 'PreviousMonth', label: 'Прошлый месяц' },
  { value: 'Custom', label: 'Период…' },
];

const isPreset = (value: string | null): value is PeriodPreset =>
  PERIOD_PRESETS.some((p) => p.value === value);

export function readPeriod(search: string): Period {
  const params = new URLSearchParams(search);
  const preset = params.get('period');
  if (!isPreset(preset)) return DEFAULT_PERIOD;
  if (preset !== 'Custom') return { preset };

  const from = params.get('from');
  const to = params.get('to');
  return from && to ? { preset, from, to } : DEFAULT_PERIOD;
}

export function writePeriod(period: Period, search: string): string {
  const params = new URLSearchParams(search);
  params.set('period', period.preset);
  if (period.preset === 'Custom' && period.from && period.to) {
    params.set('from', period.from);
    params.set('to', period.to);
  } else {
    params.delete('from');
    params.delete('to');
  }
  return `?${params.toString()}`;
}

export const periodToQuery = (period: Period): QueryParams => ({
  preset: period.preset,
  from: period.preset === 'Custom' ? period.from : undefined,
  to: period.preset === 'Custom' ? period.to : undefined,
});

/**
 * Период хранится в URL — единый источник правды: ссылкой можно поделиться,
 * после перезагрузки выбранный период сохраняется, работает кнопка «назад».
 */
export function usePeriod(): [Period, (next: Period) => void] {
  const [period, setPeriod] = useState<Period>(() => readPeriod(window.location.search));

  useEffect(() => {
    const onPopState = () => setPeriod(readPeriod(window.location.search));
    window.addEventListener('popstate', onPopState);
    return () => window.removeEventListener('popstate', onPopState);
  }, []);

  const update = useCallback((next: Period) => {
    window.history.pushState(null, '', writePeriod(next, window.location.search));
    setPeriod(next);
  }, []);

  return [period, update];
}

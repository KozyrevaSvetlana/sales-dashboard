// Всё форматирование в одном месте: единые правила для денег, процентов и дат во всех блоках.
// Метрики здесь не считаются — только отображаются.

const EMPTY = '—';

const moneyFormat = new Intl.NumberFormat('ru-RU', { style: 'currency', currency: 'RUB', maximumFractionDigits: 0 });
const compactMoneyFormat = new Intl.NumberFormat('ru-RU', {
  style: 'currency',
  currency: 'RUB',
  notation: 'compact',
  maximumFractionDigits: 1,
});
const countFormat = new Intl.NumberFormat('ru-RU');
const percentFormat = new Intl.NumberFormat('ru-RU', { style: 'percent', maximumFractionDigits: 1 });
const signedPercentFormat = new Intl.NumberFormat('ru-RU', {
  style: 'percent',
  maximumFractionDigits: 1,
  signDisplay: 'exceptZero',
});
const signedNumberFormat = new Intl.NumberFormat('ru-RU', { maximumFractionDigits: 1, signDisplay: 'exceptZero' });
const dateFormat = new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'short' });

export const formatMoney = (value: number | null): string => (value === null ? EMPTY : moneyFormat.format(value));

export const formatMoneyCompact = (value: number | null): string =>
  value === null ? EMPTY : compactMoneyFormat.format(value);

export const formatCount = (value: number | null): string => (value === null ? EMPTY : countFormat.format(value));

/** 0.253 → «25,3 %» */
export const formatPercent = (fraction: number | null): string =>
  fraction === null ? EMPTY : percentFormat.format(fraction);

export type ChangeKind = 'relative' | 'points';

/** relative: 0.12 → «+12 %»; points: 0.015 → «+1,5 п.п.» */
export function formatChange(change: number | null, kind: ChangeKind = 'relative'): string {
  if (change === null) return EMPTY;
  return kind === 'points' ? `${signedNumberFormat.format(change * 100)} п.п.` : signedPercentFormat.format(change);
}

export type Trend = 'up' | 'down' | 'flat' | 'none';

export function trendOf(change: number | null): Trend {
  if (change === null) return 'none';
  if (Math.abs(change) < 0.0005) return 'flat';
  return change > 0 ? 'up' : 'down';
}

/** yyyy-MM-dd → «24 сент.» (без сдвига часового пояса: дата бизнес-дня, а не момент времени). */
export function formatDate(isoDate: string): string {
  const [year, month, day] = isoDate.split('-').map(Number);
  return dateFormat.format(new Date(year ?? 1970, (month ?? 1) - 1, day ?? 1));
}

export const formatDateRange = (from: string, to: string): string =>
  from === to ? formatDate(from) : `${formatDate(from)} — ${formatDate(to)}`;

export const initials = (fullName: string): string =>
  fullName
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('');

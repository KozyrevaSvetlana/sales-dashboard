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
// Целые: во время анимации счётчика значения дробные — показываем без дробной части.
const countFormat = new Intl.NumberFormat('ru-RU', { maximumFractionDigits: 0 });
const percentFormat = new Intl.NumberFormat('ru-RU', { style: 'percent', maximumFractionDigits: 1 });
const signedPercentFormat = new Intl.NumberFormat('ru-RU', {
  style: 'percent',
  maximumFractionDigits: 1,
  signDisplay: 'exceptZero',
});
const signedNumberFormat = new Intl.NumberFormat('ru-RU', { maximumFractionDigits: 1, signDisplay: 'exceptZero' });
const dateFormat = new Intl.DateTimeFormat('ru-RU', { day: 'numeric', month: 'short' });
const monthFormat = new Intl.DateTimeFormat('ru-RU', { month: 'short' });

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

/** yyyy-MM-01 → «сент. 26» — подпись точки помесячного графика. */
export function formatMonth(isoDate: string): string {
  const [year = 1970, month = 1] = isoDate.split('-').map(Number);
  // Intl с year: '2-digit' добавляет «г.», поэтому год дописываем сами.
  return `${monthFormat.format(new Date(year, month - 1, 1))} ${String(year % 100).padStart(2, '0')}`;
}

/**
 * «2026-09-24T14:05:00+03:00» → «24 сент., 14:05».
 * Берём дату и время как есть из строки: API уже отдаёт их в часовом поясе компании,
 * а пересчёт в часовой пояс браузера исказил бы бизнес-время.
 */
export function formatDateTime(isoDateTime: string): string {
  const [date = '', time = ''] = isoDateTime.split('T');
  return `${formatDate(date)}, ${time.slice(0, 5)}`;
}

export const initials = (fullName: string): string =>
  fullName
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? '')
    .join('');

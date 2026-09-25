import { formatChange, formatCount, formatDateTime, formatMoney, formatMonth, formatPercent, initials, trendOf } from './format';

// Intl вставляет неразрывные пробелы — нормализуем, чтобы тест проверял смысл, а не типографику.
const plain = (text: string) => text.replace(/[  ]/g, ' ');

describe('format', () => {
  it('formats money in rubles without kopecks', () => {
    expect(plain(formatMoney(1234567.89))).toBe('1 234 568 ₽');
  });

  it('shows dash for undefined values instead of 0 or NaN', () => {
    expect(formatMoney(null)).toBe('—');
    expect(formatPercent(null)).toBe('—');
    expect(formatChange(null)).toBe('—');
  });

  it('formats relative change with sign', () => {
    expect(plain(formatChange(0.123))).toBe('+12,3 %');
    expect(plain(formatChange(-0.05))).toBe('-5 %');
  });

  it('formats margin change in percentage points', () => {
    expect(plain(formatChange(0.015, 'points'))).toBe('+1,5 п.п.');
  });

  it('detects trend direction', () => {
    expect(trendOf(0.2)).toBe('up');
    expect(trendOf(-0.2)).toBe('down');
    expect(trendOf(0)).toBe('flat');
    expect(trendOf(null)).toBe('none');
  });

  it('shows business date and time from API without converting to browser time zone', () => {
    expect(plain(formatDateTime('2026-09-24T14:05:00+03:00'))).toBe('24 сент., 14:05');
  });

  it('formats month bucket label', () => {
    expect(plain(formatMonth('2026-09-01'))).toBe('сент. 26');
  });

  it('rounds counts during count-up animation', () => {
    expect(plain(formatCount(1234.6))).toBe('1 235');
  });

  it('builds initials from full name', () => {
    expect(initials('Анна Смирнова')).toBe('АС');
  });
});

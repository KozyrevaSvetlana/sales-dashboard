import { formatChange, formatMoney, formatPercent, initials, trendOf } from './format';

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

  it('builds initials from full name', () => {
    expect(initials('Анна Смирнова')).toBe('АС');
  });
});

import { formatRuDate, maskRuDate, parseRuDate } from './ruDate';

describe('ruDate', () => {
  it('inserts dots while typing and ignores non-digits', () => {
    expect(maskRuDate('2')).toBe('2');
    expect(maskRuDate('240')).toBe('24.0');
    expect(maskRuDate('24092026')).toBe('24.09.2026');
    expect(maskRuDate('24/09/2026')).toBe('24.09.2026');
    expect(maskRuDate('24.09.20261')).toBe('24.09.2026');
  });

  it('parses a complete date to ISO', () => {
    expect(parseRuDate('24.09.2026')).toBe('2026-09-24');
    expect(parseRuDate('29.02.2024')).toBe('2024-02-29');
  });

  it('rejects incomplete and non-existent dates', () => {
    expect(parseRuDate('24.09.20')).toBeNull();
    expect(parseRuDate('31.02.2026')).toBeNull();
    expect(parseRuDate('29.02.2026')).toBeNull();
    expect(parseRuDate('00.13.2026')).toBeNull();
  });

  it('formats ISO for display', () => {
    expect(formatRuDate('2026-09-24')).toBe('24.09.2026');
    expect(formatRuDate('')).toBe('');
    expect(formatRuDate(undefined)).toBe('');
  });
});

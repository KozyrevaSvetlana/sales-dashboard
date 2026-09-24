import { DEFAULT_PERIOD, periodToQuery, readPeriod, writePeriod } from './period';

describe('period in URL', () => {
  it('falls back to default for unknown or missing preset', () => {
    expect(readPeriod('')).toEqual(DEFAULT_PERIOD);
    expect(readPeriod('?period=Yesterday')).toEqual(DEFAULT_PERIOD);
  });

  it('requires both dates for custom period', () => {
    expect(readPeriod('?period=Custom&from=2026-09-01')).toEqual(DEFAULT_PERIOD);
    expect(readPeriod('?period=Custom&from=2026-09-01&to=2026-09-10')).toEqual({
      preset: 'Custom',
      from: '2026-09-01',
      to: '2026-09-10',
    });
  });

  it('round-trips through URL and drops stale custom dates', () => {
    const custom = writePeriod({ preset: 'Custom', from: '2026-09-01', to: '2026-09-10' }, '?tab=x');
    const preset = writePeriod({ preset: 'Last7Days' }, custom);

    expect(readPeriod(custom)).toEqual({ preset: 'Custom', from: '2026-09-01', to: '2026-09-10' });
    expect(preset).toBe('?tab=x&period=Last7Days');
  });

  it('sends dates to API only for custom period', () => {
    expect(periodToQuery({ preset: 'Today', from: '2026-01-01', to: '2026-01-02' })).toEqual({
      preset: 'Today',
      from: undefined,
      to: undefined,
    });
  });
});

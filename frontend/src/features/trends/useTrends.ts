import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { getJson } from '../../api/client';
import type { TrendGranularity, TrendsResponse } from '../../api/types';
import { periodToQuery, type Period } from '../../shared/period';

/** granularity = null — сервер сам выберет день/неделю/месяц по длине периода. */
export function useTrends(period: Period, granularity: TrendGranularity | null) {
  return useQuery({
    queryKey: ['trends', period, granularity],
    queryFn: ({ signal }) =>
      getJson<TrendsResponse>('/dashboard/trends', { ...periodToQuery(period), granularity: granularity ?? undefined }, signal),
    placeholderData: keepPreviousData,
  });
}

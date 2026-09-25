import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { getJson } from '../../api/client';
import type { RecentSalesResponse } from '../../api/types';
import { periodToQuery, type Period } from '../../shared/period';

export const RECENT_SALES_LIMIT = 15;

export function useRecentSales(period: Period) {
  return useQuery({
    queryKey: ['recent-sales', period],
    queryFn: ({ signal }) =>
      getJson<RecentSalesResponse>('/dashboard/sales/recent', { ...periodToQuery(period), limit: String(RECENT_SALES_LIMIT) }, signal),
    placeholderData: keepPreviousData,
  });
}

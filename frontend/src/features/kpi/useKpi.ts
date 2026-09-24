import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { getJson } from '../../api/client';
import type { KpiResponse } from '../../api/types';
import { periodToQuery, type Period } from '../../shared/period';

/** Хук — единственное место, где блок KPI знает про HTTP. Компоненты получают только данные и статусы. */
export function useKpi(period: Period) {
  return useQuery({
    queryKey: ['kpi', period],
    queryFn: ({ signal }) => getJson<KpiResponse>('/dashboard/kpi', periodToQuery(period), signal),
    placeholderData: keepPreviousData,
  });
}

import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { getJson } from '../../api/client';
import type { RankingResponse, RankingSort } from '../../api/types';
import { periodToQuery, type Period } from '../../shared/period';

export function useRanking(period: Period, sortBy: RankingSort) {
  return useQuery({
    queryKey: ['ranking', period, sortBy],
    queryFn: ({ signal }) =>
      getJson<RankingResponse>('/dashboard/managers/ranking', { ...periodToQuery(period), sortBy }, signal),
    placeholderData: keepPreviousData,
  });
}

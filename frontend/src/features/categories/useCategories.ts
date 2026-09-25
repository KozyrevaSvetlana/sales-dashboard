import { keepPreviousData, useQuery } from '@tanstack/react-query';
import { getJson } from '../../api/client';
import type { CategoriesResponse } from '../../api/types';
import { periodToQuery, type Period } from '../../shared/period';

export const TOP_PRODUCTS = 5;

export function useCategories(period: Period) {
  return useQuery({
    queryKey: ['categories', period],
    queryFn: ({ signal }) =>
      getJson<CategoriesResponse>('/dashboard/categories', { ...periodToQuery(period), top: String(TOP_PRODUCTS) }, signal),
    placeholderData: keepPreviousData,
  });
}

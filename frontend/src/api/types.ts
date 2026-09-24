// Контракт API. Сейчас описан вручную; следующий шаг — генерировать из Swagger
// (openapi-typescript), чтобы фронт и бэк не расходились.

export type PeriodPreset = 'Today' | 'Last7Days' | 'Last30Days' | 'CurrentMonth' | 'PreviousMonth' | 'Custom';
export type RankingSort = 'GrossProfit' | 'AverageCheck';

/** Даты в формате yyyy-MM-dd, включительно. */
export interface PeriodDto {
  from: string;
  to: string;
  previousFrom: string;
  previousTo: string;
}

/**
 * change: для денежных и счётных метрик — относительное изменение (0.12 = +12%),
 * для маржи — абсолютное в долях (0.015 = +1,5 п.п.). null — сравнить не с чем.
 */
export interface MetricDto {
  value: number | null;
  previous: number | null;
  change: number | null;
}

export interface TopManagerDto {
  id: number;
  fullName: string;
  grossProfit: number;
}

export interface KpiResponse {
  period: PeriodDto;
  revenue: MetricDto;
  grossProfit: MetricDto;
  margin: MetricDto;
  salesCount: MetricDto;
  averageCheck: MetricDto;
  topManager: TopManagerDto | null;
}

export interface RankingItemDto {
  rank: number;
  managerId: number;
  fullName: string;
  team: string;
  salesCount: number;
  revenue: number;
  grossProfit: number;
  margin: number | null;
  averageCheck: number | null;
  change: number | null;
}

export interface RankingResponse {
  period: PeriodDto;
  sortBy: RankingSort;
  items: RankingItemDto[];
}

// Типы контракта API. Источник правды — OpenAPI-схема бэкенда:
// schema.d.ts генерируется командой `npm run gen:api` (при запущенном API).
// Здесь только короткие имена, чтобы компоненты не зависели от формата генератора.

import type { components } from './schema';

type Schemas = components['schemas'];

export type PeriodDto = Schemas['PeriodDto'];
export type MetricDto = Schemas['MetricDto'];
export type TopManagerDto = Schemas['TopManagerDto'];
export type KpiResponse = Schemas['KpiResponse'];

export type RankingSort = Schemas['RankingSort'];
export type RankingItemDto = Schemas['RankingItemDto'];
export type RankingResponse = Schemas['RankingResponse'];

export type TrendGranularity = Schemas['TrendGranularity'];
export type TrendPointDto = Schemas['TrendPointDto'];
export type TrendsResponse = Schemas['TrendsResponse'];

export type CategorySalesDto = Schemas['CategorySalesDto'];
export type ProductSalesDto = Schemas['ProductSalesDto'];
export type CategoriesResponse = Schemas['CategoriesResponse'];

export type SaleStatus = Schemas['SaleStatus'];
export type RecentSaleDto = Schemas['RecentSaleDto'];
export type RecentSalesResponse = Schemas['RecentSalesResponse'];

/**
 * Пресет периода — это query-параметр, а не часть ответа; Swashbuckle может описать его
 * inline, без отдельной схемы. Поэтому тип задан явно и сверяется с enum PeriodPreset на бэкенде.
 */
export type PeriodPreset = 'Today' | 'Last7Days' | 'Last30Days' | 'CurrentMonth' | 'PreviousMonth' | 'Custom';

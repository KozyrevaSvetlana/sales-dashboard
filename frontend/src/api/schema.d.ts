/**
 * Типы контракта API в формате openapi-typescript.
 *
 * ⚠ Этот файл генерируется из OpenAPI-схемы бэкенда — не редактируйте его вручную:
 *     1. запустите API (http://localhost:8080);
 *     2. в папке frontend выполните `npm run gen:api`.
 *
 * Начальная версия записана вручную по C#-DTO (Features/*Response.cs), чтобы проект собирался
 * до первой генерации; первый же запуск `npm run gen:api` перезапишет файл целиком.
 * Приложение импортирует типы только через src/api/types.ts.
 */

export type paths = Record<string, never>;
export type webhooks = Record<string, never>;

export interface components {
  schemas: {
    PeriodDto: {
      /** Format: date */
      from: string;
      /** Format: date */
      to: string;
      /** Format: date */
      previousFrom: string;
      /** Format: date */
      previousTo: string;
    };
    MetricDto: {
      /** Format: double */
      value: number | null;
      /** Format: double */
      previous: number | null;
      /** Format: double */
      change: number | null;
    };
    TopManagerDto: {
      /** Format: int32 */
      id: number;
      fullName: string;
      /** Format: double */
      grossProfit: number;
    };
    KpiResponse: {
      period: components['schemas']['PeriodDto'];
      revenue: components['schemas']['MetricDto'];
      grossProfit: components['schemas']['MetricDto'];
      margin: components['schemas']['MetricDto'];
      salesCount: components['schemas']['MetricDto'];
      averageCheck: components['schemas']['MetricDto'];
      topManager: components['schemas']['TopManagerDto'] | null;
    };
    /** @enum {string} */
    RankingSort: 'GrossProfit' | 'AverageCheck';
    RankingItemDto: {
      /** Format: int32 */
      rank: number;
      /** Format: int32 */
      managerId: number;
      fullName: string;
      team: string;
      /** Format: int32 */
      salesCount: number;
      /** Format: double */
      revenue: number;
      /** Format: double */
      grossProfit: number;
      /** Format: double */
      margin: number | null;
      /** Format: double */
      averageCheck: number | null;
      /** Format: double */
      change: number | null;
    };
    RankingResponse: {
      period: components['schemas']['PeriodDto'];
      sortBy: components['schemas']['RankingSort'];
      items: components['schemas']['RankingItemDto'][];
    };
    /** @enum {string} */
    TrendGranularity: 'Day' | 'Week' | 'Month';
    TrendPointDto: {
      /** Format: date */
      date: string;
      /** Format: double */
      revenue: number;
      /** Format: double */
      grossProfit: number;
      /** Format: int32 */
      salesCount: number;
    };
    TrendsResponse: {
      period: components['schemas']['PeriodDto'];
      granularity: components['schemas']['TrendGranularity'];
      points: components['schemas']['TrendPointDto'][];
    };
    CategorySalesDto: {
      /** Format: int32 */
      categoryId: number;
      name: string;
      /** Format: double */
      revenue: number;
      /** Format: double */
      grossProfit: number;
      /** Format: double */
      margin: number | null;
      /** Format: double */
      share: number | null;
      /** Format: int32 */
      units: number;
    };
    ProductSalesDto: {
      /** Format: int32 */
      productId: number;
      name: string;
      categoryName: string;
      /** Format: int32 */
      units: number;
      /** Format: double */
      revenue: number;
      /** Format: double */
      grossProfit: number;
      /** Format: double */
      margin: number | null;
    };
    CategoriesResponse: {
      period: components['schemas']['PeriodDto'];
      categories: components['schemas']['CategorySalesDto'][];
      topProducts: components['schemas']['ProductSalesDto'][];
    };
    /** @enum {string} */
    SaleStatus: 'Paid' | 'Cancelled' | 'Refunded';
    RecentSaleDto: {
      /** Format: int32 */
      id: number;
      /** Format: date-time */
      soldAt: string;
      status: components['schemas']['SaleStatus'];
      countsTowardsRevenue: boolean;
      managerName: string;
      customerName: string;
      customerCompany: string;
      products: string[];
      /** Format: int32 */
      units: number;
      /** Format: double */
      amount: number;
      /** Format: double */
      grossProfit: number;
    };
    RecentSalesResponse: {
      period: components['schemas']['PeriodDto'];
      items: components['schemas']['RecentSaleDto'][];
    };
  };
  responses: never;
  parameters: never;
  requestBodies: never;
  headers: never;
  pathItems: never;
}

export type $defs = Record<string, never>;
export type operations = Record<string, never>;

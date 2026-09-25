import type { RecentSaleDto, SaleStatus } from '../../api/types';
import { formatDateTime, formatMoney } from '../../shared/format';
import type { Period } from '../../shared/period';
import { AsyncBlock } from '../../shared/ui/AsyncBlock';
import { Skeleton } from '../../shared/ui/States';
import { RECENT_SALES_LIMIT, useRecentSales } from './useRecentSales';

const STATUS_LABELS: Record<SaleStatus, string> = {
  Paid: 'Оплачена',
  Cancelled: 'Отменена',
  Refunded: 'Возврат',
};

export function RecentSales({ period }: { period: Period }) {
  const query = useRecentSales(period);

  return (
    <section className="card panel" aria-labelledby="recent-sales-title">
      <header className="panel__header">
        <h2 id="recent-sales-title">Последние продажи</h2>
        <span className="muted">до {RECENT_SALES_LIMIT} за период, все статусы</span>
      </header>

      <AsyncBlock
        query={query}
        skeleton={<SalesSkeleton />}
        isEmpty={(data) => data.items.length === 0}
        emptyMessage="За выбранный период продаж нет"
      >
        {(data) => (
          <table className="table">
            <thead>
              <tr>
                <th>Дата</th>
                <th>Менеджер / клиент</th>
                <th>Товары</th>
                <th>Статус</th>
                <th className="num">Сумма</th>
                <th className="num">Прибыль</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((sale) => (
                <SaleRow key={sale.id} sale={sale} />
              ))}
            </tbody>
          </table>
        )}
      </AsyncBlock>
    </section>
  );
}

function SaleRow({ sale }: { sale: RecentSaleDto }) {
  const [firstProduct, ...otherProducts] = sale.products;
  // Отмены и возвраты не входят в выручку — показываем суммы зачёркнутыми, чтобы это было видно.
  const amountClass = sale.countsTowardsRevenue ? 'num' : 'num is-excluded';

  return (
    <tr className={sale.countsTowardsRevenue ? undefined : 'is-dimmed'}>
      <td className="nowrap">{formatDateTime(sale.soldAt)}</td>
      <td>
        <p className="person__name">{sale.managerName}</p>
        <p className="muted">
          {sale.customerName} · {sale.customerCompany}
        </p>
      </td>
      <td>
        {firstProduct ?? '—'}
        {otherProducts.length > 0 && <span className="muted"> +{otherProducts.length}</span>}
      </td>
      <td>
        <span className={`badge badge--${sale.status.toLowerCase()}`}>{STATUS_LABELS[sale.status]}</span>
      </td>
      <td className={amountClass} title={sale.countsTowardsRevenue ? undefined : 'Не учитывается в выручке'}>
        {formatMoney(sale.amount)}
      </td>
      <td className={amountClass}>{formatMoney(sale.grossProfit)}</td>
    </tr>
  );
}

function SalesSkeleton() {
  return (
    <div className="stack">
      {Array.from({ length: 8 }, (_, i) => (
        <Skeleton key={i} height={36} />
      ))}
    </div>
  );
}

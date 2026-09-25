import type { CategoriesResponse } from '../../api/types';
import { formatCount, formatMoney, formatPercent } from '../../shared/format';
import type { Period } from '../../shared/period';
import { AsyncBlock } from '../../shared/ui/AsyncBlock';
import { Skeleton } from '../../shared/ui/States';
import { useCategories } from './useCategories';

export function CategoryBreakdown({ period }: { period: Period }) {
  const query = useCategories(period);

  return (
    <section className="card panel" aria-labelledby="categories-title">
      <header className="panel__header">
        <h2 id="categories-title">Категории и товары</h2>
      </header>

      <AsyncBlock
        query={query}
        skeleton={<CategoriesSkeleton />}
        isEmpty={(data) => data.categories.every((c) => c.revenue === 0)}
        emptyMessage="За выбранный период нет оплаченных продаж ни в одной категории"
      >
        {(data) => <CategoriesContent data={data} />}
      </AsyncBlock>
    </section>
  );
}

function CategoriesContent({ data }: { data: CategoriesResponse }) {
  const maxRevenue = Math.max(...data.categories.map((c) => c.revenue), 1);

  return (
    <div className="stack stack--loose">
      <ul className="bars" aria-label="Выручка по категориям">
        {data.categories.map((category) => (
          <li key={category.categoryId} className={category.revenue === 0 ? 'bars__row is-dimmed' : 'bars__row'}>
            <div className="bars__head">
              <span className="bars__name">{category.name}</span>
              <span className="num">
                {formatMoney(category.revenue)}
                <span className="muted"> · {formatPercent(category.share)}</span>
              </span>
            </div>
            <div className="bars__track" aria-hidden="true">
              <div className="bars__fill" style={{ width: `${(category.revenue / maxRevenue) * 100}%` }} />
            </div>
            <p className="muted">
              прибыль {formatMoney(category.grossProfit)} · маржа {formatPercent(category.margin)} · {formatCount(category.units)} шт.
            </p>
          </li>
        ))}
      </ul>

      <div>
        <h3 className="panel__subtitle">Топ товаров по выручке</h3>
        <table className="table">
          <thead>
            <tr>
              <th>Товар</th>
              <th className="num">Шт.</th>
              <th className="num">Выручка</th>
              <th className="num">Маржа</th>
            </tr>
          </thead>
          <tbody>
            {data.topProducts.map((product) => (
              <tr key={product.productId}>
                <td>
                  <p className="person__name">{product.name}</p>
                  <p className="muted">{product.categoryName}</p>
                </td>
                <td className="num">{formatCount(product.units)}</td>
                <td className="num">{formatMoney(product.revenue)}</td>
                <td className="num">{formatPercent(product.margin)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function CategoriesSkeleton() {
  return (
    <div className="stack">
      {Array.from({ length: 6 }, (_, i) => (
        <Skeleton key={i} height={28} />
      ))}
    </div>
  );
}

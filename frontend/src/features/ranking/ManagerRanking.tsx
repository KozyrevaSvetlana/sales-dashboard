import { useState } from 'react';
import type { RankingItemDto, RankingSort } from '../../api/types';
import { formatCount, formatMoney, formatPercent, initials } from '../../shared/format';
import type { Period } from '../../shared/period';
import { AsyncBlock } from '../../shared/ui/AsyncBlock';
import { Delta } from '../../shared/ui/Delta';
import { Skeleton } from '../../shared/ui/States';
import { useRanking } from './useRanking';

const SORT_OPTIONS: ReadonlyArray<{ value: RankingSort; label: string }> = [
  { value: 'GrossProfit', label: 'По прибыли' },
  { value: 'AverageCheck', label: 'По среднему чеку' },
];

export function ManagerRanking({ period }: { period: Period }) {
  const [sortBy, setSortBy] = useState<RankingSort>('GrossProfit');
  const query = useRanking(period, sortBy);

  return (
    <section className="card panel" aria-labelledby="ranking-title">
      <header className="panel__header">
        <h2 id="ranking-title">Рейтинг менеджеров</h2>
        <div className="segmented segmented--small" role="radiogroup" aria-label="Сортировка рейтинга">
          {SORT_OPTIONS.map((option) => (
            <button
              key={option.value}
              type="button"
              role="radio"
              aria-checked={sortBy === option.value}
              className={sortBy === option.value ? 'segmented__item is-active' : 'segmented__item'}
              onClick={() => setSortBy(option.value)}
            >
              {option.label}
            </button>
          ))}
        </div>
      </header>

      <AsyncBlock
        query={query}
        skeleton={<RankingSkeleton />}
        isEmpty={(data) => data.items.every((item) => item.salesCount === 0)}
        emptyMessage="За выбранный период ни у одного менеджера нет оплаченных продаж"
      >
        {(data) => <RankingTable items={data.items} />}
      </AsyncBlock>
    </section>
  );
}

function RankingTable({ items }: { items: RankingItemDto[] }) {
  return (
    <table className="table">
      <thead>
        <tr>
          <th className="num">#</th>
          <th>Менеджер</th>
          <th className="num">Продажи</th>
          <th className="num">Выручка</th>
          <th className="num">Прибыль</th>
          <th className="num">Ср. чек</th>
          <th className="num">Маржа</th>
          <th className="num">Динамика</th>
        </tr>
      </thead>
      <tbody>
        {items.map((item) => (
          <tr key={item.managerId} className={item.salesCount === 0 ? 'is-dimmed' : undefined}>
            <td className="num">{item.rank}</td>
            <td>
              <div className="person">
                <span className="avatar avatar--small">{initials(item.fullName)}</span>
                <div>
                  <p className="person__name">{item.fullName}</p>
                  <p className="muted">{item.team}</p>
                </div>
              </div>
            </td>
            <td className="num">{formatCount(item.salesCount)}</td>
            <td className="num">{formatMoney(item.revenue)}</td>
            <td className="num">{formatMoney(item.grossProfit)}</td>
            <td className="num">{formatMoney(item.averageCheck)}</td>
            <td className="num">{formatPercent(item.margin)}</td>
            <td className="num">
              <Delta change={item.change} />
            </td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

function RankingSkeleton() {
  return (
    <div className="stack">
      {Array.from({ length: 8 }, (_, i) => (
        <Skeleton key={i} height={36} />
      ))}
    </div>
  );
}

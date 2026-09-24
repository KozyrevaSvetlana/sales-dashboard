import type { KpiResponse, MetricDto } from '../../api/types';
import { formatCount, formatMoney, formatMoneyCompact, formatPercent, initials, type ChangeKind } from '../../shared/format';
import type { Period } from '../../shared/period';
import { AsyncBlock } from '../../shared/ui/AsyncBlock';
import { Delta } from '../../shared/ui/Delta';
import { Skeleton } from '../../shared/ui/States';
import { useKpi } from './useKpi';

interface CardSpec {
  key: keyof Pick<KpiResponse, 'revenue' | 'grossProfit' | 'margin' | 'salesCount' | 'averageCheck'>;
  label: string;
  format: (value: number | null) => string;
  changeKind?: ChangeKind;
}

// Конфигурация вместо пяти почти одинаковых компонентов.
const CARDS: CardSpec[] = [
  { key: 'revenue', label: 'Выручка', format: formatMoney },
  { key: 'grossProfit', label: 'Валовая прибыль', format: formatMoney },
  { key: 'margin', label: 'Маржа', format: formatPercent, changeKind: 'points' },
  { key: 'salesCount', label: 'Продажи', format: formatCount },
  { key: 'averageCheck', label: 'Средний чек', format: formatMoney },
];

export function KpiCards({ period }: { period: Period }) {
  const query = useKpi(period);

  return (
    <section className="kpi-grid" aria-label="Ключевые показатели">
      <AsyncBlock query={query} skeleton={<KpiSkeleton />}>
        {(data) => (
          <div className="kpi-grid__inner">
            {CARDS.map((card) => (
              <KpiCard key={card.key} label={card.label} metric={data[card.key]} format={card.format} changeKind={card.changeKind} />
            ))}
            <TopManagerCard data={data} />
          </div>
        )}
      </AsyncBlock>
    </section>
  );
}

interface KpiCardProps {
  label: string;
  metric: MetricDto;
  format: (value: number | null) => string;
  changeKind?: ChangeKind;
}

function KpiCard({ label, metric, format, changeKind }: KpiCardProps) {
  return (
    <article className="card kpi-card">
      <p className="kpi-card__label">{label}</p>
      <p className="kpi-card__value">{format(metric.value)}</p>
      <p className="kpi-card__footer">
        <Delta change={metric.change} kind={changeKind} />
        <span className="muted">было {format(metric.previous)}</span>
      </p>
    </article>
  );
}

function TopManagerCard({ data }: { data: KpiResponse }) {
  const top = data.topManager;
  return (
    <article className="card kpi-card">
      <p className="kpi-card__label">Лучший менеджер</p>
      {top ? (
        <div className="top-manager">
          <span className="avatar">{initials(top.fullName)}</span>
          <div>
            <p className="top-manager__name">{top.fullName}</p>
            <p className="muted">прибыль {formatMoneyCompact(top.grossProfit)}</p>
          </div>
        </div>
      ) : (
        <p className="muted">Нет продаж за период</p>
      )}
    </article>
  );
}

function KpiSkeleton() {
  return (
    <div className="kpi-grid__inner">
      {Array.from({ length: CARDS.length + 1 }, (_, i) => (
        <div key={i} className="card kpi-card">
          <Skeleton width="50%" />
          <Skeleton height={28} width="80%" />
          <Skeleton width="40%" />
        </div>
      ))}
    </div>
  );
}

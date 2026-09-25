import { useMemo, useState } from 'react';
import type { TrendGranularity, TrendPointDto, TrendsResponse } from '../../api/types';
import { formatCount, formatDate, formatMoney, formatMoneyCompact, formatMonth } from '../../shared/format';
import { useElementWidth } from '../../shared/hooks/useElementWidth';
import type { Period } from '../../shared/period';
import { AsyncBlock } from '../../shared/ui/AsyncBlock';
import { Skeleton } from '../../shared/ui/States';
import { areaPath, labelEvery, linePath, linearScale, nearestIndex, niceTicks, spreadX } from './chartMath';
import { useTrends } from './useTrends';

type MetricKey = 'revenue' | 'grossProfit' | 'salesCount';

const METRICS: ReadonlyArray<{ key: MetricKey; label: string; format: (v: number | null) => string; axis: (v: number) => string }> = [
  { key: 'revenue', label: 'Выручка', format: formatMoney, axis: formatMoneyCompact },
  { key: 'grossProfit', label: 'Прибыль', format: formatMoney, axis: formatMoneyCompact },
  { key: 'salesCount', label: 'Продажи', format: formatCount, axis: (v) => formatCount(v) },
];

const GRANULARITIES: ReadonlyArray<{ value: TrendGranularity | null; label: string }> = [
  { value: null, label: 'Авто' },
  { value: 'Day', label: 'Дни' },
  { value: 'Week', label: 'Недели' },
  { value: 'Month', label: 'Месяцы' },
];

const HEIGHT = 260;
const MARGIN = { top: 16, right: 16, bottom: 28, left: 72 };

export function TrendChart({ period }: { period: Period }) {
  const [metric, setMetric] = useState<MetricKey>('revenue');
  const [granularity, setGranularity] = useState<TrendGranularity | null>(null);
  const query = useTrends(period, granularity);

  return (
    <section className="card panel" aria-labelledby="trends-title">
      <header className="panel__header">
        <h2 id="trends-title">Динамика</h2>
        <div className="panel__controls">
          <Segmented label="Метрика" options={METRICS.map((m) => ({ value: m.key, label: m.label }))} value={metric} onChange={setMetric} />
          <Segmented label="Детализация" options={GRANULARITIES} value={granularity} onChange={setGranularity} />
        </div>
      </header>

      <AsyncBlock
        query={query}
        skeleton={<Skeleton height={HEIGHT} />}
        isEmpty={(data) => data.points.every((p) => p.salesCount === 0)}
        emptyMessage="За выбранный период нет оплаченных продаж — графику нечего показать"
      >
        {(data) => <Chart data={data} metric={metric} />}
      </AsyncBlock>
    </section>
  );
}

function Chart({ data, metric }: { data: TrendsResponse; metric: MetricKey }) {
  const [containerRef, width] = useElementWidth<HTMLDivElement>();
  const [hovered, setHovered] = useState<number | null>(null);
  const spec = METRICS.find((m) => m.key === metric) ?? METRICS[0]!;

  const geometry = useMemo(() => {
    const values = data.points.map((p) => p[metric]);
    const ticks = niceTicks(values);
    const y = linearScale(ticks[0] ?? 0, ticks[ticks.length - 1] ?? 1, HEIGHT - MARGIN.bottom, MARGIN.top);
    const xs = spreadX(data.points.length, MARGIN.left, width - MARGIN.right);
    const points = xs.map((x, i) => ({ x, y: y(values[i] ?? 0) }));
    return { ticks, y, xs, points, baseline: y(0) };
  }, [data.points, metric, width]);

  const every = labelEvery(data.points.length, width - MARGIN.left - MARGIN.right);
  const active = hovered === null ? null : data.points[hovered];
  const activePoint = hovered === null ? null : geometry.points[hovered];

  return (
    <div className="trend-chart" ref={containerRef}>
      <svg
        width={width}
        height={HEIGHT}
        role="img"
        aria-label={`График: ${spec.label.toLowerCase()} по ${granularityLabel(data.granularity)}`}
        onMouseMove={(event) => {
          const box = event.currentTarget.getBoundingClientRect();
          setHovered(nearestIndex(geometry.xs, event.clientX - box.left));
        }}
        onMouseLeave={() => setHovered(null)}
      >
        {geometry.ticks.map((tick) => (
          <g key={tick}>
            <line className="trend-chart__grid" x1={MARGIN.left} x2={width - MARGIN.right} y1={geometry.y(tick)} y2={geometry.y(tick)} />
            <text className="trend-chart__axis" x={MARGIN.left - 8} y={geometry.y(tick)} textAnchor="end" dominantBaseline="middle">
              {spec.axis(tick)}
            </text>
          </g>
        ))}

        {data.points.map((point, i) =>
          i % every === 0 ? (
            <text key={point.date} className="trend-chart__axis" x={geometry.xs[i]} y={HEIGHT - 8} textAnchor="middle">
              {bucketLabel(point, data.granularity)}
            </text>
          ) : null,
        )}

        {/* key по метрике и периоду — при смене данные «прорисовываются» заново (CSS-анимация) */}
        <g key={`${metric}-${data.period.from}-${data.granularity}`} className="trend-chart__series">
          <path className="trend-chart__area" d={areaPath(geometry.points, geometry.baseline)} />
          <path className="trend-chart__line" d={linePath(geometry.points)} pathLength={1} />
        </g>

        {activePoint && (
          <g className="trend-chart__cursor">
            <line x1={activePoint.x} x2={activePoint.x} y1={MARGIN.top} y2={HEIGHT - MARGIN.bottom} />
            <circle cx={activePoint.x} cy={activePoint.y} r={4.5} />
          </g>
        )}
      </svg>

      {active && activePoint && (
        <div
          className="trend-chart__tooltip"
          style={{ left: Math.min(Math.max(activePoint.x, 90), width - 90), top: Math.max(activePoint.y - 12, 0) }}
          role="status"
        >
          <p className="trend-chart__tooltip-title">{bucketLabel(active, data.granularity, true)}</p>
          <p>Выручка: <strong>{formatMoney(active.revenue)}</strong></p>
          <p>Прибыль: <strong>{formatMoney(active.grossProfit)}</strong></p>
          <p>Продажи: <strong>{formatCount(active.salesCount)}</strong></p>
        </div>
      )}
    </div>
  );
}

interface SegmentedProps<T> {
  label: string;
  options: ReadonlyArray<{ value: T; label: string }>;
  value: T;
  onChange: (value: T) => void;
}

function Segmented<T>({ label, options, value, onChange }: SegmentedProps<T>) {
  return (
    <div className="segmented segmented--small" role="radiogroup" aria-label={label}>
      {options.map((option) => (
        <button
          key={option.label}
          type="button"
          role="radio"
          aria-checked={option.value === value}
          className={option.value === value ? 'segmented__item is-active' : 'segmented__item'}
          onClick={() => onChange(option.value)}
        >
          {option.label}
        </button>
      ))}
    </div>
  );
}

function bucketLabel(point: TrendPointDto, granularity: TrendGranularity, long = false): string {
  switch (granularity) {
    case 'Month':
      return formatMonth(point.date);
    case 'Week':
      return long ? `Неделя с ${formatDate(point.date)}` : formatDate(point.date);
    default:
      return formatDate(point.date);
  }
}

const granularityLabel = (granularity: TrendGranularity) =>
  granularity === 'Month' ? 'месяцам' : granularity === 'Week' ? 'неделям' : 'дням';

import { formatChange, trendOf, type ChangeKind } from '../format';

interface DeltaProps {
  change: number | null;
  kind?: ChangeKind;
}

/** Бейдж динамики: цвет и стрелка зависят от знака, «—» если сравнивать не с чем. */
export function Delta({ change, kind = 'relative' }: DeltaProps) {
  const trend = trendOf(change);
  const arrow = trend === 'up' ? '▲' : trend === 'down' ? '▼' : '';

  return (
    <span
      className={`delta delta--${trend}`}
      title={change === null ? 'Нет данных за предыдущий период' : 'К предыдущему периоду'}
    >
      {arrow && <span aria-hidden="true">{arrow} </span>}
      {formatChange(change, kind)}
    </span>
  );
}

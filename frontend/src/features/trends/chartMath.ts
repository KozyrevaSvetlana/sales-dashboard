// Чистые функции построения графика: шкалы, «красивые» деления оси, SVG-пути.
// Вынесены из компонента, чтобы их можно было проверить тестами без DOM.

export interface Point {
  x: number;
  y: number;
}

/** Шаг деления оси: 1, 2, 2.5 или 5 × 10ⁿ — чтобы подписи были круглыми. */
export function niceStep(rawStep: number): number {
  if (rawStep <= 0 || !Number.isFinite(rawStep)) return 1;
  const magnitude = 10 ** Math.floor(Math.log10(rawStep));
  const normalized = rawStep / magnitude;
  const nice = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 2.5 ? 2.5 : normalized <= 5 ? 5 : 10;
  return nice * magnitude;
}

/**
 * Деления оси Y от нуля (или от минимума, если есть отрицательные значения — например, убыток)
 * до «круглого» максимума, покрывающего все данные.
 */
export function niceTicks(values: readonly number[], count = 4): number[] {
  const max = Math.max(0, ...values);
  const min = Math.min(0, ...values);
  if (max === min) return [0, 1];

  const step = niceStep((max - min) / count);
  const start = Math.floor(min / step) * step;
  const end = Math.ceil(max / step) * step;

  const ticks: number[] = [];
  for (let tick = start; tick <= end + step / 2; tick += step) {
    ticks.push(Number(tick.toFixed(10)));
  }
  return ticks;
}

/** Линейная шкала: значение из [d0, d1] → координата в [r0, r1]. */
export function linearScale(d0: number, d1: number, r0: number, r1: number) {
  const span = d1 - d0 || 1;
  return (value: number) => r0 + ((value - d0) / span) * (r1 - r0);
}

/** Координаты X точек равномерно по ширине; одна точка — по центру. */
export function spreadX(count: number, left: number, right: number): number[] {
  if (count <= 0) return [];
  if (count === 1) return [(left + right) / 2];
  const step = (right - left) / (count - 1);
  return Array.from({ length: count }, (_, i) => left + i * step);
}

export function linePath(points: readonly Point[]): string {
  return points.map((p, i) => `${i === 0 ? 'M' : 'L'}${round(p.x)},${round(p.y)}`).join(' ');
}

/** Заливка под линией до базовой линии (y нуля). */
export function areaPath(points: readonly Point[], baselineY: number): string {
  const first = points[0];
  const last = points[points.length - 1];
  if (!first || !last) return '';
  return `${linePath(points)} L${round(last.x)},${round(baselineY)} L${round(first.x)},${round(baselineY)} Z`;
}

/** Индекс ближайшей по X точки — для подсказки при наведении. */
export function nearestIndex(xs: readonly number[], x: number): number {
  let best = 0;
  xs.forEach((value, i) => {
    if (Math.abs(value - x) < Math.abs((xs[best] ?? 0) - x)) best = i;
  });
  return best;
}

/** Какие подписи оси X показывать, чтобы они не налезали друг на друга. */
export function labelEvery(count: number, width: number, minLabelWidth = 64): number {
  const fits = Math.max(1, Math.floor(width / minLabelWidth));
  return Math.max(1, Math.ceil(count / fits));
}

const round = (value: number) => Math.round(value * 10) / 10;

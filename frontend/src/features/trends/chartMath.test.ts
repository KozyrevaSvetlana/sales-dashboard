import { areaPath, labelEvery, linePath, linearScale, nearestIndex, niceStep, niceTicks, spreadX } from './chartMath';

describe('chartMath', () => {
  it('rounds axis step to 1/2/2.5/5 × 10ⁿ', () => {
    expect(niceStep(0.8)).toBe(1);
    expect(niceStep(1.7)).toBe(2);
    expect(niceStep(2.3)).toBe(2.5);
    expect(niceStep(3200)).toBe(5000);
    expect(niceStep(0)).toBe(1);
  });

  it('builds ticks from zero that cover the maximum', () => {
    expect(niceTicks([0, 130, 410])).toEqual([0, 200, 400, 600]);
  });

  it('includes negative values (loss) in ticks', () => {
    const ticks = niceTicks([-50, 100]);
    expect(ticks[0]).toBeLessThanOrEqual(-50);
    expect(ticks[ticks.length - 1]).toBeGreaterThanOrEqual(100);
    expect(ticks).toContain(0);
  });

  it('returns a valid axis for all-zero data (empty period)', () => {
    expect(niceTicks([0, 0, 0])).toEqual([0, 1]);
  });

  it('maps values linearly and inverts Y for SVG', () => {
    const y = linearScale(0, 100, 200, 0);
    expect(y(0)).toBe(200);
    expect(y(50)).toBe(100);
    expect(y(100)).toBe(0);
  });

  it('spreads points across the width, single point centered', () => {
    expect(spreadX(3, 0, 100)).toEqual([0, 50, 100]);
    expect(spreadX(1, 0, 100)).toEqual([50]);
    expect(spreadX(0, 0, 100)).toEqual([]);
  });

  it('builds line and closed area paths', () => {
    const points = [{ x: 0, y: 10 }, { x: 50, y: 5 }];
    expect(linePath(points)).toBe('M0,10 L50,5');
    expect(areaPath(points, 20)).toBe('M0,10 L50,5 L50,20 L0,20 Z');
    expect(areaPath([], 20)).toBe('');
  });

  it('finds the nearest point for hover', () => {
    expect(nearestIndex([0, 50, 100], 70)).toBe(1);
    expect(nearestIndex([0, 50, 100], 90)).toBe(2);
  });

  it('thins out x-axis labels so they do not overlap', () => {
    expect(labelEvery(7, 800)).toBe(1);
    expect(labelEvery(30, 640)).toBe(3);
  });
});

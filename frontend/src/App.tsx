import { CategoryBreakdown } from './features/categories/CategoryBreakdown';
import { KpiCards } from './features/kpi/KpiCards';
import { PeriodPicker } from './features/period/PeriodPicker';
import { ManagerRanking } from './features/ranking/ManagerRanking';
import { RecentSales } from './features/sales/RecentSales';
import { TrendChart } from './features/trends/TrendChart';
import { usePeriod } from './shared/period';

export function App() {
  const [period, setPeriod] = usePeriod();

  return (
    <div className="layout">
      <header className="topbar">
        <div>
          <h1 className="topbar__title">Эффективность продаж</h1>
          <p className="muted">Все суммы — по оплаченным продажам, без отмен и возвратов</p>
        </div>
        <PeriodPicker value={period} onChange={setPeriod} />
      </header>

      <main className="dashboard">
        <KpiCards period={period} />
        <TrendChart period={period} />
        <ManagerRanking period={period} />

        <div className="dashboard__row dashboard__row--split">
          <CategoryBreakdown period={period} />
          <RecentSales period={period} />
        </div>
      </main>
    </div>
  );
}

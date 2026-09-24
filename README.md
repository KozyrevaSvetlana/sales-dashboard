# Sales Performance Dashboard

Дашборд эффективности продаж: KPI за период с динамикой, рейтинг менеджеров, (в работе) тренды, категории, последние продажи.

**Стек:** ASP.NET Core 8 (Minimal API) · EF Core 8 · PostgreSQL 16 · React 18 + TypeScript · TanStack Query · Docker Compose.

## Запуск

```bash
docker compose up --build
```

- Дашборд: http://localhost:3000
- Swagger: http://localhost:8080/swagger

При старте API сам применяет миграции и, если БД пустая, генерирует данные. Ручных шагов нет.

Сбросить данные: `docker compose down -v && docker compose up --build`.

### Первый запуск (однократно, для разработчика)

Скелет собирался без .NET SDK, поэтому первая миграция написана вручную и без ModelSnapshot. Перед дальнейшей работой пересоздайте её штатно:

```bash
cd backend
dotnet tool install --global dotnet-ef   # если ещё не установлен
rm -r src/SalesDashboard.Api/Infrastructure/Persistence/Migrations
dotnet ef migrations add InitialCreate -p src/SalesDashboard.Api -o Infrastructure/Persistence/Migrations
dotnet build && dotnet test
```

И во фронтенде: `cd frontend && npm install && npm test`, а затем закоммитьте `package-lock.json`.

### Локальная разработка без Docker

```bash
docker compose up db                          # только PostgreSQL
cd backend && dotnet run --project src/SalesDashboard.Api   # http://localhost:8080
cd frontend && npm run dev                    # http://localhost:5173, /api проксируется на :8080
```

## Бизнес-правила

| Правило | Решение | Где в коде |
|---|---|---|
| Какие продажи учитываются | Только `Paid`. `Cancelled` не состоялась. `Refunded` — полный возврат: выручка и прибыль по ней 0, в количество продаж не входит | `Domain/Analytics/SaleRules.cs` (одно место) |
| Выручка | Σ quantity × unit_price по оплаченным продажам | `Infrastructure/Persistence/SalesQueries.cs` |
| Себестоимость | Σ quantity × unit_cost. Цена и себестоимость — снимок на момент продажи в `sale_items` | там же |
| Валовая прибыль | Выручка − себестоимость | `KpiCalculator` |
| Маржа | Прибыль / выручка. При нулевой выручке «—», не 0% | `KpiCalculator` |
| Средний чек | Выручка / число оплаченных продаж. При 0 продаж «—» | `KpiCalculator` |
| Динамика | Относительная к предыдущему периоду. Для маржи — в п.п. При нулевой базе «—» | `KpiCalculator` |
| Периоды | Бизнес-даты в часовом поясе компании (`Dashboard:TimeZone`, по умолчанию Europe/Moscow), интервал `[from, to)` | `Domain/Periods` |
| Предыдущий период | Та же длина вплотную перед текущим. «Этот месяц» сравнивается с теми же днями прошлого месяца. «Прошлый месяц» — с позапрошлым целиком | `PeriodResolver` |
| Рейтинг при равенстве | Одинаковое место (1, 1, 3). Порядок внутри — по выручке, затем по имени | `ManagerRanking` |
| Менеджер без продаж | Остаётся в рейтинге с нулями. Уволенные показываются, только если у них есть продажи в периоде | `RankingService` |

## Архитектура и решения

```
backend/src/SalesDashboard.Api
  Domain/          сущности, периоды, правила и расчёты — чистый C#, без EF и HTTP
  Features/        по папке на блок дашборда: Endpoints + Service + Response DTO
  Infrastructure/  DbContext, конфигурации, миграции, сид
  Common/          сквозное: ошибки → ProblemDetails
frontend/src
  api/             HTTP-клиент и типы контракта
  shared/          форматирование, период в URL, UI-состояния (AsyncBlock)
  features/        по папке на блок: хук данных + компоненты
```

- **Один проект API с папками вместо четырёх проектов Clean Architecture.** Для объёма задачи этого достаточно: слои разделены, код легко найти, накладных расходов нет. Домен не зависит от EF и HTTP, поэтому его легко вынести в отдельный проект, если понадобится.
- **Vertical slices** (`Features/*`). Одна фича соответствует одному блоку UI и одному эндпоинту. Раздельные эндпоинты позволяют блокам загружаться и падать независимо.
- **Без Generic Repository и MediatR.** `DbContext` уже является Unit of Work и репозиторием. Сервис фичи строит `IQueryable` и агрегирует в БД.
- **Бизнес-правила — чистые функции** (`KpiCalculator`, `ManagerRanking`, `PeriodResolver`), которые покрыты юнит-тестами без БД.
- **`TimeProvider` вместо `DateTime.Now`**, поэтому «сегодня» подменяется в тестах.
- **Ошибки в едином формате ProblemDetails (RFC 7807)**; фронт показывает `detail` пользователю.
- **Фронт: TanStack Query** даёт кэш, отмену запросов и `keepPreviousData`, чтобы при смене периода не было белого экрана. `AsyncBlock` реализует состояния загрузки, ошибки и пустых данных один раз для всех блоков. Период хранится в URL.

### Индексы

| Индекс | Зачем |
|---|---|
| `sales(status, sold_at_utc)` | KPI, категории: «оплаченные за период» |
| `sales(manager_id, sold_at_utc)` | рейтинг: группировка по менеджеру в периоде (покрывает и FK) |
| `sales(sold_at_utc)` | лента последних продаж, тренды |
| FK-индексы `sale_items(sale_id)`, `sale_items(product_id)`, `products(category_id)` | джойны |

TODO: подтвердить планы запросов через `EXPLAIN ANALYZE` на сиде.

### Данные (seed)

20 менеджеров с явными профилями (`Star`, `Solid`, `Discounter` с низкой маржой, `Newbie`; у троих провал в одном из месяцев), 80 клиентов (розница, SMB, enterprise), около 25 товаров в 8 категориях с разной маржой. Всего около 4 тыс. продаж за 12 месяцев с сезонностью, выходными, отменами, возвратами и редкими очень крупными заказами. Генератор детерминирован: фиксированный seed; даты привязаны к «сегодня».

## Тесты

```bash
cd backend && dotnet test
cd frontend && npm test
```

- **Бэкенд:** расчёт KPI (включая пустой период и отрицательную маржу), правило статусов, пресеты периодов и границы месяцев, часовой пояс, рейтинг (сортировка, равенство, менеджер без продаж, динамика).
- **Фронт:** форматирование, период в URL, все состояния `AsyncBlock`.

## Статус и что осталось

Готово в скелете:
- [x] Docker Compose, автоматическая миграция и сид
- [x] Модель данных, индексы
- [x] KPI с динамикой, рейтинг с двумя режимами сортировки
- [x] Периоды: пресеты и произвольный диапазон, сравнение с предыдущим
- [x] Состояния UI: загрузка, обновление, ошибка, пусто

Дальше:
- [ ] Пересоздать миграцию через `dotnet ef` (см. «Первый запуск»), включить `TreatWarningsAsErrors`
- [ ] Тренды: `GET /api/dashboard/trends?granularity=day|week` (`date_trunc` в SQL) и график (Recharts)
- [ ] Категории и топ товаров: `GET /api/dashboard/categories`
- [ ] Последние продажи: `GET /api/dashboard/sales/recent?limit=20` (проекция в DTO, без N+1)
- [ ] Интеграционные тесты агрегатов на реальном PostgreSQL (Testcontainers)
- [ ] Генерация TS-типов из Swagger (openapi-typescript)
- [ ] Анимация перестановки строк рейтинга, count-up в KPI

## Работа с AI

Промпты: [`AI_PROMPTS.md`](AI_PROMPTS.md). Что сгенерировано, что проверено и что изменено вручную: [`AI_NOTES.md`](AI_NOTES.md).

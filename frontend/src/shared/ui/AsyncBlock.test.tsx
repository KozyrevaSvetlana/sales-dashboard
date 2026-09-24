import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import type { UseQueryResult } from '@tanstack/react-query';
import { AsyncBlock } from './AsyncBlock';

type Query = UseQueryResult<string[], Error>;

// Минимальная заглушка результата React Query: AsyncBlock читает только эти поля.
const query = (overrides: Partial<Query>): Query =>
  ({
    isPending: false,
    isError: false,
    isFetching: false,
    isPlaceholderData: false,
    data: undefined,
    error: null,
    refetch: vi.fn(),
    ...overrides,
  }) as unknown as Query;

const renderBlock = (q: Query) =>
  render(
    <AsyncBlock query={q} skeleton={<p>loading…</p>} isEmpty={(d) => d.length === 0} emptyMessage="Нет продаж">
      {(data) => <ul>{data.map((x) => <li key={x}>{x}</li>)}</ul>}
    </AsyncBlock>,
  );

describe('AsyncBlock', () => {
  it('shows skeleton on first load', () => {
    renderBlock(query({ isPending: true }));
    expect(screen.getByText('loading…')).toBeInTheDocument();
  });

  it('shows error with retry that refetches', async () => {
    const refetch = vi.fn();
    renderBlock(query({ isError: true, error: new Error('Сервер недоступен'), refetch }));

    expect(screen.getByRole('alert')).toHaveTextContent('Сервер недоступен');
    await userEvent.click(screen.getByRole('button', { name: 'Повторить' }));
    expect(refetch).toHaveBeenCalledOnce();
  });

  it('shows explicit empty state instead of a blank block', () => {
    renderBlock(query({ data: [] }));
    expect(screen.getByText('Нет продаж')).toBeInTheDocument();
  });

  it('keeps previous data visible while a new period is loading', () => {
    const { container } = renderBlock(query({ data: ['A'], isFetching: true, isPlaceholderData: true }));

    expect(screen.getByText('A')).toBeInTheDocument();
    expect(container.querySelector('.is-refreshing')).not.toBeNull();
  });
});

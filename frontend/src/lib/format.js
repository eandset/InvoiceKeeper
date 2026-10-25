const currencySymbol = { RUB: '₽', USD: '$', EUR: '€' };

export function money(amount, currency) {
  const value = Number(amount || 0).toLocaleString('ru-RU', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  });
  return `${value} ${currencySymbol[currency] || currency || ''}`.trim();
}

export function date(iso) {
  if (!iso) return '—';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return '—';
  return d.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

export const periodicityLabel = {
  monthly: 'Ежемесячно',
  weekly: 'Еженедельно',
  quarterly: 'Ежеквартально',
  yearly: 'Ежегодно'
};

export function statusOf(occurrence) {
  if (occurrence.isPaid || occurrence.status === 'Paid') return { key: 'paid', label: 'Оплачено' };
  if (occurrence.isOverdue || occurrence.status === 'Overdue') return { key: 'overdue', label: 'Просрочено' };
  return { key: 'unpaid', label: 'Не оплачено' };
}

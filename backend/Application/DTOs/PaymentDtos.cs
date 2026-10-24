using System.ComponentModel.DataAnnotations;

namespace InvoiceKeeper.Application.DTOs;
using InvoiceKeeper.Application.Validation;

public record CreatePaymentDto(
    [Required, MaxLength(255)] string Title,
    [Range(0.01, 1_000_000_000)] decimal Amount,
    [Required, AllowedValues("RUB", "USD", "EUR")] string Currency,
    [Required] DateTime NextPaymentDate,
    [MaxLength(20)] string? Periodicity = null);

public record PaymentOccurrenceResponseDto(
    Guid Id,
    Guid RecurringPaymentId,
    string Title,
    decimal Amount,
    string Currency,
    DateTime DueDate,
    string Status,
    DateTime? ActualPaymentDate,
    bool IsOverdue,
    int DaysUntilDue);

public record PaymentResponseDto(
    Guid Id,
    string Title,
    decimal Amount,
    string Currency,
    string Periodicity,
    DateTime NextPaymentDate,
    bool IsDeleted,
    DateTime CreatedAt,
    PaymentOccurrenceResponseDto? NextOccurrence,
    int PaidOccurrencesCount,
    int UnpaidOccurrencesCount);

public record UpdatePaymentStatusDto(
    bool IsPaid,
    DateTime? ActualPaymentDate);

public record ReminderOccurrenceDto(
    Guid Id,
    Guid RecurringPaymentId,
    string Title,
    decimal Amount,
    string Currency,
    DateTime DueDate,
    int DaysUntilDue,
    bool IsOverdue);

public record PaymentSummaryDto(
    decimal TotalRub,
    decimal TotalUsd,
    decimal TotalEur,
    int PendingCount,
    int OverdueCount);

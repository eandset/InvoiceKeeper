namespace InvoiceKeeper.Application.DTOs;

public record OccurrenceFilterDto(
    bool? IsPaid = null,
    bool? IsOverdue = null,
    DateTime? DueBefore = null,
    DateTime? DueAfter = null);

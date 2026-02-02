namespace VmsService.DTOs;

public record CreateRequisitionDto(string Skill, int RequiredCount, Guid EngagementId);

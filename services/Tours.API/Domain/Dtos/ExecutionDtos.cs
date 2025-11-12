namespace Tours.API.Domain.Dtos;

public record StartExecDto(Guid TourId);
public record PollDto(Guid ExecutionId);
public record FinishDto(Guid ExecutionId, bool Success);
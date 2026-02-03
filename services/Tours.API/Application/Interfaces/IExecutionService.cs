using Tours.API.Domain.Dtos;
using Tours.API.Domain.Models;

namespace Tours.API.Application.Interfaces;

public interface IExecutionService
{
    Task<Guid> StartAsync(Guid userId, StartExecDto dto);
    Task<object> PollAsync(Guid userId, PollDto dto);
    Task FinishAsync(Guid userId, FinishDto dto);
    Task<List<TourExecution>> GetUserExecutionsAsync(Guid userId);
}
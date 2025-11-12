using Tours.API.Domain.Dtos;
using Tours.API.Domain.Models;

namespace Tours.API.Application.Interfaces;

public interface IPositionService
{
    Task SetAsync(Guid userId, SetPosDto dto);
    Task<UserPosition?> GetAsync(Guid userId);
}
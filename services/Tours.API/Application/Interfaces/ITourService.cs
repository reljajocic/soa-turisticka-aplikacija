using Tours.API.Domain.Dtos;
using Tours.API.Domain.Models;

namespace Tours.API.Application.Interfaces;

public interface ITourService
{
    Task<Guid> CreateAsync(Guid authorId, CreateTourDto dto);
    Task<bool> AddKeypointAsync(Guid tourId, CreateKeypointDto dto);
}
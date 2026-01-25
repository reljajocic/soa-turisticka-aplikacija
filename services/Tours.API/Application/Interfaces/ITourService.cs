using Tours.API.Domain.Dtos;
using Tours.API.Domain.Enums;
using Tours.API.Domain.Models;

namespace Tours.API.Application.Interfaces;

public interface ITourService
{
    // Osnovno
    Task<Guid> CreateAsync(Guid authorId, CreateTourDto dto);
    Task<bool> AddKeypointAsync(Guid tourId, CreateKeypointDto dto);

    // Čitanje
    Task<IEnumerable<TourDto>> GetAllAsync();
    Task<IEnumerable<TourDto>> GetByAuthorAsync(Guid authorId);

    // CRUD + Status (Ovo ti je falilo)
    Task<bool> UpdateStatusAsync(Guid id, TourStatus status);
    Task<bool> DeleteAsync(Guid id, Guid authorId);
    Task<bool> UpdateAsync(Guid id, Guid authorId, CreateTourDto dto);
}
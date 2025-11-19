using OSFRS.Backend.DTOs;

namespace OSFRS.Backend.Interfaces;

public interface IBaseReadService<TDto>
{
    Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
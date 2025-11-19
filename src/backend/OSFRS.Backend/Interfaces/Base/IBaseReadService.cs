namespace OSFRS.Backend.Interfaces.Base;

public interface IBaseReadService<TDto>
{
    Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
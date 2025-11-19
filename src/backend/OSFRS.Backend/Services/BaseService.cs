using OSFRS.Backend.Interfaces;

namespace OSFRS.Backend.Services;

public class BaseService<TEntity, TDto> : IBaseService<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    protected readonly IBaseRepository<TEntity> _repo;
    protected readonly Func<TEntity, TDto> _mapToDto;

    public BaseService(IBaseRepository<TEntity> repo, Func<TEntity, TDto> mapToDto)
    {
        _repo = repo;
        _mapToDto = mapToDto;
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repo.GetAllAsync(cancellationToken);
        return entities.Select(_mapToDto);
    }

    public virtual async Task<TDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.GetByIdAsync(id, cancellationToken);
        return entity is null ? null : _mapToDto(entity);
    }

    public TDto MapToDto(TEntity entity) => _mapToDto(entity);
}
using Hangfire.Storage.Monitoring;
using OSFRS.Backend.Interfaces;

namespace OSFRS.Backend.Services;

public class BaseCrudService<TEntity, TCreateDto, TUpdateDto, TDto>
    : BaseService<TEntity, TDto>, ICrudService<TCreateDto, TUpdateDto, TDto>
    where TEntity : class
    where TDto : class
{
    private readonly Func<TCreateDto, TEntity> _mapFromCreate;
    private readonly Action<TEntity, TUpdateDto> _mapFromUpdate;

    public BaseCrudService(
        IBaseRepository<TEntity> repo,
        Func<TEntity, TDto> mapToDto,
        Func<TCreateDto, TEntity> mapFromCreate,
        Action<TEntity, TUpdateDto> mapFromUpdate
    ) : base(repo, mapToDto)
    {
        _mapFromCreate = mapFromCreate;
        _mapFromUpdate = mapFromUpdate;
    }

    public virtual async Task<TDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapFromCreate(dto);
        await _repo.AddAsync(entity, cancellationToken);
        await _repo.SaveChangesAsync(cancellationToken);
        return _mapToDto(entity);
    }

    public virtual async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return false;

        _repo.Remove(entity);
        await _repo.SaveChangesAsync(cancellationToken);

        return true;
    }

    public virtual async Task<TDto?> UpdateAsync(int id, TUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repo.GetByIdAsync(id, cancellationToken);
        if (entity is null)
            return null;

        _mapFromUpdate(entity, dto);
        _repo.Update(entity);
        await _repo.SaveChangesAsync(cancellationToken);

        return _mapToDto(entity);
    }
}
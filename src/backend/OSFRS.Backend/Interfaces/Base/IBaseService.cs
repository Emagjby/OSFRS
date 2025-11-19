namespace OSFRS.Backend.Interfaces;

public interface IBaseService<TEntity, TDto> : IBaseReadService<TDto>
{
    TDto MapToDto(TEntity entity);
}
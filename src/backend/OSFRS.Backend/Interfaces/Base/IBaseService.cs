namespace OSFRS.Backend.Interfaces.Base;

public interface IBaseService<TEntity, TDto> : IBaseReadService<TDto>
{
    TDto MapToDto(TEntity entity);
}
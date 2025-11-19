namespace OSFRS.Backend.Interfaces.Base;

public interface ICrudService<TCreateDto, TUpdateDto, TDto> : IBaseReadService<TDto>
{
    Task<TDto> CreateAsync(TCreateDto dto, CancellationToken cancellationToken = default);
    Task<TDto?> UpdateAsync(int id, TUpdateDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
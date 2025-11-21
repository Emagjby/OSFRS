namespace OSFRS.Backend.Interfaces.Validator;

public interface IUpdateValidator<T, TEntity>
{
    Task ValidateAsync(T instance, TEntity existing);
}
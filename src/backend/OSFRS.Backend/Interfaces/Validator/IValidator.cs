namespace OSFRS.Backend.Interfaces.Validator;

public interface IValidator<T>
{
    Task ValidateAsync(T instance);
}
namespace AlAtaaClinic.Application.Common.ExceptionHandling;

public interface IExceptionHandler
{
    ErrorDto Handle(Exception exception);
}

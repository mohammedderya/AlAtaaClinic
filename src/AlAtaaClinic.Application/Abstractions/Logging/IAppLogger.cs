namespace AlAtaaClinic.Application.Abstractions.Logging;

public interface IAppLogger<T>
{
    void Information(string message);
    void Warning(string message);
    void Error(Exception exception, string message);
}

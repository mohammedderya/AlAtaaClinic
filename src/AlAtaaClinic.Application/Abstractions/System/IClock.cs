namespace AlAtaaClinic.Application.Abstractions.System;

public interface IClock
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}

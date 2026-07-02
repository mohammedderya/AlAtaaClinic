namespace AlAtaaClinic.Application.Common.ExceptionHandling;

public sealed record ErrorDto(string Code, string Message, IReadOnlyDictionary<string, string[]> Details);

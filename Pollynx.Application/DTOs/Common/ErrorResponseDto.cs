namespace Pollynx.Application.DTOs.Common;

public class ErrorResponseDto
{
    public string Code { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? TraceId { get; set; }

    public DateTime Timestamp { get; set; }
}
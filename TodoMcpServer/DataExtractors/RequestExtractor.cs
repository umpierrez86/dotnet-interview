using System.Text.Json;
using FluentValidation;

namespace TodoMcpServer.DataExtractors;

public class RequestExtractor<TDto>
{
    private readonly IValidator<IReadOnlyDictionary<string, JsonElement>> _argsValidator;
    private readonly IValidator<TDto> _objectValidator;
    private readonly Func<IReadOnlyDictionary<string, JsonElement>, TDto> _dtoFactory;

    public RequestExtractor(
        IValidator<IReadOnlyDictionary<string, JsonElement>> argsValidator,
        IValidator<TDto> objectValidator,
        Func<IReadOnlyDictionary<string, JsonElement>, TDto> dtoFactory)
    {
        _argsValidator = argsValidator;
        _objectValidator = objectValidator;
        _dtoFactory = dtoFactory;
    }

    public async Task<TDto> ExtractAsync(
        IReadOnlyDictionary<string, JsonElement> arguments,
        CancellationToken cancellationToken)
    {
        var argsValidation = await _argsValidator.ValidateAsync(arguments, cancellationToken);
        if (!argsValidation.IsValid)
            throw new ArgumentException(string.Join(", ", argsValidation.Errors.Select(e => e.ErrorMessage)));

        var dto = _dtoFactory(arguments);

        var dtoValidation = await _objectValidator.ValidateAsync(dto, cancellationToken);
        if (!dtoValidation.IsValid)
            throw new ArgumentException(string.Join(", ", dtoValidation.Errors.Select(e => e.ErrorMessage)));

        return dto;
    }
}
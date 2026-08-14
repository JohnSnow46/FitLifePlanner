using Microsoft.JSInterop;

namespace FitLifePlanner.Tests.Web;

internal sealed class FakeJsRuntime : IJSRuntime
{
    public Dictionary<string, string> Storage { get; } = new();

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        switch (identifier)
        {
            case "localStorage.getItem":
                var getKey = (string)args![0]!;
                var stored = Storage.TryGetValue(getKey, out var value) ? value : null;
                return ValueTask.FromResult((TValue)(object)stored!);

            case "localStorage.setItem":
                var setKey = (string)args![0]!;
                var setValue = (string)args[1]!;
                Storage[setKey] = setValue;
                return ValueTask.FromResult(default(TValue)!);

            case "localStorage.removeItem":
                var removeKey = (string)args![0]!;
                Storage.Remove(removeKey);
                return ValueTask.FromResult(default(TValue)!);

            default:
                throw new NotSupportedException($"Identifier '{identifier}' is not supported by {nameof(FakeJsRuntime)}.");
        }
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        => InvokeAsync<TValue>(identifier, args);
}

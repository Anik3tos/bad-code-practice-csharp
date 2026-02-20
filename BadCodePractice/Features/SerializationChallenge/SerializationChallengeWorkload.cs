using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace BadCodePractice.Features.SerializationChallenge;

internal static class SerializationChallengeWorkload
{
    internal static async Task ExecuteAsync<T>(
        IReadOnlyList<T> items,
        int maxConcurrency,
        Func<T, CancellationToken, Task> operation,
        CancellationToken cancellationToken)
    {
        var tasks = new List<Task>(items.Count);
        using var gate = new SemaphoreSlim(maxConcurrency);

        foreach (var item in items)
        {
            await gate.WaitAsync(cancellationToken);
            var captured = item;

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await operation(captured, cancellationToken);
                }
                finally
                {
                    gate.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
    }
}

internal static class SerializationPayloadFactory
{
    internal static IReadOnlyList<SerializationRichPayload> Create(int count, int extraFieldLength)
    {
        var payloads = new List<SerializationRichPayload>(count);
        for (var i = 0; i < count; i++)
        {
            var detailBlob = new string((char)('A' + i % 6), extraFieldLength);

            payloads.Add(new SerializationRichPayload(
                OrderId: i + 1,
                CustomerId: $"CUST-{10000 + i}",
                CustomerName: $"Customer {i % 200}",
                City: $"City-{i % 30}",
                Total: 25 + i % 1000,
                Currency: "USD",
                OrderedAt: DateTime.UtcNow.AddMinutes(-i),
                ShippingAddress: $"{i % 500} Main Street, District {i % 15}",
                BillingAddress: $"{i % 500} Main Street, District {i % 15}",
                Notes: $"order-notes-{i}-{detailBlob}",
                InternalStatus: i % 3 == 0 ? "ManualReview" : "Ready",
                SalesRep: $"rep-{i % 40}",
                Region: $"R-{i % 8}",
                Campaign: $"CMP-{i % 25}",
                DeviceFingerprint: $"{i:X8}-{i % 1024:X4}",
                Tags: new[]
                {
                    $"tier-{i % 5}",
                    $"source-{i % 4}",
                    $"segment-{i % 9}"
                },
                Metadata: new Dictionary<string, string>
                {
                    ["source"] = i % 2 == 0 ? "web" : "mobile",
                    ["channel"] = i % 3 == 0 ? "ads" : "organic",
                    ["regionHint"] = $"hint-{i % 7}",
                    ["trace"] = $"trace-{i % 1000}"
                }));
        }

        return payloads;
    }

    internal static SerializationLeanPayload ToLean(SerializationRichPayload payload)
    {
        return new SerializationLeanPayload(
            payload.OrderId,
            payload.CustomerName,
            payload.City,
            payload.Total);
    }
}

internal sealed class ReflectionDictionarySerializer(bool useMetadataCache)
{
    private readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();
    private int _cacheMisses;

    internal int CacheMisses => _cacheMisses;

    internal string Serialize(object value)
    {
        var properties = ResolveProperties(value.GetType());
        var dictionary = new Dictionary<string, object?>(properties.Length);
        foreach (var property in properties)
        {
            dictionary[property.Name] = property.GetValue(value);
        }

        return JsonSerializer.Serialize(dictionary);
    }

    private PropertyInfo[] ResolveProperties(Type type)
    {
        if (!useMetadataCache)
        {
            Interlocked.Increment(ref _cacheMisses);
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        return _propertyCache.GetOrAdd(type, t =>
        {
            Interlocked.Increment(ref _cacheMisses);
            return t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        });
    }
}

public sealed record SerializationRichPayload(
    int OrderId,
    string CustomerId,
    string CustomerName,
    string City,
    decimal Total,
    string Currency,
    DateTime OrderedAt,
    string ShippingAddress,
    string BillingAddress,
    string Notes,
    string InternalStatus,
    string SalesRep,
    string Region,
    string Campaign,
    string DeviceFingerprint,
    string[] Tags,
    Dictionary<string, string> Metadata);

public sealed record SerializationLeanPayload(
    int OrderId,
    string CustomerName,
    string City,
    decimal Total);

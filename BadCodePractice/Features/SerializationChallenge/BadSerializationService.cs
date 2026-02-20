using System.Text.Json;

namespace BadCodePractice.Features.SerializationChallenge;

public sealed class BadSerializationService : SerializationServiceBase
{
    public override string Name => "Bad serialization strategy";

    protected override async Task<int> ExecuteCoreAsync(
        IReadOnlyList<SerializationRichPayload> payloads,
        SerializationChallengeOptions options,
        CancellationToken cancellationToken)
    {
        var serializer = new ReflectionDictionarySerializer(useMetadataCache: false);

        await SerializationChallengeWorkload.ExecuteAsync(
            payloads,
            options.ConcurrentWorkers,
            (payload, _) =>
            {
                // Anti-patterns: reflection in hot path, synchronous serialize/deserialize, over-serialization.
                var json = serializer.Serialize(payload);
                var roundTrip = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
                if (roundTrip is null)
                {
                    throw new InvalidOperationException("Round-trip failed.");
                }

                return Task.CompletedTask;
            },
            cancellationToken);

        return serializer.CacheMisses;
    }
}

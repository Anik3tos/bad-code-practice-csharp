using System.Text.Json;

namespace BadCodePractice.Features.SerializationChallenge;

public sealed class RefactoredSerializationService : SerializationServiceBase
{
    public override string Name => "Refactored serialization strategy";

    protected override async Task<int> ExecuteCoreAsync(
        IReadOnlyList<SerializationRichPayload> payloads,
        SerializationChallengeOptions options,
        CancellationToken cancellationToken)
    {
        await SerializationChallengeWorkload.ExecuteAsync(
            payloads,
            options.ConcurrentWorkers,
            async (payload, ct) =>
            {
                var leanPayload = SerializationPayloadFactory.ToLean(payload);

                // Refactor: reduced payload, async APIs, and source-generated metadata (AOT-friendly).
                await using var stream = new MemoryStream(capacity: 256);
                await JsonSerializer.SerializeAsync(
                    stream,
                    leanPayload,
                    SerializationChallengeJsonContext.Default.SerializationLeanPayload,
                    ct);

                stream.Position = 0;
                var roundTrip = await JsonSerializer.DeserializeAsync(
                    stream,
                    SerializationChallengeJsonContext.Default.SerializationLeanPayload,
                    ct);

                if (roundTrip is null || roundTrip.OrderId <= 0)
                {
                    throw new InvalidOperationException("Round-trip failed.");
                }
            },
            cancellationToken);

        return 0;
    }
}

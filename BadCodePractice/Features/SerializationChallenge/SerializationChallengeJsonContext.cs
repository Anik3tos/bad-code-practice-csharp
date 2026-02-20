using System.Text.Json.Serialization;

namespace BadCodePractice.Features.SerializationChallenge;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(SerializationLeanPayload))]
internal partial class SerializationChallengeJsonContext : JsonSerializerContext
{
}

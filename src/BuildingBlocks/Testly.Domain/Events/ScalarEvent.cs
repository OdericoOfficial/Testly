namespace Testly.Domain.Events
{
    public record struct ScalarEvent
    {
        public int Index { get; init; }

        public float Avg { get; init; }

        public float Mid { get; init; }

        public float Min { get; init; }

        public float Max { get; init; }

        public float Std { get; init; }
    }
}

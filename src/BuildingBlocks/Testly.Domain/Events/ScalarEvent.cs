namespace Testly.Domain.Events
{
    public class ScalarEvent
    {
        public int Index { get; set; }

        public float Avg { get; set; }

        public float Mid { get; set; }

        public float Min { get; set; }

        public float Max { get; set; }

        public float Std { get; set; }
    }
}

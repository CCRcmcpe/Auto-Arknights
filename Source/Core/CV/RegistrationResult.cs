namespace REVUnit.AutoArknights.Core.CV
{
    public class RegistrationResult
    {
        public RegistrationResult(Quadrilateral region, double confidence)
        {
            Region = region;
            Confidence = confidence;
        }

        public double Confidence { get; }

        public Quadrilateral Region { get; }
    }
}
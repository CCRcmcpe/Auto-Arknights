namespace REVUnit.AutoArknights.Core.CV
{
    public class RegistrationResult
    {
        public RegistrationResult(Quadrilateral32 region, double confidence)
        {
            Region = region;
            Confidence = confidence;
        }

        public double Confidence { get; }

        public Quadrilateral32 Region { get; }
    }
}
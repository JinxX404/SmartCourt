using SmartCourt.Common.Enums;

namespace SmartCourt.Features.Case.BusinessRules
{
    public class MatchingStrategy
    {
        public double LocationWeight { get; init; }
        public double ExperienceWeight { get; init; }
        public double RatingWeight { get; init; }
        public double ResponseTimeWeight { get; init; }

        public static MatchingStrategy GetStrategy(CaseComplexity complexity)
        {
            MatchingStrategy strategy = complexity switch
            {
                CaseComplexity.Routine => new()
                {
                    LocationWeight = 0.45,
                    ExperienceWeight = 0.20,
                    RatingWeight = 0.25,
                    ResponseTimeWeight = 0.10
                },

                CaseComplexity.Standard => new()
                {
                    LocationWeight = 0.30,
                    ExperienceWeight = 0.40,
                    RatingWeight = 0.20,
                    ResponseTimeWeight = 0.10
                },

                CaseComplexity.Advanced => new()
                {
                    LocationWeight = 0.20,
                    ExperienceWeight = 0.55,
                    RatingWeight = 0.15,
                    ResponseTimeWeight = 0.10
                },

                CaseComplexity.Exceptional => new()
                {
                    LocationWeight = 0.05,
                    ExperienceWeight = 0.70,
                    RatingWeight = 0.20,
                    ResponseTimeWeight = 0.05
                }
            };

            return strategy;
        }
    }
}

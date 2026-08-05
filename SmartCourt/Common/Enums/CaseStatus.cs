namespace SmartCourt.Common.Enums
{
    public enum CaseStatus : byte
    {
        /// <summary>Case created, not yet submitted.</summary>
        Draft = 0,

        /// <summary>Case submitted for AI review.</summary>
        Submitted = 1,

        /// <summary>AI review completed. Client can edit or finalize.</summary>
        Reviewed = 2,

        /// <summary>Client finalized. Triggers analysis + matching pipeline.</summary>
        FinalSubmitted = 3,

        /// <summary>AI analysis complete. CaseProfile created.</summary>
        Analyzed = 4,

        /// <summary>Matching complete. Recommendations available.</summary>
        Matched = 5,

        /// <summary>Case lifecycle ended.</summary>
        Closed = 6,
    }
}

namespace SmartCourt.Common.Options
{
    public sealed class SupabaseOptions
    {
        public const string SectionName = "Supabase";
        public string Url { get; init; }
        public string ApiKey { get; init; }
        public string Bucket { get; init; }
    }
}
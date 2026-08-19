using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCourt.Features.ChatAgent.Entities;
using System;

namespace SmartCourt.Features.ChatAgent.Configurations;

public class ModelPricingConfiguration : IEntityTypeConfiguration<ModelPricing>
{
    public void Configure(EntityTypeBuilder<ModelPricing> builder)
    {
        builder.ToTable("ModelPricings");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.InputPricePerMillion).HasPrecision(18, 6);
        builder.Property(x => x.OutputPricePerMillion).HasPrecision(18, 6);

        // Seed Initial Pricing for Singapore Region
        builder.HasData(
            new ModelPricing
            {
                Id = 1,
                ModelName = "qwen-flash",
                Region = "Singapore",
                InputPricePerMillion = 0.022m,
                OutputPricePerMillion = 0.216m,
                EffectiveFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EffectiveTo = null,
                IsActive = true
            },
            new ModelPricing
            {
                Id = 2,
                ModelName = "text-embedding-v4",
                Region = "Singapore",
                InputPricePerMillion = 0.07m,
                OutputPricePerMillion = 0.0m, // Embeddings don't have output cost
                EffectiveFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EffectiveTo = null,
                IsActive = true
            },
            new ModelPricing
            {
                Id = 3,
                ModelName = "qwen3-rerank",
                Region = "Singapore",
                InputPricePerMillion = 0.10m,
                OutputPricePerMillion = 0.0m, // Reranker doesn't have output cost
                EffectiveFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                EffectiveTo = null,
                IsActive = true
            }
        );
    }
}

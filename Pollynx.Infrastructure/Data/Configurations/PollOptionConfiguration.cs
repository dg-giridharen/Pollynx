using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pollynx.Domain.Entities;

namespace Pollynx.Infrastructure.Data.Configurations;

public class PollOptionConfiguration : IEntityTypeConfiguration<PollOption>
{
    public void Configure(EntityTypeBuilder<PollOption> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OptionText)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => new
        {
            x.PollId,
            x.OptionText
        })
        .IsUnique();

        builder.HasMany(x => x.Votes)
            .WithOne(x => x.PollOption)
            .HasForeignKey(x => x.PollOptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

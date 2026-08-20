using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pollynx.Domain.Entities;

namespace Pollynx.Infrastructure.Data.Configurations;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.Property(x => x.EndTime)
            .IsRequired();

        builder.Property(x => x.IsPublic)
            .IsRequired();

        builder.Property(x => x.IsClosed)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasMany(x => x.Options)
            .WithOne(x => x.Poll)
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Votes)
            .WithOne(x => x.Poll)
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

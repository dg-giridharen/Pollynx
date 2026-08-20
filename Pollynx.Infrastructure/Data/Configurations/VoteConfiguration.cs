using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pollynx.Domain.Entities;

namespace Pollynx.Infrastructure.Data.Configurations;

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Poll)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.PollId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PollOption)
            .WithMany(x => x.Votes)
            .HasForeignKey(x => x.PollOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.UserId,
            x.PollId
        })
        .IsUnique();
    }
}

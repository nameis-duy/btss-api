using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure
{
    //public class AccountConfig : IEntityTypeConfiguration<Account>
    //{
    //    public void Configure(EntityTypeBuilder<Account> builder)
    //    {
    //        builder.OwnsOne(a => a.Statistics, b => b.ToJson());
    //    }
    //}
    //public class TransactionConfig : IEntityTypeConfiguration<Transaction>
    //{
    //    public void Configure(EntityTypeBuilder<Transaction> builder)
    //    {
    //        builder.HasOne(t => t.Sender).WithMany(a => a.OutboundTransactions).OnDelete(DeleteBehavior.Cascade);
    //        builder.HasOne(t => t.Receiver).WithMany(a => a.InboundTransactions).OnDelete(DeleteBehavior.Cascade);
    //    }
    //}
    public class DestinationConfig : IEntityTypeConfiguration<Destination>
    {
        public void Configure(EntityTypeBuilder<Destination> builder)
        {
            builder.HasGeneratedTsVectorColumn(d => d.NameVector, "simple", d => new { d.Name, d.UnaccentName })
                   .HasIndex(d => d.NameVector)
                   .HasMethod("GIN");
            builder.HasIndex(d => d.UnaccentName).HasMethod("GIN").HasOperators("gin_trgm_ops");
        }
    }
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.OwnsMany(o => o.Traces, b => b.ToJson());
        }
    }
    public class PlanConfig : IEntityTypeConfiguration<Plan>
    {
        public void Configure(EntityTypeBuilder<Plan> builder)
        {
            builder.HasMany(p => p.Copies).WithOne(p => p.Source).HasForeignKey(p => p.SourceId);
        }
    }
}

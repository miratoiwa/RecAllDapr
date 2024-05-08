using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RecAll.Contrib.MaksedTestList.Api.Models;

namespace RecAll.Contrib.MaksedTestList.Api.Services;

public class MaskedTestListContext: DbContext {
    public DbSet<Models.MaskedTestList> MaskedTestLists { get; set; }

    public MaskedTestListContext(DbContextOptions<MaskedTestListContext> options) :
        base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.ApplyConfiguration(new MaskedTestListConfiguration());
    }
}

public class MaskedTestListConfiguration : IEntityTypeConfiguration<Models.MaskedTestList> {
    public void Configure(EntityTypeBuilder<Models.MaskedTestList> builder) {
        builder.ToTable("maskedtestlists");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).UseHiLo("maskedtestlist_hilo");

        builder.Property(p => p.MaskedId).IsRequired(false);
        builder.HasIndex(p => p.MaskedId).IsUnique();

        builder.Property(p => p.Content).IsRequired();
        builder.Property(p => p.MaskedContent).IsRequired();

        builder.Property(p => p.UserIdentityGuid).IsRequired();
        builder.HasIndex(p => p.UserIdentityGuid).IsUnique(false);

        builder.Property(p => p.IsDeleted).IsRequired();
    }
}

public class
    MaskedTestListContextDesignFactory : IDesignTimeDbContextFactory<
    MaskedTestListContext> {
    public MaskedTestListContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<MaskedTestListContext>()
            .UseSqlServer(
                "Server=.;Initial Catalog=RecAll.MaskedTestListDb;Integrated Security=true")
            .Options);
}
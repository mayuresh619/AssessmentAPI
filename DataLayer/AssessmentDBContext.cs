using System;
using AssessmentAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace AssessmentAPI.DataLayer
{
    public partial class AssessmentDBContext : DbContext
    {
        IConfiguration _config;
        public AssessmentDBContext(IConfiguration configuration)
        {
            _config = configuration;
        }

        public AssessmentDBContext(DbContextOptions<AssessmentDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Attributes> Attributes { get; set; }
        public virtual DbSet<Batch> Batch { get; set; }
        public virtual DbSet<BusinessUnits> BusinessUnits { get; set; }
        public virtual DbSet<Files> Files { get; set; }
        public virtual DbSet<Groups> Groups { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(_config.GetValue<string>(BatchConstants.CONNECTION_STRING));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Attributes>(entity =>
            {
                entity.HasKey(e => e.AttributeId)
                    .HasName("PK__Attribut__C18929EAF4D09FE8");

                entity.Property(e => e.BatchId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Key).IsUnicode(false);

                entity.Property(e => e.Value).IsUnicode(false);

                entity.HasOne(d => d.Batch)
                    .WithMany(p => p.Attributes)
                    .HasForeignKey(d => d.BatchId)
                    .HasConstraintName("FK__Attribute__Batch__693CA210");

                entity.HasOne(d => d.File)
                    .WithMany(p => p.Attributes)
                    .HasForeignKey(d => d.FileId)
                    .HasConstraintName("FK__Attribute__FileI__6A30C649");
            });

            modelBuilder.Entity<Batch>(entity =>
            {
                entity.Property(e => e.BatchId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.BatchPublishedDate).HasColumnType("datetime");

                entity.Property(e => e.ExpiryDate).HasColumnType("datetime");

                entity.Property(e => e.ReadGroups).IsUnicode(false);

                entity.Property(e => e.ReadUsers).IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<BusinessUnits>(entity =>
            {
                entity.HasKey(e => e.BusinessUnitId)
                    .HasName("PK__Business__19FA599D9183A212");

                entity.Property(e => e.BusinessUnitName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Files>(entity =>
            {
                entity.HasKey(e => e.FileId)
                    .HasName("PK__Files__6F0F98BF4C59A799");

                entity.Property(e => e.BatchId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FileName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Hash)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Links).IsUnicode(false);

                entity.Property(e => e.Mimetype)
                    .HasColumnName("MIMEType")
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Groups>(entity =>
            {
                entity.HasKey(e => e.GroupId)
                    .HasName("PK__Groups__149AF36AFEA65DF1");

                entity.Property(e => e.GroupName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__Users__1788CC4C547DB646");

                entity.Property(e => e.UserName)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

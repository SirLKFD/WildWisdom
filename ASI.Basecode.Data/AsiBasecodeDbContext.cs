using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data
{
    public partial class AsiBasecodeDBContext : DbContext
    {
        //public AsiBasecodeDBContext()
        //{
        //}

        public AsiBasecodeDBContext(DbContextOptions<AsiBasecodeDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<Admin> Admins { get; set; }
        public virtual DbSet<OrgBooks> OrgBooks { get; set; }
        public virtual DbSet<BorrowTicket> BorrowTickets { get; set; }
        public virtual DbSet<Publisher> Publishers { get; set; }
        public virtual DbSet<BookCategory> BookCategories { get; set; }
        public virtual DbSet<BorrowingPolicy> BorrowingPolicies { get; set; }
        public virtual DbSet<FinePolicy> FinePolicies { get; set; }
        public virtual DbSet<RenewalPolicy> RenewalPolicies { get; set; }
        public virtual DbSet<ClaimPolicy> ClaimPolicies { get; set; }
        public virtual DbSet<BookReview> BookReviews { get; set; }
        public virtual DbSet<BookReviewLike> BookReviewLikes { get; set; }
        public virtual DbSet<BookCheckout> BookCheckouts { get; set; }
        public virtual DbSet<Wishlist> Wishlists { get; set; }
        public virtual DbSet<PendingUserRegistration> PendingUserRegistrations { get; set; }
        public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.UserId, "UQ__Users__1788CC4D5F4A160F")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "UQ__Users__Email")
                    .IsUnique();

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Role)
                    .HasDefaultValue(0);

                entity.Property(e => e.ProfilePicture)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.IsVerified)
                    .HasDefaultValue(false);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.OrganizationId)
                    .HasConstraintName("FK_User_Organization");
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasIndex(e => e.Name, "UQ__Organizations__Name")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasIndex(e => e.AdminUsername, "UQ__Admins__AdminUsername")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "UQ__Admins__Email")
                    .IsUnique();

                entity.Property(e => e.AdminUsername)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AdminPassword)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.ProfilePicture)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.IsVerified)
                    .HasDefaultValue(false);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.Admins)
                    .HasForeignKey(d => d.OrganizationId)
                    .HasConstraintName("FK_Admin_Organization");
            });

            modelBuilder.Entity<OrgBooks>(entity =>
            {
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Author)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ISBN)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Publisher)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Edition)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Category)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Genre)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.CoverImage)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.OrgBooks)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_OrgBooks_Organization");
            });

            modelBuilder.Entity<BorrowTicket>(entity =>
            {
                entity.HasIndex(e => e.ReferenceCode, "UQ__BorrowTicket__ReferenceCode")
                    .IsUnique();

                entity.Property(e => e.ReferenceCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Notes)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
                entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
                entity.Property(e => e.ClaimedAt).HasColumnType("datetime");
                entity.Property(e => e.CheckedOutAt).HasColumnType("datetime");
                entity.Property(e => e.DueAt).HasColumnType("datetime");
                entity.Property(e => e.ReturnedAt).HasColumnType("datetime");
                entity.Property(e => e.CancelledAt).HasColumnType("datetime");

                entity.Property(e => e.FinePerDay)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.AccruedFine)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.RenewCount)
                    .HasDefaultValue(0);

                entity.Property(e => e.IsFinePaid)
                    .HasDefaultValue(false);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.BorrowTickets)
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BorrowTicket_Organization");

                entity.HasOne(d => d.OrgBook)
                    .WithMany()
                    .HasForeignKey(d => d.OrgBookId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BorrowTicket_OrgBook");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BorrowTicket_User");

                entity.HasOne(d => d.ClaimedByEmployee)
                    .WithMany()
                    .HasForeignKey(d => d.ClaimedByEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BorrowTicket_ClaimedByEmployee");

                entity.HasOne(d => d.CheckedOutByEmployee)
                    .WithMany()
                    .HasForeignKey(d => d.CheckedOutByEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BorrowTicket_CheckedOutByEmployee");

                entity.HasOne(d => d.ReturnedToEmployee)
                    .WithMany()
                    .HasForeignKey(d => d.ReturnedToEmployeeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BorrowTicket_ReturnedToEmployee");
            });

            modelBuilder.Entity<Publisher>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Country)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<BookCategory>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<BorrowingPolicy>(entity =>
            {
                entity.HasIndex(e => e.OrganizationId, "UQ__BorrowingPolicy__OrganizationId")
                    .IsUnique();

                entity.Property(e => e.BorrowLimitPerUser)
                    .HasDefaultValue(10);

                entity.Property(e => e.BorrowDaysDefault)
                    .HasDefaultValue(14);

                entity.Property(e => e.MaxActiveTicketsPerUser)
                    .HasDefaultValue(5);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.Organization)
                    .WithOne(p => p.BorrowingPolicy)
                    .HasForeignKey<BorrowingPolicy>(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BorrowingPolicy_Organization");
            });

            modelBuilder.Entity<FinePolicy>(entity =>
            {
                entity.HasIndex(e => e.OrganizationId, "UQ__FinePolicy__OrganizationId")
                    .IsUnique();

                entity.Property(e => e.FinePerDay)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(5.00m);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.Organization)
                    .WithOne(p => p.FinePolicy)
                    .HasForeignKey<FinePolicy>(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_FinePolicy_Organization");
            });

            modelBuilder.Entity<RenewalPolicy>(entity =>
            {
                entity.HasIndex(e => e.OrganizationId, "UQ__RenewalPolicy__OrganizationId")
                    .IsUnique();

                entity.Property(e => e.AllowRenewals)
                    .HasDefaultValue(true);

                entity.Property(e => e.MaxRenewals)
                    .HasDefaultValue(2);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.Organization)
                    .WithOne(p => p.RenewalPolicy)
                    .HasForeignKey<RenewalPolicy>(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_RenewalPolicy_Organization");
            });

            modelBuilder.Entity<ClaimPolicy>(entity =>
            {
                entity.HasIndex(e => e.OrganizationId, "UQ__ClaimPolicy__OrganizationId")
                    .IsUnique();

                entity.Property(e => e.ClaimWindowHours)
                    .HasDefaultValue(48);

                entity.Property(e => e.AllowOverlappingReservations)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.Organization)
                    .WithOne(p => p.ClaimPolicy)
                    .HasForeignKey<ClaimPolicy>(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ClaimPolicy_Organization");
            });

            modelBuilder.Entity<BookReview>(entity =>
            {
                entity.Property(e => e.CreatedTime).HasColumnType("datetime");
                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.ReviewText)
                    .HasMaxLength(250)
                    .IsUnicode(true);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BookReview_User");

                entity.HasOne(d => d.OrgBook)
                    .WithMany()
                    .HasForeignKey(d => d.OrgBookId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BookReview_OrgBook");

                // Create unique index to ensure one review per user per book
                entity.HasIndex(e => new { e.UserId, e.OrgBookId }, "UQ__BookReview__UserId_OrgBookId")
                    .IsUnique();
            });

            modelBuilder.Entity<BookReviewLike>(entity =>
            {
                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.BookReview)
                    .WithMany()
                    .HasForeignKey(d => d.BookReviewId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BookReviewLike_BookReview");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BookReviewLike_User");

                // Create unique index to ensure one user can only like a review once
                entity.HasIndex(e => new { e.UserId, e.BookReviewId }, "UQ__BookReviewLike__UserId_BookReviewId")
                    .IsUnique();
            });

            modelBuilder.Entity<BookCheckout>(entity =>
            {
                entity.Property(e => e.AddedAt).HasColumnType("datetime");
                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedTime).HasColumnType("datetime");
                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BookCheckout_User");

                entity.HasOne(d => d.OrgBook)
                    .WithMany()
                    .HasForeignKey(d => d.OrgBookId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BookCheckout_OrgBook");

                entity.HasOne(d => d.Organization)
                    .WithMany()
                    .HasForeignKey(d => d.OrganizationId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_BookCheckout_Organization");

                // Create unique index to ensure one user can only have one cart item per book
                entity.HasIndex(e => new { e.UserId, e.OrgBookId }, "UQ__BookCheckout__UserId_OrgBookId")
                    .IsUnique();
            });

            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.Property(e => e.AddedAt).HasColumnType("datetime");
                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedTime).HasColumnType("datetime");
                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Wishlist_User");

                entity.HasOne(d => d.OrgBook)
                    .WithMany()
                    .HasForeignKey(d => d.OrgBookId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_Wishlist_OrgBook");

                // Create unique index to ensure one user can only have one wishlist item per book
                entity.HasIndex(e => new { e.UserId, e.OrgBookId }, "UQ__Wishlist__UserId_OrgBookId")
                    .IsUnique();
            });

            modelBuilder.Entity<PendingUserRegistration>(entity =>
            {
                entity.HasIndex(e => e.EmailId, "UQ__PendingUserRegistrations__EmailId")
                    .IsUnique();

                entity.Property(e => e.EmailId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EncryptedOTP)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VerificationToken)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.TokenExpiration).HasColumnType("datetime");

                entity.Property(e => e.IsVerified)
                    .HasDefaultValue(false);

                entity.Property(e => e.Role)
                    .IsRequired();

                entity.Property(e => e.OrganizationName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.OrganizationType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");
                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.TokenId);

                entity.HasIndex(e => e.Token, "UQ__PasswordResetToken__Token")
                    .IsUnique();

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Role)
                    .IsRequired();

                entity.Property(e => e.ExpirationTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.IsUsed)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedTime)
                    .HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}

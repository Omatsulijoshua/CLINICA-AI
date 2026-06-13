using Microsoft.EntityFrameworkCore;
using ClinicaAI.Core.Entities;

namespace ClinicaAI.Infrastructure.Data
{
    public class ClinicaDbContext : DbContext
    {
        public ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<UserProfile> Profiles { get; set; } = null!;
        public DbSet<Conversation> Conversations { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<MedicalHistoryEntry> MedicalHistoryEntries { get; set; } = null!;
        public DbSet<UploadedFile> UploadedFiles { get; set; } = null!;
        public DbSet<UploadedReport> UploadedReports { get; set; } = null!;
        public DbSet<MemoryStore> MemoryStores { get; set; } = null!;
        public DbSet<SourceReference> SourceReferences { get; set; } = null!;
        public DbSet<VideoReference> VideoReferences { get; set; } = null!;
        public DbSet<Subscription> Subscriptions { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table mapping names
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<UserProfile>().ToTable("profiles");
            modelBuilder.Entity<Conversation>().ToTable("conversations");
            modelBuilder.Entity<Message>().ToTable("messages");
            modelBuilder.Entity<MedicalHistoryEntry>().ToTable("medical_history");
            modelBuilder.Entity<UploadedFile>().ToTable("uploaded_files");
            modelBuilder.Entity<UploadedReport>().ToTable("uploaded_reports");
            modelBuilder.Entity<MemoryStore>().ToTable("memory_store");
            modelBuilder.Entity<SourceReference>().ToTable("source_references");
            modelBuilder.Entity<VideoReference>().ToTable("video_references");
            modelBuilder.Entity<Subscription>().ToTable("subscriptions");
            modelBuilder.Entity<AuditLog>().ToTable("audit_logs");

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.PasswordHash).IsRequired();
            });

            // Profile configuration
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.HasOne(e => e.User)
                      .WithOne(u => u.Profile)
                      .HasForeignKey<UserProfile>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Conversation configuration
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Conversations)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Message configuration
            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Conversation)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(e => e.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Medical History configuration
            modelBuilder.Entity<MedicalHistoryEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Profile)
                      .WithMany(p => p.HistoryEntries)
                      .HasForeignKey(e => e.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UploadedFile configuration
            modelBuilder.Entity<UploadedFile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Conversation)
                      .WithMany(c => c.UploadedFiles)
                      .HasForeignKey(e => e.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UploadedReport configuration
            modelBuilder.Entity<UploadedReport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.File)
                      .WithOne(f => f.UploadedReport)
                      .HasForeignKey<UploadedReport>(e => e.FileId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Profile)
                      .WithMany(p => p.UploadedReports)
                      .HasForeignKey(e => e.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // MemoryStore configuration
            modelBuilder.Entity<MemoryStore>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Profile)
                      .WithMany(p => p.MemoryStores)
                      .HasForeignKey(e => e.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // SourceReference configuration
            modelBuilder.Entity<SourceReference>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Message)
                      .WithMany(m => m.SourceReferences)
                      .HasForeignKey(e => e.MessageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // VideoReference configuration
            modelBuilder.Entity<VideoReference>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Message)
                      .WithMany(m => m.VideoReferences)
                      .HasForeignKey(e => e.MessageId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Subscription configuration
            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Subscriptions)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.AuditLogs)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}

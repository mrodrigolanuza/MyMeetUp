using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyMeetUp.Web.Models;

namespace MyMeetUp.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAttendance> EventAttendances { get; set; }
        public DbSet<EventAttendanceState> EventAttendanceStates { get; set; }
        public DbSet<EventCategory> EventCategories { get; set; }
        public DbSet<EventComment> EventComments { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Group_GroupCategory> Group_GroupCategories { get; set; }
        public DbSet<GroupCategory> GroupCategories { get; set; }
        public DbSet<GroupMemberProfile> GroupMemberProfiles { get; set; }
        public DbSet<GroupMembers> GroupMembers { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            
            //Todas entidades tienen por defecto un Id como clave principal. 
            //Añado además un índice único en las tablas que lo requieran
            modelBuilder.Entity<Group>()
               .HasIndex(g => g.Name)
               .IsUnique();

            modelBuilder.Entity<Group_GroupCategory>()
                .HasIndex(ggc => new { ggc.GroupCategoryId, ggc.GroupId })
                .IsUnique();
            
            modelBuilder.Entity<GroupCategory>()
                .HasIndex(gc => gc.Name)
                .IsUnique();

            modelBuilder.Entity<GroupMemberProfile>()
                .HasIndex(gmp => gmp.Name)
                .IsUnique();

            modelBuilder.Entity<GroupMembers>()
                .HasIndex(gm => new { gm.GroupId, gm.ApplicationUserId })
                .IsUnique();

            modelBuilder.Entity<EventAttendanceState>()
                .HasIndex(eas => eas.State)
                .IsUnique();

            modelBuilder.Entity<EventAttendance>()
                .HasIndex(ea => new { ea.ApplicationUserId, ea.EventId })
                .IsUnique();

            modelBuilder.Entity<Event>()
                .HasIndex(e => new { e.Title, e.GroupId })
                .IsUnique();

            modelBuilder.Entity<EventCategory>()
                .HasIndex(ec => ec.Name)
                .IsUnique();
        }
    }
}

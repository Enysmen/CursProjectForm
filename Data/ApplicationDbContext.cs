using System.Reflection.Emit;

using CursProject.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CursProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public DbSet<SurveyTemplate> SurveyTemplates { get; set; }
        public DbSet<SurveyResponse> SurveyResponses { get; set; }
        public DbSet<TemplateComment> TemplateComments { get; set; }
        public DbSet<TemplateLike> TemplateLikes { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<ApplicationUser>().HasIndex(u => u.NormalizedEmail).IsUnique();

            builder.Entity<TemplateComment>()
                .HasOne(tc => tc.SurveyTemplate)
                .WithMany(st => st.Comments)
                .HasForeignKey(tc => tc.SurveyTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TemplateLike>()
                .HasOne(tl => tl.SurveyTemplate)
                .WithMany(st => st.Likes)
                .HasForeignKey(tl => tl.SurveyTemplateId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SurveyResponse>()
                .HasOne(sr => sr.SurveyTemplate)
                .WithMany(st => st.Responses)
                .HasForeignKey(sr => sr.SurveyTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

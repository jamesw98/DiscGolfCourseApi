using Microsoft.EntityFrameworkCore;

namespace DiscGolfCourseApi.Models;

public partial class DiscgolfDbContext : DbContext
{
    private IConfiguration _config;
    
    public DiscgolfDbContext()
    {
    }

    public DiscgolfDbContext(IConfiguration config, DbContextOptions<DiscgolfDbContext> options)
        : base(options)
    {
        _config = config;
    }

    public virtual DbSet<Course> Courses { get; set; }
    public virtual DbSet<CourseDriveTime> CourseDrivetimes { get; set; }
    public virtual DbSet<UsGeography> UsGeographies { get; set; }
    public virtual DbSet<StateAbbreviations> StateAbbreviations { get; set; }
    public virtual DbSet<UsGeographyRelationships> UsGeographyRelationships { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(_config.GetConnectionString("Database"), 
            x=> x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("TBL_COURSES_pk");

            entity.ToTable("TBL_COURSES");
            
            entity.Property(e => e.CourseId)
                .ValueGeneratedOnAdd()
                .HasColumnName("COURSE_ID");
            entity.Property(e => e.CourseAddress)
                .HasMaxLength(300)
                .HasColumnName("COURSE_ADDRESS");
            entity.Property(e => e.CourseDesc).HasColumnName("COURSE_DESC");
            entity.Property(e => e.CourseName)
                .HasMaxLength(100)
                .HasColumnName("COURSE_NAME");
            entity.Property(e => e.CourseRating).HasColumnName("COURSE_RATING");
            entity.Property(e => e.CourseType).HasColumnName("COURSE_TYPE");
            entity.Property(e => e.CreatedDate)
                .HasColumnType("datetime")
                .HasColumnName("CREATED_DATE");
            entity.Property(e => e.Directions).HasColumnName("DIRECTIONS");
            entity.Property(e => e.HoleCount).HasColumnName("HOLE_COUNT");
            entity.Property(e => e.Latest).HasColumnName("LATEST");
            entity.Property(e => e.LocationType)
                .HasMaxLength(100)
                .HasColumnName("LOCATION_TYPE");
            entity.Property(e => e.LongHoles).HasColumnName("LONG_HOLES");
            entity.Property(e => e.MediumHoles).HasColumnName("MEDIUM_HOLES");
            entity.Property(e => e.ShortHoleCount).HasColumnName("SHORT_HOLE_COUNT");
            entity.Property(e => e.TargetType)
                .HasMaxLength(100)
                .HasColumnName("TARGET_TYPE");
            entity.Property(e => e.TeeType)
                .HasMaxLength(100)
                .HasColumnName("TEE_TYPE");
            entity.Property(e => e.Point)
                .HasColumnType("Geography")
                .HasColumnName("POINT");
            entity.Property(e => e.RawCourseName)
                .HasColumnName("RAW_COURSE_NAME");
        });

        modelBuilder.Entity<CourseDriveTime>(entity =>
        {
            entity.HasKey(e => e.DrivetimeId).HasName("TBL_COURSE_DRIVETIMES_pk");

            entity.ToTable("TBL_COURSE_DRIVETIMES");

            entity.Property(e => e.DrivetimeId)
                .ValueGeneratedNever()
                .HasColumnName("DRIVETIME_ID");
            entity.Property(e => e.CourseId).HasColumnName("COURSE_ID");
            entity.Property(e => e.DrivetimeMinutes).HasColumnName("DRIVETIME_MINUTES");
        });

        modelBuilder.Entity<UsGeography>(entity =>
        {
            entity.HasKey(e => new
            {
                e.GeoId
            }).HasName("TBL_US_GEOGRAPHIES_pk");

            entity.ToTable("TBL_US_GEOGRAPHIES");
            
            entity.Property(e => e.GeoId)
                .HasColumnName("GEO_ID");
            entity.Property(e => e.GeoName)
                .HasColumnName("GEO_NAME");
            entity.Property(e => e.GeoType)
                .HasColumnName("GEO_TYPE");
            entity.Property(e => e.Boundary)
                .HasColumnType("Geography")
                .HasColumnName("BOUNDARY");
        });

        modelBuilder.Entity<StateAbbreviations>(entity =>
        {
            entity.ToTable("TBL_US_STATE_ABBREVIATIONS");
            
            entity.HasKey(e => e.FullName).HasName("TBL_US_STATE_ABBREVIATIONS_pk");
            
            entity.Property(e => e.FullName)
                .HasColumnName("FULL_NAME");
            entity.Property(e => e.Abbreviation)
                .HasColumnName("ABBREVIATION");
            entity.Property(e => e.StateNumber)
                .HasColumnName("STATE_NUMBER");
        });
        
        modelBuilder.Entity<UsGeographyRelationships>(entity =>
        {
            entity.ToTable("TBL_US_GEOGRAPHY_RELATIONSHIPS");

            entity.HasKey(e => new
            {
                e.RelationshipId
            }).HasName("TBL_US_GEOGRAPHY_RELATIONSHIPS_pk");
            
            entity.Property(e => e.GeoName)
                .HasColumnName("GEO_NAME");
            entity.Property(e => e.ParentGeoName)
                .HasColumnName("PARENT_GEO_NAME");
            entity.Property(e => e.RelationshipId)
                .HasColumnName("RELATIONSHIP_ID");
            entity.Property(e => e.GeoId)
                .HasColumnName("GEO_ID");
            entity.Property(e => e.ParentGeoId)
                .HasColumnName("PARENT_GEO_ID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

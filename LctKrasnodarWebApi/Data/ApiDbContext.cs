using System.Runtime.InteropServices.ComTypes;
using LctKrasnodarWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LctKrasnodarWebApi.Data;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public ApiDbContext()
    {
        throw new NotImplementedException();
    }

    // Account related
    public DbSet<User> Users { get; set; }
    public DbSet<WorkerCase> WorkerCases { get; set; }

    // Partner related
    public DbSet<PartnerInfo> PartnerInfos { get; set; }

    // Constant related
    public DbSet<ConstantTaskSize> ConstantTaskSizes { get; set; }
    public DbSet<ConstantTaskRule> ConstantTaskRules { get; set; }
    public DbSet<Office> Offices { get; set; }

    // Task related
    public DbSet<AssignedTask> AssignedTasks { get; set; }
    public DbSet<AssignTags> AssignTags { get; set; }
    public DbSet<AssignTagChanges> AssignTagChanges { get; set; }
    
    // Manual related
    public DbSet<AvailableWorkerPosition> AvailableWorkerPositions { get; set; }
    
    public DbSet<WorkerPosition> WorkerPositions { get; set; }
    
    // Worker related
    public DbSet<TaskDto> TaskDtos { get; set; }
    public DbSet<CourierDto> CourierDtos { get; set; }
}
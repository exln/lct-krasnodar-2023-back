using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.EntityFrameworkCore;

namespace LctKrasnodarWebApi.Models;

public class Manual
{
}

[PrimaryKey("Id")]
public class ConstantTaskRule
{
    public int Id { get; set; }
    [Required] public required string Description { get; set; }
    [Required] public required List<Condition> Conditions { get; set; }
    [Required] public required List<Target> Targets { get; set; }
    [Required] public required List<string> Values {get; set; }
}

public class ConstantTaskRuleCreationDto
{
    [Required] public required string Description { get; set; }
    [Required] public required List<Condition> Conditions { get; set; }
    [Required] public required List<Target> Targets { get; set; }
    [Required] public required List<string> Values {get; set; }
}

public class ConstantTaskRuleIdDto
{
    public int Id { get; set; }
}

public class ConstantTaskRuleRead
{
    public int Id { get; set; }
    [Required] public required string Description { get; set; }
    [Required] public required List<string> Conditions { get; set; }
    [Required] public required List<string> Targets { get; set; }
    [Required] public required List<string> Values {get; set; }
}



public enum Condition
{
    Equal,
    NotEqual,
    Greater,
    Less,
    GreaterOrEqual,
    LessOrEqual,
    Between,
    NotBetween,
    In,
    NotIn,
    NotNullOrZero,
    NullOrZero,
    MoreThanAHalf
}

public enum Target
{
    WhenPointConnected,
    AreCardsAndMaterialsDelivered,
    DaysSinceLastCardIssue,
    NumberOfApprovedApplications,
    NumberOfGivenCards
}

public enum Priority
{
    High,
    Medium,
    Low
}


[PrimaryKey("Id")]
public class ConstantTaskSize
{
    public int Id { get; set; }
    [Required] public required string Name { get; set; }
    [Required] public required string Value { get; set; }
    [Required] public required List<Grade> Grades { get; set; }
    [Required] public required List<int> Rules { get; set; }
    
    [Required] public required RuleQuantor RuleQuantor { get; set; } 
    [Required] public required Priority Priority { get; set; }
    
}

public class ConstantTaskSizeCreationDto
{
    [Required] public required string Name { get; set; }
    [Required] public required string Value { get; set; }
    [Required] public required List<Grade> Grades { get; set; }
    [Required] public required List<int> Rules { get; set; }
    [Required] public required RuleQuantor RuleQuantor { get; set; } 
    [Required] public required Priority Priority { get; set; }
}

public enum RuleQuantor
{
    Any,
    All
}

public class ConstantTaskSizeIdDto
{
    [Required] public required int Id { get; set; }
}

public class ConstantTaskSizePatchDto: ConstantTaskSizeIdDto
{
    public string? Name { get; set; }
    public string? Value { get; set; }
    public List<Grade>? Grades { get; set; }
    public List<int>? Rules { get; set; }
}

public class ConstantTaskSizeRead
{
    [Required] public required int Id { get; set; }
    [Required] public required string Name { get; set; }
    [Required] public required string Value { get; set; }
    
    [Required] public required List<Grade> Grades { get; set; }
    [Required] public required List<int> Rules { get; set; }

    [Required] public required RuleQuantor RuleQuantor { get; set; }
    [Required] public required Priority Priority { get; set; }
}


public class TargetDataset
{
    public string WhenPointConnected { get; set; }
    public string AreCardsAndMaterialsDelivered { get; set; }
    public int DaysSinceLastCardIssue { get; set; }
    public int NumberOfApprovedApplications { get; set; }
    public int NumberOfGivenCards { get; set; }
}

public enum TargetDatasetEnum
{
    WhenPointConnected,
    AreCardsAndMaterialsDelivered,
    DaysSinceLastCardIssue,
    NumberOfApprovedApplications,
    NumberOfGivenCards
}


[PrimaryKey("Id")]
public class Office
{
    [Required] public required int Id { get; set; }
    [Required] public required string Address { get; set; }
    [Required] public required List<double> LocationCoordinates { get; set; }
}

public class OfficesWithStaffersAndGrades
{
    [Required] public required List<OfficeWithStaffersAndTags> OfficeWithStaffersAndGradesList { get; set; }
}

public class OfficesWithStaffers
{
    [Required] public required List<OfficeWithStaffers> OfficeWithStaffersList { get; set; }
}

public class OfficeWithStaffers
{
    [Required] public required Office Office { get; set; }
    [Required] public required List<UserShortRead> Workers { get; set; }
}

public abstract class OfficeWithStaffersAndTags : OfficeWithStaffers
{
    [Required] public required List<LocationTag> LocationTags { get; set; }
}

public class Solutions
{
    [Required] public required List<SolutionRead> SolutionList { get; set; }
}

public class SolutionRead
{
    [Required] public required UserShortRead User { get; set; }
    [Required] public required Dictionary<int, Endpnt> EndPointsList { get; set; }
}

public class Endpnt
{
    [Required] public required List<double> Coordinates { get; set; }
    [Required] public required string RouteToEndpoint { get; set; }
}

public enum LocationTag
{
    Senior,
    Middle,
    Junior
}

[PrimaryKey("Id")]
public class TaskDto
{
    public int Id { get; set; }
    [Required] public required int TaskId { get; set; }
    public Guid? WorkerId { get; set; }
    [Required] public required int Duration { get; set; } // in minutes
    [Required] public required DateTime? CreatedAt { get; set; } = DateTimeOffset.Now.UtcDateTime;
    public int? PartnerId { get; set; }
    public List<double> LocationCoordinates { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    [Required] public required bool isDone { get; set; } = false;
    [Required] public required Priority Priority { get; set; }
    [Required] public required List<Grade> Grades { get; set; }
    public int? TravelTime { get; set; } // in minutes, from previous task
}

[PrimaryKey("Id")]
public class CourierDto
{
    public int Id { get; set; }
    [Required] public required Guid WorkerId { get; set; }
    public int? TaskId { get; set; } // id of the task that courier is currently doing
    [Required] public required string Status { get; set; } = "idle"; // idle, busy, onBreak, onLunch, onWayToPartner
    [Required] public required DateTime Date { get; set; } = DateTimeOffset.Now.UtcDateTime;
    public List<int> TaskIds { get; set; } // ids of the tasks that courier is currently doing
    [Required] public required Grade Grade { get; set; }
    [Required] public required List<double> LocationCoordinates { get; set; }
    [Required] public required int WorkTime { get; set; } // in minutes
    [Required] public required DateTime ModifiedAt { get; set; } = DateTimeOffset.Now.UtcDateTime;
}

public class CourierDtoPatch
{
    [Required] public required int Id { get; set; }
    [Required] public required Guid WorkerId { get; set; }
    [Required] public required List<double> LocationCoordinates { get; set; }
     
}



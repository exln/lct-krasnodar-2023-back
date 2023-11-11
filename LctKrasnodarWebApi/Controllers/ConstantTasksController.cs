using System.Data;
using LctKrasnodarWebApi.Data;
using LctKrasnodarWebApi.Models;
using Microsoft.AspNetCore.Mvc;
namespace LctKrasnodarWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ConstantTasksController : Controller
{
    private readonly ApiDbContext _context;

    public ConstantTasksController(
        ApiDbContext context)
    {
        _context = context;
    }

    [HttpGet("Get", Name = "Get Constant Tasks")]
    [ProducesResponseType(200, Type = typeof(List<ConstantTaskSizeRead>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetConstantTasks()
    {
        var constantTasks = _context.ConstantTaskSizes
            .ToList();

        List<ConstantTaskSizeRead> constantTaskSizeReads = new List<ConstantTaskSizeRead>();
        foreach (var constantTask in constantTasks)
        {
            var constantTaskSizeRead = new ConstantTaskSizeRead()
            {
                Id = constantTask.Id,
                Name = constantTask.Name,
                Value = constantTask.Value,
                Grades = constantTask.Grades,
                Rules = constantTask.Rules,
                Priority = constantTask.Priority,
                RuleQuantor = constantTask.RuleQuantor
            };
            constantTaskSizeReads.Add(constantTaskSizeRead);
        }
        
        return Ok(constantTaskSizeReads);
        
    }

    [HttpPost("New", Name = "New Constant Task")]
    [ProducesResponseType(200, Type = typeof(ConstantTaskSizeRead))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult NewConstantTask([FromBody] ConstantTaskSizeCreationDto constantTaskSizeCreationDto)
    {
        var constantTaskSize = new ConstantTaskSize()
        {
            Name = constantTaskSizeCreationDto.Name,
            Value = constantTaskSizeCreationDto.Value,
            Grades = constantTaskSizeCreationDto.Grades,
            Rules = constantTaskSizeCreationDto.Rules,
            Priority = constantTaskSizeCreationDto.Priority,
            RuleQuantor = constantTaskSizeCreationDto.RuleQuantor
        };
        _context.ConstantTaskSizes.Add(constantTaskSize);
        _context.SaveChanges();
        
        var constantTaskSizeRead = new ConstantTaskSizeRead()
        {
            Id = constantTaskSize.Id,
            Name = constantTaskSize.Name,
            Value = constantTaskSize.Value,
            Grades = constantTaskSize.Grades,
            Rules = constantTaskSize.Rules,
            Priority = constantTaskSize.Priority,
            RuleQuantor = constantTaskSize.RuleQuantor
        };  
        
        return Ok(constantTaskSizeRead);
    }

    [HttpPost("Patch", Name = "Update Constant Task")]
    [ProducesResponseType(200, Type = typeof(ConstantTaskSizeRead))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult PatchConstantTask([FromBody] ConstantTaskSize constantTaskSize)
    {
        _context.ConstantTaskSizes.Update(constantTaskSize);
        _context.SaveChanges();
        
        var constantTaskSizeRead = new ConstantTaskSizeRead()
        {
            Id = constantTaskSize.Id,
            Name = constantTaskSize.Name,
            Value = constantTaskSize.Value,
            Grades = constantTaskSize.Grades,
            Rules = constantTaskSize.Rules,
            Priority = constantTaskSize.Priority,
            RuleQuantor = constantTaskSize.RuleQuantor
        };
        
        return Ok(constantTaskSizeRead);
    }

    [HttpPost("Delete", Name = "Delete Constant Task")]
    [ProducesResponseType(200, Type = typeof(ConstantTaskSizeRead))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult DeleteConstantTask([FromBody] ConstantTaskSizeIdDto constantTaskSizeIdDto)
    {
        var constantTaskSize = _context.ConstantTaskSizes
            .FirstOrDefault(x => x.Id == constantTaskSizeIdDto.Id);
        if (constantTaskSize == null)
        {
            return BadRequest("Constant Task Size not found");
        }

        _context.ConstantTaskSizes.Remove(constantTaskSize);
        _context.SaveChanges();
        
        var constantTaskSizeRead = new ConstantTaskSizeRead()
        {
            Id = constantTaskSize.Id,
            Name = constantTaskSize.Name,
            Value = constantTaskSize.Value,
            Grades = constantTaskSize.Grades,
            Rules = constantTaskSize.Rules,
            Priority = constantTaskSize.Priority,
            RuleQuantor = constantTaskSize.RuleQuantor
        };
        
        return Ok(constantTaskSizeRead);
    }

    [HttpGet("Rules/Get", Name = "Get Constant Task Rules")]
    [ProducesResponseType(200, Type = typeof(List<ConstantTaskRule>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetConstantTaskRules()
    {
        var constantTaskRules = _context.ConstantTaskRules
            .ToList();
        return Ok(constantTaskRules);
    }

    [HttpPost("Rule/New", Name = "New Constant Task Rule")]
    [ProducesResponseType(200, Type = typeof(ConstantTaskRule))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult NewConstantTaskRule([FromBody] ConstantTaskRuleCreationDto constantTaskRuleCreationDto)
    {
        var constantTaskRule = new ConstantTaskRule()
        {
            Description = constantTaskRuleCreationDto.Description,
            Conditions = constantTaskRuleCreationDto.Conditions,
            Targets = constantTaskRuleCreationDto.Targets,
            Values = constantTaskRuleCreationDto.Values
        };
        _context.ConstantTaskRules.Add(constantTaskRule);
        _context.SaveChanges();
        return Ok(constantTaskRule);
    }

    [HttpPost("Rule/Patch", Name = "Update Constant Task Rule")]
    [ProducesResponseType(200, Type = typeof(ConstantTaskRule))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult PatchConstantTaskRule([FromBody] ConstantTaskRule constantTaskRule)
    {
        _context.ConstantTaskRules.Update(constantTaskRule);
        _context.SaveChanges();
        return Ok(constantTaskRule);
    }

    [HttpPost("Rule/Delete", Name = "Delete Constant Task Rule")]
    [ProducesResponseType(200, Type = typeof(ConstantTaskRule))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult DeleteConstantTaskRule([FromBody] ConstantTaskRuleIdDto constantTaskRuleIdDto)
    {
        var constantTaskRule = _context.ConstantTaskRules
            .FirstOrDefault(x => x.Id == constantTaskRuleIdDto.Id);
        if (constantTaskRule == null)
        {
            return BadRequest("Constant Task Rule not found");
        }

        _context.ConstantTaskRules.Remove(constantTaskRule);
        _context.SaveChanges();
        return Ok(constantTaskRule);
    }

    [HttpPost("Rule/Test", Name = "Check If Any Rule Is Suitable")]
    [ProducesResponseType(200, Type = typeof(List<ConstantTaskRule>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult TestConstantTaskRule([FromBody] TargetDataset targetDataset)
    {
        var suitableRules = new List<Rule>();

        var constantTaskRules = _context.ConstantTaskRules
            .ToList();

        foreach (var constantTaskRule in constantTaskRules)
        {

        }


        return Ok(suitableRules);
    }
    
    private string GetConditionString(Condition condition)
    {
        switch (condition)
        {
            case Condition.Equal:
                return "равно";
            case Condition.NotEqual:
                return "не равно";
            case Condition.Greater:
                return "больше";
            case Condition.Less:
                return "меньше";
            case Condition.GreaterOrEqual:
                return "больше или равно";
            case Condition.LessOrEqual:
                return "меньше или равно";
            case Condition.Between:
                return "между";
            case Condition.NotBetween:
                return "не между";
            case Condition.In:
                return "в";
            case Condition.NotIn:
                return "не в";
            case Condition.NotNullOrZero:
                return "не нулевое";
            case Condition.NullOrZero:
                return "нулевое";
            case Condition.MoreThanAHalf:
                return "больше половины";
            default:
                return "не равно";
        }
    }
    
    private string GetTargetString(Target target)
    {
        switch (target)
        {
            case Target.WhenPointConnected:
                return "когда точка подключена";
            case Target.AreCardsAndMaterialsDelivered:
                return "доставлены ли карты и материалы";
            case Target.DaysSinceLastCardIssue:
                return "дней с момента последнего выдачи карты";
            case Target.NumberOfApprovedApplications:
                return "количество одобренных заявок";
            case Target.NumberOfGivenCards:
                return "количество выданных карт";
            default:
                return "когда точка подключена";
        }
    }
    
    private Condition SetCondition(string value)
    {
        switch (value)
        {
            case "равно":
                return Condition.Equal;
            case "не равно":
                return Condition.NotEqual;
            case "больше":
                return Condition.Greater;
            case "меньше":
                return Condition.Less;
            case "больше или равно":
                return Condition.GreaterOrEqual;
            case "меньше или равно":
                return Condition.LessOrEqual;
            case "между":
                return Condition.Between;
            case "не между":
                return Condition.NotBetween;
            case "в":
                return Condition.In;
            case "не в":
                return Condition.NotIn;
            case "не нулевое":
                return Condition.NotNullOrZero;
            case "нулевое":
                return Condition.NullOrZero;
            case "больше половины":
                return Condition.MoreThanAHalf;
            default:
                return Condition.NotEqual;
        }
    }
    
    private Target SetTarget(string value)
    {
        switch (value)
        {
            case "когда точка подключена":
                return Target.WhenPointConnected;
            case "доставлены ли карты и материалы":
                return Target.AreCardsAndMaterialsDelivered;
            case "дней с момента последнего выдачи карты":
                return Target.DaysSinceLastCardIssue;
            case "количество одобренных заявок":
                return Target.NumberOfApprovedApplications;
            case "количество выданных карт":
                return Target.NumberOfGivenCards;
            default:
                return Target.WhenPointConnected;
        }
    }

    /*
     * public class TargetDataset
{
    public string WhenPointConnected { get; set; }
    public string AreCardsAndMaterialsDelivered { get; set; }
    public int DaysSinceLastCardIssue { get; set; }
    public int NumberOfApprovedApplications { get; set; }
    public int NumberOfGivenCards { get; set; }
}
     */

    private YesNo GetYesNo(string value)
    {
        switch (value)
        {
            case "Да":
                return YesNo.Yes;
            case "Нет":
                return YesNo.No;
            default:
                return YesNo.No;
        }
    }

    private WhenConnected GetWhenConnected(string value)
    {
        switch (value)
        {
            case "Давно":
                return WhenConnected.Long;
            case "Вчера":
                return WhenConnected.Yesterday;
            default:
                return WhenConnected.Yesterday;
        }
    }


    private void CheckRuleOnData(TargetDataset targetDataset, ConstantTaskRule rule)
    {
    }
}
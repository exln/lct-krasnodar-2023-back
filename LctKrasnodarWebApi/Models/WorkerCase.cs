using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LctKrasnodarWebApi.Models;

[PrimaryKey("Id")]
public class WorkerCase
{
    [Required] public required Guid Id { get; set; }
    [Required] public required WrkrСase Case { get; set; }
}

public class WorkerCaseRead
{
    public WrkrСase _сase
    {
        get => _сase;
        set
        {
            switch (value)
            {
                case WrkrСase.Vacation:
                    Case = "Отпуск";
                    break;
                case WrkrСase.Sick:
                    Case = "Больничный";
                    break;
                case WrkrСase.Work:
                    Case = "Доступен";
                    break;
                case WrkrСase.Rest:
                    Case = "Отдых";
                    break;
            }
        }
    }
    public string Case
    {
        get; set;
    }

}

public class WorkerCasePatchDto
{
    [Required] public required Guid Id { get; set; }
    [Required] public required string Case { get; set; }
}

public enum WrkrСase
{
    Vacation,
    Sick,
    Work,
    Rest
}

public enum WrkrStatus
{
    Available,
    Busy,
    Rest
}
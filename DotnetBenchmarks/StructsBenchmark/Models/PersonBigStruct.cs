using System;

namespace StructsBenchmark.Models;

public struct PersonBigStruct : IPerson
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string MiddleName { get; set; }

    public string LastName { get; set; }

    public DateTime BirthDate { get; set; }

    public int PositionId { get; set; }

    public string Position { get; set; }

    public string Country { get; set; }

    public string Address { get; set; }

    public decimal Salary { get; set; }

    public DateTime HireDate { get; set; }

    public DateTime? ContractEndDate { get; set; }

    public int LeaveDays { get; set; }
}
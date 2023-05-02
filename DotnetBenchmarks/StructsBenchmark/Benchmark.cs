using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using StructsBenchmark.Models;

namespace StructsBenchmark;

[MemoryDiagnoser]
public class Benchmark
{
    private static readonly PersonStruct Struct;
    private static readonly PersonBigStruct BigStruct;
    private static readonly PersonClass Class;
    private static int _id;

    static Benchmark()
    {
        Struct = new PersonStruct
        {
            Id = 1,
            FirstName = "First name",
            LastName = "Last name"
        };

        BigStruct = new PersonBigStruct
        {
            Id = 1,
            FirstName = "First name",
            MiddleName = "Middle name",
            LastName = "Last name",
            Address = "Rzeszów, 30-100, ul. 3-go maja 31a",
            Country = "Poland",
            PositionId = 1,
            Position = "Manager",
            BirthDate = new DateTime(1990, 10, 10),
            HireDate = new DateTime(2014, 05, 01),
            ContractEndDate = new DateTime(2030, 01, 01),
            Salary = 3000.00m,
            LeaveDays = 20
        };

        Class = new PersonClass
        {
            Id = 1,
            FirstName = "First name",
            LastName = "Last name"
        };
    }

    [Benchmark(Description = "Class", Baseline = true)]
    public void RunForClass()
    {
        _id = GetIdClass(Class);
    }

    [Benchmark(Description = "Class - interface")]
    public void RunForClassInterface()
    {
        _id = GetIdInterface(Class);
    }

    [Benchmark(Description = "Struct")]
    public void RunForStruct()
    {
        _id = GetIdStruct(Struct);
    }

    [Benchmark(Description = "Struct - in")]
    public void RunForInStruct()
    {
        _id = GetIdInStruct(in Struct);
    }

    [Benchmark(Description = "Struct - interface")]
    public void RunForInterfaceStruct()
    {
        _id = GetIdInterface(Struct);
    }

    [Benchmark(Description = "Big struct")]
    public void RunForBigStruct()
    {
        _id = GetIdBigStruct(BigStruct);
    }

    [Benchmark(Description = "Big struct - in")]
    public void RunForInBigStruct()
    {
        _id = GetInIdBigStruct(in BigStruct);
    }

    [Benchmark(Description = "Big struct - interface")]
    public void RunForInterfaceBigStruct()
    {
        _id = GetIdInterface(BigStruct);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int GetIdStruct(PersonStruct @struct)
    {
        return @struct.Id;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int GetIdInStruct(in PersonStruct @struct)
    {
        return @struct.Id;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int GetIdInterface(IPerson person)
    {
        return person.Id;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int GetIdClass(PersonClass @class)
    {
        return @class.Id;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int GetIdBigStruct(PersonBigStruct @struct)
    {
        return @struct.Id;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int GetInIdBigStruct(in PersonBigStruct @struct)
    {
        return @struct.Id;
    }
}
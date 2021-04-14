using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using StructsBenchmark.Models;

namespace StructsBenchmark
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        private const int NumberOfIterations = 1_00_000;
        private const int NumberOfElements = 1000;
        private static readonly PersonStruct[] StructsList = new PersonStruct[NumberOfElements];
        private static readonly PersonBigStruct[] BigStructsList = new PersonBigStruct[NumberOfElements];
        private static readonly PersonClass[] ClassList = new PersonClass[NumberOfElements];

        static Benchmark()
        {
            for (var i = 0; i < NumberOfElements; i++)
            {
                StructsList[i] = new PersonStruct
                {
                    Id = i,
                    FirstName = $"FirstName_{i}",
                    LastName = $"LastName_{i}"
                };

                ClassList[i] = new PersonClass
                {
                    Id = i,
                    FirstName = $"FirstName_{i}",
                    LastName = $"LastName_{i}"
                };
                
                BigStructsList[i] = new PersonBigStruct
                {
                    Id = i,
                    FirstName = $"FirstName_{i}",
                    MiddleName = $"MiddleName_{i}",
                    LastName = $"LastName_{i}",
                    Position = $"Boss_{i}",
                    BirthDate = new DateTime(1980, 1, 1).AddDays(i),
                    PositionId = i,
                    Country = "Poland",
                    Address = $"Rzeszów, 30-100, ul. Wasilewskiego {i}a"
                };
            }
        }

        [Benchmark(Description = "Struct")]
        public void RunForStruct()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var sum = 0L;
                for (var j = 0; j < StructsList.Length; j++)
                {
                    sum += GetIdStruct(StructsList[j]);
                }
            }
        }

        private static int GetIdStruct(PersonStruct @struct)
        {
            return @struct.Id;
        }

        [Benchmark(Description = "Struct ref")]
        public void RunForRefStruct()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var sum = 0L;
                for (var j = 0; j < StructsList.Length; j++)
                {
                    sum += GetIdRefStruct(ref StructsList[j]);
                }
            }
        }

        private static int GetIdRefStruct(ref PersonStruct @struct)
        {
            return @struct.Id;
        }

        [Benchmark(Description = "Struct interface")]
        public void RunForInterfaceStruct()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var sum = 0L;
                for (var j = 0; j < StructsList.Length; j++)
                {
                    sum += GetIdInterface(StructsList[j]);
                }
            }
        }

        private static int GetIdInterface(IPerson person)
        {
            return person.Id;
        }

        [Benchmark(Description = "Class")]
        public void RunForClass()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var sum = 0L;
                for (var j = 0; j < ClassList.Length; j++)
                {
                    sum += GetIdClass(ClassList[j]);
                }
            }
        }

        private static int GetIdClass(PersonClass @class)
        {
            return @class.Id;
        }

        [Benchmark(Description = "Class interface")]
        public void RunForClassInterface()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var sum = 0L;
                for (var j = 0; j < ClassList.Length; j++)
                {
                    sum += GetIdInterface(ClassList[j]);
                }
            }
        }
        
        [Benchmark(Description = "Big struct")]
        public void RunForBigStruct()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var sum = 0L;
                for (var j = 0; j < BigStructsList.Length; j++)
                {
                    sum += GetIdBigStruct(BigStructsList[j]);
                }
            }
        }

        private static int GetIdBigStruct(PersonBigStruct @struct)
        {
            return @struct.Id;
        }

        [Benchmark(Description = "Big struct ref")]
        public void RunForRefBigStruct()
        {
            for (var i = 0; i < NumberOfIterations; i++)
            {
                var sum = 0L;
                for (var j = 0; j < BigStructsList.Length; j++)
                {
                    sum += GetIdRefBigStruct(ref BigStructsList[j]);
                }
            }
        }

        private static int GetIdRefBigStruct(ref PersonBigStruct @struct)
        {
            return @struct.Id;
        }
    }
}
using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using StructsBenchmark.Models;

namespace StructsBenchmark
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        private static PersonStruct Struct;
        private static PersonBigStruct BigStruct;
        private static PersonClass Class;

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
                Position = "Manager",
                BirthDate = new DateTime(1990, 10, 10),
                PositionId = 1
            };

            Class = new PersonClass
            {
                Id = 1,
                FirstName = "First name",
                LastName = "Last name"
            };
        }

        [Benchmark(Description = "Struct")]
        public void RunForStruct()
        {
            _ = GetIdStruct(Struct);
        }

        private static int GetIdStruct(PersonStruct @struct)
        {
            return @struct.Id;
        }

        [Benchmark(Description = "Struct ref")]
        public void RunForRefStruct()
        {
            _ = GetIdRefStruct(ref Struct);
        }

        private static int GetIdRefStruct(ref PersonStruct @struct)
        {
            return @struct.Id;
        }

        [Benchmark(Description = "Struct interface")]
        public void RunForInterfaceStruct()
        {
            _ = GetIdInterface(Struct);
        }

        private static int GetIdInterface(IPerson person)
        {
            return person.Id;
        }

        [Benchmark(Description = "Class")]
        public void RunForClass()
        {
            _ = GetIdClass(Class);
        }

        private static int GetIdClass(PersonClass @class)
        {
            return @class.Id;
        }

        [Benchmark(Description = "Class interface")]
        public void RunForClassInterface()
        {
            _ = GetIdInterface(Class);
        }
        
        [Benchmark(Description = "Big struct")]
        public void RunForBigStruct()
        {
            _ = GetIdBigStruct(BigStruct);
        }

        private static int GetIdBigStruct(PersonBigStruct @struct)
        {
            return @struct.Id;
        }

        [Benchmark(Description = "Big struct ref")]
        public void RunForRefBigStruct()
        {
            _ = GetIdRefBigStruct(ref BigStruct);
        }

        private static int GetIdRefBigStruct(ref PersonBigStruct @struct)
        {
            return @struct.Id;
        }
    }
}
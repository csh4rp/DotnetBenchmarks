using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EnumsBenchmark.Models
{
    public abstract class Enumeration
    {
        public int Id { get; protected set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        protected Enumeration(int id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
        
        public static implicit operator int(Enumeration enumeration) => enumeration.Id;
    }
    
    public abstract class Enumeration<TEnumeration> : Enumeration where TEnumeration : Enumeration
    {
        protected Enumeration(int id, string name, string description) : base(id, name, description)
        {
        }

        private static readonly List<TEnumeration> Values = CreateValues();

        private static readonly Dictionary<string, TEnumeration> ValuesDictionary =
            Values.ToDictionary(k => k.Name, v => v);

        public static IReadOnlyList<TEnumeration> GetValues() => Values;
        
        public override string ToString() => Name;
        
        private static List<TEnumeration> CreateValues()
            => typeof(TEnumeration).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(prop => typeof(Enumeration).IsAssignableFrom(prop.FieldType))
                .Select(member => (TEnumeration) member.GetValue(null))
                .ToList();

        public static bool IsDefined(int value) => Values.Any(v => v.Id == value);
        public static bool IsDefined(string value) => ValuesDictionary.ContainsKey(value);
        public static TEnumeration Parse(string value) => ValuesDictionary[value];

        public static TEnumeration Parse(string value, bool ignoreCase) => ignoreCase 
            ? Values.First(v => v.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase))
            : ValuesDictionary[value];

        public static bool TryParse(string value, out TEnumeration enumeration) =>
            ValuesDictionary.TryGetValue(value, out enumeration);

        public static bool TryParse(string value, bool ignoreCase, out TEnumeration enumeration)
        {
            if (!ignoreCase)
            {
                return ValuesDictionary.TryGetValue(value, out enumeration);
            }
            
            enumeration = Values.FirstOrDefault(v =>
                v.Name.Equals(value, StringComparison.InvariantCultureIgnoreCase));

            return enumeration != default;
        }
    }
}
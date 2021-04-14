using System;
using System.ComponentModel;
using System.Reflection;

namespace EnumsBenchmark.Models
{
    public static class Extensions
    {
        public static string GetDescription(this Enum @enum)
        {
            var type = @enum.GetType();
            var member = type.GetMember(@enum.ToString());
            var attr = member[0].GetCustomAttribute<DescriptionAttribute>();
            return attr!.Description;
        }
    }
}
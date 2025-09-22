using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
            return attr?.Description ?? value.ToString();
        }

        public static TEnum ToEnumByDescription<TEnum>(string description) where TEnum : Enum
        {
            foreach (var value in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                if (value.GetDescription().Equals(description, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            throw new ArgumentException($"No {typeof(TEnum).Name} with description '{description}' found.");
        }
    }
}
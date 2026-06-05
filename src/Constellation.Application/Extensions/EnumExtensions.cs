namespace Constellation.Application.Extensions;

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class EnumExtensions
{
    extension(Enum value)
    {
        public string GetDisplayName()
        {
            Type enumType = value.GetType();
            string enumValue = Enum.GetName(enumType, value);
            MemberInfo member = enumType.GetMember(enumValue)[0];

            object[] attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);

            if (attrs.Length == 0)
                return enumValue;

            var displayAttribute = (DisplayAttribute)attrs[0];

            if (displayAttribute.ResourceType != null)
                return displayAttribute.GetName();

            return displayAttribute.Name ?? enumValue;
        }
    }
}
namespace Constellation.Application.Extensions;

using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum value)
    {
        Type enumType = value.GetType();
        string enumValue = Enum.GetName(enumType, value);
        MemberInfo member = enumType.GetMember(enumValue)[0];

        object[] attrs = member.GetCustomAttributes(typeof(DisplayAttribute), false);
        string outString = ((DisplayAttribute)attrs[0]).Name;

        if (((DisplayAttribute)attrs[0]).ResourceType != null)
        {
            outString = ((DisplayAttribute)attrs[0]).GetName();
        }

        return outString;
    }
}
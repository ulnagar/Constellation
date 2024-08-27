namespace Constellation.Core.Models.Grade.Enums;

using Common;

public sealed class Grade : IntEnumeration<Grade>
{
    public static Grade Year05 => new(5, "Year 5");
    public static Grade Year06 => new(6, "Year 6");
    public static Grade Year07 => new(7, "Year 7");
    public static Grade Year08 => new(8, "Year 8");
    public static Grade Year09 => new(9, "Year 9");
    public static Grade Year10 => new(10, "Year 10");
    public static Grade Year11 => new(11, "Year 11");
    public static Grade Year12 => new(12, "Year 12");
    public static Grade Special => new(13, "Special");

    private Grade(int value, string name) 
        : base(value, name) { }
}
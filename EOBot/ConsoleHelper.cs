using System;
using System.Linq;
using System.Reflection;

namespace EOBot
{
    internal sealed class StringValue : Attribute
    {
        public string Value { get; set; }

        public StringValue(string value) => Value = value;
    }

    internal static class EnumExtensions
    {
        public static string String(this Enum item)
        {
            var enumType = item.GetType();
            var memberInfos = enumType.GetMember(item.ToString());
            var enumValueMemberInfo = memberInfos.FirstOrDefault(m => m.DeclaringType == enumType);
            var valueAttributes = enumValueMemberInfo.GetCustomAttributes<StringValue>(false);
            return valueAttributes.Single().Value;
        }
    }

    public static class ConsoleHelper
    {
        public enum Type
        {
            [StringValue("    ")]
            None,
            [StringValue("WRN ")]
            Warning,
            [StringValue("ERR ")]
            Error,
            [StringValue("ATK ")]
            Attack,
            [StringValue("MOVE")]
            Move,
            [StringValue("WALK")]
            Walk,
            [StringValue("FACE")]
            Face,
            [StringValue("TAKE")]
            TakeItem,
            [StringValue("JUNK")]
            JunkItem,
            [StringValue("USE ")]
            UseItem,
            [StringValue("CAST")]
            Cast,
            [StringValue("SIT ")]
            Sit,
            [StringValue("HIT ")]
            Hit,
            [StringValue("EXP ")]
            Experience,
            [StringValue("HEAL")]
            Heal,
            [StringValue("DMG ")]
            Damage,
            [StringValue("DEAD")]
            Dead,
            [StringValue("ITEM")]
            Item,
            [StringValue("CHAT")]
            Chat
        }

        public static void WriteMessage(Type messageType, string message, ConsoleColor color = ConsoleColor.Gray)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;

            Console.WriteLine($"[{messageType.String()}] {message}");

            Console.ForegroundColor = oldColor;
        }
    }
}

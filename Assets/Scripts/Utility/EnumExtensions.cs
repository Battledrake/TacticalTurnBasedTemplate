using System;
using System.Linq;

namespace BattleDrakeCreations.TacticalTurnBasedTemplate
{
    public static class EnumExtensions
    {
        public static TEnum Next<TEnum>(this TEnum value) where TEnum : Enum
        {
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>().ToList();
            var currentIndex = values.IndexOf(value);
            var nextIndex = (currentIndex + 1) % values.Count;
            return values[nextIndex];
        }
    }
}
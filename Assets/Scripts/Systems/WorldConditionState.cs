using UnityEngine;

namespace Afterworld.Systems
{
    public enum WorldTimeOfDay
    {
        Dawn,
        Day,
        Dusk,
        Night
    }

    public enum WorldWeather
    {
        Clear,
        Mist,
        Rain,
        Storm
    }

    public class WorldConditionState : MonoBehaviour
    {
        [Header("Current Conditions")]
        [SerializeField] private WorldTimeOfDay timeOfDay = WorldTimeOfDay.Day;
        [SerializeField] private WorldWeather weather = WorldWeather.Clear;

        public WorldTimeOfDay TimeOfDay => timeOfDay;
        public WorldWeather Weather => weather;
    }
}

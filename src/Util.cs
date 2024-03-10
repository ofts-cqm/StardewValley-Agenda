using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace MyAgenda
{
    internal class Util
    {
        public static double previousLuckLevel = 0;
        public static bool MainlandRained = false, IslandRained = false;

        public static void drawMiddle(SpriteBatch b, string text, Rectangle box, Color color, SpriteFont font)
        {
            Vector2 measured = font.MeasureString(text);
            int startX = (int)((box.Width - measured.X)/2);
            int startY = (int)((box.Height - measured.Y)/2);
            b.DrawString(font, text, new Vector2(box.X + startX, box.Y + startY), color);
        }

        public static Vector2 drawStr(SpriteBatch b, string str, Rectangle rec, SpriteFont font, int start_x = 0)
        {
            int baseIndex = 0, ypos = rec.Y;
            for (int i = 0; i < str.Length; i++)
            {
                Vector2 measured = font.MeasureString(str.Substring(baseIndex, i - baseIndex));
                if (measured.Y + ypos > rec.Y + rec.Height)
                {
                    measured.X = rec.X;
                    measured.Y = rec.Y + rec.Height;
                    return measured;
                }
                if (measured.X + start_x > rec.Width)
                {
                    b.DrawString(font, str.Substring(baseIndex, i - baseIndex - 1), new Vector2(rec.X + start_x, ypos), Color.Black);
                    ypos += (int)measured.Y;
                    start_x = 0;
                    baseIndex = i - 1;
                }
            }
            b.DrawString(font, str.Substring(baseIndex), new Vector2(rec.X, ypos), Color.Black);
            Vector2 finalPoint = font.MeasureString(str.Substring(baseIndex));
            finalPoint.Y += ypos;
            finalPoint.X += rec.X;
            return finalPoint;
        }

        public static byte examinHelper(int[] trigger)
        {
            if (trigger[1] == 11)
            {
                return 0xA0;
            }

            trigger[1]--;

            if (trigger[0] == 3) return (trigger[1] == 0) ? (byte)0xC0 : (byte)0x80; 
            return (trigger[1] == 0) ? (byte)0xE0 : (byte)0xA0;
        }

        public static bool isWeatherRain(int weather)
        {
            return weather == Game1.weather_rain || weather == Game1.weather_snow || weather == Game1.weather_lightning;
        }

        public static bool isRainTomorrow()
        {
            var date = new WorldDate(Game1.Date);
            ++date.TotalDays;
            var tomorrowWeather = Game1.IsMasterGame
                ? Game1.weatherForTomorrow
                : Game1.netWorldState.Value.WeatherForTomorrow;
            int weather =  Game1.getWeatherModificationsForDate(date, tomorrowWeather);
            return isWeatherRain(weather);
        }

        public static bool isIslandRainTomorrow()
        {
            return isWeatherRain(Game1.netWorldState.Value.GetWeatherForLocation(GameLocation.LocationContext.Island).weatherForTomorrow.Value);
        }

        public static bool isRainHere(GameLocation.LocationContext context)
        {
            Trigger.monitor.Log($"context: {context}", LogLevel.Info);
            var weather = Game1.netWorldState.Value.GetWeatherForLocation(context);
            Trigger.monitor.Log($"weather: {weather.isRaining.Value}, {weather.isSnowing.Value}, {weather.isLightning.Value}", LogLevel.Info);
            return weather.isRaining.Value || weather.isSnowing.Value || weather.isLightning.Value;
        }

        /*
         * 1: is triggered
         * 2: should be deleted
         * 3: today (1) or tomorrow (0)
         */
        public static byte examinDate(int[] trigger)
        {
            if (trigger[0] == 0 || trigger[1] == 0 || trigger[2] == 0) { return 1; }
            if ((trigger[2] == 12 || trigger[2] == 13) && trigger[0] == 3) { return 2; }

            if (trigger[2] > 0 && trigger[2] < 8)
                if((Game1.dayOfMonth-1) % 7 == trigger[2] - 1) return examinHelper(trigger);

            
            switch (trigger[0])
            {
                case 1: 
                    if (trigger[2] == 8) return MainlandRained ? examinHelper(trigger) : (byte)4;
                    if (trigger[2] == 9) return IslandRained ? examinHelper(trigger) : (byte)5;
                    if (trigger[2] == 10) return MainlandRained ? (byte)0 : examinHelper(trigger);
                    if (trigger[2] == 11) return IslandRained ? (byte)0 : examinHelper(trigger);
                    if (trigger[2] == 12 && previousLuckLevel > 0.02) return examinHelper(trigger);
                    if (trigger[2] == 13 && previousLuckLevel < -0.02) return examinHelper(trigger);
                    break;
                case 2:
                    if (trigger[2] == 8) return isRainHere(GameLocation.LocationContext.Default) ? examinHelper(trigger) : (byte)0;
                    if (trigger[2] == 9) return isRainHere(GameLocation.LocationContext.Island) ? examinHelper(trigger) : (byte)0;
                    if (trigger[2] == 10) return isRainHere(GameLocation.LocationContext.Default) ? (byte)6 : examinHelper(trigger);
                    if (trigger[2] == 11) return isRainHere(GameLocation.LocationContext.Island) ? (byte)7 : examinHelper(trigger);
                    if (trigger[2] == 12 && Game1.player.DailyLuck > 0.02) return examinHelper(trigger);
                    if (trigger[2] == 13 && Game1.player.DailyLuck < -0.02) return examinHelper(trigger);
                    break;
                case 3:
                    if (trigger[2] == 8) return isRainTomorrow() ? examinHelper(trigger) : (byte)8;
                    if (trigger[2] == 9) return isIslandRainTomorrow() ? examinHelper(trigger) : (byte)9;
                    if (trigger[2] == 10) return isRainTomorrow() ? (byte)0 : examinHelper(trigger);
                    if (trigger[2] == 11) return isIslandRainTomorrow() ? (byte)0 : examinHelper(trigger);
                    break;
            }
            /*
            if (trigger[0] == 2)
            {
                if (trigger[2] == 12 && Game1.player.DailyLuck > 0.02) return examinHelper(trigger);
                if (trigger[2] == 13 && Game1.player.DailyLuck < -0.02) return examinHelper(trigger);
            }

            if (trigger[0] == 1)
            {
                if (trigger[2] == 12 && previousLuckLevel > 0.02) return examinHelper(trigger);
                if (trigger[2] == 13 && previousLuckLevel < -0.02) return examinHelper(trigger);
                if (trigger[2] == 8) return MainlandRained ? examinHelper(trigger) : (byte)0;
                if (trigger[2] == 9) return IslandRained ? examinHelper(trigger) : (byte)0;
                if (trigger[2] == 10) return MainlandRained ? (byte)0 : examinHelper(trigger);
                if (trigger[2] == 11) return IslandRained ? (byte)0 : examinHelper(trigger);
            }

            if (trigger[0] == 3)
            {
                if (trigger[2] == 8) return isRainTomorrow() ? examinHelper(trigger) : (byte)0;
                if (trigger[2] == 9) return isIslandRainTomorrow() ? examinHelper(trigger) : (byte)0;
                if (trigger[2] == 10) return isRainTomorrow() ? (byte)0 : examinHelper(trigger);
                if (trigger[2] == 11) return isIslandRainTomorrow() ? (byte)0 : examinHelper(trigger);
            }*/

            return 3;
        }
    }
}

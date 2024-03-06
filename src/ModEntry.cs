using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace MyAgenda
{
    internal sealed class ModEntry : Mod
    {
        ModConfig Config;

        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();
            Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            Helper.Events.Display.MenuChanged += this.OnUIUpdated;
            Helper.Events.Display.WindowResized += this.onWindowResized;
            Helper.Events.GameLoop.Saving += this.onSaveSaved;
            Helper.Events.GameLoop.SaveLoaded += this.onSaveLoaded;
            Helper.Events.GameLoop.DayStarted += this.dailyCheck;
            Helper.ConsoleCommands.Add("agenda", "check the items on agenda at the specified date\nUsage: agenda [season(0-3)] [date(0-27)]", query);

            Agenda.monitor = this.Monitor;
            AgendaPage.monitor = this.Monitor;
        }
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this.Config.AgendaKey.JustPressed() && Context.IsWorldReady && Game1.activeClickableMenu == null)
            {
                Game1.activeClickableMenu =Agenda.Instance;
            }
        }

        private void OnUIUpdated(object sender, MenuChangedEventArgs e) 
        {
            if(e.OldMenu != null && e.OldMenu is AgendaPage page)
            {
                 Agenda.save(page.season, page.day - 1);
            }

            if(e.NewMenu is Billboard board && !Helper.Reflection.GetField<bool>(board, "dailyQuestBoard").GetValue() && Config.Replace_Calender_With_Agenda)
            {
                board.exitThisMenu();
                Game1.activeClickableMenu = Agenda.Instance;
            }
        }

        private void onWindowResized(object sender, WindowResizedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            Agenda.Instance.gameWindowSizeChanged(new Rectangle(0, 0, e.OldSize.X, e.OldSize.Y), new Rectangle(0, 0, e.NewSize.X, e.NewSize.Y));
            Agenda.agendaPage.resize();
        }
        
        private void onSaveSaved(object sender, SavingEventArgs e)
        {
            Agenda.write(Helper);
        }

        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Agenda.Instance = new Agenda(Helper);
            Agenda.agendaPage = new AgendaPage(Helper);
        }

        private void dailyCheck(object sender, DayStartedEventArgs e)
        {
            if(Agenda.hasSomethingToDo(Utility.getSeasonNumber(Game1.currentSeason), Game1.dayOfMonth - 1))
            {
                Game1.addHUDMessage(new HUDMessage(Helper.Translation.Get("pop_up"), 2));
            }
        }

        private void query(string commend, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                Monitor.Log("Save not Loaded Yet!", LogLevel.Error);
            }

            int season, day;
            try
            {
                season = int.Parse(args[0]);
                day = int.Parse(args[1]);
                Monitor.Log($"retrieving item on season {Utility.getSeasonNameFromNumber(season)}, day {day + 1}\ntitle: \n{Agenda.pageTitle[season, day]}\nBirthday: {Agenda.pageBirthday[season, day]}, Festival: {Agenda.pageFestival[season, day]}\nNotes: \n{Agenda.pageNote[season, day]}", LogLevel.Info);
            }catch (System.Exception)
            {
                Monitor.Log("INCOMPLETE COMMEND!", LogLevel.Error);
            }
        }
    }
    public sealed class ModConfig
    {
        public KeybindList AgendaKey { get; set; } = KeybindList.Parse("G");
        public bool Replace_Calender_With_Agenda { get; set; } = false;
    }
}
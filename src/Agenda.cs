using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley;
using StardewModdingAPI;

namespace MyAgenda
{
    public class Agenda : IClickableMenu
    {
        public static Agenda Instance;
        public static AgendaPage agendaPage;
        public static Texture2D agendaTexture, buttonTexture;
        public static ClickableTextureComponent prev, next, hover;
        public static string[,] pageTitle, pageBirthday, pageFestival, pageNote, titleSubsitute;
        public static int season;
        public static Rectangle[] bounds;
        public static Rectangle hoverBounds;
        public static IMonitor monitor;
        public static IModHelper helper;
        public static string hoverText = "";

        public Agenda(IModHelper helper)
            : base(0, 0, 0, 0, showUpperRightCloseButton: true)
        {
            Agenda.helper = helper;
            agendaTexture = helper.ModContent.Load<Texture2D>("assets\\Agenda");
            buttonTexture = helper.ModContent.Load<Texture2D>("assets\\buttons");

            pageTitle = helper.Data.ReadSaveData<string[,]>("title");
            titleSubsitute = new string[4, 28];
            pageBirthday = new string[4, 28];
            pageFestival = new string[4, 28];
            pageNote = helper.Data.ReadSaveData<string[,]>("notes");

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 28; j++)
                {
                    pageBirthday[i, j] = "";
                    pageFestival[i, j] = "";
                    titleSubsitute[i, j] = "";
                }
            }

            if (pageTitle == null)
            {
                pageTitle = new string[4, 28];
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        pageTitle[i, j] = "";
                    }
                }
            }
            if (pageNote == null)
            {
                pageNote = new string[4, 28];
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 28; j++)
                    {
                        pageNote[i, j] = "";
                    }
                }
            }
            bounds = new Rectangle[28];

            season = Utility.getSeasonNumber(Game1.currentSeason);
            width = 316 * 4;
            height = 230 * 4;
            Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            xPositionOnScreen = (int)topLeftPositionForCenteringOnScreen.X;
            yPositionOnScreen = (int)topLeftPositionForCenteringOnScreen.Y;
            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 20, yPositionOnScreen, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
            prev = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 875, yPositionOnScreen + 50, 96, 48), buttonTexture, new Rectangle(0, 24, 48, 24), 2f);
            next = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 971, yPositionOnScreen + 50, 96, 48), buttonTexture, new Rectangle(0, 0, 48, 24), 2f);
            hoverBounds = new Rectangle(-200, -100, 48 * 4, 24 * 4);
            hover = new ClickableTextureComponent(hoverBounds, buttonTexture, new Rectangle(0, 48, 48, 24), 2f);
            for (int i = 0; i < 28; i++)
            {
                bounds[i] = new Rectangle(xPositionOnScreen + (i) % 7 * 40 * 4 + 75, yPositionOnScreen + 220 + (i) / 7 * 40 * 4, 38 * 4, 38 * 4);
            }
            foreach (NPC allCharacter in Utility.getAllCharacters())
            {
                if (allCharacter.isVillager() && allCharacter.Birthday_Season != null && (Game1.player.friendshipData.ContainsKey(allCharacter.Name) || (!allCharacter.Name.Equals("Dwarf") && !allCharacter.Name.Equals("Sandy") && !allCharacter.Name.Equals("Krobus"))))
                {
                    pageBirthday[Utility.getSeasonNumber(allCharacter.Birthday_Season), allCharacter.Birthday_Day - 1] = allCharacter.displayName;
                    titleSubsitute[Utility.getSeasonNumber(allCharacter.Birthday_Season), allCharacter.Birthday_Day - 1] += helper.Translation.Get("birthday_title", new { character = allCharacter.displayName });
                }
            }
            var festivals = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\FestivalDates");

            foreach (KeyValuePair<string, string> p in festivals)
            {
                int[] date = getDate(p.Key);
                pageFestival[date[0], date[1]] = p.Value;
                titleSubsitute[date[0], date[1]] += p.Value;
            }

            pageFestival[3, 14] += helper.Translation.Get("nightMarket");
            pageFestival[3, 15] += helper.Translation.Get("nightMarket");
            pageFestival[3, 16] += helper.Translation.Get("nightMarket");
            titleSubsitute[3, 14] += helper.Translation.Get("nightMarket");
            titleSubsitute[3, 15] += helper.Translation.Get("nightMarket");
            titleSubsitute[3, 16] += helper.Translation.Get("nightMarket");

            if (Game1.options.SnappyMenus)
            {
                populateClickableComponentList();
                snapToDefaultClickableComponent();
            }
        }

        public static int[] getDate(String date)
        {
            for (int i = 0; i < date.Length; i++)
            {
                if (date[i] < 'a')
                {
                    return new int[] { Utility.getSeasonNumber(date.Substring(0, i)), int.Parse(date.Substring(i)) - 1 };
                }
            }
            return null;
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = getComponentWithID(0);
            snapCursorToCurrentSnappedComponent();
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            Game1.playSound("bigDeSelect");
            exitThisMenu();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if(next.containsPoint(x, y))
            {
                season ++;
                season %= 4;
                return;
            }

            if (prev.containsPoint(x, y))
            {
                season+=3;
                season %= 4;
                return;
            }

            for (int i = 0; i < 28; i++)
            {
                if (bounds[i].Contains(x, y))
                {
                    exitThisMenu();
                    Game1.keyboardDispatcher.Subscriber = AgendaPage.tbox;
                    agendaPage.note = pageNote[season, i];
                    agendaPage.title = pageTitle[season, i];
                    agendaPage.subsituteTitle = titleSubsitute[season, i];
                    agendaPage.birthday = pageBirthday[season, i];
                    agendaPage.festival = pageFestival[season, i];
                    agendaPage.season = season;
                    agendaPage.day = i + 1;
                    Game1.activeClickableMenu = agendaPage;
                    return;
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            hoverText = "";

            if(prev.containsPoint(x, y))
            {
                hover.bounds = prev.bounds;
            }
            else if(next.containsPoint(x, y))
            {
                hover.bounds = next.bounds;
            }
            else
            {
                hover.bounds = hoverBounds;
            }

            if (bounds == null)
            {
                return;
            }

            for (int i = 0; i < 28; i++)
            {
                if (bounds[i].Contains(x, y))
                {
                    hoverText = pageNote[season, i].Substring(0, Math.Min(pageNote[season, i].Length, 20));
                    return;
                }
            }
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            b.Draw(agendaTexture, new Vector2(xPositionOnScreen, yPositionOnScreen - 226 * 4), new Rectangle(0, 0, 316, 456), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            b.DrawString(Game1.dialogueFont, Utility.getSeasonNameFromNumber(season), new Vector2(xPositionOnScreen + 160, yPositionOnScreen + 80), Game1.textColor);
            for (int i = 0; i < 28; i++)
            {
                drawStr(b, (pageTitle[season, i] == "" ? titleSubsitute[season, i] : pageTitle[season, i]), bounds[i], Game1.dialogueFont);

                if (season != Utility.getSeasonNumber(Game1.currentSeason)) { continue; }

                if (Game1.dayOfMonth > i + 1)
                {
                    b.Draw(Game1.staminaRect, bounds[i], Color.Gray * 0.25f);
                }
                else if (Game1.dayOfMonth == i + 1)
                {
                    int num = (int)(4f * Game1.dialogueButtonScale / 8f);
                    drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), bounds[i].X - num, bounds[i].Y - num, bounds[i].Width + num * 2, bounds[i].Height + num * 2, Color.Blue, 4f, drawShadow: false);
                }
            }

            base.draw(b);
            prev.draw(b);
            next.draw(b);
            hover.draw(b);
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
            if (hoverText.Length > 0)
            {
                drawHoverText(b, hoverText, Game1.dialogueFont);
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            width = 316 * 4;
            height = 230 * 4;
            Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            xPositionOnScreen = (int)topLeftPositionForCenteringOnScreen.X;
            yPositionOnScreen = (int)topLeftPositionForCenteringOnScreen.Y;
            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 20, yPositionOnScreen, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
            prev = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 875, yPositionOnScreen + 50, 96, 48), buttonTexture, new Rectangle(0, 24, 48, 24), 2f);
            next = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 971, yPositionOnScreen + 50, 96, 48), buttonTexture, new Rectangle(0, 0, 48, 24), 2f);
            hoverBounds = new Rectangle(-200, -100, 48 * 4, 24 * 4);
            hover = new ClickableTextureComponent(hoverBounds, buttonTexture, new Rectangle(0, 48, 48, 24), 2f);
            for (int i = 0; i < 28; i++)
            {
                bounds[i] = new Rectangle(xPositionOnScreen + (i) % 7 * 40 * 4 + 75, yPositionOnScreen + 224 + (i) / 7 * 40 * 4, 40 * 4, 40 * 4);
            }
        }

        public static void save(int season, int day)
        {
            pageTitle[season, day] = agendaPage.title;
            pageNote[season, day] = agendaPage.note;
            agendaPage.selected = 0;
        }

        public static void write(IModHelper helper)
        {
            helper.Data.WriteSaveData("title", pageTitle);
            helper.Data.WriteSaveData("notes", pageNote);
        }

        public static Vector2 drawStr(SpriteBatch b, string str, Rectangle rec, SpriteFont font, int start_x = 0)
        {
            int baseIndex = 0, ypos = rec.Y;
            for(int i = 0; i < str.Length; i++)
            {
                Vector2 measured = font.MeasureString(str.Substring(baseIndex, i - baseIndex));
                if(measured.Y + ypos > rec.Y + rec.Height)
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

        public static bool hasSomethingToDo(int season, int day)
        {
            return pageTitle[season, day] != "" || titleSubsitute[season, day] != "" || pageNote[season, day] != "";
        }
    }
}

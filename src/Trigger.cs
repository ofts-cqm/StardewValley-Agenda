using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley;
using StardewModdingAPI;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using static System.Net.Mime.MediaTypeNames;
using System;

namespace MyAgenda
{
    internal class Trigger : IClickableMenu
    {
        public static Trigger Instance;
        public static Texture2D pageTexture;
        public static int baseheight, basewidth, currentIndex = -1, index1, index2, index3, selected = 0, ticks = 0;
        public static int[] selectedTrigger = new int[3], renderOrder = new int[3];
        public static ChooseFromListMenu triggerListMenu;
        public static ClickableTextureComponent warning, hover;
        public static Rectangle[] bounds = new Rectangle[8];
        public static Rectangle hoverBounds;
        public static IModHelper helper;
        public static IMonitor monitor;
        public static TextBox tbox;
        public static string title, note;
        public static string[][] choices;
        public static bool warningshown = false, choosing = false;

        public Trigger() : base(0, 0, 0 ,0, true){
            pageTexture = helper.ModContent.Load<Texture2D>("assets\\page");
            tbox = new TextBox(null, null, Game1.dialogueFont, Color.Black);
            tbox.X = 100000000;
            tbox.Y = 100000000;
            tbox.Width = 114514;
            tbox.Height = 114514;
            tbox.OnEnterPressed += textBoxEnter;
            resize();
            reloadTriggerOptions(null, null);
        }

        public void loadTrigger(int index)
        {
            choosing = true;
            monitor.Log($"selected trigger index {index}", LogLevel.Info);
            currentIndex = index;
            triggerListMenu = new ChooseFromListMenu(new List<string>(choices[index]), triggerChose, default_selection : choices[index][selectedTrigger[index]]);
        }

        public void triggerChose(string str)
        {
            choosing = false;
            monitor.Log(str, LogLevel.Info);
            for (int i = 0; i < choices[currentIndex].Length; i++) 
            {
                if (choices[currentIndex][i] == str)
                {
                    selectedTrigger[currentIndex] = i;
                }
            }
        }

        public static void saveTrigger() 
        {
            monitor.Log("Trigger saved", LogLevel.Info);
            Instance.exitThisMenu();
            Game1.activeClickableMenu = Agenda.agendaPage;
        }

        public override void draw(SpriteBatch b)
        {
            ticks++;
            ticks %= 60;
            if (selected == 1)
            {
                title = tbox.Text;
            }
            else if (selected == 2)
            {
                note = tbox.Text;
            }

            if(!warningshown && !choosing) b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            b.Draw(pageTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(0, 0, 200, 238), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            if (selected == 1 && ticks >= 30)
            {
                Agenda.drawStr(b, title == "" ? helper.Translation.Get("subsitute") : title + "|", bounds[0], Game1.dialogueFont);
            }
            else
            {
                Agenda.drawStr(b, title == "" ? helper.Translation.Get("subsitute") : title, bounds[0], Game1.dialogueFont);
            }

            Agenda.drawStr(b, helper.Translation.Get("trigger"), bounds[1], Game1.dialogueFont);
            Rectangle triggerBox = bounds[2];
            index1 = (int)Agenda.drawStr(b, choices[renderOrder[0]][selectedTrigger[renderOrder[0]]], triggerBox, Game1.smallFont).X;
            triggerBox.X = index1;
            index2 = (int)Agenda.drawStr(b, choices[renderOrder[1]][selectedTrigger[renderOrder[1]]], triggerBox, Game1.smallFont).X;
            triggerBox.X = index2;
            index3 = (int)Agenda.drawStr(b, choices[renderOrder[2]][selectedTrigger[renderOrder[2]]], triggerBox, Game1.smallFont).X;

            if (selected == 2 && ticks >= 30)
            {
                Agenda.drawStr(b, note + "|", bounds[3], Game1.smallFont);
            }
            else
            {
                Agenda.drawStr(b, note, bounds[3], Game1.smallFont);
            }

            if (warningshown)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
                warning.draw(b);
                hover.draw(b);
                b.DrawString(Game1.dialogueFont, helper.Translation.Get("save"), new Vector2(bounds[4].X + 12, bounds[4].Y + 20), Color.Black);
                b.DrawString(Game1.dialogueFont, helper.Translation.Get("dont_save"), new Vector2(bounds[5].X + 12, bounds[4].Y + 20), Color.Black);
                b.DrawString(Game1.dialogueFont, helper.Translation.Get("cancel"), new Vector2(bounds[6].X + 12, bounds[4].Y + 20), Color.Black);
                //Agenda.drawStr(b, helper.Translation.Get("save"), bounds[4], Game1.dialogueFont);
                //Agenda.drawStr(b, helper.Translation.Get("dont_save"), bounds[5], Game1.dialogueFont);
                //Agenda.drawStr(b, helper.Translation.Get("cancel"), bounds[6], Game1.dialogueFont);
                Agenda.drawStr(b, helper.Translation.Get("warning"), bounds[7], Game1.dialogueFont);
            }
            else if (choosing)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
                triggerListMenu.draw(b);
            }
            else
            {
                base.draw(b);
            }

            tbox.Draw(b);
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }

        public override void performHoverAction(int x, int y)
        {
            if (choosing)
            {
                triggerListMenu.performHoverAction(x, y);
                return;
            }

            base.performHoverAction(x, y);

            if (bounds[4].Contains(x, y))
            {
                hover.bounds = bounds[4];
            }
            else if (bounds[5].Contains(x, y))
            {
                hover.bounds = bounds[5];
            }
            else if (bounds[6].Contains(x, y))
            {
                hover.bounds = bounds[6];
            }
            else
            {
                hover.bounds = hoverBounds;
            }
        }

        public void resize()
        {
            width = 200 * 4;
            height = 238 * 4;
            Vector2 topLeftPositionForCenteringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            xPositionOnScreen = (int)topLeftPositionForCenteringOnScreen.X;
            yPositionOnScreen = (int)topLeftPositionForCenteringOnScreen.Y;
            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 20, yPositionOnScreen, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
            bounds[0] = new Rectangle(xPositionOnScreen + 52 * 4, yPositionOnScreen + 20 * 4, 130 * 4, 80);
            bounds[1] = new Rectangle(xPositionOnScreen + 52 * 4, yPositionOnScreen + 35 * 4, 130 * 4, 80);
            bounds[2] = new Rectangle(xPositionOnScreen + 52 * 4, yPositionOnScreen + 48 * 4, 130 * 4, 80);
            bounds[3] = new Rectangle(xPositionOnScreen + 52 * 4, yPositionOnScreen + 70 * 4, 130 * 4, 160 * 4);
            bounds[4] = new Rectangle(xPositionOnScreen + 5 * 4 + 8, yPositionOnScreen + 100 * 4 + 220, 48 * 4, 24 * 4);
            bounds[5] = new Rectangle(xPositionOnScreen + 56 * 4 + 8, yPositionOnScreen + 100 * 4 + 220, 48 * 4, 24 * 4);
            bounds[6] = new Rectangle(xPositionOnScreen + 143 * 4 + 8, yPositionOnScreen + 100 * 4 + 220, 48 * 4, 24 * 4);
            bounds[7] = new Rectangle(xPositionOnScreen + 10 * 4 + 8, yPositionOnScreen + 20 * 4 + 220, 176 * 4, 75 * 4);
            warning = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 8, yPositionOnScreen + 220, 196 * 4, 128 * 4), helper.ModContent.Load<Texture2D>("assets\\notification"), new Rectangle(0, 0, 192, 128), 4f);
            hoverBounds = new Rectangle(-2000, -1000, 48 * 4, 24 * 4);
            hover = new ClickableTextureComponent(hoverBounds, Agenda.buttonTexture, new Rectangle(0, 48, 48, 24), 4f);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (choosing)
            {
                if(triggerListMenu.cancelButton.containsPoint(x, y))
                {
                    choosing = false;
                    return;
                }
                triggerListMenu.receiveLeftClick(x, y, playSound);
                return;
            }

            if (warningshown)
            {
                if (bounds[4].Contains(x, y))
                {
                    warningshown = false;
                    saveTrigger(); 
                }
                else if (bounds[5].Contains(x, y))
                {
                    warningshown = false;
                    exitThisMenu();
                    Game1.activeClickableMenu = Agenda.Instance;
                    monitor.Log("Warn: trigger not saved", LogLevel.Warn);
                }
                else if (bounds[6].Contains(x, y))
                {
                    warningshown = false;
                    monitor.Log("Canceled", LogLevel.Info);
                }
                return;
            }

            //base.receiveLeftClick(x, y, playSound);
            if (upperRightCloseButton.containsPoint(x, y))
            {
                if (selectedTrigger[0] > 0 && selectedTrigger[0] < 4 &&
                    selectedTrigger[1] > 0 && selectedTrigger[1] < 12 &&
                    selectedTrigger[2] > 0 && selectedTrigger[2] < 14)
                {
                    saveTrigger();
                }
                else
                {
                    warningshown = true;
                }
                return;
            }

            if (bounds[2].Contains(x, y))
            {
                if(x <= index1)
                {
                    loadTrigger(renderOrder[0]);
                    return;
                }
                else if(x <= index2)
                {
                    loadTrigger(renderOrder[1]);
                    return;
                }
                else if(x <= index3)
                {
                    loadTrigger(renderOrder[2]);
                    return;
                }
            }

            if (bounds[0].Contains(x, y))
            {
                if (selected == 0)
                {
                    tbox.SelectMe();
                    tbox.Text = "";
                }
                if (selected != 1)
                {
                    tbox.Text = title;
                }
                selected = 1;
                return;
            }
            if (bounds[3].Contains(x, y))
            {
                if (selected == 0)
                {
                    tbox.SelectMe();
                    tbox.Text = "";
                }
                if (selected != 2)
                {
                    tbox.Text = note;
                }
                selected = 2;
                return;
            }
            if (selected != 0)
            {
                selected = 0;
                tbox.Selected = false;
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            if (choosing)
            {
                triggerListMenu.gameWindowSizeChanged(oldBounds, newBounds);
            }
            resize();
        }

        public void textBoxEnter(TextBox sender)
        {
            if (warningshown) return;
            sender.Text += "\n";
        }

        public override void receiveKeyPress(Keys key)
        {
            if (warningshown) return;
            if (!tbox.Selected && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
            {
                base.receiveKeyPress(key);
            }
        }

        public static void reloadTriggerOptions(object sender, LocaleChangedEventArgs e)
        {
            var tmp = helper.Translation.Get("trigger_order").ToString().Split(' ');
            renderOrder[0] = tmp[0][0] - '0';
            renderOrder[1] = tmp[1][0] - '0';
            renderOrder[2] = tmp[2][0] - '0';

            choices = new string[][] {
            new string[]{
                helper.Translation.Get("trigger_1-0"), 
                helper.Translation.Get("trigger_1-1"),
                helper.Translation.Get("trigger_1-2"),
                helper.Translation.Get("trigger_1-3")
            },
            new string[]{
                helper.Translation.Get("trigger_2-0"),
                helper.Translation.Get("trigger_2-1"),
                helper.Translation.Get("trigger_2-2"),
                helper.Translation.Get("trigger_2-3"),
                helper.Translation.Get("trigger_2-4"),
                helper.Translation.Get("trigger_2-5"),
                helper.Translation.Get("trigger_2-6"),
                helper.Translation.Get("trigger_2-7"),
                helper.Translation.Get("trigger_2-8"),
                helper.Translation.Get("trigger_2-9"),
                helper.Translation.Get("trigger_2-10"),
                helper.Translation.Get("trigger_2-11")
            },
            new string[]{
                helper.Translation.Get("trigger_3-0"),
                helper.Translation.Get("trigger_3-1"),
                helper.Translation.Get("trigger_3-2"),
                helper.Translation.Get("trigger_3-3"),
                helper.Translation.Get("trigger_3-4"),
                helper.Translation.Get("trigger_3-5"),
                helper.Translation.Get("trigger_3-6"),
                helper.Translation.Get("trigger_3-7"),
                helper.Translation.Get("trigger_3-8"),
                helper.Translation.Get("trigger_3-9"),
                helper.Translation.Get("trigger_3-10"),
                helper.Translation.Get("trigger_3-11"),
                helper.Translation.Get("trigger_3-12"),
                helper.Translation.Get("trigger_3-13")
            }
        };
    }
    }
}

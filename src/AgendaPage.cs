using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using StardewValley;
using Microsoft.Xna.Framework.Input;

namespace MyAgenda
{
    public class AgendaPage : IClickableMenu
    {
        public string festival, birthday, title, subsituteTitle, note;
        public int season, day, selected, ticks = 0;
        public static Texture2D pageTexture;
        public static Rectangle[] bounds = new Rectangle[4];
        public static IMonitor monitor;
        public static IModHelper helper;
        public static TextBox tbox;

        public AgendaPage(IModHelper helper) : base(0, 0, 0, 0, showUpperRightCloseButton: true)
        {
            pageTexture = helper.ModContent.Load<Texture2D>("assets\\page");
            tbox = new TextBox(null, null, Game1.dialogueFont, Color.Black);
            tbox.X = 100000000;
            tbox.Y = 100000000;
            tbox.Width = 114514;
            tbox.Height = 114514;
            tbox.OnEnterPressed += textBoxEnter;
            resize();
            AgendaPage.helper = helper;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (bounds[0].Contains(x, y))
            {
                if(selected == 0)
                {
                    tbox.SelectMe();
                    tbox.Text = "";
                }
                if(selected != 1)
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
            if(selected != 0)
            {
                selected = 0;
                tbox.Selected = false;
            }
            exitThisMenu();
            Game1.activeClickableMenu = Agenda.Instance;
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            resize();
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
        }

        public override void draw(SpriteBatch b)
        {
            ticks++;
            ticks %= 60;
            if(selected == 1)
            {
                title = tbox.Text;
            }
            else if(selected == 2)
            {
                note = tbox.Text;
            }

            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            b.Draw(pageTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(0, 0, 200, 238), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);

            if (selected == 1 && ticks >= 30)
            {
                Util.drawStr(b, getSuitableTitle() + "|", bounds[0], Game1.dialogueFont);
            }
            else
            {
                Util.drawStr(b, getSuitableTitle(), bounds[0], Game1.dialogueFont);
            }

            Util.drawStr(b, helper.Translation.Get("festival") + (festival == "" ? helper.Translation.Get("none") : festival), bounds[1], Game1.dialogueFont);
            Util.drawStr(b, helper.Translation.Get("birthday_page") + (birthday == "" ? helper.Translation.Get("none") : birthday), bounds[2], Game1.dialogueFont);
            
            if(selected == 2 && ticks >= 30)
            {
                Util.drawStr(b, note + "|", bounds[3], Game1.smallFont);
            }
            else
            {
                Util.drawStr(b, note, bounds[3], Game1.smallFont);
            }
            

            base.draw(b);
            tbox.Draw(b);
            Game1.mouseCursorTransparency = 1f;
            drawMouse(b);
        }

        public override void receiveKeyPress(Keys key)
        {
            if (!tbox.Selected && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
            {
                base.receiveKeyPress(key);
            }
        }
        public string getSuitableTitle()
        {
            if(title != "")
            {
                return title;
            }

            if(subsituteTitle != "")
            {
                return subsituteTitle;
            }

            return helper.Translation.Get("subsitute");
        }

        public void textBoxEnter(TextBox sender)
        {
            sender.Text += "\n";
        }
    }
}
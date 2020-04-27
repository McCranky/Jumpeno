using System.Numerics;
using System.Drawing;

namespace Jumpeno.JumpenoComponents.Entities {
    public enum State {
        IDLE, WALKING, FALLING, DEAD
    }

    /**
     * Umožnuje animovať zvhľad tela
     */
    public class Animation {
        public static readonly string[] _Skins = { "mageSprite_aer", "mageSprite_water", "mageSprite_earth", "mageSprite_fire", "mageSprite_magic"};
        public Vector2 Posiotion { get; set; }
        public string TextureName { get; }
        public string CssTexturePathBig => "images/big/" + TextureName;
        public string TexturePathBig => "wwwroot/images/big/" + TextureName;
        public string CssTexturePathSmall => "images/small/" + TextureName;
        public string TexturePathSmall => "wwwroot/images/small/" + TextureName;
        public Vector2 Size { get; set; }
        public State State { get; set; } = State.IDLE;
        public int CurrentImage { get; set; } = 0;
        public int ImageCount { get; set; }
        public int Delay { get; set; } = 10;
        public string CssStyle => $@"
            width: {Size.X}px;
            height: {Size.Y}px;
            background: url({CssTexturePathBig}) {-Posiotion.X}px {-Posiotion.Y}px;
            ";

        public Animation(string texture, Vector2 proportion, out Vector2 bodySize) {
            TextureName = texture;
            Image image = Image.FromFile(TexturePathBig); //"./wwwroot/images/" + texture

            Size = new Vector2((int)image.Width / proportion.X, (int)image.Height / proportion.Y);
            bodySize = new Vector2(Size.X, Size.Y);
            Posiotion = new Vector2 { X = 0, Y = 0 };
            ImageCount = (int)proportion.X;
        }

        public string GetFrameStyle(State ofState, int frame = 0) {
            return $@"
            width: {Size.X}px;
            height: {Size.Y}px;
            background: url({CssTexturePathBig}) {-(Size.X * frame)}px {-(Size.Y * (int)ofState)}px;
            ";
        }

        public void Update(int fpsTick) {
            if (fpsTick % Delay == 0) {
                CurrentImage = (CurrentImage + 1) % ImageCount;
            }

            if (State == State.DEAD) {
                CurrentImage = 0;
            } else if (State == State.FALLING) {
                CurrentImage = 1;
            }

            if (State == State.DEAD) {
                Posiotion = new Vector2
                {
                    Y = Size.Y * 2,
                    X = Size.X * CurrentImage
                };
            } else {
                Posiotion = new Vector2
                {
                    Y = Size.Y * (int)State,
                    X = Size.X * CurrentImage
                };
            }

            //DEBUG sprite position
            //System.Console.WriteLine($@"Posiotion [{Posiotion.X}, {Posiotion.Y}]");
        }
    }
}


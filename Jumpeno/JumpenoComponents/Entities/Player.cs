using System;
using System.Numerics;

namespace Jumpeno.JumpenoComponents.Entities {
    /**
     * Reprezentuje telo hráča s ktorým sa pohybuje
     */
    public class Player : MoveableJumpenoComponent {
        public static readonly string[] _UserNames = { "Whistlejacket", "Niatross", "Exterminator", "Sunline", "Buckpasser", "Ajax", "Crisp", "Longfellow", "Nugget", "Inky", "Joker", "Kermit"};
        public bool Spectator { get; set; } = false;
        public int Kills { get; set; }
        public bool Alive { get; set; }
        public bool InGame { get; set; }
        public string Skin { get; set; }
        public bool SmallScreen { get; set; } = false;
        public override string CssStyle(bool smallScreen) => smallScreen ? $@"
            top: {((int)Math.Round(Y/2, 0)).ToString()}px ;
            left: {((int)Math.Round(X/2, 0)).ToString()}px ;
            width: {Animation.Size.X/2}px;
            height: {Animation.Size.Y/2}px;
            background: url({Animation.CssTexturePathSmall}) {-Animation.Posiotion.X/2}px {-Animation.Posiotion.Y/2}px;
            " : $@"
            top: {((int)Math.Round(Y, 0)).ToString()}px ;
            left: {((int)Math.Round(X, 0)).ToString()}px ;
            width: {Animation.Size.X}px;
            height: {Animation.Size.Y}px;
            background: url({Animation.CssTexturePathBig}) {-Animation.Posiotion.X}px {-Animation.Posiotion.Y}px;
            ";

        public void SetBody() {
            Vector2 bodySize;
            Animation = new Animation(Skin + ".png", new Vector2(4, 3), out bodySize);
            Body.Size = bodySize;
            Body.Origin = Body.Size / 2;
        }

        public void Die() {
            Alive = false;
            Animation.State = State.DEAD;
            Velocity.Y = 0;
        }

        internal void Freeze() {
            for (int i = 0; i < Movement.Length; i++) {
                Movement[i] = false;
            }
        }
    }
}

using System;
using System.Numerics;

namespace Jumpeno.JumpenoComponents.Entities {
    /**
     * Reprezentuje platformu, po ktorej skáču hráči
     */
    public class Platform : JumpenoComponent {
        public override string CssStyle(bool smallScreen) => smallScreen ? $@"
            position: absolute;
            top: {((int)Math.Round(Y/2, 0)).ToString()}px ;
            left: {((int)Math.Round(X/2, 0)).ToString()}px ;
            width: {Body.Size.X/2}px ;
            height: {Body.Size.Y/2}px ;
            background: url({Animation.CssTexturePathSmall}) {-Animation.Posiotion.X/2}px {Animation.Posiotion.Y/2}px;
            " : $@"
            position: absolute;
            top: {((int)Math.Round(Y, 0)).ToString()}px ;
            left: {((int)Math.Round(X, 0)).ToString()}px ;
            width: {Body.Size.X}px ;
            height: {Body.Size.Y}px ;
            background: url({Animation.CssTexturePathBig}) {-Animation.Posiotion.X}px {Animation.Posiotion.Y}px;
            ";
        public Platform(string texture, Vector2 position) {
            Vector2 bodySize;
            Animation = new Animation(texture, new Vector2(1, 1), out bodySize);
            Body.Size = bodySize;
            Body.Origin = Body.Size / 2;
            Body.Position = position;
            Solid = true;
        }
    }
}

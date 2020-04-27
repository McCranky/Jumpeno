using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Jumpeno.JumpenoComponents.Entities {
    /**
     * Reprezentuje základnú časť hry a obsahuje všetky potrebné informácie pre vykreslovanie
     */
    public class JumpenoComponent {
        protected static readonly Random rnd = new Random();
        private Guid key = Guid.NewGuid();
        public string GetKey => key.ToString();
        public string Name { get; set; }
        public bool Visible { get; set; } = true;
        public Body Body { get; set; } = new Body(0, 0, 0);
        public float X { set { Body.Position = new Vector2(value, Body.Position.Y); } get { return Body.Position.X; } }
        public float Y { set { Body.Position = new Vector2(Body.Position.X, value); } get { return Body.Position.Y; } }
        public bool Solid { set; get; } = false; // able to walk thru
        public bool FacingRight { get; set; } = true;
        public Game.Game Game { get; set; }
        public Animation Animation { get; set; }
        public string CssClass => this.GetType().Name.ToLower() + (FacingRight ? " flippedHorizontal" : "");
        public virtual string CssStyle(bool smallScreen) => smallScreen ? $@"
            top: {(Y/2).ToString()}px ;
            left: {(X/2).ToString()}px ;
            " : $@"
            top: {Y.ToString()}px ;
            left: {X.ToString()}px ;
            ";

        public Collider GetCollider() { 
            return new Collider(Body, Solid); 
        }

        public virtual async Task Update(int fpsTickNum) {
            //TO DO some stuff if needed
            Animation?.Update(fpsTickNum);
            await Task.CompletedTask;
        }
    }
}

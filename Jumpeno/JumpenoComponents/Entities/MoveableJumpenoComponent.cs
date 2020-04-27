using System;
using System.Numerics;
using System.Threading.Tasks;


namespace Jumpeno.JumpenoComponents.Entities {
    public enum MovementAction {
        LEFT, RIGHT, JUMP
    }
    /**
     * Umožňuje pohyb základnému prvku hry
     */
    public class MoveableJumpenoComponent : JumpenoComponent {
        public MovementAction MovementAction { get; protected set; }
        protected bool[] Movement = { false, false, false, false, false}; // UP, LEFT, DOWN, RIGHT
        protected Vector2 Velocity = new Vector2(0, 0);
        public float JumpHeight { get; protected set; } = 200f;
        public float SpeedBase { get; protected set; } = 100;
        public bool CanJump { get; set; }
        public bool Falling { get { return Velocity.Y > 0; }}
        public bool RightColission { get; set; }
        public bool LeftColission { get; set; }
        public void OnCollision(Vector2 direction) {
            if (direction.X < 0f) { // left
                Velocity.X = 0;
                LeftColission = true;
            }
            else if (direction.X > 0f) { // right
                Velocity.X = 0;
                RightColission = true;
            }
            if (direction.Y > 0f) { // bottom
                Velocity.Y = 0;
                CanJump = true;
            }
            else if (direction.Y < 0f) { // top
                Velocity.Y = 0;
            }
        }
        public void SetMovement(MovementAction action, bool active) {
            Movement[(int)action] = active;
        }
        
        public override async Task Update(int fpsTick) {
            if (Y > Game.Map.Y) {
                Visible = false;
                return;
            }

            //plynule zastavenie
            Velocity.X *= 0.6f;
            if (MathF.Abs(Velocity.X) < 0.2) {
                Velocity.X = 0;
            }

            if (Movement[(int)MovementAction.RIGHT]) { 
                Velocity.X += SpeedBase; 
            }
            if (Movement[(int)MovementAction.LEFT]) { 
                Velocity.X -= SpeedBase; 
            }
            if (Movement[(int)MovementAction.JUMP] && CanJump) {
                CanJump = false;
                Velocity.Y = -MathF.Sqrt(3f * 981f * JumpHeight);
            }

            Velocity.Y += 1581f * (1f / 60f); // gravitacia

            if (Animation.State != State.DEAD) {
                if (Falling && !CanJump) {
                    Animation.State = State.FALLING;
                    FacingRight = (Velocity.X > 0) ? false : true;
                } else {
                    if (Velocity.X == 0) {
                        Animation.State = State.IDLE;
                    } else {
                        Animation.State = State.WALKING;
                        FacingRight = (Velocity.X > 0) ? false : true;
                    }
                }
            }
            

            Body.Position = Body.Position + Velocity * (1 / 60f);

            //DEBUG player position
            //System.Console.WriteLine($"Player at: [{X}, {Y}]");
            await base.Update(fpsTick);
        }
    }
}

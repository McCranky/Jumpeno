using System;
using System.Numerics;

namespace Jumpeno.JumpenoComponents.Entities {
    /**
     * Reprezentuje telo, krtoré dokáže kolidovať a zisťuje či a kde nastala kolízia.
     */
    public class Collider {
        public bool Solid { get; set; }
        //public RectangleShape Body { get; private set; }
        public Body Body { get; private set; }
        public Collider(Body body, bool solid) {
            Body = body;
            Solid = solid;
        }

        public void Move(float deltaX, float deltaY) {
            Body.Position = new Vector2 {
                X = Body.Position.X + deltaX,
                Y = Body.Position.Y + deltaY
            };
        }

        public bool CheckCollision(Collider other, ref Vector2 direction, float pushForce, bool move = true) {
            float deltaX = (other.Body.Position.X + other.Body.Origin.X) - (Body.Position.X + Body.Origin.X);
            float deltaY = (other.Body.Position.Y + other.Body.Origin.Y) - (Body.Position.Y + Body.Origin.Y);
            float intersectX = Math.Abs(deltaX) - (other.Body.Size.X / 2 + Body.Size.X / 2);
            float intersectY = Math.Abs(deltaY) - (other.Body.Size.Y / 2 + Body.Size.Y / 2);

            if (intersectX < 0f && intersectY < 0f) { // it is a collision
                pushForce = MathF.Min(MathF.Max(pushForce, 0), 1);

                if (intersectX > intersectY) {
                    if (deltaX > 0f) { // collision with other on right
                        if (move) {
                            if (other.Solid) {
                                Move(intersectX * (1f - pushForce), 0f);
                                other.Move(-intersectX * pushForce, 0f);
                            }
                        }
                        direction.X = 1f;
                        direction.Y = 0f;
                    } else {
                        if (move) {
                            if (other.Solid) {
                                Move(-intersectX * (1f - pushForce), 0f);
                                other.Move(intersectX * pushForce, 0f);
                            }
                        }
                        direction.X = -1f;
                        direction.Y = 0f;
                    }
                } else {
                    if (deltaY > 0f) { // kolizia s other dole
                        if (move) {
                            if (other.Solid) {
                                Move(0f, intersectY * (1f - pushForce));
                                other.Move(0f, -intersectY * pushForce);
                            }
                        }
                        direction.X = 0f;
                        direction.Y = deltaY; // presnejšiu hodnotu na koliziu
                    } else {
                        if (move) {
                            if (other.Solid) {
                                Move(0f, -intersectY * (1f - pushForce));
                                other.Move(0f, intersectY * pushForce);
                            }
                        }
                        direction.X = 0f;
                        direction.Y = -1f;
                    }
                }
                return true;
            }
            return false;
        }
    }
}

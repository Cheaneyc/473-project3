namespace PrisonStep
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Content;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class Dalek
    {
        public Vector3 location;
        public float rotationAngle;
        public float bodyAngle;
        private bool forward = true;
        public Dalek(Vector3 pos )
        {
            location = pos;
            rotationAngle = 0;
            bodyAngle = 0;
        }

        public void Update(double time, Vector3 VictoriaPos)
        {
            Vector3 displacement = VictoriaPos - location;
            rotationAngle = (float)Math.Atan2(displacement.X, displacement.Z);

            location.Z += 100 * (float)time *(float) Math.Cos(bodyAngle);
            if (forward && location.Z >= -200)
            {
                forward = false;
                location.Z = -200;
                bodyAngle = (float) Math.PI;
            }
            else if (!forward && location.Z <= -500)
            {
                forward = true;
                location.Z = -500;
                bodyAngle = 0;
            }


        }

    }
}

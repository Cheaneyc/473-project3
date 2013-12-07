namespace PrisonStep
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using Microsoft.Xna.Framework.Content;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class PieClass
    {
        public int pieNumber = 0;
        private Vector3 BasePoint;
        public float fireAngle = 0;
        public Vector3 Position;
        private float time = 0;
        private float speed = 500;

        public bool notMoving = false;
        public bool devoured = false;
        public bool reloaded = false;

        private Matrix Transform;

        public PieClass(int number, Vector3 originPoint, float angle)
        {
            pieNumber = number;
            BasePoint = originPoint;
            fireAngle = angle;
            Position = originPoint;
        }

        public void Update(double deltaTotal)
        {
            if (!(notMoving || devoured))
                time += (float)deltaTotal;

            Position = BasePoint + new Vector3(speed * time * ((float)Math.Cos(fireAngle - 1.6) + .2f * (float)Math.Cos(fireAngle)), 0, -speed * time * ((float)Math.Sin(fireAngle - 1.6) + .2f * (float)Math.Sin(fireAngle)));
        }

        public void reloadUpdate(double deltaTotal, float spineOrientation)
        {
            time += (float)deltaTotal;
            if (time >= 1)
                reloaded = true;


        }
    }
}

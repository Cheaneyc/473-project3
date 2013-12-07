using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
namespace PrisonStep
{
    
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ClickableItem
    {
        public Vector3 centerLocation;
        public float hitSphereRadius;

        public ClickableItem(Vector3 location, float radius)
        {
            centerLocation = location;
            
            hitSphereRadius = radius;
        }
        public bool clicked(Vector3 victoriaPos, Vector3 clickVector,float yAdjust)
        {
            Vector3 circleVector = centerLocation - victoriaPos;
            circleVector.Y *= yAdjust;
            clickVector.Y *= yAdjust;
            float dot = Vector3.Dot(circleVector, clickVector);
            dot = dot / (circleVector.Length() * clickVector.Length());
            if (dot < 0)
                return false;
            double angle = Math.Acos(dot);
            double distance = Math.Sin(angle)*circleVector.Length();
            if(distance>hitSphereRadius)
                return false;
            else
                return true;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace PrisonStep
{
    /// <summary>
    /// Handles Door Gamestate and movement
    /// </summary>
    public class DoorClass
    {
        public enum DoorState{open=0,opening=1,closing=2,closed=3};
        public DoorState currentstate;
        private float height;
        private const int raiseSpeed=100;
        public float PercentOpen { get { return height/200; } }
        public DoorClass()
        {
            currentstate = DoorState.closed;
            height = 0;
        }
        /// <summary>
        /// Returns the displacement matrix for each door
        /// </summary>
        /// <param name="gameTime">Amount of time that has passed since last call</param>
        /// <param name="startClosing"> should the door be closing,</param>
        /// <param name="openSignal"> should the door be opening</param>
        /// <returns> displacement matrix for the door</returns>
        public Matrix Update(GameTime gameTime, bool startClosing)
        {
            Vector3 translate = new Vector3(0,0,0);
            switch (currentstate)
            {
                case(DoorState.open):
                    if (startClosing)
                       currentstate= DoorState.closing;
                   break;

                case(DoorState.opening):
                   if (startClosing)
                   {
                       currentstate = DoorState.closing;
                       break;
                   }
                   float change =(float) gameTime.ElapsedGameTime.TotalSeconds * raiseSpeed;
                   height += change;
                   if (height > 200)
                   {
                       height = 200;
                       currentstate = DoorState.open;
                   }
                   break;

                 case(DoorState.closing):
                   if (!startClosing)
                   {
                       currentstate = DoorState.opening;
                       break;
                   }
                    change =(float) gameTime.ElapsedGameTime.TotalSeconds * raiseSpeed;
                    height -= change;
                    if (height < 0)
                    {
                        height = 0;
                        currentstate = DoorState.closed;
                    }
                   break; 

                case(DoorState.closed):
                   if (!startClosing)
                       currentstate = DoorState.opening;
                    break;
                    
            }
            translate.Y = height;
            return Matrix.CreateTranslation(translate);
        }
    }
}

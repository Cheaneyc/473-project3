using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class Particle
    {
        private Vector2 position;
        private Vector2 velocity;
        private Vector2 acceleration;
        private float lifetime;
        private float age;
        private float scale;
        private float orientation;
        private float angularVelocity;

        /// <summary>
        /// Position of the particle in space
        /// </summary>
        public Vector2 Position { get { return position; } set { position = value; } }
        
        /// <summary>
        /// 2D particle velocity
        /// </summary>
        public Vector2 Velocity { get { return velocity; } set { velocity = value; } }
        
        /// <summary> 
        /// 2D particle acceleration 
        /// </summary> 
        public Vector2 Acceleration { get { return acceleration; } set { acceleration = value; } }
        
        /// <summary>
        /// How long this particle will live
        /// </summary> 
        public float Lifetime { get { return lifetime; } set { lifetime = value; } }
        
        /// <summary>
        /// The scale of this particle
        /// </summary> 
        public float Scale { get { return scale; } set { scale = value; } }
        
        /// <summary>
        /// How fast does it rotate?
        /// </summary> 
        public float AngularVelocity { get { return angularVelocity; } set { angularVelocity = value; } }
        
        /// <summary>
        /// How long as this particle been in existence?
        /// </summary> 
        public float Age { get { return age; } set { age = value; } }
        
        /// <summary>
        /// Orientation of the particle in radians
        /// </summary> 
        public float Orientation { get { return orientation; } set { orientation = value; } }

        ///<summary>
        ///Is this particle still alive?
        /// </summary>
        public bool Active { get { return Age < Lifetime; } }

        /// <summary>        
        /// Initialize is called by the particle when to set up a particle and prepare 
        /// it for use. 
        /// </summary>   
        /// <param name="position"></param>
        /// <param name="velocity"></param>    
        /// <param name="acceleration"></param>
        /// <param name="lifetime"></param>   
        /// <param name="scale"></param>      
        /// <param name="rotationSpeed"></param> 
        public void Initialize(Vector2 position, Vector2 velocity, Vector2 acceleration,
            float lieftime, float scale, float rotationSpeed, float orientation)
        {
            this.Position = position;
            this.Velocity = velocity;
            this.Acceleration = acceleration;
            this.Lifetime = lifetime;
            this.Scale = scale;
            this.AngularVelocity = rotationSpeed;
            this.Age = 0.0f;
            this.Orientation = orientation;
        }

        /// <summary>
        /// Update for the particle. Does an Euler step.
        /// </summary>
        /// <param name="delta">Time step</param>
        public void Update(float delta)
        {
            // Update velocity
            Velocity += Acceleration * delta;
            // Update position
            Position += Velocity * delta;
            // Update orientation
            Orientation += AngularVelocity * delta;
            // Update age
            Age += delta;
        }

    }
}
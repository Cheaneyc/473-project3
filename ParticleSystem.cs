using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    public class ParticleSystem
    {
        private SpriteBatch spriteBatch;
        private Texture2D texture;

        // the origin of the textures. 
        private Vector2 origin; 

        //List of live particles
        private LinkedList<Particle> liveParticles = new LinkedList<Particle>();

        //List of available particles
        private LinkedList<Particle> availableParticles = new LinkedList<Particle>();

        //how many active explosions or smoke puffs active at any time
        private int howManyEffect;

        #region Constant

        private int minNumParticles = 20;
        private int maxNumParticles = 25;

        private float minInitialSpeed = 40;
        private float maxInitialSpeed = 500;

        private float minScale = 0.3f;
        private float maxScale = 1.0f;

        private float minRotationSpeed = -(float)Math.PI / 4;
        private float maxRotationSpeed = (float)Math.PI / 4;

        private float minLifetime = 0.5f;
        private float maxLifetime = 0.7f;
        #endregion
        

        // a random number generator that the whole system can share.
        private static Random random = new Random();
        public static Random Random { get { return random; }}

        public ParticleSystem()
        {
        }

        private void Initialize()
        {
           // InitializeConstant();

            for (int i = 0; i < howManyEffect * maxNumParticles; i++)
            {
                availableParticles.AddLast(new Particle());
            }
        }

        public void AddParticles(Vector2 where)
        {
            // the number of particles we want for this effect is a random number
            // somewhere between the two constants specified by the subclasses.
            int numParticles = Random.Next(minNumParticles, maxNumParticles);

            //create this many particles, if you can
            for (int i = 0; i < numParticles && availableParticles.Count > 0; i++)
            {
                //Remove the node from the list od available particles
                LinkedListNode<Particle> node = availableParticles.First;
                availableParticles.Remove(node);

                //Initialize the particle
                Particle p = node.Value;
                InitializeParticle(p, where);

                //Add to the list of live particles
                liveParticles.AddLast(node);
            }
        }

        public virtual void InitializeParticle(Particle p, Vector2 where)
        {
            // Determine the initial particle direction
            Vector2 direction = PickParticleDirection();

            // pick some random values for our particle
            float velocity = RandomBetween(minInitialSpeed, maxInitialSpeed);
            //float acceleration = RandomBetween(minAcceleration, maxAcceleration);
            p.Acceleration = -p.Velocity/p.Lifetime;
            float lifetime = RandomBetween(minLifetime, maxLifetime);
            float scale = RandomBetween(minScale, maxScale);
            float rotationSpeed = RandomBetween(minRotationSpeed, maxRotationSpeed);
            float orientation = RandomBetween(0, (float)Math.PI * 2);

            // then initialize it with those random values. initialize will save those,
            // and make sure it is marked as active.
            p.Initialize(where, velocity * direction, p.Acceleration * direction,
            lifetime, scale, rotationSpeed, orientation);
        }
   
        protected virtual Vector2 PickParticleDirection() 
        {
            float angle = ParticleSystem.RandomBetween(MathHelper.ToRadians(80), MathHelper.ToRadians(100));
            Vector2 direction = Vector2.Zero;

            direction.X = (float)Math.Cos(angle);
            direction.Y = -(float)Math.Sin(angle);      
            return direction;            
        }  

        public static float RandomBetween(float min, float max)
        {
         return min + (float)random.NextDouble() * (max - min);
        }

        public void Update(double deltaTime)
        {
            float delta = (float)deltaTime;

            for (LinkedListNode<Particle> node = liveParticles.First; node != null; )
            {
                LinkedListNode<Particle> nextNode = node.Next;
                node.Value.Update(delta);
                if (!node.Value.Active)
                {
                    liveParticles.Remove(node);
                    availableParticles.AddLast(node);
                }
                node = nextNode;
            }
        }

        public void Draw()
        {
            // tell sprite batch to begin, using the spriteBlendMode specified in
            // initializeConstants
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);

            foreach (Particle p in liveParticles)
            {
                // Life time as a value from 0 to 1
                float normalizedAge = p.Age / p.Lifetime;
                float alpha = 4 * normalizedAge * (1 - normalizedAge);
                Color color = new Color(new Vector4(1, 1, 1, alpha));

                // make particles grow as they age. they'll start at 75% of their size,
                // and increase to 100% once they're finished.
                float scale = p.Scale * (.75f + .25f * normalizedAge );
                spriteBatch.Draw(texture, p.Position, null, color,
                    p.Orientation, origin, scale, SpriteEffects.None, 0.0f);
            }
            spriteBatch.End();
        }


    }
}
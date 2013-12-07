using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace PrisonStep
{
    /// <summary>
    /// This class implements one section of our prison ship
    /// </summary>
    public class PrisonModel
    {
        #region Fields
       

        public static double[] lightData =
            {   1,      568,      246,    1036,   0.53,   0.53,   0.53,     821,     224, 
              941,  14.2941,       45, 43.9412,    814,    224,   1275,    82.5,       0,  0,
                2,       -5,      169,     428, 0.3964,  0.503, 0.4044,    -5.4,     169,
             1020, 129.4902, 107.5686, 41.8039,   -5.4,    169,   -138, 37.8275,      91, 91,
                3,      113,      217,    -933,    0.5,      0,      0,    -129,     185,
            -1085,	     50,        0,       0,    501,    185,  -1087,      48,       0,  0,
                4,      781,      209,    -998,    0.2, 0.1678, 0.1341,    1183,     209,
             -998,	     50,  41.9608, 33.5294,    984,    113,   -932,       0,      80,  0,
                5,      782,      177,    -463,   0.65, 0.5455, 0.4359,     563,     195,
             -197,	     50,        0,       0,   1018,    181,   -188,      80,       0,  0,
                6,     1182,      177,   -1577,   0.65, 0.5455, 0.4359,     971,     181,
            -1801,        0,  13.1765,      80,   1406,    181,  -1801,       0, 13.1765,  80};


        /// <summary>
        /// The section (6) of the ship
        /// </summary>
        private int section;

        /// <summary>
        /// The name of the asset (FBX file) for this section
        /// </summary>
        private string asset;

        /// <summary>
        /// The game we are associated with
        /// </summary>
        private PrisonGame game;

        /// <summary>
        /// The XNA model for this part of the ship
        /// </summary>
        private Model model;


        /// <summary>
        /// To make animation possible and easy, we save off the initial (bind) 
        /// transformation for all of the model bones. 
        /// </summary>
        private Matrix[] bindTransforms;

        /// <summary>
        /// The is the transformations for all model bones, potentially after we
        /// have made some change in the tranformation.
        /// </summary>
        private Matrix[] boneTransforms;

        /// <summary>
        /// A list of all of the door bones in the model.
        /// </summary>
        public List<int> doors = new List<int>();

        public List<DoorClass> doorState = new List<DoorClass>();

        /// <summary>
        /// The effect we will use to draw the model (one for now)
        /// </summary>
        private Effect effect;

        private Effect effectT;

        private float sliming = 0;
        #endregion

        #region Construction and Loading

        /// <summary>
        /// Constructor. Creates an object for a section.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="section"></param>
        public PrisonModel(PrisonGame game, int section)
        {
            this.game = game;
            this.section = section;
            this.asset = "AntonPhibes" + section.ToString();
        }

        /// <summary>
        /// This function is called to load content into this component
        /// of our game.
        /// </summary>
        /// <param name="content">The content manager to load from.</param>
        public void LoadContent(ContentManager content)
        {
            // Load the second model
            model = content.Load<Model>(asset);
          
            // Save off all of hte bone information
            int boneCnt = model.Bones.Count;
            bindTransforms = new Matrix[boneCnt];
            boneTransforms = new Matrix[boneCnt];

            model.CopyBoneTransformsTo(bindTransforms);
            model.CopyBoneTransformsTo(boneTransforms);

            // Find all of the doors and save the index for the bone
            for (int b = 0; b < boneCnt; b++)
            {
                if (model.Bones[b].Name.StartsWith("DoorInner") || model.Bones[b].Name.StartsWith("DoorOuter"))
                {
                    doors.Add(b);
                    doorState.Add(new DoorClass());
                }

            }

            effect = content.Load<Effect>("PhibesEffect1");
            effectT = content.Load<Effect>("PhibesEffect2");
            
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// This function is called to update this component of our game
        /// to the current game time.
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime,bool startClosing, int doorNumber)
        {
            int i=0;
            foreach (int num in doors)
            {
                bool startclosing = true;
                if (i==doorNumber &&!startClosing)
                {
                    startclosing = startClosing;
                }
                if (Player.isBeingSlimed&&sliming<1.1f)
                {
                    sliming += (float)gameTime.ElapsedGameTime.TotalSeconds*.5f;
                    if (sliming > 1.1f)
                        sliming = 1.1f;
                }
                else if (sliming > 0)
                {
                    sliming -= (float)gameTime.ElapsedGameTime.TotalSeconds*.5f;
                    if (sliming < 0)
                        sliming = 0;
                }
                boneTransforms[num] = doorState[i].Update(gameTime, startclosing) * bindTransforms[num];
                i++;
            }
        }

        /// <summary>
        /// This function is called to draw this game component.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {

            DrawModel(graphics, model, Matrix.Identity);
        }

        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world)
        {
            // Apply the bone transforms
            Matrix[] absoTransforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsFrom(boneTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);
            
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(absoTransforms[mesh.ParentBone.Index] * world);
                    effect.Parameters["View"].SetValue(game.Camera.View);
                    effect.Parameters["Projection"].SetValue(game.Camera.Projection);
                    effect.Parameters["yValue"].SetValue(1-2*sliming);
                }
                mesh.Draw();
            }
        }

        #endregion

        #region Effect Functions
        /// <summary>
        /// Replace the model effect with a new effect we load ourselves
        /// </summary>
        private void SetEffect()
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    BasicEffect bEffect = part.Effect as BasicEffect;
                    if (bEffect.Texture != null)
                    {
                        // We are using the texture effect

                        part.Effect = effectT.Clone();

                        part.Effect.Parameters["Texture"].SetValue(bEffect.Texture);
                        
                    }
                    else
                    {
                        // We are using the diffuse color effect

                        part.Effect = effect.Clone();

                        part.Effect.Parameters["DiffuseColor"].SetValue(bEffect.DiffuseColor);
                    }


                    part.Effect.Parameters["Light1Location"].SetValue(LightInfo(section, 0));
                    part.Effect.Parameters["Light1Color"].SetValue(LightInfo(section, 1));

                }
            }
        }

        /// <summary>
        /// Get light information for a section. This pulls data from the lightData
        /// array.
        /// </summary>
        /// <param name="section">Section number 1-6</param>
        /// <param name="item">Item 0 for light 1 location, 1 for light 1 color, etc.</param>
        /// <returns></returns>
        public static Vector3 LightInfo(int section, int item)
        {
            int offset = (section - 1) * 19 + 1 + (item * 3);
            return new Vector3((float)lightData[offset], (float)lightData[offset + 1], (float)lightData[offset + 2]);
        }
        #endregion
    }
}

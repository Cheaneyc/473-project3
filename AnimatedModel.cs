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
using XnaAux;

namespace PrisonStep
{
    class AnimatedModel
    {
        #region Fields
        /// <summary>
        /// Reference to the game that uses this class
        /// </summary>
        private PrisonGame game;

        /// <summary>
        /// Name of the asset we are going to load
        /// </summary>
        private string asset;

        private Matrix[] skinTransforms = null;
        private Vector3 light = new Vector3(0, 0, 0);
        private Dictionary<string, AssetClip> assetClips = new Dictionary<string, AssetClip>();

        private Model model;
        public Model Model { get { return model; } }
        private float angle = 0;

        private float sliming = 0;

        public float spineChange = 0;
        private List<int> skelToBone = null;
        private Matrix[] inverseBindTransforms = null;
        /// <summary>
        /// The bond transforms as loaded from the model
        /// </summary>
        private Matrix[] bindTransforms;

        // <summary>
        /// Access the current animation player
        /// </summary>
        public AnimationPlayer AnimPlayer { get { return player; } }

        /// <summary>
        /// The number of skinning matrices in SkinnedEffect.fx. This must
        /// match the number in SkinnedEffect.fx.
        /// </summary>
        public const int NumSkinBones = 57;

        /// <summary>
        /// The current bone transforms we will use
        /// </summary>
        private Matrix[] boneTransforms;

        /// <summary>
        /// The computed absolute transforms
        /// </summary>
        private Matrix[] absoTransforms;
        
        private AnimationPlayer player = null;
        private AnimationClips.Clip clip = null;
        private Player playInfo;
        private Matrix rootMatrixRaw = Matrix.Identity;
        private Matrix deltaMatrix = Matrix.Identity;
        
        public Matrix DeltaMatrix { get { return deltaMatrix; } }
        public Vector3 DeltaPosition;
        public Matrix RootMatrix { get { return inverseBindTransforms[skelToBone[0]] * rootMatrixRaw; } }
     
        #endregion

        #region basic

        public AnimatedModel(PrisonGame game, string asset,Player play)
        {
            playInfo = play;
            this.game = game;
            this.asset = asset;
            skinTransforms = new Matrix[57];
            for (int i = 0; i < skinTransforms.Length; i++)
            {
                skinTransforms[i] = Matrix.Identity;
            }
        }

        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>(asset);

            // Allocate the array to the number of bones we have
            int boneCnt = model.Bones.Count;
            bindTransforms = new Matrix[boneCnt];
            boneTransforms = new Matrix[boneCnt];
            absoTransforms = new Matrix[boneCnt];

            // Copy the bone transforms from the model to our local arrays
            model.CopyBoneTransformsTo(bindTransforms);
            model.CopyBoneTransformsTo(boneTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);

            AnimationClips clips = model.Tag as AnimationClips;
            if (/*clips != null &&*/ clips.SkelToBone.Count > 0)
            {
                skelToBone = clips.SkelToBone;

                inverseBindTransforms = new Matrix[boneCnt];
                skinTransforms = new Matrix[NumSkinBones];

                model.CopyAbsoluteBoneTransformsTo(inverseBindTransforms);

                for (int b = 0; b < inverseBindTransforms.Length; b++)
                    inverseBindTransforms[b] = Matrix.Invert(inverseBindTransforms[b]);

                for (int i = 0; i < skinTransforms.Length; i++)
                    skinTransforms[i] = Matrix.Identity;
            }
            foreach (AssetClip clip in assetClips.Values)
            {
                Model clipmodel = content.Load<Model>(clip.Asset);
                AnimationClips modelclips = clipmodel.Tag as AnimationClips;
                clip.TheClip = modelclips.Clips["Take 001"];
            }
            
        }

        public void Update(double delta)
        {

            if (Player.isBeingSlimed && sliming < 1.1f)
            {
                sliming += (float)delta * .5f;
                if (sliming > 1.1f)
                    sliming = 1.1f;
            }
            else if (sliming > 0)
            {
                sliming -= (float)delta * .5f;
                if (sliming < 0)
                    sliming = 0;
            }

           
            if (player != null)
            {
                // Update the clip
                player.Update(delta);

                for (int b = 0; b < player.BoneCount; b++)
                {
                    AnimationPlayer.Bone bone = player.GetBone(b);
                    if (!bone.Valid)
                        continue;

                    Vector3 scale = new Vector3(bindTransforms[b].Right.Length(),
                        bindTransforms[b].Up.Length(),
                        bindTransforms[b].Backward.Length());

                    boneTransforms[b] = Matrix.CreateScale(scale) *
                        Matrix.CreateFromQuaternion(bone.Rotation) *
                        Matrix.CreateTranslation(bone.Translation);
                    if (model.Bones["Bip01 Spine1"].Index == b)
                    {
                        boneTransforms[b] = Matrix.CreateScale(scale) *
                       Matrix.CreateFromQuaternion(bone.Rotation) *
                       Matrix.CreateRotationX(spineChange)*
                       Matrix.CreateTranslation(bone.Translation);
                    }
                }

                if (skelToBone != null)
                {
                    int rootBone = skelToBone[0];
                    deltaMatrix = Matrix.Invert(rootMatrixRaw) * boneTransforms[rootBone];
                    DeltaPosition = boneTransforms[rootBone].Translation - rootMatrixRaw.Translation;

                    rootMatrixRaw = boneTransforms[rootBone];
                    boneTransforms[rootBone] = bindTransforms[rootBone];
                    
                }

                model.CopyBoneTransformsFrom(boneTransforms);
            }

           
            model.CopyBoneTransformsFrom(boneTransforms);
            model.CopyAbsoluteBoneTransformsTo(absoTransforms);
        }

        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime, Matrix transform)
        {
            DrawModel(graphics, model, transform);
        }

        private void DrawModel(GraphicsDeviceManager graphics, Model model, Matrix world)
        {
            if (skelToBone != null)
            {
                for (int b = 0; b < skelToBone.Count; b++)
                {
                    int n = skelToBone[b];
                    skinTransforms[b] = inverseBindTransforms[n] * absoTransforms[n];
                }
            }


            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(absoTransforms[mesh.ParentBone.Index] * world);
                    effect.Parameters["yValue"].SetValue(1 - 2 * sliming);
                    effect.Parameters["Light4Location"].SetValue(light);
                    if (skelToBone != null)
                    {
                        effect.Parameters["Bones"].SetValue(skinTransforms);
                    }
                    effect.Parameters["View"].SetValue(game.Camera.View);
                    effect.Parameters["Projection"].SetValue(game.Camera.Projection);
                    effect.Parameters["Light1Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 0));
                    effect.Parameters["Light1Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 1));
                    effect.Parameters["Light2Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 2));
                    effect.Parameters["Light2Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 3));
                    effect.Parameters["Light3Location"].SetValue(PrisonModel.LightInfo(playInfo.section,4));
                    effect.Parameters["Light3Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 5));
                    effect.Parameters["Light4Location"].SetValue(PrisonModel.LightInfo(playInfo.previousSection, 0));
                    effect.Parameters["Light4Color"].SetValue(PrisonModel.LightInfo(playInfo.previousSection, 1));
                    effect.Parameters["doorOpenPercent"].SetValue(playInfo.doorOpenPercentage);
                }
                mesh.Draw();
            }

        }
        #endregion

        /// <summary>
        /// This class describes a single animation clip we load from
        /// an asset.
        /// </summary>
        private class AssetClip
        {
            public AssetClip(string name, string asset)
            {
                Name = name;
                Asset = asset;
                TheClip = null;
            }

            public string Name { get; set; }
            public string Asset { get; set; }
            public AnimationClips.Clip TheClip { get; set; }
        }

        /// <summary>
        /// Play an animation clip on this model.
        /// </summary>
        /// <param name="name"></param>
        public AnimationPlayer PlayClip(string name)
        {
            player = null;
            AnimationClips clips = model.Tag as AnimationClips;
            /*
            if (clips != null && clips.Clips.Count > 0)
            {

                clip = clips.Clips[name];

                player = new AnimationPlayer(clip);
             
                player.Looping = false;
                player.Initialize();
                Update(0);
            }
             * */
            if (assetClips.Count > 0)
            {
                clip = assetClips[name].TheClip;
                player = new AnimationPlayer(clip);
                player.Looping = false;
                player.Initialize();
                Update(0);
            }


            return player;
        }
       
        /// <summary>
        /// Add an asset clip to the dictionary.
        /// </summary>
        /// <param name="name">Name we will use for the clip</param>
        /// <param name="asset">The FBX asset to load</param>
        public void AddAssetClip(string name, string asset)
        {
            assetClips[name] = new AssetClip(name, asset);
        }

        public Matrix AbsoulutePosition(int index)
        {
           
           Matrix basematrix= Model.Bones[index].Transform;
            ModelBone parent = Model.Bones[index].Parent;
            while (parent != null)
            {
                basematrix *= parent.Transform;
                parent = parent.Parent;
            }

            return basematrix;
        }

        public void PieDraw(GraphicsDeviceManager graphics, GameTime gameTime, Matrix transform, int pieNumber)
        {
            if (skelToBone != null)
            {
                for (int b = 0; b < skelToBone.Count; b++)
                {
                    int n = skelToBone[b];
                    skinTransforms[b] = inverseBindTransforms[n] * absoTransforms[n];
                }
            }
            int meshes = 1;
            if (pieNumber > 0)
                meshes = 2;
            if (pieNumber == 2)
                pieNumber++;
            //0 draws the closest one
            //1 draws a tin 2 draws the pie
            //3 draws a tin and 4 draws the pie
            for (int i = 0; i < meshes; i++)
            {
                ModelMesh mesh = model.Meshes[pieNumber + i];

                foreach (Effect effect in mesh.Effects)
                {
                    effect.Parameters["World"].SetValue(absoTransforms[mesh.ParentBone.Index] * transform);
                    effect.Parameters["yValue"].SetValue(1 - 2 * sliming);
                    effect.Parameters["Light4Location"].SetValue(light);
                    if (skelToBone != null)
                    {
                        effect.Parameters["Bones"].SetValue(skinTransforms);
                    }
                    effect.Parameters["View"].SetValue(game.Camera.View);
                    effect.Parameters["Projection"].SetValue(game.Camera.Projection);
                    effect.Parameters["Light1Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 0));
                    effect.Parameters["Light1Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 1));
                    effect.Parameters["Light2Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 2));
                    effect.Parameters["Light2Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 3));
                    effect.Parameters["Light3Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 4));
                    effect.Parameters["Light3Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 5));
                    effect.Parameters["Light4Location"].SetValue(PrisonModel.LightInfo(playInfo.previousSection, 0));
                    effect.Parameters["Light4Color"].SetValue(PrisonModel.LightInfo(playInfo.previousSection, 1));
                    effect.Parameters["doorOpenPercent"].SetValue(playInfo.doorOpenPercentage);
                }

                mesh.Draw();
            }
        }

        public void DalekDraw(GraphicsDeviceManager graphics, GameTime gameTime, Matrix transform, Dalek dalek)
        {
            
            
            for (int i = 0; i < model.Meshes.Count; i++)
            {
                ModelMesh mesh = model.Meshes[i];
                {
                    if (i != 0 && i != 1)
                    {
                        foreach (Effect effect in mesh.Effects)
                        {
                            effect.Parameters["World"].SetValue(absoTransforms[mesh.ParentBone.Index]* transform);
                            effect.Parameters["yValue"].SetValue(1 - 2 * sliming);
                            effect.Parameters["Light1Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 0));
                            effect.Parameters["Light1Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 1));
                            effect.Parameters["Light2Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 2));
                            effect.Parameters["Light2Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 3));
                            effect.Parameters["Light3Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 4));
                            effect.Parameters["Light3Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 5));
                            effect.Parameters["Light4Location"].SetValue(PrisonModel.LightInfo(playInfo.previousSection, 0));
                            effect.Parameters["Light4Color"].SetValue(PrisonModel.LightInfo(playInfo.previousSection, 1));
               
                            if (skelToBone != null)
                            {
                                effect.Parameters["Bones"].SetValue(skinTransforms);
                            }
                            effect.Parameters["View"].SetValue(game.Camera.View);
                            effect.Parameters["Projection"].SetValue(game.Camera.Projection);
                            effect.Parameters["doorOpenPercent"].SetValue(playInfo.doorOpenPercentage);

                        }
                        mesh.Draw();
                    }
                    else
                    {
                        foreach (Effect effect in mesh.Effects)
                        {
                            Vector3 fixTrans;
                            if(dalek.bodyAngle==0)
                                fixTrans= new Vector3(0, 0, -10);
                            else
                                fixTrans = new Vector3(0, 0, 10);
                        
                            effect.Parameters["World"].SetValue(absoTransforms[mesh.ParentBone.Index]*Matrix.CreateTranslation(0, 0, 10)*Matrix.CreateRotationY(dalek.rotationAngle-dalek.bodyAngle)*transform*Matrix.CreateTranslation(fixTrans));
                            effect.Parameters["yValue"].SetValue(1 - 2 * sliming);
                            effect.Parameters["Light1Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 0));
                            effect.Parameters["Light1Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 1));
                            effect.Parameters["Light2Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 2));
                            effect.Parameters["Light2Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 3));
                            effect.Parameters["Light3Location"].SetValue(PrisonModel.LightInfo(playInfo.section, 4));
                            effect.Parameters["Light3Color"].SetValue(PrisonModel.LightInfo(playInfo.section, 5));
                            effect.Parameters["Light4Location"].SetValue(PrisonModel.LightInfo(playInfo.previousSection, 0));
                            effect.Parameters["Light4Color"].SetValue(PrisonModel.LightInfo(playInfo.previousSection, 1));
                            effect.Parameters["doorOpenPercent"].SetValue(playInfo.doorOpenPercentage);
                            if (skelToBone != null)
                            {
                                effect.Parameters["Bones"].SetValue(skinTransforms);
                            }
                            effect.Parameters["View"].SetValue(game.Camera.View);
                            effect.Parameters["Projection"].SetValue(game.Camera.Projection);
                        }
                        mesh.Draw();
                    }
                }
            }
        }
    }

 
}

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

namespace XnaAux
{
    public class AnimationPlayer
    {
        private bool looping = false;
        private double speed = 1.0f;

        /// <summary>
        /// Indicates if the playback should "loop" or not.
        /// </summary>
        public bool Looping { get { return looping; } set { looping = value; } }

        /// <summary>
        /// Playback speed
        /// </summary>
        public double Speed { get { return speed; } set { speed = value; } }
        private double time = 0;
        private BoneInfo[] boneInfos;
        private AnimationClips.Clip clip;
        public AnimationPlayer(AnimationClips.Clip clips)
        {
            clip = clips;
        }

        public void Initialize()
        {
            
            clip.BoneCount = clip.Keyframes.Length;
            boneInfos = new BoneInfo[clip.BoneCount];

            time = 0;
            for (int b = 0; b < clip.BoneCount; b++)
            {
                boneInfos[b].CurrentKeyframe = -1;
                boneInfos[b].Valid = false;
            }
        }

        private struct BoneInfo : AnimationClips.Bone
        {
            private int currentKeyframe;     // Current keyframe for bone
            private bool valid;

            private Quaternion rotation;
            private Vector3 translation;

            public int CurrentKeyframe { get { return currentKeyframe; } set { currentKeyframe = value; } }
            public bool Valid { get { return valid; } set { valid = value; } }
            public Quaternion Rotation { get { return rotation; } set { rotation = value; } }
            public Vector3 Translation { get { return translation; } set { translation = value; } }
        }

        /// <summary>
        /// Update the clip position
        /// </summary>
        /// <param name="delta">The amount of time that has passed.</param>
        public void Update(double delta)
        {
            time += delta * Speed;
            //handles looping, resets keyframes and time when animation exceeds duration
            if (Looping && time > clip.Duration)
            {
                time = time - clip.Duration;
                for (int b = 0; b < boneInfos.Length; b++)
                    boneInfos[b].CurrentKeyframe = 0;
            }

            for (int b = 0; b < boneInfos.Length; b++)
            {
                List<AnimationClips.Keyframe> keyframes = clip.Keyframes[b];
                if (keyframes.Count == 0)
                    continue;

                // The time needs to be greater than or equal to the
                // current keyframe time and less than the next keyframe 
                // time.
                while (boneInfos[b].CurrentKeyframe < 0 ||
                    (boneInfos[b].CurrentKeyframe < keyframes.Count - 1 &&
                    keyframes[boneInfos[b].CurrentKeyframe + 1].Time <= time))
                {
                    // Advance to the next keyframe
                    boneInfos[b].CurrentKeyframe++;
                }

                //
                // Update the bone
                //

                int c = boneInfos[b].CurrentKeyframe;

                if (c >= 0)
                {
                    if (boneInfos[b].CurrentKeyframe < keyframes.Count - 1)
                    {
                        AnimationClips.Keyframe keyframe1 = keyframes[boneInfos[b].CurrentKeyframe];
                        AnimationClips.Keyframe keyframe2 = keyframes[boneInfos[b].CurrentKeyframe + 1];
                        float t = (float)((time - keyframe1.Time) / (keyframe2.Time - keyframe1.Time));
                        boneInfos[b].Rotation = Quaternion.Slerp(keyframe1.Rotation, keyframe2.Rotation, t);
                        Vector3 translation = Vector3.Lerp(keyframe1.Translation, keyframe2.Translation, t);
                        boneInfos[b].Translation = translation;
                    }
                    else
                    {
                        AnimationClips.Keyframe keyframe = keyframes[c];
                        boneInfos[b].Rotation = keyframe.Rotation;
                        boneInfos[b].Translation = keyframe.Translation;
                    }

                    boneInfos[b].Valid = true;
                }
            }
        }

        public AnimationClips.Bone GetBone(int b) { return boneInfos[b]; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using XnaAux;
namespace PrisonStep
{
    /// <summary>
    /// This class describes our player in the game. 
    /// </summary>
    public class Player
    {
        #region Fields
        private SpriteBatch sprite;
         private SpriteFont ScoreFont;
        private AnimatedModel DalekModel;
        private Dalek dalekInfo;
        /// <summary>
        /// List of regions for collision testing. The list of Vector2 objects
        /// is the list of triangles. Each triangle will have 3 vertices. 
        /// </summary>
        private Dictionary<string, List<Vector2>> regions = new Dictionary<string, List<Vector2>>();

        private ClickableItem pieReload;
        private int height;
        private int width;
        /// <summary>
        /// Game that uses this player
        /// </summary>
        private PrisonGame game;


        public int section = 1;
        public int previousSection = 2;
        public float doorOpenPercentage=0;

        private PieClass activePie = null;
        private PieClass reloadingPie = null;
        private List<PieClass> stuckPies;
        private int pieNumber = 0;
        private int pieHits = 0;
        private float pieCooldown = 0;
        /// <summary>
        /// Our animated model
        /// </summary>
        private AnimatedModel victoria;

       
        /// <summary>
        /// bazooka
        /// </summary>
        private  AnimatedModel bazooka;

        private AnimatedModel Pies;

        private AnimatedModel Alien;
        private Vector3 alienPosition = new Vector3(-11, 0, -600);
        private float alienOrientation = 0;
        private States alienState = States.Start;
        private Matrix alienMatrix = Matrix.Identity;
        private bool aboutFace=false;
        private float spitTime = 0;
        private float spitCooldown = 50;

        private AnimatedModel spit;
        private Vector3 spitLocation = new Vector3(0, 0, 0);
        private bool drawSpit = false;
        private bool foraward = false;
        

        private int pieCount=10;

        /// <summary>
        /// Player location in the prison. Only x/z are important. y still stay zero
        /// unless we add some flying or jumping behavior later on.
        /// </summary>
        private Vector3 location = new Vector3(375, 0, 1053);
        private float orientation = 1.6f;

        private float spineOrientation = 0;
        public float SpineOrientation { get { return spineOrientation; } }
        private enum States { Start, StanceStart, Stance, WalkStart, WalkLoopStart, WalkLoop, Right, Left,RightStart,LeftStart,aiming, startaiming, aimed, lowering, lowerStart,crouching,crouchStart,pieCatchstart, pieCatch, pieEat,spitting }
        private States state = States.Start;
        /// <summary>
        /// The player transformation matrix. Places the player where they need to be.
        /// </summary>
        private Matrix transform;

        /// <summary>
        /// The rotation rate in radians per second when player is rotating
        /// </summary>
        private float panRate = 2;

        /// <summary>
        /// The player move rate in centimeters per second
        /// </summary>
        private float moveRate = 500;

        public static bool isBeingSlimed=false;
        public bool Slimed { get { return isBeingSlimed; } }
        #endregion

        #region basicFunctions
        public Player(PrisonGame game)
        {

            
            this.game = game;
            SetPlayerTransform();
            #region models
            
            dalekInfo = new Dalek(new Vector3(700, 0, -500));
            stuckPies = new List<PieClass>();
            victoria = new AnimatedModel(game, "Victoria",this);
            bazooka = new AnimatedModel(game, "PieBazooka",this);
            DalekModel = new AnimatedModel(game, "Dalek",this);
            Pies = new AnimatedModel(game, "pies",this);
            Alien = new AnimatedModel(game, "Alien",this);
            spit = new AnimatedModel(game, "Spit",this);
            #endregion
            #region victoriaClips
            victoria.AddAssetClip("dance", "Victoria-dance");
            victoria.AddAssetClip("stance", "Victoria-stance");
            victoria.AddAssetClip("walk", "Victoria-walk");
            victoria.AddAssetClip("walkstart", "Victoria-walkstart");
            victoria.AddAssetClip("walkloop", "Victoria-walkloop");
            victoria.AddAssetClip("leftturn", "Victoria-leftturn");
            victoria.AddAssetClip("rightturn", "Victoria-rightturn");
            //bazooka
            victoria.AddAssetClip("crouch", "Victoria-crouchbazooka");
            victoria.AddAssetClip("lowerB", "Victoria-lowerbazooka");
            victoria.AddAssetClip("raiseB", "Victoria-raisebazooka");
            victoria.AddAssetClip("walkstartB", "Victoria-walkstartbazooka");
            victoria.AddAssetClip("walkloopB", "Victoria-walkloopbazooka");
            #endregion
            #region Alien
            
            Alien.AddAssetClip("eat", "Alien-catcheat");
            Alien.AddAssetClip("ob", "Alien-ob");
            Alien.AddAssetClip("walkStart", "Alien-walkstart");
            Alien.AddAssetClip("walkloop", "Alien-walkloop");
            Alien.AddAssetClip("stance", "Alien-stance");
            Alien.AddAssetClip("tantrum", "Alien-trantrum");
            #endregion
        }

        /// <summary>
        /// Set the value of transform to match the current location
        /// and orientation.
        /// </summary>
        private void SetPlayerTransform()
        {
            transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;
        }

        public void LoadContent(ContentManager content)
        {
            height = game.GraphicsDevice.Viewport.Height;
            width = game.GraphicsDevice.Viewport.Width;
            pieReload = new ClickableItem(new Vector3(500, 150, 1100),15);
            ScoreFont = content.Load<SpriteFont>("scorefont");
            sprite = new SpriteBatch(game.GraphicsDevice);
            DalekModel.LoadContent(content);
            victoria.LoadContent(content);
            bazooka.LoadContent(content);
            Pies.LoadContent(content);
            Alien.LoadContent(content);
            spit.LoadContent(content);
            Model model = content.Load<Model>("AntonPhibesCollision");
            Matrix[] M = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(M);
            
            foreach (ModelMesh mesh in model.Meshes)
            {
                // For accumulating the triangles for this mesh
                List<Vector2> triangles = new List<Vector2>();

                // Loop over the mesh parts
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // 
                    // Obtain the vertices for the mesh part
                    //

                    int numVertices = meshPart.VertexBuffer.VertexCount;
                    VertexPositionColorTexture[] verticesRaw = new VertexPositionColorTexture[numVertices];
                    meshPart.VertexBuffer.GetData<VertexPositionColorTexture>(verticesRaw);

                    //
                    // Obtain the indices for the mesh part
                    //

                    int numIndices = meshPart.IndexBuffer.IndexCount;
                    short[] indices = new short[numIndices];
                    meshPart.IndexBuffer.GetData<short>(indices);

                    //
                    // Build the list of triangles
                    //

                    for (int i = 0; i < meshPart.PrimitiveCount * 3; i++)
                    {
                        // The actual index is relative to a supplied start position
                        int index = i + meshPart.StartIndex;

                        // Transform the vertex into world coordinates
                        Vector3 v = Vector3.Transform(verticesRaw[indices[index] + meshPart.VertexOffset].Position, M[mesh.ParentBone.Index]);
                        triangles.Add(new Vector2(v.X, v.Z));
                    }

                   
                }

                regions[mesh.Name] = triangles;
            }
         
            location.Y = 0;
            SetPlayerTransform();
        }


        public void Update(GameTime gameTime)
        {
            int mouseX = Mouse.GetState().X;
                int mouseY= Mouse.GetState().Y;
            if (section == 1 && Mouse.GetState().LeftButton==ButtonState.Pressed && mouseX<=width && mouseX>=0 && mouseY<=height &&mouseY>=0)
            {
                Vector3 nearsource = new Vector3((float)mouseX, (float)mouseY, 0f);
                Vector3 farsource = new Vector3((float)mouseX, (float)mouseY, 1f);

                Matrix world = Matrix.CreateTranslation(0, 0, 0);

                Vector3 nearPoint = game.GraphicsDevice.Viewport.Unproject(nearsource,
                    game.Camera.Projection, game.Camera.View, world);

                Vector3 farPoint = game.GraphicsDevice.Viewport.Unproject(farsource,
                    game.Camera.Projection, game.Camera.View, world);
                Vector3 direction = farPoint - nearPoint;
                direction.Normalize();
                bool test = pieReload.clicked(game.Camera.Eye, direction,4f);
                if (test)
                    pieCount = 10;
            }
            

            double deltaTotal = gameTime.ElapsedGameTime.TotalSeconds;
            double deltaTotal2=deltaTotal;
            spitCooldown +=(float) gameTime.ElapsedGameTime.TotalSeconds;

            #region spit
            if(drawSpit)
            {
                spitTime+=(float)gameTime.ElapsedGameTime.TotalSeconds;
                if(spitTime>10)
                {
                    drawSpit=false;
                    spitTime=0;
                }
                if(foraward)
                    spitLocation.Z += 150*(float)gameTime.ElapsedGameTime.TotalSeconds;
                else
                    spitLocation.Z -= 150 * (float)gameTime.ElapsedGameTime.TotalSeconds;
              }
            #endregion

            #region pieUpdate
            pieCooldown -= (float)deltaTotal;
            //updating pie location
            if (activePie != null)
                activePie.Update(deltaTotal);
            ////
            if (reloadingPie != null)
            {
                reloadingPie.reloadUpdate(deltaTotal, spineOrientation);
                if (reloadingPie.reloaded)
                    reloadingPie = null;
            }
            
            ////
            foreach (PieClass pie in stuckPies)
            {
                pie.Update(deltaTotal);
            }
            #endregion
            
            #region alien Update
            do
            {
                double delta = deltaTotal2;
                dalekInfo.Update(delta, transform.Translation);
                KeyboardState keyboardState = Keyboard.GetState();
                GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
                
                
                AlienStateMachine(ref delta);
                Alien.Update(delta);

               //alien collision
                if (activePie != null)
                {
                    if (Vector3.Distance(alienPosition, activePie.Position) < 150 && alienState==States.pieCatch)
                    {
                        alienState = States.pieEat;
                        activePie.devoured = true;
                        
                    }
                    if(Vector3.Distance(alienPosition,activePie.Position)<500 && alienState!=States.pieCatch && alienState!=States.pieCatchstart && alienState!=States.pieEat)
                    {
                        alienState = States.pieCatchstart;
                        alienOrientation = (float)(Math.PI)-activePie.fireAngle;
                    }
                    else if (alienState == States.pieCatch && Vector3.Distance(alienPosition, activePie.Position) > 600)
                    {
                        if (aboutFace)
                            alienOrientation = (float)Math.PI;
                        else
                            alienOrientation = 0;
                        alienState = States.WalkLoopStart;
                    }
                }
                else if (alienState == States.pieCatch && activePie==null)
                {
                    alienState = States.WalkLoopStart;
                    if (aboutFace)
                        alienOrientation = (float)Math.PI;
                    else
                        alienOrientation = 0;
                }

                if (!drawSpit && Vector3.Distance(location, alienPosition) < 500 && spitCooldown>60)
                {
                    drawSpit = true;
                    spitCooldown = 0;
                    if (location.Z > alienPosition.Z)
                    {
                        alienOrientation = 0;
                        foraward = true;
                    }
                    else
                    {
                        alienOrientation = (float)Math.PI;
                        foraward = false;
                    }
                    spitLocation = alienPosition;
                    spitLocation.Y = 180;
                    alienState = States.spitting;
                }
                 //
                // Part 1:  Compute a new orientation
                //
                
                    Matrix deltaMatrix = Alien.DeltaMatrix;
                    float deltaAngle = (float)Math.Atan2(deltaMatrix.Backward.X, deltaMatrix.Backward.Z);
                    float newOrientation = alienOrientation + deltaAngle;

                    //
                    // Part 2:  Compute a new location
                    //

                    // We are likely rotated from the angle the model expects to be in
                    // Determine that angle.
                    Matrix rootMatrix = Alien.RootMatrix;
                    float actualAngle = (float)Math.Atan2(rootMatrix.Backward.X, rootMatrix.Backward.Z);
                    Vector3 newLocation;

                    newLocation = alienPosition + Vector3.TransformNormal(Alien.DeltaPosition,
                                   Matrix.CreateRotationY(newOrientation - actualAngle));

                
                if (alienState == States.spitting)
                {
                    newLocation = alienPosition;
                }
                //
                // I'm just taking these here.  You'll likely want to add something 
                // for collision detection instead.
                //
                if (!aboutFace && alienPosition.Z > 800)
                {
                    alienOrientation = (float)Math.PI;
                    aboutFace = true;
                }
                else if (aboutFace && alienPosition.Z < -600)
                {
                    alienOrientation = 0;
                    aboutFace = false;
                }

                alienPosition = newLocation;
                alienMatrix = Matrix.CreateRotationY(alienOrientation) * Matrix.CreateTranslation(alienPosition);
                deltaTotal2 -= delta;
            } while (deltaTotal2 > 0);
            #endregion
            
            #region VictoriaUpdate
            do
            {
                #region infoUpdate
                double delta = deltaTotal;
                bazooka.Update(delta);
                Pies.Update(delta);
                DalekModel.Update(delta);
                
                KeyboardState keyboardState = Keyboard.GetState();
                GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
                float speed = 0;

                float turn = GetDesiredTurnRate(ref keyboardState, ref gamePadState) * (float)delta;
                StateMachine(ref delta, keyboardState, ref speed,ref gamePadState, ref turn);
                
                // 
                // State update
                //

                if (state != States.lowering && state != States.lowerStart && state != States.aimed && state != States.aiming && state != States.startaiming)
                    orientation+=turn;
                else if (state == States.aimed)
                {
                    spineOrientation += turn;
                    if (spineOrientation < - Math.PI/3)
                        spineOrientation = (float)-Math.PI / 3;
                    else if (spineOrientation > Math.PI/3)
                        spineOrientation = (float) Math.PI/3;

                }

                victoria.Update(delta);
                //bazooka.Update(delta);
                //
                // Part 1:  Compute a new orientation
                //

                Matrix deltaMatrix = victoria.DeltaMatrix;
                float deltaAngle = (float)Math.Atan2(deltaMatrix.Backward.X, deltaMatrix.Backward.Z);
                float newOrientation = orientation + deltaAngle;

                //
                // Part 2:  Compute a new location
                //

                // We are likely rotated from the angle the model expects to be in
                // Determine that angle.
                Matrix rootMatrix = victoria.RootMatrix;
                float actualAngle = (float)Math.Atan2(rootMatrix.Backward.X, rootMatrix.Backward.Z);
                Vector3 newLocation = location + Vector3.TransformNormal(victoria.DeltaPosition,
                               Matrix.CreateRotationY(newOrientation - actualAngle));

                //
                // I'm just taking these here.  You'll likely want to add something 
                // for collision detection instead.
                //

                //location = newLocation;
                orientation = newOrientation;
#endregion

                #region collision
                Vector3 adjustedLocation = location;
                adjustedLocation.Y = spitLocation.Y;
                if(drawSpit && Vector3.Distance(adjustedLocation,spitLocation)<50 && state!=States.crouching && state!=States.crouchStart)
                {
                    pieHits = 0;
                    Player.isBeingSlimed=true;
                    drawSpit=false;
                    spitTime=0;

                }

                if (activePie != null)
                {
                    String collidsion;
                    if (activePie.pieNumber == 1)
                        collidsion = TestRegion(activePie.Position);
                    else if (activePie.pieNumber == 2)
                        collidsion=TestRegion(activePie.Position-new Vector3(10*(float)Math.Cos(activePie.fireAngle - 1.6),0,-10*(float)Math.Sin(activePie.fireAngle - 1.6)));
                    else
                         collidsion=TestRegion(activePie.Position-new Vector3(20*(float)Math.Cos(activePie.fireAngle - 1.6),0,-20*(float)Math.Sin(activePie.fireAngle - 1.6)));

                    if (collidsion.Contains("Door") || collidsion == "")
                    {
                        activePie.notMoving = true;
                        stuckPies.Add(activePie);
                        activePie = null;
                    }

                }
               
                string region = TestRegion(newLocation);

                if (!region.Contains("Door") && region.Contains("1"))
                {
                    isBeingSlimed = false;
                }
                if (region != "" && !region.Contains("Door"))
                {
                    UpdateDoor(-1, gameTime);
                    if (region[region.Length-1].ToString()!=section.ToString())
                    {
                        previousSection = section;
                        if (!Int32.TryParse(region[region.Length - 1].ToString(), out section))
                            throw new Exception();
                    }
                    location = newLocation;
                }
                else if (region.Contains("Door"))
                {
                    switch (region[region.Length - 1])
                    {
                        case '1':
                            UpdateDoor(0, gameTime);
                            if (game.phibesModel[0].doorState[0].currentstate == DoorClass.DoorState.open)
                                location = newLocation;
                            break;
                        case '2':
                            UpdateDoor(1, gameTime);
                            if (game.phibesModel[1].doorState[0].currentstate == DoorClass.DoorState.open)
                                location = newLocation;
                            break;
                        case '3':
                            UpdateDoor(2, gameTime);
                            if (game.phibesModel[2].doorState[1].currentstate == DoorClass.DoorState.open)
                                location = newLocation;
                            break;
                        case '4':
                            UpdateDoor(3, gameTime);
                            if (game.phibesModel[4].doorState[0].currentstate == DoorClass.DoorState.open)
                                location = newLocation;
                            break;
                        case '5':
                            UpdateDoor(4, gameTime);
                            if (game.phibesModel[5].doorState[0].currentstate == DoorClass.DoorState.open)
                                location = newLocation;
                            break;
                    }

                }
                #endregion

                SetPlayerTransform();
                deltaTotal -= delta;

            } while (deltaTotal > 0);
            #endregion

            //
            // Make the camera follow the player
            //
            

            Vector3 cameraBehind = new Vector3((float)Math.Sin(orientation+spineOrientation), 0,(float) Math.Cos(orientation+spineOrientation));
            game.Camera.Eye = location + new Vector3(0, 180, 0) /*- 100 * cameraBehind*/;
            Matrix stuff = Matrix.CreateRotationY(orientation + spineOrientation);
            game.Camera.Center = game.Camera.Eye + stuff.Backward +new Vector3(0, -0.1f, 0);
          
   
        }

        /// <summary>
        /// This function is called to draw the player.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="gameTime"></param>
        public void Draw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            Matrix transform = Matrix.CreateRotationY(orientation);
            transform.Translation = location;

            int handbone = victoria.Model.Bones["Bip01 R Hand"].Index;
            Matrix handLocation = victoria.AbsoulutePosition(handbone) * transform;
            victoria.Draw(graphics, gameTime, transform);
            
            victoria.spineChange = spineOrientation;
            
            Matrix bazMat = Matrix.CreateRotationX(MathHelper.ToRadians(109.5f)) *
                            Matrix.CreateRotationY(MathHelper.ToRadians(9.7f)) *
                            Matrix.CreateRotationZ(MathHelper.ToRadians(72.9f)) *
                            Matrix.CreateTranslation(new Vector3(-9.6f, 11.85f,21.1f))*
                            handLocation;
           

            bazooka.Draw(graphics, gameTime, bazMat);

            //Draw all pies
            if (activePie != null)
            {
                if (activePie.devoured)
                {
                    int alienBone = Alien.Model.Bones["Bip01 L Finger0"].Index;
                    activePie.Position = (Alien.AbsoulutePosition(alienBone)*alienMatrix).Translation;
                    
                }
                float angle = activePie.fireAngle;
                Matrix pie = Matrix.CreateRotationZ(-(angle)) *        //pie direction
                            Matrix.CreateRotationX(MathHelper.ToRadians(90)) *
                            Matrix.CreateTranslation(activePie.Position);

                Pies.PieDraw(graphics, gameTime, pie, activePie.pieNumber);
            }
            //draw the clickable pie

            Matrix upPIeMatrix =Matrix.CreateScale(2)*Matrix.CreateTranslation(pieReload.centerLocation-new Vector3(0,4f,0));
            Pies.PieDraw(graphics, gameTime, upPIeMatrix, 0);


            if (reloadingPie != null)
            {
                float angle = spineOrientation + orientation;
                float distance = (pieCooldown)/1;
                Vector3 VertexPosition;
                if (reloadingPie.pieNumber == 2)
                    VertexPosition = new Vector3(-35 * (float)Math.Sin(angle), 15, -35 * (float)Math.Cos(angle));
                else if (reloadingPie.pieNumber == 1)
                    VertexPosition = new Vector3(-45 * (float)Math.Sin(angle), 15, -45 * (float)Math.Cos(angle));
                else
                    VertexPosition = new Vector3(-25 * (float)Math.Sin(angle), 15, -25 * (float)Math.Cos(angle));
                VertexPosition += distance*new Vector3(-45 *(float)Math.Sin(angle), 0, -45 * (float)Math.Cos(angle));
                VertexPosition += handLocation.Translation;
                Matrix pie = Matrix.CreateRotationZ(-(angle)) *        //pie direction
                            Matrix.CreateRotationX(MathHelper.ToRadians(90)) *
                            Matrix.CreateTranslation(VertexPosition);
                Pies.PieDraw(graphics, gameTime, pie, reloadingPie.pieNumber);
            }


            foreach (PieClass pie in stuckPies)
            {
                float angle = pie.fireAngle;
                Matrix pies = Matrix.CreateRotationZ(-(angle)) *        //pie direction
                            Matrix.CreateRotationX(MathHelper.ToRadians(90)) *
                            Matrix.CreateTranslation(pie.Position);

                Pies.PieDraw(graphics, gameTime, pies, pie.pieNumber);
            }
            //draw Dalek
            
            Matrix dalekMat = Matrix.CreateRotationY(dalekInfo.bodyAngle)* Matrix.CreateTranslation(dalekInfo.location);
            DalekModel.DalekDraw(graphics, gameTime, dalekMat,dalekInfo);
             
            //Matrix alienMat = Matrix.CreateTranslation(alienPosition);


             Alien.Draw(graphics, gameTime, alienMatrix);

             sprite.Begin();
             sprite.DrawString(ScoreFont,"you have hit the alien: "+pieHits+" times", new Vector2(0,0), Color.White);
             sprite.DrawString(ScoreFont, "you have " + pieCount + " pies left", new Vector2(0, 20), Color.White);
             sprite.End();
             graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            
          
        }

        #endregion

        #region otherStuff
        public void StateMachine(ref double delta, KeyboardState keyboardState,ref float speed, ref GamePadState gamePadState,ref float turn)
        {
            switch (state)
            {
                case States.Start:
                    location.Y = 0;
                    state = States.StanceStart;
                    delta = 0;
                    break;

                case States.StanceStart:
                    location.Y = 0;
                    spineOrientation = 0;
                    victoria.PlayClip("raiseB");
                    victoria.AnimPlayer.Speed = 0;
                    state = States.Stance;
                    break;

                case States.Stance:
                    speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                    location.Y = 0;
                    if (speed > 0)
                    {
                        // We need to leave the stance state and start walking
                        victoria.PlayClip("walkstartB");
                        victoria.AnimPlayer.Speed = speed;
                        state = States.WalkStart;
                    }
                    else if (turn > 0)
                    {
                        victoria.PlayClip("leftturn");
                        state = States.LeftStart;
                    }
                    else if (turn < 0)
                    {
                        victoria.PlayClip("rightturn");
                        state = States.RightStart;
                    }
                    else if (keyboardState.IsKeyDown(Keys.RightControl) || keyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        state = States.crouchStart;
                    }
                    else if (keyboardState.IsKeyDown(Keys.Enter))
                    {
                        state = States.startaiming;
                    }

                    break;

                case States.WalkStart:
                case States.WalkLoop:
                    
                    if (delta > victoria.AnimPlayer.Clip.Duration - victoria.AnimPlayer.Time)
                    {
                        delta = victoria.AnimPlayer.Clip.Duration - victoria.AnimPlayer.Time;
                        location.Y = 0;
                        // The clip is done after this update
                        state = States.WalkLoopStart;
                    }

                    speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                    if (speed == 0)
                    {
                        delta = 0;
                        state = States.StanceStart;
                    }
                    else
                    {
                        victoria.AnimPlayer.Speed = speed;
                    }
                    
                    break;
                case States.WalkLoopStart:
                    victoria.PlayClip("walkloopB");
                    victoria.PlayClip("walkloopB").Speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                    state = States.WalkLoop;
                    break;
                case States.Left:
                    if (delta > victoria.AnimPlayer.Clip.Duration - victoria.AnimPlayer.Time)
                    {
                        delta = victoria.AnimPlayer.Clip.Duration - victoria.AnimPlayer.Time;
                        location.Y = 0;
                        // The clip is done after this update
                        state = States.LeftStart;
                    }

                    speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                    if (speed > 0)
                    {
                        victoria.PlayClip("walkstartB");
                        victoria.AnimPlayer.Speed = speed;
                        state = States.WalkStart;
                    }
                    if (turn <= 0)
                    {
                        delta = 0;
                        state = States.StanceStart;
                    }
                    break;
                case States.LeftStart:
                    {
                        victoria.PlayClip("leftturn").Speed = 1;
                        state = States.Left;
                        break;
                    }
                case States.Right:
                    if (delta > victoria.AnimPlayer.Clip.Duration - victoria.AnimPlayer.Time)
                    {
                        delta = victoria.AnimPlayer.Clip.Duration - victoria.AnimPlayer.Time;
                        location.Y = 0;
                        // The clip is done after this update
                        state = States.RightStart;
                    }

                    speed = GetDesiredSpeed(ref keyboardState, ref gamePadState);
                    if (speed > 0)
                    {
                        victoria.PlayClip("walkstartB");
                        victoria.AnimPlayer.Speed = speed;
                        state = States.WalkStart;
                    }
                    if (turn >= 0)
                    {
                        delta = 0;
                        state = States.StanceStart;
                    }
                    break;
                case States.RightStart:
                    {
                        victoria.PlayClip("rightturn").Speed = 1;
                        state = States.Right;
                        break;
                    }
                case States.startaiming:
                    spineOrientation = 0;
                    victoria.PlayClip("raiseB");
                    victoria.AnimPlayer.Speed = 1;
                    state = States.aiming;
                    break;
                case States.aiming:
                    if (delta > victoria.AnimPlayer.Clip.Duration - victoria.AnimPlayer.Time)
                    {
                        if (keyboardState.IsKeyDown(Keys.Enter))
                        {
                            state = States.StanceStart;
                        }
                        state = States.aimed;
                    }
                    break;
                case States.aimed:
                    location.Y = 0;
                    if (keyboardState.IsKeyDown(Keys.Enter))
                    {
                        state = States.lowerStart;
                    }
                    if (keyboardState.IsKeyDown(Keys.Space) && pieCount > 0 && activePie == null)
                    {
                        if (pieCount > 0 && pieCooldown < 0)
                        {
                            pieCooldown = 1;
                            activePie = createPie();
                            pieCount--;
                            pieNumber = (pieNumber + 1) % 3;
                            ///////
                            if (pieCount > 0)
                                reloadingPie = createReload();
                            ///////
                            
                        }

                    }
                    break;
                case States.lowerStart:

                    victoria.PlayClip("lowerB");
                    victoria.AnimPlayer.Speed = 1;
                    state = States.lowering;
                    break;
                case States.lowering:
                    if (delta > victoria.AnimPlayer.Clip.Duration - victoria.AnimPlayer.Time)
                    {
                        state = States.StanceStart;
                    }
                    spineOrientation = spineOrientation * (1 - .25f * ((float)victoria.AnimPlayer.Time / (float)victoria.AnimPlayer.Clip.Duration));
                    break;
                case States.crouchStart:
                    victoria.PlayClip("crouch");
                    victoria.AnimPlayer.Speed = .5f;
                    state = States.crouching;
                    break;
                case States.crouching:
                    if (delta > victoria.AnimPlayer.Clip.Duration - victoria.AnimPlayer.Time)
                    {
                        state = States.StanceStart;
                    }
                    break;

            }
        }

        public void AlienStateMachine(ref double delta)
        {
            switch (alienState)
            {
                case States.Start:
                    alienPosition.Y = 10;
                    alienState = States.WalkLoopStart;
                    delta = 0;
                    break;
                case States.StanceStart:
                    alienPosition.Y = 10;
                    alienState = States.Stance;
                    Alien.PlayClip("stance");
                    delta = 0;
                    break;
                case States.Stance:
                    break;
                case States.WalkStart:
                    if (delta > Alien.AnimPlayer.Clip.Duration - Alien.AnimPlayer.Time)
                    {
                        delta = Alien.AnimPlayer.Clip.Duration - Alien.AnimPlayer.Time;
                        alienPosition.Y = 10;
                        // The clip is done after this update
                        alienState = States.WalkLoopStart;
                    }
                    else
                        Alien.AnimPlayer.Speed = 1;
                    break;
                case States.WalkLoopStart:
                   Alien.PlayClip("walkloop");
                    alienState = States.WalkStart;
                    break;
                case States.pieCatchstart:
                    Alien.PlayClip("eat");
                    Alien.AnimPlayer.Speed = 1;
                    alienState = States.pieCatch;
                    break;
                case States.pieCatch:
                    if (Alien.AnimPlayer.Time > 1)
                    {
                        Alien.AnimPlayer.Speed = 0;
                    }
                    break;
                case States.pieEat:
                    Alien.AnimPlayer.Speed = 1;
                    if (delta > Alien.AnimPlayer.Clip.Duration - Alien.AnimPlayer.Time)
                    {
                        delta = Alien.AnimPlayer.Clip.Duration - Alien.AnimPlayer.Time;
                        alienPosition.Y = 10;
                        activePie = null;
                        pieHits++;
                        if (aboutFace)
                            alienOrientation = (float)Math.PI;
                        else
                            alienOrientation = 0;
                        alienState = States.WalkLoopStart;
                    }
                    break;
                case States.spitting:
                    if (spitTime > 5||!drawSpit)
                    {
                        alienState = States.WalkLoopStart;
                        if (aboutFace)
                            alienOrientation = (float)Math.PI;
                        else
                            alienOrientation = 0;
                        alienPosition.Y = 10;
                     
                    }
                    break;



            }

        }

       

        
        public void SpitDraw(GraphicsDeviceManager graphics, GameTime gameTime)
        {
            if (drawSpit)
            {
                graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                Matrix spitMatrix = Matrix.CreateRotationY(0) * Matrix.CreateTranslation(spitLocation);
                spit.Draw(graphics, gameTime, spitMatrix);
                graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            }

        }

        /// <summary>
        /// Test to see if we are in some region.
        /// </summary>
        /// <param name="v3">The region name or a blank string if not in a region.</param>
        /// <returns></returns>
        private string TestRegion(Vector3 v3)
        {
            // Convert to a 2D Point
            float x = v3.X;
            float y = v3.Z;

            foreach (KeyValuePair<string, List<Vector2>> region in regions)
            {
           
                if (region.Key.StartsWith("W"))
                    continue;

                for (int i = 0; i < region.Value.Count; i += 3)
                {
                    float x1 = region.Value[i].X;
                    float x2 = region.Value[i + 1].X;
                    float x3 = region.Value[i + 2].X;
                    float y1 = region.Value[i].Y;
                    float y2 = region.Value[i + 1].Y;
                    float y3 = region.Value[i + 2].Y;

                    float d = 1 / ((x1 - x3) * (y2 - y3) - (x2 - x3) * (y1 - y3));
                    float l1 = ((y2 - y3) * (x - x3) + (x3 - x2) * (y - y3)) * d;
                    if (l1 < 0)
                        continue;

                    float l2 = ((y3 - y1) * (x - x3) + (x1 - x3) * (y - y3)) * d;
                    if (l2 < 0)
                        continue;

                    float l3 = 1 - l1 - l2;
                    if (l3 < 0)
                        continue;

                    return region.Key;
                }
            }

            return "";
        }

        /// <summary>
        /// manually updates the doors
        /// </summary>
        /// <param name="i"> the door to change</param>
        /// <param name="gameTime"> the change in time</param>
        private void UpdateDoor(int i,GameTime gameTime)
        {
            switch (i)
            {
                    //open the first door
                case 0:
                    if (section == 1)
                    {
                        previousSection = 2;
                        doorOpenPercentage = game.phibesModel[0].doorState[0].PercentOpen;
                    }
                    else
                    {
                        previousSection = 1;
                        doorOpenPercentage = game.phibesModel[1].doorState[1].PercentOpen;
                    }
                    
                    game.phibesModel[0].Update(gameTime, false, 0);
                    game.phibesModel[1].Update(gameTime, false, 1);
                    game.phibesModel[2].Update(gameTime, true, -1);
                    game.phibesModel[3].Update(gameTime, true,-1);
                    game.phibesModel[4].Update(gameTime, true, -1);
                    game.phibesModel[5].Update(gameTime, true, -1);
                    break;
                case 1:
                    if (section == 2)
                    {
                        doorOpenPercentage= game.phibesModel[1].doorState[0].PercentOpen;
                        previousSection = 3;
                    }
                    else
                    {
                        doorOpenPercentage = game.phibesModel[2].doorState[0].PercentOpen;
                        previousSection = 2;
                    }
                    game.phibesModel[0].Update(gameTime, true, -1);
                    game.phibesModel[1].Update(gameTime, false, 0);
                    game.phibesModel[2].Update(gameTime, false, 0);
                    game.phibesModel[3].Update(gameTime, true, -1);
                    game.phibesModel[4].Update(gameTime, true, -1);
                    game.phibesModel[5].Update(gameTime, true, -1);
                    break;
                case 2:
                    if (section == 3)
                    {
                        doorOpenPercentage = game.phibesModel[2].doorState[1].PercentOpen;
                        previousSection = 4;
                    }
                    else
                    {
                        previousSection = 3;
                        doorOpenPercentage = game.phibesModel[3].doorState[0].PercentOpen;
                    }
                    game.phibesModel[0].Update(gameTime, true, -1);
                    game.phibesModel[1].Update(gameTime, true, -1);
                    game.phibesModel[2].Update(gameTime, false,1);
                    game.phibesModel[3].Update(gameTime, false, 0);
                    game.phibesModel[4].Update(gameTime, true, -1);
                    game.phibesModel[5].Update(gameTime, true, -1);
                    break;
                case 3:
                    if (section == 4)
                    {
                        previousSection = 5;
                        doorOpenPercentage = game.phibesModel[3].doorState[1].PercentOpen;
                    }
                    else
                    {
                        doorOpenPercentage = game.phibesModel[4].doorState[0].PercentOpen;
                        previousSection = 4;
                    }
                   game.phibesModel[0].Update(gameTime, true, -1);
                    game.phibesModel[1].Update(gameTime, true, -1);
                    game.phibesModel[2].Update(gameTime, true, -1);
                    game.phibesModel[3].Update(gameTime, false, 1);
                    game.phibesModel[4].Update(gameTime, false, 0);
                    game.phibesModel[5].Update(gameTime, true, -1);
                    break;
                case 4:
                    if (section == 4)
                    {
                        doorOpenPercentage = game.phibesModel[3].doorState[2].PercentOpen;
                        previousSection = 6;
                    }
                    else
                    {
                        doorOpenPercentage = game.phibesModel[5].doorState[0].PercentOpen;
                        previousSection = 4;
                    }
                    game.phibesModel[0].Update(gameTime, true, -1);
                    game.phibesModel[1].Update(gameTime, true, -1);
                    game.phibesModel[2].Update(gameTime, true, -1);
                    game.phibesModel[3].Update(gameTime, false, 2);
                    game.phibesModel[4].Update(gameTime, true, -1);
                    game.phibesModel[5].Update(gameTime, false, 0);
                    break;

                default:
                    float max = 0;
                    foreach (DoorClass state in game.phibesModel[section - 1].doorState)
                    {
                        if (max < state.PercentOpen)
                        {
                            max = state.PercentOpen;
                        }
                    }
                    doorOpenPercentage = max;
                    game.phibesModel[0].Update(gameTime, true, -1);
                    game.phibesModel[1].Update(gameTime, true, -1);
                    game.phibesModel[2].Update(gameTime, true, -1);
                    game.phibesModel[3].Update(gameTime, true, -1);
                    game.phibesModel[4].Update(gameTime, true, -1);
                    game.phibesModel[5].Update(gameTime, true, -1);
                    break;
    
            }
        }

        private float GetDesiredSpeed(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.Up))
                return 2;

            float speed = gamePadState.ThumbSticks.Right.Y;

            // I'm not allowing you to walk backwards
            if (speed < 0)
                speed = 0;

            return speed;
        }

        private float GetDesiredTurnRate(ref KeyboardState keyboardState, ref GamePadState gamePadState)
        {
            if (keyboardState.IsKeyDown(Keys.Left))
            {
                return panRate;
            }

            if (keyboardState.IsKeyDown(Keys.Right))
            {
                return -panRate;
            }

            return -gamePadState.ThumbSticks.Right.X * panRate;
        }
        /// <summary>
        /// Creates the pie to be thrown
        /// </summary>
        /// <returns></returns>
        public PieClass createPie()
        {
            float angle=orientation+spineOrientation;
            Vector3 VertexPosition;

            int handbone = victoria.Model.Bones["Bip01 R Hand"].Index;
            Matrix handLocation = victoria.AbsoulutePosition(handbone) * transform;

            if(pieNumber==2)
                VertexPosition = new Vector3(-35f * (float)Math.Sin(angle - 1.6) + 60 * (float)Math.Cos(angle - 1.6), 15, -35f * (float)Math.Cos(angle - 1.6) - 60 * (float)Math.Sin(angle - 1.6));
            else if (pieNumber==1)
                VertexPosition = new Vector3(-35f * (float)Math.Sin(angle - 1.6) + 50 * (float)Math.Cos(angle - 1.6), 15, -35f * (float)Math.Cos(angle - 1.6) - 50 * (float)Math.Sin(angle - 1.6));
            else
                VertexPosition = new Vector3(-35f * (float)Math.Sin(angle - 1.6) + 70 * (float)Math.Cos(angle - 1.6), 15, -35f * (float)Math.Cos(angle - 1.6) - 70 * (float)Math.Sin(angle - 1.6));
            
            VertexPosition += handLocation.Translation;

            return  new PieClass(pieNumber,VertexPosition,angle);
        }

        /// <summary>
        /// Creates the pie to be thrown
        /// </summary>
        /// <returns></returns>
        public PieClass createReload()
        {
            float angle = orientation + spineOrientation;
            Vector3 VertexPosition;

            int handbone = victoria.Model.Bones["Bip01 R Hand"].Index;
            Matrix handLocation = victoria.AbsoulutePosition(handbone) * transform;

            if (pieNumber == 2)
                VertexPosition = new Vector3(-35 * (float)Math.Sin(angle), 15, -35 * (float)Math.Cos(angle));
            else if (pieNumber == 1)
                VertexPosition = new Vector3(-45*(float)Math.Sin(angle),15,-45*(float)Math.Cos(angle));
            else
                VertexPosition = new Vector3(-25 * (float)Math.Sin(angle), 15, -25 * (float)Math.Cos(angle));

            
            VertexPosition += handLocation.Translation;

            return new PieClass(pieNumber, VertexPosition, angle);
        }

        #endregion
    }
    
}

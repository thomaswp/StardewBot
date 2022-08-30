using BlocklyBridge;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using Farmtronics;

namespace StardewBot
{

    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }

    [ScriptableBehavior("Movement", 60)]
    public class Movement : ActionCategory
    {

        public Movement(BotController controller)
            : base(controller)
        {

        }
        
        public static Vector2 DirToVec(int direction)
        {
            switch ((Direction) direction)
            {
                case Direction.Up: return new Vector2(0, -1);
                case Direction.Right: return new Vector2(1, 0);
                case Direction.Down: return new Vector2(0, 1);
                case Direction.Left: return new Vector2(-1, 0);
            }
            return Vector2.Zero;
        }

        public static Vector2 TileToPos(Vector2 tile)
        {
            return tile * Game1.tileSize;
        }

        //private void StartMovingInDirection(Vector2 direction)
        //{
        //    if (direction.Y < 0) Bot.SetMovingUp(true);
        //    if (direction.X > 0) Bot.SetMovingRight(true);
        //    if (direction.Y > 0) Bot.SetMovingDown(true);
        //    if (direction.X < 0) Bot.SetMovingLeft(true);
        //}

        [ScriptableMethod]
        public AsyncMethod TurnRight()
        {
            return DoInstantly(() => Bot.Rotate(1));
        }

        [ScriptableMethod]
        public AsyncMethod TurnLeft()
        {
            return DoInstantly(() => Bot.Rotate(-1));
        }

        [ScriptableMethod]
        public AsyncMethod FaceDirection(Direction dir)
        {
            return DoInstantly(() => Bot.FacingDirection = (int)dir);
        }

        //[ScriptableMethod]
        //public AsyncMethod FacePlayer(Direction dir)
        //{
        //    return new AsyncMethod().Do(() => NPC.faceTowardFarmer());
        //}

        [ScriptableMethod]
        public AsyncMethod MoveDirection(Direction dir)
        {
            return new AsyncMethod()
                .Do(FaceDirection(dir))
                .Do(MoveForward());
        }

        [ScriptableMethod]
        public AsyncMethod MoveForward()
        {
            return new AsyncMethod()
                .Do(() => Bot.MoveForward())
                .Do(finishAction);

            // TODO: tile selection should also be in the async
            //Vector2 startTile = Bot.getTileLocation();
            //Vector2 newTile = startTile + DirToVec(Bot.FacingDirection);
            //return MoveDirectlyToTile(newTile);
        }

        //private AsyncMethod MoveDirectlyToTile(Vector2 newTile)
        //{
        //    // TODO: All calculations need to be done inside the Async method,
        //    // in case the direction has changed before this is called
        //    // Therefore need a way to bail out of a method

        //    Logger.Log($"new: {newTile}, passable: {IsPassable(newTile)}");
        //    if (!IsPassable(newTile)) return AsyncMethod.NoOp;

        //    Vector2 startTile = Bot.getTileLocation();
        //    Logger.Log($"current tile: {startTile}, pos: {Bot.Position}");

        //    Vector2 targetPosision = TileToPos(newTile);
        //    Logger.Log($"target tile: {newTile}, pos: {targetPosision}");

        //    Vector2 dir = targetPosision - Bot.Position;
        //    Logger.Log($"dir: {dir}");

        //    // TODO: Need to stop directions separately
        //    List<Func<bool>> conditions = new List<Func<bool>>();
        //    if (dir.X > 0) conditions.Add(() => Bot.position.X >= targetPosision.X);
        //    else if (dir.X < 0) conditions.Add(() => Bot.position.X <= targetPosision.X);
        //    if (dir.Y > 0) conditions.Add(() => Bot.position.Y >= targetPosision.Y);
        //    else if (dir.Y < 0) conditions.Add(() => Bot.position.Y <= targetPosision.Y);

        //    return new AsyncMethod()
        //        .Do(() => {
        //            StartMovingInDirection(dir);
        //        })
        //        .UpdateUntil(() => conditions.All(c => c()) || !Bot.isMoving())
        //        .Do(() => Bot.Halt());
        //}

        //[ScriptableMethod]
        //public AsyncMethod Jump()
        //{
        //    Logger.Log(Bot.yJumpOffset);
        //    return new AsyncMethod()
        //        .Do(() => Bot.jump())
        //        .UpdateUntil(() => Bot.yJumpOffset != 0)
        //        .UpdateUntil(() => Bot.yJumpOffset == 0);
        //}

        public void Update()
        {
            //var player = Game1.player;
            //var location = Location;
            //NPC.updateMovement(NPC.currentLocation, Game1.currentGameTime);
            //if (NPC.isMoving() && location.isCollidingPosition(NPC.nextPosition(NPC.getDirection()), Game1.viewport, true))
            //{
            //    Logger.Log($"Stopping at {location.Name} for {NPC.nextPosition(NPC.getDirection())}");
            //    var playerPos = player.nextPosition(player.getDirection());
            //    Logger.Log($"Player is {playerPos} {location.isCollidingPosition(playerPos, Game1.viewport, true)}");
            //    NPC.Halt();
            //}
            //if (Bot.isMoving() && !location.isTilePassable(Bot.nextPositionTile(), Game1.viewport))
            //{
            //    Logger.Log($"Stopping at {location.Name} for {Bot.nextPositionTile()}");
            //    var playerTile = player.nextPositionTile();
            //    Logger.Log($"Player is {playerTile} {location.isTilePassable(playerTile, Game1.viewport)}");
            //    Bot.Halt();
            //}
            //Bot.MovePosition(Game1.currentGameTime, Game1.viewport, location);

            //bool inDialogNow = NPC.CurrentDialogue.Count == 0;
            //if (inDialog != inDialogNow)
            //{
            //    inDialog = inDialogNow;
            //    if (inDialog)
            //    {
            //        OnDialogStart();
            //        Logger.Log("Dialog starting!");
            //    }
            //    else
            //    {
            //        OnDialogEnd();
            //        Logger.Log("Dialog ended!");
            //    }
            //}
        }
    }
}

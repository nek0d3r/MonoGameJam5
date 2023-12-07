using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.Tiled;

public enum Facing
{
    North = 0,
    South = 1,
    West = 2,
    East = 3
}
public abstract class Entity : ICollisionActor
{
    protected string _animation;
    public abstract int Identifier { get; set; }
    public abstract Vector2 Position { get; set; }
    protected abstract Vector2 ColliderPosition { get; }
    public abstract AnimatedSprite Sprite { get; set; }
    public abstract Facing Direction { get; set; }
    public abstract string Animation { get; set; }
    public abstract IShapeF Bounds { get; protected set; }
    public abstract int DrawPriority { get; set; }
    public abstract float Speed { get; set; }

    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch, bool drawCollider = false);
    public abstract void OnCollision(CollisionEventArgs collisionInfo);

    public static TiledMapObject GetGameObjectById(TiledMap tiledMap, int id)
    {
        IEnumerable<TiledMapObject> gameObjects = tiledMap.ObjectLayers.SelectMany(layer => layer.Objects);
        return gameObjects.First(gameObject => gameObject.Identifier == id);
    }

    public static List<Entity> CreateEntities(TiledMap tiledMap, SpriteSheet spriteSheet)
    {
        List<Entity> entities = new List<Entity>();
        // For every object layer in the map
        foreach(TiledMapObjectLayer layer in tiledMap.ObjectLayers)
        {
            // For every object in the layer
            foreach(TiledMapObject tiledObject in layer.Objects)
            {
                // Tiled game objects have the bottom left corner as an origin point
                // This offsets it to MonoGame's default origin of center
                Vector2 position = tiledObject.Position;
                position.X += TileRender.TILE_SIZE / 2;
                position.Y -= TileRender.TILE_SIZE / 2;

                switch (tiledObject.Type)
                {
                    case "player":
                        entities.Add(new Player()
                        {
                            Identifier = tiledObject.Identifier,
                            Speed = Convert.ToSingle(tiledObject.Properties["speed"]),
                            Sprite = new AnimatedSprite(spriteSheet),
                            Animation = tiledObject.Properties["animation"],
                            Position = position,
                            DrawPriority = 2
                        });
                        break;
                    case "employee":
                        entities.Add(new NPC()
                        {
                            Identifier = tiledObject.Identifier,
                            Speed = Convert.ToSingle(tiledObject.Properties["speed"]),
                            Sprite = new AnimatedSprite(spriteSheet),
                            Animation = tiledObject.Properties["animation"],
                            Position = position,
                            DrawPriority = 2,
                            IdleActions = new SingleLinkedList<Action>()
                        });
                        break;
                    case "manager":
                        entities.Add(new Enemy()
                        {
                            Identifier = tiledObject.Identifier,
                            Speed = Convert.ToSingle(tiledObject.Properties["speed"]),
                            Sprite = new AnimatedSprite(spriteSheet),
                            Animation = tiledObject.Properties["animation"],
                            Position = position,
                            DrawPriority = 2,
                            IdleActions = new SingleLinkedList<Action>(),
                            SoundsToParse = new List<Point>()
                        });
                        break;
                    case "conveyor":
                        entities.Add(new Conveyor()
                        {
                            Identifier = tiledObject.Identifier,
                            Speed = Convert.ToSingle(tiledObject.Properties["speed"]),
                            Sprite = new AnimatedSprite(spriteSheet),
                            Animation = tiledObject.Properties["animation"],
                            Direction = (Facing)Convert.ToInt32(tiledObject.Properties["facing"]),
                            ConveyorType = (ConveyorType)Convert.ToInt32(tiledObject.Properties["conveyorType"]),
                            Position = position,
                            DrawPriority = 1
                        });
                        break;
                    case "box":
                        entities.Add(new Box()
                        {
                            Identifier = tiledObject.Identifier,
                            Sprite = new AnimatedSprite(spriteSheet),
                            Animation = tiledObject.Properties["animation"],
                            Position = position,
                            DrawPriority = 2
                        });
                        break;
                    case "wall":
                        entities.Add(new Wall()
                        {
                            Identifier = tiledObject.Identifier,
                            ColliderSize = tiledObject.Size,
                            Position = position
                        });
                        break;
                    case "toilet":
                        entities.Add(new Toilet()
                        {
                            Identifier = tiledObject.Identifier,
                            Sprite = new AnimatedSprite(spriteSheet),
                            Animation = tiledObject.Properties["animation"],
                            Position = position,
                            DrawPriority = 2
                        });
                        break;
                    default:
                        break;
                }
            }
        }

        return entities;
    }

    public static void ParseActions(TiledMap tiledMap, Entity entity)
    {
        if (entity is NPC || entity is Enemy)
        {
            TiledMapObject gameObject = GetGameObjectById(tiledMap, entity.Identifier);
            SingleLinkedList<Action> actions = new SingleLinkedList<Action>();

            if (gameObject.Properties.ContainsKey("actions"))
            {
                int actionObjectId = Convert.ToInt32(gameObject.Properties["actions"]);

                while (!actions.Any(action => ((SingleLinkedListNode<Action>)action).Value.GameObjectIdentifier == actionObjectId))
                {
                    TiledMapObject actionObject = GetGameObjectById(tiledMap, actionObjectId);

                    Action idleAction = new Action()
                    {
                        GameObjectIdentifier = actionObjectId,
                        ThisAction = (Action.ActionType)Convert.ToInt32(actionObject.Properties["actionType"])
                    };

                    switch (idleAction.ThisAction)
                    {
                        case Action.ActionType.Move:
                            if (actionObject is TiledMapPolygonObject)
                            {
                                idleAction.Destinations = new List<Vector2>();
                                ((TiledMapPolygonObject)actionObject).Points
                                    .ToList()
                                    .ForEach(point =>
                                        idleAction.Destinations.Add(
                                            new Vector2(
                                                point.X + actionObject.Position.X,
                                                point.Y + actionObject.Position.Y
                                            )
                                        )
                                    );
                            }
                            else if (actionObject is TiledMapPolylineObject)
                            {
                                idleAction.Destinations = new List<Vector2>();
                                ((TiledMapPolylineObject)actionObject).Points
                                    .ToList()
                                    .ForEach(point =>
                                        idleAction.Destinations.Add(
                                            new Vector2(
                                                point.X + actionObject.Position.X,
                                                point.Y + actionObject.Position.Y
                                            )
                                        )
                                    );
                            }
                            break;
                        case Action.ActionType.Pause:
                            idleAction.Duration = Convert.ToSingle(actionObject.Properties["time"]);
                            break;
                        case Action.ActionType.Wander:
                            if (actionObject.Properties.ContainsKey("time"))
                            {
                                idleAction.Duration = Convert.ToSingle(actionObject.Properties["time"]);
                            }

                            if (actionObject is TiledMapEllipseObject)
                            {
                                idleAction.WanderArea = new EllipseF(
                                    ((TiledMapEllipseObject)actionObject).Center,
                                    ((TiledMapEllipseObject)actionObject).Radius.X,
                                    ((TiledMapEllipseObject)actionObject).Radius.Y
                                );
                            }
                            else if (actionObject is TiledMapRectangleObject)
                            {
                                idleAction.WanderArea = new RectangleF(
                                    ((TiledMapRectangleObject)actionObject).Position,
                                    ((TiledMapRectangleObject)actionObject).Size
                                );
                            }
                            break;
                        case Action.ActionType.Interact:
                            break;
                        default:
                            break;
                    }

                    actions.Push(new SingleLinkedListNode<Action>(idleAction));

                    if (!actionObject.Properties.ContainsKey("actions"))
                    {
                        break;
                    }
                    actionObjectId = Convert.ToInt32(actionObject.Properties["actions"]);
                    if (actions.First.Value.GameObjectIdentifier == actionObjectId)
                    {
                        actions.Last.Next = actions.First;
                    }
                }
            }

            if (entity is NPC)
            {
                ((NPC)entity).IdleActions = actions;
            }
            else
            {
                ((Enemy)entity).IdleActions = actions;
            }
        }
    }
}
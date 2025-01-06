using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

public partial class Grid : Node2D
{
    [Signal]
    public delegate void MoveMadeEventHandler();
    [Signal]
    public delegate void GameOverEventHandler();
    [Signal]
    public delegate void ScoreAddedEventHandler();

    const int Border = 15;
    const int TileSize = 100;
    const int BoardSize = 4;

    private Tile[,] tiles = new Tile[4, 4];
    private PackedScene tileScene;

    enum BoardInputAction
    {
        None,
        Left,
        Right,
        Up,
        Down,
    }

    struct MovementResult {
        public int accumulatedScore;
        public bool movementOccurred;
    };

    public override void _Ready()
    {
        tileScene = GD.Load<PackedScene>("res://Scenes/Tile.tscn");

        InsertInitialTiles();
    }

    private void InsertInitialTiles()
    {
        Vector2I firstLocation = default;
        Vector2I secondLocation = default;
        while (firstLocation == secondLocation)
        {
            firstLocation = VectorHelpers.Randf(BoardSize, BoardSize);
            secondLocation = VectorHelpers.Randf(BoardSize, BoardSize);
        }
        SpawnTile(firstLocation, 2);
        SpawnTile(secondLocation, 2);
    }

    private void SpawnTile(Vector2I location, int value)
    {
        var tile = tileScene.Instantiate<Tile>();
        tile.Value = value;
        tile.Position = MapToLocal(location);
        tiles[location.X, location.Y] = tile;
        AddChild(tile);

        tile.Scale = Vector2.Zero;
        var tw = tile.CreateTween();
        tw.TweenProperty(tile, "scale", Vector2.One, 0.1f);
    }

    public void Reset() {
        SetProcessInput(true);
        for (var y = 0; y < BoardSize; y++) {
            for (var x = 0; x < BoardSize; x++) {
                if (tiles[x, y] != null) {
                    var tile = tiles[x, y];
                    tiles[x, y] = null;
                    RemoveChild(tile);
                    tile.QueueFree();
                }
            }
        }

        InsertInitialTiles();
    }

    private Vector2 MapToLocal(Vector2I map)
    {
        return new Vector2
        {
            X = Border + (map.X * TileSize) + (map.X * Border),
            Y = Border + (map.Y * TileSize) + (map.Y * Border),
        };
    }

    public override void _Input(InputEvent @event)
    {
        BoardInputAction action = BoardInputAction.None;
        if (Input.IsActionJustPressed("move_left"))
        {
            action = BoardInputAction.Left;
        }
        else if (Input.IsActionJustPressed("move_right"))
        {
            action = BoardInputAction.Right;
        }
        else if (Input.IsActionJustPressed("move_up"))
        {
            action = BoardInputAction.Up;
        }
        else if (Input.IsActionJustPressed("move_down"))
        {
            action = BoardInputAction.Down;
        }
        if (action != BoardInputAction.None)
        {
            EmitSignal(SignalName.MoveMade);
            GetViewport().SetInputAsHandled();
            var movementResult = HandleInputAction(action);
            if (movementResult.movementOccurred) {
                SpawnRandomTile();
                GD.Print(string.Format("acquired {0} score", movementResult.accumulatedScore));
                if (movementResult.accumulatedScore > 0) {
                    EmitSignal(SignalName.ScoreAdded, movementResult.accumulatedScore);
                }
                if (!HasAnyPossibleMoves()) {
                    GD.Print("Game Over!!");
                    EmitSignal(SignalName.GameOver);
                    SetProcessInput(false);
                }
            }
        }
    }

    private void SpawnRandomTile()
    {
        var spaces = new List<(int, int)>();
        for (var j = 0; j < BoardSize; j++) {
            for (var i = 0; i < BoardSize; i++) {
                if (tiles[i, j] == null) {
                    spaces.Add((i, j));
                }
            }
        }
        // Pick a random one.
        int index = GD.RandRange(0, spaces.Count - 1);
        var space = spaces[index];

        var twoOrFour = GD.Randf() * 10 < 5; 
        var value = twoOrFour ? 2 : 4;
        SpawnTile(new Vector2I(space.Item1, space.Item2), value);
    }

    private MovementResult HandleInputAction(BoardInputAction action)
    {
        var discarded = new List<Tile>();
        var anyMovementOccurred = false;
        int accumulatedScore = 0;

        // Determine movement direction
        int dx = 0, dy = 0;
        if (action == BoardInputAction.Left) dx = -1;
        if (action == BoardInputAction.Right) dx = 1;
        if (action == BoardInputAction.Up) dy = -1;
        if (action == BoardInputAction.Down) dy = 1;

        // Process tiles based on direction
        for (int startX = (dx > 0 ? BoardSize - 1 : 0); startX >= 0 && startX < BoardSize; startX += (dx == 0 ? 1 : -dx))
        {
            for (int startY = (dy > 0 ? BoardSize - 1 : 0); startY >= 0 && startY < BoardSize; startY += (dy == 0 ? 1 : -dy))
            {
                int x = startX;
                int y = startY;

                if (tiles[x, y] == null)
                    continue;

                // Move the tile in the given direction
                while (true)
                {
                    int nextX = x + dx;
                    int nextY = y + dy;

                    if (nextX < 0 || nextX >= BoardSize || nextY < 0 || nextY >= BoardSize)
                        break;

                    var nextTile = tiles[nextX, nextY];

                    if (nextTile == null)
                    {
                        // Move to the empty space
                        tiles[nextX, nextY] = tiles[x, y];
                        tiles[x, y] = null;
                        x = nextX;
                        y = nextY;
                        anyMovementOccurred = true;
                    }
                    else if (nextTile.Value == tiles[x, y].Value)
                    {
                        // Merge tiles
                        nextTile.Value *= 2;
                        accumulatedScore += nextTile.Value;
                        discarded.Add(tiles[x, y]);
                        tiles[x, y] = null;
                        anyMovementOccurred = true;
                        break;
                    }
                    else
                    {
                        // Cannot move further
                        break;
                    }
                }

                // Update tile position
                if (tiles[x, y] != null) {
                    var tw = tiles[x, y].CreateTween();
                    tw.TweenProperty(tiles[x, y], "position", MapToLocal(new Vector2I(x, y)), 0.1f);
                }
            }
        }

        // Remove merged tiles
        foreach (var discard in discarded)
        {
            RemoveChild(discard);
            discard.QueueFree();
        }

        return new MovementResult{
            movementOccurred = anyMovementOccurred,
            accumulatedScore = accumulatedScore,
        };
    }

    private bool HasAnyPossibleMoves() {

        var directions = new (int, int)[] {
            (-1, 0),
            (1, 0),
            (0, -1),
            (0, 1)
        };

        for (var y = 0; y < BoardSize; y++) {
            for (var x = 0; x < BoardSize; x++) {
                if (tiles[x, y] == null) 
                {
                    return true;
                }
                else
                {
                    foreach (var direction in directions) {
                        var nx = direction.Item1 + x;
                        var ny = direction.Item2 + y;
                        if (nx < 0 || ny < 0 || nx >= BoardSize || ny >= BoardSize) {
                            continue;
                        }
                        if (tiles[nx, ny] != null && tiles[nx, ny].Value == tiles[x, y].Value) {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
}

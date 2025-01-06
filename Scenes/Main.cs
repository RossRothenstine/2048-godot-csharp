using Godot;
using System;

public partial class Main : Node
{
    private int _score = 0;
    private int Score { 
        get => _score;
        set {
            _score = value;
            OnScoreChanged();
        }
    }

    private void OnScoreChanged()
    {
        ui.Score = Score;
    }

    private int _moves = 0;
    private int Moves {
        get => _moves;
        set {
            _moves = value;
            OnMovesChanged();
        }
    }

    private void OnMovesChanged()
    {
        ui.Moves = Moves;
    }

    private Grid grid;
    private UI ui;

    public override void _Ready()
    {
        grid = GetNode<Grid>("%Grid");
        ui = GetNode<UI>("%UI");

        grid.Connect(Grid.SignalName.MoveMade, Callable.From(OnGridMoveMade));
        grid.Connect(Grid.SignalName.ScoreAdded, Callable.From<int>(OnScoreAdded));
        grid.Connect(Grid.SignalName.GameOver, Callable.From(OnGameOver));
        ui.Connect(UI.SignalName.TryAgainClicked, Callable.From(OnTryAgain));
    }

    private void OnTryAgain()
    {
        Score = 0;
        Moves = 0;
        grid.Reset();
    }

    private void OnGameOver()
    {
        ui.ShowGameOver();
    }

    private void OnScoreAdded(int amount)
    {
        Score += amount;
    }

    private void OnGridMoveMade()
    {
        Moves += 1;
    }
}

using Godot;
using Microsoft.VisualBasic;
using System;

public partial class UI : Control
{
    [Signal]
    public delegate void TryAgainClickedEventHandler();

    private int _moves;
    public int Moves
    {
        get => _moves;
        set
        {
            _moves = value;
            OnMovesChanged();
        }
    }

    private int _score;
    public int Score
    {
        get => _score;
        set
        {
            _score = value;
            OnScoreChanged();
        }
    }

    public override void _Ready()
    {
        var tryAgainButton = GetNode<Button>("%TryAgainButton");
        tryAgainButton.Connect(BaseButton.SignalName.Pressed, Callable.From(OnTryAgainButtonPressed));

        var mobileSwipeControl = GetNode<Control>("%MobileSwipeReceiver");
        mobileSwipeControl.Visible = OS.GetName() == "iOS" || OS.GetName() == "Android";
    }

    private void OnTryAgainButtonPressed()
    {
        EmitSignal(SignalName.TryAgainClicked);
        HideGameOver();
    }

    public void ShowGameOver()
    {
        var gameOverPanel = GetNode<PanelContainer>("%GameOverPanel");
        gameOverPanel.Visible = true;
    }

    public void HideGameOver() {
        var gameOverPanel = GetNode<PanelContainer>("%GameOverPanel");
        gameOverPanel.Visible = false;
    }

    private void OnMovesChanged()
    {
        if (!IsInsideTree())
        {
            return;
        }
        var movesLabel = GetNode<Label>("%MovesLabel");
        movesLabel.Text = Moves.ToString();
    }

    private void OnScoreChanged()
    {
        if (!IsInsideTree())
        {
            return;
        }
        var scoreLabel = GetNode<Label>("%ScoreLabel");
        scoreLabel.Text = Score.ToString();
    }
}

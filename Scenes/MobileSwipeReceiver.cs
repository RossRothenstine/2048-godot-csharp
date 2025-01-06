using Godot;
using System;

public partial class MobileSwipeReceiver : Control
{
    // Swipe distance of finger to register as a swipe.
    public const float SwipeDistance = 100.0f;

    [Export]
    public StringName upAction;
    [Export]
    public StringName downAction;
    [Export]
    public StringName rightAction;
    [Export]
    public StringName leftAction;

    [Export]
    public bool invertY;

    private Vector2 startMousePosition = default;
    private bool isMouseDown = false;
    private bool isActionCommitted = false;
    private StringName lastAction = null;

    public override void _GuiInput(InputEvent @event) {
        if (@event is InputEventMouseButton mouseEvent) {
            isMouseDown = mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed;
            startMousePosition = mouseEvent.Position;
            isActionCommitted = false;
            lastAction = null;
            GetViewport().SetInputAsHandled();
        } else if (@event is InputEventMouseMotion motionEvent) {
            if (!isMouseDown) {
                return;
            }
            var distance = motionEvent.Position.DistanceTo(startMousePosition);
            if (distance > SwipeDistance) {
                // Far enough to track an event, see which direction it is.
                var direction = (motionEvent.Position - startMousePosition).Normalized();

                StringName action = null;
                float actionStrength = 0.0f;

                if (Mathf.Abs(direction.X) > Mathf.Abs(direction.Y)) {
                    if (direction.X > 0) {
                        action = rightAction;
                        actionStrength = 1.0f;
                    } else {
                        action = leftAction;
                        actionStrength = -1.0f;
                    }
                } else {
                    if (direction.Y > 0) {
                        action = invertY ? upAction : downAction;
                        actionStrength = invertY ? -1.0f : 1.0f;
                    } else {
                        action = invertY ? downAction : upAction;
                        actionStrength = invertY ? 1.0f : -1.0f;
                    }
                }
                startMousePosition = motionEvent.Position;
                if (lastAction == action) {
                    // Don't double commit.
                    return;
                }

                // Send the input action into the input handlers.
                GD.Print(string.Format("submitting action: {0}, strength {1}", action, actionStrength));
                var inputEventPressed = new InputEventAction();
                inputEventPressed.Action = action;
                inputEventPressed.Strength = actionStrength;
                inputEventPressed.Pressed = true;
                Input.ParseInputEvent(inputEventPressed);
                isActionCommitted = true;
                lastAction = action;
                Callable.From(() => {
                    inputEventPressed.Pressed = false;
                    Input.ParseInputEvent(inputEventPressed);
                }).CallDeferred();
            }
            GetViewport().SetInputAsHandled();
        }
    }
}

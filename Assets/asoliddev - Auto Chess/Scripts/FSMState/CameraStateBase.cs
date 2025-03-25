public class CameraStateBase : StateBase
{
    protected CameraManager CameraManager;
    protected readonly InputControls inputControls;
    public CameraStateBase(int stateID) : base(stateID)
    {
        inputControls = new();
    }
    
    public override void DoOnEnter()
    {
        base.DoOnEnter();
        inputControls.Enable();
        if (CameraManager == null)
            CameraManager = StateMachine.gameObject.GetComponent<CameraManager>();
    }

    public override void DoOnExit()
    {
        base.DoOnExit();
        inputControls.Disable();
    }
}

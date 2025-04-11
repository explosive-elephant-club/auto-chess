namespace WeSkill
{
    public interface IState
    {
        public void InitState(BlackBoard blackBoard);
        public StateExeResult ExeState();
    }

    public abstract class BlackBoard
    {
    }

    public class ChampionBlackBoard : BlackBoard
    {
    }

    public enum StateExeResult
    {
        Ing,
        Done,
        Fail
    }

    public class DamageAddHpState : IState
    {
        public void InitState(BlackBoard blackBoard)
        {
        }
        public StateExeResult ExeState()
        {
            return StateExeResult.Done;
        }
    }
}
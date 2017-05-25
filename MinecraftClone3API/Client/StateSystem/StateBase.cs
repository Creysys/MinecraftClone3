namespace MinecraftClone3API.Client.StateSystem
{
    public abstract class StateBase
    {
        public bool IsDead = false;

        public abstract void Update();
        public abstract void Render();
        public virtual void Exit() { }
    }
}

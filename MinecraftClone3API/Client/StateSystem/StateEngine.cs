using MinecraftClone3API.Util;
using System.Collections.Generic;
using System.Linq;

namespace MinecraftClone3API.Client.StateSystem
{
    public static class StateEngine
    {
        private static List<StateBase> _states = new List<StateBase>();
        private static List<StateBase> _overlays = new List<StateBase>();

        public static void Update()
        {
            var overlaysToRemove = new List<StateBase>();
            _overlays.ReverseForEach(o =>
            {
                o.Update();
                if (o.IsDead) overlaysToRemove.Add(o);
            });
            overlaysToRemove.ForEach(o => _overlays.Remove(o));

            var last = _states.LastOrDefault();
            if(last != null)
            {
                if (last.IsDead)
                {
                    _states.RemoveAt(_states.Count - 1);
                    last.Exit();
                }
                else last.Update();
            }
        }

        public static void Render()
        {
            _overlays.ReverseForEach(s => s.Render());
            _states.LastOrDefault()?.Render();
        }

        public static void Exit()
        {
            _overlays.ReverseForEach(o => o.Exit());
            _states.ReverseForEach(s => s.Exit());
        }

        public static void AddState(StateBase state) => _states.Add(state);
        public static void AddOverlay(StateBase overlay) => _overlays.Add(overlay);
    }
}

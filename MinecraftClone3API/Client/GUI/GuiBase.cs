using System.Collections.Generic;
using MinecraftClone3API.Client.StateSystem;

namespace MinecraftClone3API.Client.GUI
{
    public abstract class GuiBase : StateBase
    {
        protected List<GuiElementBase> Elements = new List<GuiElementBase>();

        public override void Update() => Elements.ForEach(e => e.Update());

        public override void Render() => Elements.ForEach(e => e.Render());
    }
}

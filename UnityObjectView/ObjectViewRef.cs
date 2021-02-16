namespace MelonECS.UnityObjectView
{
    public struct ObjectViewRef : IComponent
    {
        public ObjectView view;

        public ObjectViewRef(ObjectView view)
            => this.view = view;
    }
}
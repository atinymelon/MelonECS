namespace MelonECS.UnityObjectView
{
    public struct ObjectViewRef : IComponent
    {
        public int id;

        public ObjectViewRef(int id)
            => this.id = id;

        public ObjectViewRef(ObjectView view)
            => this.id = view.Id;
    }
}
using System.Collections.Generic;
using System.Data;

namespace MelonECS
{
	public class SystemGroup
	{
		private readonly World world;
		private readonly List<System> systems = new List<System>();
		
		public SystemGroup(World world)
		{
			this.world = world;
		}

		public void Run()
		{
			for (int i = 0; i < systems.Count; i++)
			{
				systems[i].Run();
			}
		}

		public void RegisterSystem<T>() where T : System, new()
		{
			var system = new T();
			system.AttachWorld(world);
			systems.Add(system);
		}
	}
}
using System;

namespace ScrollsModLoader.Interfaces
{
	public interface ModAPI
	{
		//GUI Stuff
		bool AddScene (String desc, SceneProvider provider);
		void LoadScene (String providerDesc);
	}
}
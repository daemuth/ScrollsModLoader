using System;
using System.Reflection;
using System.Collections.Generic;
using Mono.Cecil;
using LinFu.AOP.Interfaces;
using UnityEngine;
using ScrollsModLoader.Interfaces;

namespace ScrollsModLoader
{
	public class PatchModsMenu : Patch, SceneProvider, IListCallback
	{
		public PatchModsMenu(TypeDefinitionCollection types) : base (types) {

			//get mod list


			//add mods
			repositories.Add (new Repo("ScrollsGuide", "http://Scrollsguide.com/repo"));

			downloadableMods.Add (new Mod("GameRecorder", "Record Games!", "v1.0"));
			downloadableMods.Add (new Mod("DeckSync", "Export/Import Decks!", "v1.0"));
			for (int i=0; i < 30; i++)
				downloadableMods.Add (new Mod("TestMod", "Fill that List!", "v2.0!"));

			installedMods.Add(new Mod("Logger", "Debug Network logging", "v0.1"));

			//downloadableMods.Add (new Mod("DeckSync"));

		}

		private bool first = true;
		private int defaultTextSize;
		private Texture2D text;
		private ModAPI modAPI;
		private List<Item> repositories = new List<Item>();
		private List<Item> downloadableMods = new List<Item>();
		private List<Item> installedMods = new List<Item>();
		private UIListPopup repoListPopup;
		private UIListPopup downloadableListPopup;
		private UIListPopup modListPopup;// = new CardListPopup();
		/*
		private GUISkin tradeSkin;
		private GUISkin tradeSkinClose;
		private GUISkin lobbySkin;
		*/
		public override MethodDefinition[] patchedMethods() {
			MethodDefinition DrawHeaderButtons = Hooks.getMethDef (Hooks.getTypeDef (assembly, "LobbyMenu"), "drawHeaderButtons");
			return new MethodDefinition[] {DrawHeaderButtons};
		}

		public override object Intercept (IInvocationInfo info)
		{
			//typeof(LobbyMenu).GetField ("_hoverButtonInside", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).SetValue (info.Target, false);

			/*if (typeof(LobbyMenu).GetField ("_sceneToLoad", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue (info.Target).Equals("_Settings")) {
				typeof(LobbyMenu).GetField ("_sceneToLoad", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).SetValue (info.Target, "_SomethingElse");
			}*/

			try {
				Type lobbyMenu = typeof(LobbyMenu);

				if (first) {

					GUISkin gUISkin7 = ScriptableObject.CreateInstance<GUISkin> ();

					text = new Texture2D(87, 39); //115, 39
					text.LoadImage(System.Reflection.Assembly.GetExecutingAssembly ().GetManifestResourceStream ("ScrollsModLoader.Mods.png").ReadToEnd ());

					gUISkin7.button.normal.background = text;
					gUISkin7.button.hover.background = text;
					gUISkin7.button.active.background = text;

					defaultTextSize = GUI.skin.label.fontSize;

					//info.Target.GUISkins.Add (gUISkin5);
					FieldInfo GUISkins = lobbyMenu.GetField("GUISkins", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);
					((List<GUISkin>)(GUISkins.GetValue(info.Target))).Add(gUISkin7);
					first = false;
				}

				MethodInfo drawHeader = lobbyMenu.GetMethod ("drawHeaderButton", BindingFlags.NonPublic | BindingFlags.Instance);
				drawHeader.Invoke(info.Target, new object[] {6, "_Mods"});

			} catch (Exception exp) {
				Console.WriteLine (exp);
			}	
			return info.TargetMethod.Invoke(info.Target, info.Arguments);
		}

		public void Initialize(ModAPI api) {
			this.modAPI = api;
			this.modAPI.AddScene ("_Mods", this);
		}

		public string SceneName ()
		{
			return "_Mods";
		}
		public void OnCreate (MonoBehaviour parentScene)
		{
			//null, null, true, true, false, false, ResourceManager.LoadTexture ("ChatUI/buttonicon_add"), true);
			//tradeSkin = (GUISkin)Resources.Load ("_GUISkins/TradeSystem");
			//tradeSkinClose = (GUISkin)Resources.Load ("_GUISkins/TradeSystemCloseButton");
			//lobbySkin = (GUISkin)Resources.Load ("_GUISkins/Lobby");

			App.ChatUI.Show (false);
			repoListPopup = new GameObject ("Repo List").AddComponent<UIListPopup> ();
			repoListPopup.transform.parent = parentScene.transform;
			repoListPopup.Init (new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f+40.0f, (float)Screen.width/4.5f, (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f), false, true, repositories, this, null, null, true, true, true, true, null, true, false);
			repoListPopup.enabled = true;
			repoListPopup.SetOpacity(1f);

			downloadableListPopup = new GameObject ("Downloadable List").AddComponent<UIListPopup> ();
			downloadableListPopup.transform.parent = parentScene.transform;
			downloadableListPopup.Init (new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f*1.5f+(float)Screen.width/4.5f, (float)Screen.height/5.0f+(float)Screen.height/30.0f+40.0f, (float)Screen.width/4.1f, (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f), false, true, downloadableMods, this, null, null, true, true, true, true, null, true, false);
			downloadableListPopup.enabled = true;
			downloadableListPopup.SetOpacity(1f);

			modListPopup = new GameObject ("Mod List").AddComponent<UIListPopup> ();
			modListPopup.transform.parent = parentScene.transform;
			modListPopup.Init (new Rect((float)Screen.width/15.0f*9.5f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f+40.0f, (float)Screen.width/15.0f*4.5f-(float)Screen.width/35.0f*2.0f, (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f), false, true, installedMods, this, null, null, true, true, true, true, null, true, true);
			modListPopup.enabled = true;
			modListPopup.SetOpacity(1f);
		}
		public void OnGUI ()
		{
			GUI.DrawTexture (new Rect (0f, 0f, (float)Screen.width, (float)Screen.height), ResourceManager.LoadTexture ("DeckBuilder/bg"));
			new ScrollsFrame (new Rect((float)Screen.width/15.0f, (float)Screen.height/5.0f, (float)Screen.width/15.0f*8.0f, (float)Screen.height/6.0f*4.0f)).AddNinePatch (ScrollsFrame.Border.LIGHT_CURVED, NinePatch.Patches.CENTER).Draw();
			new ScrollsFrame (new Rect((float)Screen.width/15.0f*9.5f, (float)Screen.height/5.0f, (float)Screen.width/15.0f*4.5f, (float)Screen.height/6.0f*4.0f)).AddNinePatch (ScrollsFrame.Border.LIGHT_CURVED, NinePatch.Patches.CENTER).Draw();

			GUI.skin.label.fontSize = defaultTextSize;
			Color textColor = GUI.skin.label.normal.textColor;
			GUI.skin.label.normal.textColor = Color.white;

			GUI.skin.label.fontSize = defaultTextSize;
			GUI.Label (new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f + (float)Screen.width/4.5f/2.0f - (float)Screen.width/8.0f/2.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f, (float)Screen.width/8.0f, 35.0f), "Repositories");
			GUI.Label (new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f*1.5f+(float)Screen.width/4.5f + (float)Screen.width/4.1f/2.0f - (float)Screen.width/8.0f/2.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f, (float)Screen.width/8.0f, 35.0f), "Downloadable Mods");
			GUI.Label (new Rect((float)Screen.width/15.0f*9.5f+(float)Screen.width/35.0f+((float)Screen.width/15.0f*4.5f-(float)Screen.width/35.0f*2.0f)/2.0f - (float)Screen.width/8.0f/2.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f, (float)Screen.width/8.0f, 35.0f), "Installed Mods");

			GUI.skin.label.fontSize = 20;
			GUI.Button(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/9.0f-1.0f, (float)Screen.width/35.0f), string.Empty);
			GUI.Label(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/9.0f-1.0f, (float)Screen.width/35.0f), "Add Repository");

			GUI.Button(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f + (float)Screen.width/9.0f + 1.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/9.0f-1.0f, (float)Screen.width/35.0f), string.Empty);
			GUI.Label(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f + (float)Screen.width/9.0f + 1.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/9.0f-1.0f, (float)Screen.width/35.0f), "Remove Repository");

			GUI.Button(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f*1.5f+(float)Screen.width/4.5f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/4.1f, (float)Screen.width/35.0f), string.Empty);
			GUI.Label(new Rect((float)Screen.width/15.0f+(float)Screen.width/35.0f*1.5f+(float)Screen.width/4.5f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/4.1f, (float)Screen.width/35.0f), "Apply Changes");

			GUI.Button(new Rect((float)Screen.width/15.0f*9.5f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/15.0f*4.5f-(float)Screen.width/35.0f*2.0f, (float)Screen.width/35.0f), string.Empty);
			GUI.Label(new Rect((float)Screen.width/15.0f*9.5f+(float)Screen.width/35.0f, (float)Screen.height/5.0f+(float)Screen.height/30.0f + (float)Screen.height/6.0f*4.0f-(float)Screen.height/15.0f-80.0f, (float)Screen.width/15.0f*4.5f-(float)Screen.width/35.0f*2.0f, (float)Screen.width/35.0f), "Apply (requires Restart)");

			GUI.skin.label.normal.textColor = textColor;
			GUI.skin.label.fontSize = defaultTextSize;
		}
		public void OnDestroy ()
		{
		}
		public GUISkin Skin ()
		{
			return null;
		}
		public GUISkin UISkin ()
		{
			return null;
		}
		public GUIStyle ButtonStyle ()
		{
			return null;
		}
		public GUIStyle ActiveButtonStyle ()
		{
			return null;
		}

		public void ButtonClicked (UIListPopup popup, ECardListButton button) {}
		public void ButtonClicked (UIListPopup popup, ECardListButton button, List<Item> selectedCards) {}
		public void ItemButtonClicked (UIListPopup popup, Item card) {}
		public void ItemClicked (UIListPopup popup, Item card) {}
		public void ItemHovered (UIListPopup popup, Item card) {}
	}

	//recycle the CardListPopup API

	public class Repo : Item {

		private String name;
		private String url;

		public Repo (String name, String url) {
			this.name = name;
			this.url = url;
		}

		public bool selectable ()
		{
			return true;
		}
		public Texture getImage ()
		{
			return null;
		}
		public string getName ()
		{
			return name;
		}
		public string getDesc ()
		{
			return url;
		}
	}

	public class Mod : Item {

		private String name;
		private String desc;
		private String version;

		public Mod (String name, String desc, String version) {
			this.name = name;
			this.desc = desc;
			this.version = version;
		}
	
		public bool selectable ()
		{
			return true;
		}
		public Texture getImage ()
		{
			return null;
		}
		public string getName ()
		{
			return name;
		}
		public string getDesc ()
		{
			return desc+", "+version;
		}
	}
}

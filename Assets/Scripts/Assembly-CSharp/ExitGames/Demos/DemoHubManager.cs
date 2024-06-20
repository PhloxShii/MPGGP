using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ExitGames.Demos
{
	public class DemoHubManager : MonoBehaviour
	{
		private struct DemoData
		{
			public string Title;

			public string Description;

			public string Scene;

			public string TutorialLink;

			public string DocLink;
		}

		public Text TitleText;

		public Text DescriptionText;

		public GameObject OpenSceneButton;

		public GameObject OpenTutorialLinkButton;

		public GameObject OpenDocLinkButton;

		private string MainDemoWebLink = "http://bit.ly/2f8OFu8";

		private Dictionary<string, DemoData> _data = new Dictionary<string, DemoData>();

		private string currentSelection;

		private Rect BugFixbounds = new Rect(0f, 0f, 0f, 0f);

		private void Awake()
		{
			OpenSceneButton.SetActive(value: false);
			OpenTutorialLinkButton.SetActive(value: false);
			OpenDocLinkButton.SetActive(value: false);
			_data.Add("DemoBoxes", new DemoData
			{
				Title = "Demo Boxes",
				Description = "Uses ConnectAndJoinRandom script.\n(joins a random room or creates one)\n\nInstantiates simple prefabs.\nSynchronizes positions without smoothing.\nShows that RPCs target a specific object.",
				Scene = "DemoBoxes-Scene"
			});
			_data.Add("DemoWorker", new DemoData
			{
				Title = "Demo Worker",
				Description = "Joins the default lobby and shows existing rooms.\nLets you create or join a room.\nInstantiates an animated character.\nSynchronizes position and animation state of character with smoothing.\nImplements simple in-room Chat via RPC calls.",
				Scene = "DemoWorker-Scene"
			});
			_data.Add("MovementSmoothing", new DemoData
			{
				Title = "Movement Smoothing",
				Description = "Uses ConnectAndJoinRandom script.\nShows several basic ways to synchronize positions between controlling client and remote ones.\nThe TransformView is a good default to use.",
				Scene = "DemoSynchronization-Scene"
			});
			_data.Add("BasicTutorial", new DemoData
			{
				Title = "Basic Tutorial",
				Description = "All custom code for connection, player and scene management.\nAuto synchronization of room levels.\nUses PhotonAnimatoView for Animator synch.\nNew Unity UI all around, for Menus and player health HUD.\nFull step by step tutorial available online.",
				Scene = "PunBasics-Launcher",
				TutorialLink = "http://j.mp/2dibZIM"
			});
			_data.Add("OwnershipTransfer", new DemoData
			{
				Title = "Ownership Transfer",
				Description = "Shows how to transfer the ownership of a PhotonView.\nThe owner will send position updates of the GameObject.\nTransfer can be edited per PhotonView and set to Fixed (no transfer), Request (owner has to agree) or Takeover (owner can't object).",
				Scene = "DemoChangeOwner-Scene"
			});
			_data.Add("PickupTeamsScores", new DemoData
			{
				Title = "Pickup, Teams, Scores",
				Description = "Uses ConnectAndJoinRandom script.\nImplements item pickup with RPCs.\nUses Custom Properties for Teams.\nCounts score per player and team.\nUses PhotonPlayer extension methods for easy Custom Property access.",
				Scene = "DemoPickup-Scene"
			});
			_data.Add("Chat", new DemoData
			{
				Title = "Chat",
				Description = "Uses the Chat API (now part of PUN).\nSimple UI.\nYou can enter any User ID.\nAutomatically subscribes some channels.\nAllows simple commands via text.\n\nRequires configuration of Chat App ID in scene.",
				Scene = "DemoChat-Scene",
				DocLink = "http://j.mp/2iwQkPJ"
			});
			_data.Add("RPGMovement", new DemoData
			{
				Title = "RPG Movement",
				Description = "Demonstrates how to use the PhotonTransformView component to synchronize position updates smoothly using inter- and extrapolation.\n\nThis demo also shows how to setup a Mecanim Animator to update animations automatically based on received position updates (without sending explicit animation updates).",
				Scene = "DemoRPGMovement-Scene"
			});
			_data.Add("MecanimAnimations", new DemoData
			{
				Title = "Mecanim Animations",
				Description = "This demo shows how to use the PhotonAnimatorView component to easily synchronize Mecanim animations.\n\nIt also demonstrates another feature of the PhotonTransformView component which gives you more control how position updates are inter-/extrapolated by telling the component how fast the object moves and turns using SetSynchronizedValues().",
				Scene = "DemoMecanim-Scene"
			});
			_data.Add("2DGame", new DemoData
			{
				Title = "2D Game Demo",
				Description = "Synchronizes animations, positions and physics in a 2D scene.",
				Scene = "Demo2DJumpAndRunWithPhysics-Scene"
			});
			_data.Add("FriendsAndAuth", new DemoData
			{
				Title = "Friends & Authentication",
				Description = "Shows connect with or without (server-side) authentication.\n\nAuthentication requires minor server-side setup (in Dashboard).\n\nOnce connected, you can find (made up) friends.\nJoin a room just to see how that gets visible in friends list.",
				Scene = "DemoFriends-Scene"
			});
			_data.Add("TurnBasedGame", new DemoData
			{
				Title = "'Rock Paper Scissor' Turn Based Game",
				Description = "Demonstrate TurnBased Game Mechanics using PUN.\n\nIt makes use of the TurnBasedManager Utility Script",
				Scene = "DemoRPS-Scene"
			});
		}

		public void SelectDemo(string Reference)
		{
			currentSelection = Reference;
			TitleText.text = _data[currentSelection].Title;
			DescriptionText.text = _data[currentSelection].Description;
			OpenSceneButton.SetActive(!string.IsNullOrEmpty(_data[currentSelection].Scene));
			OpenTutorialLinkButton.SetActive(!string.IsNullOrEmpty(_data[currentSelection].TutorialLink));
			OpenDocLinkButton.SetActive(!string.IsNullOrEmpty(_data[currentSelection].DocLink));
		}

		public void OpenScene()
		{
			if (string.IsNullOrEmpty(currentSelection))
			{
				Debug.LogError("Bad setup, a CurrentSelection is expected at this point");
			}
			else
			{
				SceneManager.LoadScene(_data[currentSelection].Scene);
			}
		}

		public void OpenTutorialLink()
		{
			if (string.IsNullOrEmpty(currentSelection))
			{
				Debug.LogError("Bad setup, a CurrentSelection is expected at this point");
			}
			else
			{
				Application.OpenURL(_data[currentSelection].TutorialLink);
			}
		}

		public void OpenDocLink()
		{
			if (string.IsNullOrEmpty(currentSelection))
			{
				Debug.LogError("Bad setup, a CurrentSelection is expected at this point");
			}
			else
			{
				Application.OpenURL(_data[currentSelection].DocLink);
			}
		}

		public void OpenMainWebLink()
		{
			Application.OpenURL(MainDemoWebLink);
		}

		private void OnGUI()
		{
			GUI.SetNextControlName(base.gameObject.GetHashCode().ToString());
			GUI.TextField(BugFixbounds, string.Empty, 0);
		}
	}
}

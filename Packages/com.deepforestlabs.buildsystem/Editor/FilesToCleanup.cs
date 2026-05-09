#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace DeepForestLabs.BuildSystems
{
	public class FilesToCleanup : ScriptableObject
	{
		public const string ASSET = "Assets/Scripts/BuildSystem/Editor/FilesToCleanup.asset";

		[SerializeField]
		private List<string> _filesToBackupAndRestore = new List<string>();

		[SerializeField]
		private List<string> _filesToDelete = new List<string>();

		public List<string> FilesToBackupAndRestore
		{
			get { return _filesToBackupAndRestore; }
		}

		public List<string> FilesToDelete
		{
			get { return _filesToDelete; }
		}
	}
}
#nullable disable
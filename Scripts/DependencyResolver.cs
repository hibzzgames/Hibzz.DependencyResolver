using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hibzz.DependencyResolver
{
	[UnityEditor.InitializeOnLoad]
    public class DependencyResolver : IPackageManagerExtension
    {
        static DependencyResolver()
        {
			PackageManagerExtensions.RegisterExtension(new DependencyResolver());
        }

		public VisualElement CreateExtensionUI()
		{
			return null;
		}

		public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
		{
			string package_json_path = $"{packageInfo.resolvedPath}/package.json";
			string package_json_content =  File.ReadAllText(package_json_path);
			JObject.Parse(package_json_content);
		}

		public void OnPackageRemoved(PackageInfo packageInfo) { }

		public void OnPackageSelectionChange(PackageInfo packageInfo) { }
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Hibzz.DependencyResolver
{
    [InitializeOnLoad]
    public class DependencyResolver : IPackageManagerExtension
    {
        static DependencyResolver()
        {
            PackageManagerExtensions.RegisterExtension(new DependencyResolver());
        }

        public VisualElement CreateExtensionUI() { return null; }

        public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
        {
            // get a list of git dependencies (as configured by the package developer)
            if(!RequestGitDependencies(packageInfo, out var dependencies)) { return; }

			// get a list of installed packages
			var installed_packages = PackageInfo.GetAllRegisteredPackages().ToList();
            
            // remove all dependencies from the list that has already been installed
			dependencies.RemoveAll((dependency) => IsInCollection(dependency, installed_packages));

            // install the dependencies
            InstallDependencies(dependencies, mainPackageName: packageInfo.name);
		}

        /// <summary>
        /// Request a list of git dependencies in the package
        /// </summary>
        /// <param name="packageInfo">The package to get the git dependencies from</param>
        /// <param name="dependencies">The retrieved list of git dependencies </param>
        /// <returns>Was the request successful?</returns>
        bool RequestGitDependencies(PackageInfo packageInfo, out List<string> dependencies)
        {
			// Read the contents of the package.json file
			string package_json_path = $"{packageInfo.resolvedPath}/package.json";
			string package_json_content = File.ReadAllText(package_json_path);

			// parse the json and read any list of git-dependencies
			var package_content = JObject.Parse(package_json_content);
			var git_dependencies_token = package_content["git-dependencies"];

			// if no token with the key git-dependecies is found, failed to get git dependencies
			if (git_dependencies_token is null) 
            {
                dependencies = null;
                return false; 
            }

            // convert the git dependency token to a list of strings...
            // maybe we should check for errors in this process? what if git-dependency isn't array of string?
            dependencies = Array.ConvertAll(git_dependencies_token.ToArray(), item => item.ToString()).ToList();
            return true;
		}

        /// <summary>
        /// Is the given dependency url found in the given collection
        /// </summary>
        /// <param name="dependency">The url the dependency to check for</param>
        /// <param name="collection">The collection to look through</param>
        /// <returns></returns>
        bool IsInCollection(string dependency, List<PackageInfo> collection)
        {
            // when package collection given is null, it's inferred that the dependency is not in the collection
            if(collection == null) { return false; }

            // check if any of the installed package has the dependency
            foreach(var package in collection)
            {
				// the package id for a package installed with git is `package_name@package_giturl`
				// get the repo url by performing some string manipulation on the package id
				string repourl = package.packageId.Substring(package.packageId.IndexOf('@') + 1);

				// Found!
				if (repourl == dependency) { return true; }
            }

            // the dependency wasn't found in the package collection
            return false; 
        }

        /// <summary>
        /// Install all the given dependencies
        /// </summary>
        /// <param name="dependencies">A list of dependencies to install</param>
        void InstallDependencies(List<string> dependencies, string mainPackageName)
        {
			// there are no dependencies to install, skip
			if (dependencies.Count <= 0) { return; }

			// before installing the packages, make sure that user knows what the dependencies to install are
			if (!EditorUtility.DisplayDialog($"{mainPackageName} requires additional dependencies",
				$"The following dependencies are required: \n\n{GetPrintFriendlyName(dependencies)}",
				"Install Dependencies", "Cancel"))
			{
				// user decided to cancel the installation of the dependencies...
				return;
			}

            // the user pressed install, perform the actual installation
            Client.AddAndRemove(dependencies.ToArray(), null);
		}

        /// <summary>
        /// Get a print friendly name of all dependencies to show in the dialog box
        /// </summary>
        /// <param name="dependencies">The list of dependencies to parse through</param>
        /// <returns>A print friendly string representing all the dependencies</returns>
        string GetPrintFriendlyName(List<string> dependencies) 
        {
            // ideally, we want the package name, but that requires downloading the package.json and parsing through
            // it, which is kinda too much... i could ask for the users to give a package name along with the url in
            // package.json, but again too complicated just for a dialog message... username/repo will do fine for now

            string result = string.Join("\n", dependencies);    // concatenate dependencies on a new line
            result = result.Replace(".git", "");                // remove .git from the urls
            result = result.Replace("https://github.com/", ""); // remove github link such that we only show "username/repo"

            return result;
		}

        public void OnPackageRemoved(PackageInfo packageInfo) { }

        public void OnPackageSelectionChange(PackageInfo packageInfo) { }
    }
}

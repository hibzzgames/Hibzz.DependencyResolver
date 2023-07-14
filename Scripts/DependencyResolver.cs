using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Newtonsoft.Json.Linq;

using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using UnityEditor.PackageManager.Requests;

namespace Hibzz.DependencyResolver
{
    [InitializeOnLoad]
    public class DependencyResolver
    {
        static AddAndRemoveRequest packageInstallationRequest;

        // called by the attribute [InitializeOnLoad]
        static DependencyResolver()
        {
            Events.registeredPackages += OnPackagesRegistered;
        }

        // Invoked when the package manager completes registering new packages
        static void OnPackagesRegistered(PackageRegistrationEventArgs packageRegistrationInfo)
        {
            // stores all the dependencies that needs to be installed in this step
            List<string> dependencies = new List<string>();

            // loop through all of the added packages and get their git
            // dependencies and add it to the list that contains all the
            // dependencies that need to be installed
            foreach (var package in packageRegistrationInfo.added)
            {
                // get the dependencies of the added package
                if (!GetDependencies(package, out var package_dependencies)) { continue; }

                // add it to the total list of dependencies
                dependencies.AddRange(package_dependencies);
            }

            // remove any duplicates
            dependencies = dependencies.Distinct().ToList();

            // remove any dependencies that's already installed
            var installed_packages = PackageInfo.GetAllRegisteredPackages().ToList();
            dependencies.RemoveAll((dependency) => IsInCollection(dependency, installed_packages));

            // Install the dependencies
            InstallDependencies(dependencies);
        }

        /// <summary>
        /// Request a list of git dependencies in the package
        /// </summary>
        /// <param name="packageInfo">The package to get the git dependencies from</param>
        /// <param name="dependencies">The retrieved list of git dependencies </param>
        /// <returns>Was the request successful?</returns>
        static bool GetDependencies(PackageInfo packageInfo, out List<string> dependencies)
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
        static bool IsInCollection(string dependency, List<PackageInfo> collection)
        {
            // when package collection given is null, it's inferred that the dependency is not in the collection
            if (collection == null) { return false; }

            // check if any of the installed package has the dependency
            foreach (var package in collection)
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
        static void InstallDependencies(List<string> dependencies)
        {
            // there are no dependencies to install, skip
            if (dependencies == null || dependencies.Count <= 0) { return; }

            // before installing the packages, make sure that user knows what
            // the dependencies to install are... additionally, check if the
            // application is being run on batch mode so that we can skip the
            // installation dialog
            if (!Application.isBatchMode &&
                !EditorUtility.DisplayDialog(
                    $"Dependency Resolver",
                    $"The following dependencies are required: \n\n{GetPrintFriendlyName(dependencies)}",
                    "Install Dependencies",
                    "Cancel"))
            {
                // user decided to cancel the installation of the dependencies...
                return;
            }

            // the user pressed install, perform the actual installation
            // (or the application was in batch mode)
            packageInstallationRequest = Client.AddAndRemove(dependencies.ToArray(), null);

            // show the progress bar till the installation is complete
            EditorUtility.DisplayProgressBar("Dependency Resolver", "Preparing installation of dependencies...", 0);
            EditorApplication.update += DisplayProgress;
        }

        /// <summary>
        /// Get a print friendly name of all dependencies to show in the dialog box
        /// </summary>
        /// <param name="dependencies">The list of dependencies to parse through</param>
        /// <returns>A print friendly string representing all the dependencies</returns>
        static string GetPrintFriendlyName(List<string> dependencies)
        {
            // ideally, we want the package name, but that requires downloading the package.json and parsing through
            // it, which is kinda too much... i could ask for the users to give a package name along with the url in
            // package.json, but again too complicated just for a dialog message... username/repo will do fine for now

            string result = string.Join("\n", dependencies);    // concatenate dependencies on a new line
            result = result.Replace(".git", "");                // remove .git from the urls
            result = result.Replace("https://github.com/", ""); // remove github link such that we only show "username/repo"

            return result;
        }

        /// <summary>
        /// Shows a progress bar till the AddAndRemoveRequest is completed
        /// </summary>
        static void DisplayProgress()
        {
            if(packageInstallationRequest.IsCompleted)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update -= DisplayProgress;
            }
        }
    }
}

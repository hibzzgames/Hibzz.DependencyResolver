# Hibzz.DependencyResolver
![LICENSE](https://img.shields.io/badge/LICENSE-CC--BY--4.0-ee5b32?style=for-the-badge) [![Twitter Follow](https://img.shields.io/badge/follow-%40hibzzgames-1DA1f2?logo=twitter&style=for-the-badge)](https://twitter.com/hibzzgames) [![Discord](https://img.shields.io/discord/695898694083412048?color=788bd9&label=DIscord&style=for-the-badge)](https://discord.gg/YXdJ8cZngB) ![Unity](https://img.shields.io/badge/unity-%23000000.svg?style=for-the-badge&logo=unity&logoColor=white) ![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)

***A tool used to resolve git dependencies in packages installed with Unity's Package Manager***

## Installation
**Via Github**
This package can be installed in the Unity Package Manager using the following git URL.
```
https://github.com/hibzzgames/Hibzz.DependencyResolver.git
```

Alternatively, you can download the latest release from the [releases page](https://github.com/hibzzgames/Hibzz.DependencyResolver/releases) and manually import the package into your project.

<br>

## Usage
The main issue this tool attempts to solve is the inability to define git repositories as dependencies in packages installed with Unity's Package Manager. This is one of the most requested features for the package manager from the community, but it is not currently supported. Check out this [forum post](https://forum.unity.com/threads/custom-package-with-git-dependencies.628390/) from *2019* for more information.

**For Users:**
Make sure to install this package before installing any packages that have git dependencies. If there are any git dependencies in packages that are already installed, simply press update on the package and the dependencies will be resolved.

The tool is smart enough to only install new git dependencies if they are not already installed. When a dependency needs to be installed, the tool will prompt the user to confirm the installation of the dependency. This is to prevent the user from accidentally installing a malicious package.


**For Package Developers:**

To add a git dependency to a package, you can add a new entry to the `git-dependencies` field in the `package.json` file of the package. The format of the entry is as follows:
```json
"git-dependencies": [
    "https://github.com/hibzzgames/Hibzz.Singletons.git",
    "https://github.com/hibzzgames/Hibzz.Hibernator.git"
  ]
```

Hopefully, Unity will add native support for this feature in the future and make this tool redundant, but until then, this tool can be used to resolve Git dependencies. 

<br>

## Have a question or want to contribute?
If you have any questions or want to contribute, feel free to join the [Discord server](https://discord.gg/YXdJ8cZngB) or [Twitter](https://twitter.com/hibzzgames). I'm always looking for feedback and ways to improve this tool. Thanks!

Additionally, you can support the development of these open-source projects via [GitHub Sponsors](https://github.com/sponsors/sliptrixx) and gain early access to the projects.


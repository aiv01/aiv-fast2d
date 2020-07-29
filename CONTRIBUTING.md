# Reporting an Issue
To ask for support, enhancement, new feature or to state a bug open a dedicated issue as detailed as possible.

# Working an Issue
To Contribute to an issue:
- if you are a **Contributor**, just Assign yourself the issue. A branch will be automatically created with the pattern issue-N-title. Make sure when pushing that your branch will pass Continuos Integration.
- if you are an external contributor, fork this repository to your account following github flux.

When you are ready to submit your work, open a **Pull Request** (in case of fork follow [this](https://help.github.com/articles/creating-a-pull-request-from-a-fork/)) declaring in the description which issue will close (like: "Closes #11"). 
Minumum requirement, for the Pull Request, to be accepted is to pass Continuous Integration check.

# Releasing new version
Release process can be executed by Maintainers:
1. [Github] Merge on "master" branch. Better with a Pull Request.
1. [Github] Draft New Release named "X.Y.Z". NOTE: This will trigger travis automation
1. [Travis] Wait for travis. In the end:
  * package is published on Nuget
  * api docs is published on Github Pages
  * artifacts are uploaded to the Github Release

# License
Any kind of contribution to this project will be subject to [GNU](LICENSE) license.

# Code of Conduct
Remember: any actions taken on this project must respect Code of Conduct defined [here](CODE_OF_CONDUCT.md).
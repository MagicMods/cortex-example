# WhatScript
WhatScript is a Unity editor tool that will identify the origin of missing scripts in your project.

Simply look in the Unity inspector, where Unity used to tell you simply that a script is missing, it will now
also include the filename of the missing script.

# Getting Started
Simply install the WhatScript package.

WhatScript will automatically track the scripts in your project. If one goes missing (_e.g._ you delete a script file
from your project), WhatScript will identify the missing script showing you the previous path for the script.

If you'd like to identify a script that has gone missing before you've installed WhatScript, you can use the
built in Git search utility to identify the script (and the commit where it was last seen.)

# How to Use
1. Select the game object that has the missing script
2. In the Unity inspector, the missing script will now display the filename of the previous script.
   1. Optionally, if you'd like to identify the last known Git commit that contained the script,
   you can click the `Find in Git History` button.
3. If the script cannot be identified (because it was deleted before installing WhatScript),
a `Search Git` button will be displayed. Clicking this button will search the Git history for the last commit
that contained the script.

# Settings
WhatScript has a few settings that can be configured in the Unity editor via the `Project Settings` tab in the
`WhatScript` section.

1. **Override Git Path**: Manually specify the full path of your Git client
2. **Library Location**: Choose where the internal database of scripts is stored
   1. **Project Settings (default)**: `<Project Root>/ProjectSettings/script_library.txt`
   2. **Library**: `<Project Root>/Library/script_library.txt`
   3. **Project Root**: `<Project Root>/script_library.txt`
3. **Auto Track Scripts**: If disabled, WhatScript will no longer automatically track scripts in your project when they
are imported or moved. You may still use Git search to identify scripts. This is useful if you have a large project and
feel that WhatScript is slowing down your import times.
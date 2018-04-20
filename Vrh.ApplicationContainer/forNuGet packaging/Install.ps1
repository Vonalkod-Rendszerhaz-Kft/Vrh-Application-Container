param($installPath, $toolsPath, $package, $project)

$configItem = $project.ProjectItems.Item("ApplicationContainer.Config.xml")
# set 'Copy To Output Directory' to 'Copy if newer'
$copyToOutput = $configItem.Properties.Item("CopyToOutputDirectory")
$copyToOutput.Value = 2
# set 'Build Action' to 'None'
$buildAction = $configItem.Properties.Item("BuildAction")
$buildAction.Value = 0

$configItem2 = $project.ProjectItems.Item("Plugins.Config.xml")
# set 'Copy To Output Directory' to 'Copy if newer'
$copyToOutput = $configItem2.Properties.Item("CopyToOutputDirectory")
$copyToOutput.Value = 2
# set 'Build Action' to 'None'
$buildAction = $configItem2.Properties.Item("BuildAction")
$buildAction.Value = 0

$configItem2 = $project.ProjectItems.Item("Vrh.ApplicationContainer.ConsoleHost.exe.config")
# set 'Copy To Output Directory' to 'Copy if newer'
$copyToOutput = $configItem2.Properties.Item("CopyToOutputDirectory")
$copyToOutput.Value = 2
# set 'Build Action' to 'None'
$buildAction = $configItem2.Properties.Item("BuildAction")
$buildAction.Value = 0

$configItem2 = $project.ProjectItems.Item("Vrh.ApplicationContainer.WindowsServiceHost.exe.config")
# set 'Copy To Output Directory' to 'Copy if newer'
$copyToOutput = $configItem2.Properties.Item("CopyToOutputDirectory")
$copyToOutput.Value = 2
# set 'Build Action' to 'None'
$buildAction = $configItem2.Properties.Item("BuildAction")
$buildAction.Value = 0
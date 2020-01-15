param($installPath, $toolsPath, $package, $project)

$configItem = $project.ProjectItems.Item("ApplicationContainer.Config.xml")
# set 'Copy To Output Directory' to 'Copy always'
$copyToOutput = $configItem.Properties.Item("CopyToOutputDirectory")
$copyToOutput.Value = 1
# set 'Build Action' to 'None'
$buildAction = $configItem.Properties.Item("BuildAction")
$buildAction.Value = 0

$configItem2 = $project.ProjectItems.Item("Plugins.Config.xml")
# set 'Copy To Output Directory' to 'Copy always'
$copyToOutput = $configItem2.Properties.Item("CopyToOutputDirectory")
$copyToOutput.Value = 1
# set 'Build Action' to 'None'
$buildAction = $configItem2.Properties.Item("BuildAction")
$buildAction.Value = 0

$configItem2 = $project.ProjectItems.Item("Vrh.ApplicationContainer.ConsoleHost.exe.config")
# set 'Copy To Output Directory' to 'Copy always'
$copyToOutput = $configItem2.Properties.Item("CopyToOutputDirectory")
$copyToOutput.Value = 1
# set 'Build Action' to 'None'
$buildAction = $configItem2.Properties.Item("BuildAction")
$buildAction.Value = 0

$configItem2 = $project.ProjectItems.Item("Vrh.ApplicationContainer.WindowsServiceHost.exe.config")
# set 'Copy To Output Directory' to 'Copy always'
$copyToOutput = $configItem2.Properties.Item("CopyToOutputDirectory")
$copyToOutput.Value = 1
# set 'Build Action' to 'None'
$buildAction = $configItem2.Properties.Item("BuildAction")
$buildAction.Value = 0

$configItem2 = $project.ProjectItems.Item("Vrh.ApplicationContainer.Topshelf.exe.config")
# set 'Copy To Output Directory' to 'Copy always'
$copyToOutput = $configItem2.Properties.Item("CopyToOutputDirectory")
$copyToOutput.Value = 1
# set 'Build Action' to 'None'
$buildAction = $configItem2.Properties.Item("BuildAction")
$buildAction.Value = 0

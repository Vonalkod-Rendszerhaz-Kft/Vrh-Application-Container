using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.VisualStudio.TemplateWizard;
using EnvDTE;
using System.Windows.Forms;

namespace Plugin.ProjectTemplate.Install
{
    public class WizardImplementation : IWizard
    {
        // This method is called before opening any item that   
        // has the OpenInEditor attribute.  
        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]
        public void ProjectFinishedGenerating(Project project)
        {            
            foreach (Configuration config in project.ConfigurationManager)
            {

                string startProgramValue = Path.Combine(Path.GetDirectoryName(project.FileName), config.Properties.Item("OutputPath").Value) + Path.DirectorySeparatorChar + "Vrh.ApplicationContainer.ConsoleHost.exe";
                config.Properties.Item("StartAction").Value = 1; //Launch external program
                config.Properties.Item("StartProgram").Value = startProgramValue;
                config.Properties.Item("StartArguments").Value = "";
            }
            project.Save();
        }

        // This method is only called for item templates,  
        // not for project templates.  
        public void ProjectItemFinishedGenerating(ProjectItem
            projectItem)
        {
        }

        // This method is called after the project is created.  
        public void RunFinished()
        {
        }

        public void RunStarted(object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind, object[] customParams)
        {

        }

        // This method is only called for item templates,  
        // not for project templates.  
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
@echo off
REM A bin\Release-ben vagy bin\Debug-ban létező assembly-k másolása a ConsoleHost-barejött NuGet csomag hozzáadása a helyi teszt NuGet csomag mappához.
REM 1. par: SolutionDir
REM 2. par: A konfiguráció neve (Debug vagy Release)
REM 3. par: A projekt neve
SETLOCAL ENABLEEXTENSIONS
SET me=%~n0
SET solutionDir=%~1
SET configurationName=%~2
SET projectName=%~3
ECHO %me%: %projectName% project: Copy components ...
SET binDir=%solutionDir%%projectName%\bin\%configurationName%\
SET hostDir=%solutionDir%\Vrh.ApplicationContainer.ConsoleHost\bin\%configurationName%\
SET targetDir=%hostDir%%projectName%
SET switch=/Y /R

if not exist "%targetDir%" mkdir "%targetDir%"
xcopy %switch% "%solutionDir%%projectName%\iSchedulerMonitor.Config.xml" "%targetDir%"
xcopy %switch% "%binDir%%projectName%.dll" "%targetDir%"

REM Microsoft.Report.Viewer
xcopy %switch% "%binDir%Microsoft.ReportViewer.Common.dll" "%hostDir%"
xcopy %switch% "%binDir%Microsoft.ReportViewer.ProcessingObjectModel.DLL" "%hostDir%"
xcopy %switch% "%binDir%Microsoft.ReportViewer.WebForms.DLL" "%hostDir%"

REM Egyéb rendszer összetevők
xcopy %switch% "%binDir%Antlr3.Runtime.dll" "%hostDir%"
xcopy %switch% "%binDir%PagedList.dll" "%hostDir%"
xcopy %switch% "%binDir%PagedList.Mvc.dll" "%hostDir%"
xcopy %switch% "%binDir%System.Web.Optimization.dll" "%hostDir%"
xcopy %switch% "%binDir%System.Web.Providers.dll" "%hostDir%"
xcopy %switch% "%binDir%WebGrease.dll" "%hostDir%"

REM VRH-s alapcsomagok
xcopy %switch% "%binDir%Vrh.Interfaces.dll" "%hostDir%"
xcopy %switch% "%binDir%VRH.Log4Pro.MultiLanguageManager.dll" "%hostDir%"
xcopy %switch% "%binDir%VRH.Mockable.TimeProvider.dll" "%hostDir%"
xcopy %switch% "%binDir%Vrh.Web.Common.Lib.dll" "%hostDir%"

REM Üzleti logikát hordozó csomagok
xcopy %switch% "%binDir%Vrh.iScheduler.Lib.dll" "%hostDir%"
xcopy %switch% "%binDir%Vrh.iScheduler.Report.Lib.dll" "%hostDir%"
xcopy %switch% "%binDir%Vrh.OneMessage.dll" "%hostDir%"
xcopy %switch% "%binDir%Vrh.OneReport.Lib.dll" "%hostDir%"
xcopy %switch% "%binDir%Vrh.Web.Membership.DataTier.dll" "%hostDir%"
xcopy %switch% "%binDir%Vrh.Web.Membership.Lib.dll" "%hostDir%"

ECHO %me%: %projectName% project: Copy components END
	




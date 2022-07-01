using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace AmethystSoftware
{
    public class App : IExternalApplication
    {
        public Application GetApplicationFromUIControlledApplication(UIControlledApplication uiControlledApplication)
        {
            Type type = uiControlledApplication.GetType();

            // This is the call that finally yields useful results:

            BindingFlags flags = BindingFlags.Public
              | BindingFlags.NonPublic
              | BindingFlags.GetProperty
              | BindingFlags.Instance;

            MemberInfo[] propertyMembers = type.GetMembers(
              flags);


            // Note that the field "m_application" is listed
            // in the propertyMembers array, and also the 
            // method "getUIApp"... let's grab the field:

            string propertyName = "m_uiapplication";
            flags = BindingFlags.Public | BindingFlags.NonPublic
              | BindingFlags.GetField | BindingFlags.Instance;
            Binder binder = null;
            object[] args = null;

            object result = type.InvokeMember(
                propertyName, flags, binder, uiControlledApplication, args);

            UIApplication uiapp;

            uiapp = (UIApplication)result;

            Application applicationResult = uiapp.Application;


            return applicationResult;

        }

        public Result OnStartup(UIControlledApplication uiControlledApp)
        {
            Application application= GetApplicationFromUIControlledApplication(uiControlledApp);
            string userName = application.Username;


            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string iconsDirectoryPath = Path.GetDirectoryName(assemblyLocation) + @"\icons\";

            string tabName = "Amethysts";

            uiControlledApp.CreateRibbonTab(tabName);


            #region 1. Расстановка отверстий
            {
                RibbonPanel panel = uiControlledApp.CreateRibbonPanel(tabName, "Презентация");

                panel.AddItem(new PushButtonData(nameof(AmethystSoftware.AmethystRedIExternalCommand), "AmethystRed", assemblyLocation, typeof(AmethystSoftware.AmethystRedIExternalCommand).FullName)
                {
                    LargeImage = new BitmapImage(new Uri(iconsDirectoryPath + "AmethystRedIcon.png"))
                });
            }
            #endregion

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
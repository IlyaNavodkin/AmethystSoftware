using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
namespace AmethystSoftware
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AmethystRedIExternalCommand : IExternalCommand
    {


        //        Category cat = e.Category;
        //return null != cat &&
        //  cat.Id.IntegerValue.Equals((int) BuiltInCategory.OST_RoomTags));


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            AmethystRedMainViewModel amethystRedMainViewModel = new AmethystRedMainViewModel(commandData);

            AmethystRedMainView amethystRedView = new AmethystRedMainView(amethystRedMainViewModel);

            amethystRedView.Show();

            return Result.Succeeded;
        }

    }
}
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
    public class MepElementsFilterSelection : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            Category elemCategory = elem.Category;
            if (elemCategory == null)
            {
                return false;
            }

            if (elemCategory.Id.IntegerValue == -2008000)
            {
                return true;
            }
            else if (elemCategory.Id.IntegerValue == -2008044)
            {
                return true;
            }
            else if (elemCategory.Id.IntegerValue == -2008130)
            {
                return true;
            }
            else if (elemCategory.Id.IntegerValue == -2008132)
            {
                return true;
            }
            else if (elem is FamilyInstance)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
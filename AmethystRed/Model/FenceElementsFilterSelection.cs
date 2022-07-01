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
    public class FenceElementsFilterSelection : ISelectionFilter
    {
        Document doc { get; set; }
        public FenceElementsFilterSelection(Document docForSelection)
        {
            this.doc = docForSelection;
        }
        public bool AllowElement(Element elem)
        {
            return true;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            RevitLinkInstance revitlinkinstance = doc.GetElement(reference) as RevitLinkInstance;
            Document docLink = revitlinkinstance.GetLinkDocument();
            Element eScopeBoxLink = docLink.GetElement(reference.LinkedElementId);

            if (eScopeBoxLink.Category.Id.IntegerValue == -2000011)
            {
                return true;
            }
            else if (eScopeBoxLink.Category.Id.IntegerValue == -2000032)
            {
                return true;
            }
            else if (eScopeBoxLink.Category.Id.IntegerValue == -2000035)
            {
                return true;
            }
            return false;
        }
    }
}
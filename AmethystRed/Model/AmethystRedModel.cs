using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AmethystSoftware
{

    public class AmethystRedModel
    {
        public ExternalCommandData CommandData;
        public Document doc;
        public UIApplication app;
        public UIDocument uidoc;

        public PlugInformation PlugInfo;
        public List<Level> AllLevelsForPlugInfo;

        private string NameOfParamOfPlugWidth = "Фактическая ширина отверстия";
        private string NameOfParamOfPlugHeight = "Фактическая высота отверстия";
        private string NameOfParamOfPlugDiameter = "Фактический диаметр отверстия";
        private string NameOfParamOfPlugDepth = "Фактическая глубина отверстия";
        private string NameOfParamOfRound = "Не округлять размер отверстия";
        private string NameOfParamOfRoundValueInt = "Значение округления отверстия";

        public bool NotRoundPlug = false;
        public int ValueOfStockInt = 0;
        public int ValueOfRoundInt = 25;


        public List<Family> AllFamiliesOfHorizontalPlug = new List<Family> { };
        public List<Family> AllFamiliesOfVerticalPlug = new List<Family> { };

        public FamilySymbol FamilySymbolOfHorizontalFamiliyOfPlug { get; set; }
        public FamilySymbol FamilySymbolOfVerticalFamiliyOfPlug { get; set; }
        public Element FenceElement { get; set; }

        public AmethystRedModel(ExternalCommandData commandData)
        {
            this.CommandData = commandData;
            this.doc = CommandData.Application.ActiveUIDocument.Document;
            this.app = CommandData.Application;
            this.uidoc = app.ActiveUIDocument;

            GetAllFamiliesOfProject();

        }
        public void GetAllFamiliesOfProject()
        {

            List<Family> allFamilies = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .ToElements()
            .Cast<Family>()
            .Where(familyInProject => familyInProject.FamilyCategory.Id.IntegerValue.Equals((int)BuiltInCategory.OST_SecurityDevices))
            .ToList();


            foreach (Family fam in allFamilies)
            {
                string familiesNameLowerValue = fam.Name.ToLower();

                if (familiesNameLowerValue.Contains("горизонтальное") & familiesNameLowerValue.Contains("отверстие"))
                {
                    AllFamiliesOfHorizontalPlug.Add(fam);
                }
                else if (familiesNameLowerValue.Contains("вертикальное") & familiesNameLowerValue.Contains("отверстие"))
                {
                    AllFamiliesOfVerticalPlug.Add(fam);
                }
            }

        }

        private List<FamilyInstance> GetSubFamilyInstanceInParentFamilyInstance(FamilyInstance parentFamilyInstance, Document docOfFamilyInstance)
        {

            List<FamilyInstance> subFamilyInstance = parentFamilyInstance.GetSubComponentIds()
                .Select(subFamilyInstanceId => docOfFamilyInstance
                .GetElement(subFamilyInstanceId))
                .Cast<FamilyInstance>()
                .ToList();

            return subFamilyInstance;
        }
        private DirectShape CreateDirectShapeElementFromSolid(Solid solidForDirectShapeElement, Document docForDirectShapeElement)
        {
            DirectShape directShapeElement = DirectShape
                .CreateElement(docForDirectShapeElement, new ElementId(BuiltInCategory.OST_GenericModel));
            List<GeometryObject> listOfGeometryObjectsForDirectShapeElement = new List<GeometryObject> { solidForDirectShapeElement };
            directShapeElement.SetShape(listOfGeometryObjectsForDirectShapeElement);
            return directShapeElement;
        }

        private double ReturnBiggestDouble(double firstDouble, double secondDouble)
        {
            if (firstDouble > secondDouble)
            {
                return firstDouble;
            }
            else if (firstDouble < secondDouble)
            {
                return secondDouble;
            }
            else
            {
                return firstDouble;
            }
        }

        private Solid CreateRotatedSolidFromBoundingBoxXYZAndSetPlugInfo(Solid unionSolidForPlug, PlanarFace planarFaceOfFence)
        {


            BoundingBoxXYZ boundingBoxForSolid = unionSolidForPlug.GetBoundingBox();

            XYZ origin = boundingBoxForSolid.Transform.Origin;
            boundingBoxForSolid.Min = boundingBoxForSolid.Min.Add(origin);
            boundingBoxForSolid.Max = boundingBoxForSolid.Max.Add(origin);

            XYZ centerOfBoundingBoxForSolid = boundingBoxForSolid.Max.Add(boundingBoxForSolid.Min).Multiply(0.5);


            double angleBetweenFenceAndPlug = planarFaceOfFence.FaceNormal
                .AngleOnPlaneTo(XYZ.BasisY, XYZ.BasisZ);

            Transform newTransform = Transform.CreateRotationAtPoint(XYZ.BasisZ, angleBetweenFenceAndPlug, centerOfBoundingBoxForSolid);

            Solid rotatedSolid = SolidUtils.CreateTransformed(unionSolidForPlug, newTransform);

            BoundingBoxXYZ boundingBoxOfRotatedSolid = rotatedSolid.GetBoundingBox();

            XYZ origin1 = boundingBoxOfRotatedSolid.Transform.Origin;
            boundingBoxOfRotatedSolid.Min = boundingBoxOfRotatedSolid.Min.Add(origin1);
            boundingBoxOfRotatedSolid.Max = boundingBoxOfRotatedSolid.Max.Add(origin1);

            XYZ minPointOfBoundingBoxOfRotatedSolid = boundingBoxOfRotatedSolid.Min;
            XYZ maxPointOfBoundingBoxOfRotatedSolid = boundingBoxOfRotatedSolid.Max;

            double height = maxPointOfBoundingBoxOfRotatedSolid.Z - minPointOfBoundingBoxOfRotatedSolid.Z;

            XYZ pointOne = new XYZ(minPointOfBoundingBoxOfRotatedSolid.X,
                minPointOfBoundingBoxOfRotatedSolid.Y,
                minPointOfBoundingBoxOfRotatedSolid.Z);

            XYZ pointTwo = new XYZ(maxPointOfBoundingBoxOfRotatedSolid.X,
                maxPointOfBoundingBoxOfRotatedSolid.Y,
                minPointOfBoundingBoxOfRotatedSolid.Z);

            XYZ pointThree = new XYZ(minPointOfBoundingBoxOfRotatedSolid.X,
                maxPointOfBoundingBoxOfRotatedSolid.Y,
                minPointOfBoundingBoxOfRotatedSolid.Z);

            XYZ pointFour = new XYZ(maxPointOfBoundingBoxOfRotatedSolid.X,
                minPointOfBoundingBoxOfRotatedSolid.Y,
                minPointOfBoundingBoxOfRotatedSolid.Z);

            XYZ pointFive = new XYZ(minPointOfBoundingBoxOfRotatedSolid.X,
                minPointOfBoundingBoxOfRotatedSolid.Y,
                maxPointOfBoundingBoxOfRotatedSolid.Z);

            Transform newTransformForPoints = Transform
                .CreateRotationAtPoint(XYZ.BasisZ, -angleBetweenFenceAndPlug, centerOfBoundingBoxForSolid);

            List<XYZ> rotatedPoints = new List<XYZ> { pointOne, pointTwo, pointThree, pointFour, pointFive }
            .Select(point => newTransformForPoints.OfPoint(point)).ToList();



            Line edgeOne = Line.CreateBound(rotatedPoints[0], rotatedPoints[3]);
            Line edgeTwo = Line.CreateBound(rotatedPoints[3], rotatedPoints[1]);
            Line edgeThree = Line.CreateBound(rotatedPoints[1], rotatedPoints[2]);
            Line edgeFour = Line.CreateBound(rotatedPoints[2], rotatedPoints[0]);
            Line edgeFive = Line.CreateBound(rotatedPoints[4], rotatedPoints[0]);

            List<Curve> edgesForSolid = new List<Curve> { edgeOne, edgeTwo, edgeThree, edgeFour };

            CurveLoop curveLoopForSolid = CurveLoop.Create(edgesForSolid);

            List<CurveLoop> curvesForSolid = new List<CurveLoop> { curveLoopForSolid };


            Solid solidForPlug = GeometryCreationUtilities
                .CreateExtrusionGeometry(curvesForSolid, XYZ.BasisZ, height);

            XYZ centerOfsolidForPlug = solidForPlug.ComputeCentroid();

            double sideA = edgeOne.ApproximateLength;
            double sideB = edgeFive.ApproximateLength;
            double sideC = edgeFour.ApproximateLength;

            foreach (Level levelForPlug in AllLevelsForPlugInfo)
            {
                if (levelForPlug.ProjectElevation < centerOfsolidForPlug.Z)
                {
                    PlugInfo.LevelOfPlug = levelForPlug;
                    break;
                }
            }

            if (PlugInfo.Direction == DirectionOfPlug.Horizontal)
            {
                if (PlugInfo.Shape is ShapeOfPlug.Circle)
                {
                    PlugInfo.Diameter = ReturnBiggestDouble(sideB, sideA);
                    PlugInfo.Depth = sideC;
                    PlugInfo.Center = centerOfsolidForPlug;
                    PlugInfo.Angle = -angleBetweenFenceAndPlug;
                }
                else if (PlugInfo.Shape is ShapeOfPlug.Rectagular)
                {
                    PlugInfo.Height = sideB;
                    PlugInfo.Width = sideA;
                    PlugInfo.Depth = sideC;
                    PlugInfo.Center = centerOfsolidForPlug;
                    PlugInfo.Angle = -angleBetweenFenceAndPlug;
                }
            }
            else if (PlugInfo.Direction == DirectionOfPlug.Vertical)
            {
                if (PlugInfo.Shape is ShapeOfPlug.Circle)
                {
                    PlugInfo.Diameter = ReturnBiggestDouble(sideC, sideA);
                    PlugInfo.Depth = sideB;
                    PlugInfo.Center = centerOfsolidForPlug;
                    PlugInfo.Angle = -angleBetweenFenceAndPlug;
                }
                else if (PlugInfo.Shape is ShapeOfPlug.Rectagular)
                {
                    PlugInfo.Height = sideC;
                    PlugInfo.Width = sideA;
                    PlugInfo.Depth = sideB;
                    PlugInfo.Center = centerOfsolidForPlug;
                    PlugInfo.Angle = -angleBetweenFenceAndPlug;
                }

            }


            return solidForPlug;
        }
        private Solid CreateRotatedSolidFromBoundingBoxXYZ(Solid unionSolidForPlug, PlanarFace planarFaceOfFence)
        {


            BoundingBoxXYZ boundingBoxForSolid = unionSolidForPlug.GetBoundingBox();

            XYZ origin = boundingBoxForSolid.Transform.Origin;
            boundingBoxForSolid.Min = boundingBoxForSolid.Min.Add(origin);
            boundingBoxForSolid.Max = boundingBoxForSolid.Max.Add(origin);

            XYZ centerOfBoundingBoxForSolid = boundingBoxForSolid.Max.Add(boundingBoxForSolid.Min).Multiply(0.5);


            double angleBetweenFenceAndPlug = planarFaceOfFence.FaceNormal
                .AngleOnPlaneTo(XYZ.BasisY, XYZ.BasisZ);

            Transform newTransform = Transform.CreateRotationAtPoint(XYZ.BasisZ, angleBetweenFenceAndPlug, centerOfBoundingBoxForSolid);

            Solid rotatedSolid = SolidUtils.CreateTransformed(unionSolidForPlug, newTransform);

            BoundingBoxXYZ boundingBoxOfRotatedSolid = rotatedSolid.GetBoundingBox();

            XYZ origin1 = boundingBoxOfRotatedSolid.Transform.Origin;
            boundingBoxOfRotatedSolid.Min = boundingBoxOfRotatedSolid.Min.Add(origin1);
            boundingBoxOfRotatedSolid.Max = boundingBoxOfRotatedSolid.Max.Add(origin1);

            XYZ minPointOfBoundingBoxOfRotatedSolid = boundingBoxOfRotatedSolid.Min;
            XYZ maxPointOfBoundingBoxOfRotatedSolid = boundingBoxOfRotatedSolid.Max;

            double height = maxPointOfBoundingBoxOfRotatedSolid.Z - minPointOfBoundingBoxOfRotatedSolid.Z;

            XYZ pointOne = new XYZ(minPointOfBoundingBoxOfRotatedSolid.X,
                minPointOfBoundingBoxOfRotatedSolid.Y,
                minPointOfBoundingBoxOfRotatedSolid.Z);

            XYZ pointTwo = new XYZ(maxPointOfBoundingBoxOfRotatedSolid.X,
                maxPointOfBoundingBoxOfRotatedSolid.Y,
                minPointOfBoundingBoxOfRotatedSolid.Z);

            XYZ pointThree = new XYZ(minPointOfBoundingBoxOfRotatedSolid.X,
                maxPointOfBoundingBoxOfRotatedSolid.Y,
                minPointOfBoundingBoxOfRotatedSolid.Z);

            XYZ pointFour = new XYZ(maxPointOfBoundingBoxOfRotatedSolid.X,
                minPointOfBoundingBoxOfRotatedSolid.Y,
                minPointOfBoundingBoxOfRotatedSolid.Z);

            XYZ pointFive = new XYZ(minPointOfBoundingBoxOfRotatedSolid.X,
                minPointOfBoundingBoxOfRotatedSolid.Y,
                maxPointOfBoundingBoxOfRotatedSolid.Z);

            Transform newTransformForPoints = Transform
                .CreateRotationAtPoint(XYZ.BasisZ, -angleBetweenFenceAndPlug, centerOfBoundingBoxForSolid);

            List<XYZ> rotatedPoints = new List<XYZ> { pointOne, pointTwo, pointThree, pointFour, pointFive }
            .Select(point => newTransformForPoints.OfPoint(point)).ToList();



            Line edgeOne = Line.CreateBound(rotatedPoints[0], rotatedPoints[3]);
            Line edgeTwo = Line.CreateBound(rotatedPoints[3], rotatedPoints[1]);
            Line edgeThree = Line.CreateBound(rotatedPoints[1], rotatedPoints[2]);
            Line edgeFour = Line.CreateBound(rotatedPoints[2], rotatedPoints[0]);
            Line edgeFive = Line.CreateBound(rotatedPoints[4], rotatedPoints[0]);

            List<Curve> edgesForSolid = new List<Curve> { edgeOne, edgeTwo, edgeThree, edgeFour };

            CurveLoop curveLoopForSolid = CurveLoop.Create(edgesForSolid);

            List<CurveLoop> curvesForSolid = new List<CurveLoop> { curveLoopForSolid };


            Solid solidForPlug = GeometryCreationUtilities
                .CreateExtrusionGeometry(curvesForSolid, XYZ.BasisZ, height);

            return solidForPlug;
        }
        //private Solid CreateSolidFromBoundingBoxXYZ(BoundingBoxXYZ boundingBoxForSolid)
        //{
        //    XYZ origin = boundingBoxForSolid.Transform.Origin;
        //    boundingBoxForSolid.Min = boundingBoxForSolid.Min.Add(origin);
        //    boundingBoxForSolid.Max = boundingBoxForSolid.Max.Add(origin);


        //    XYZ minPoint = boundingBoxForSolid.Min;
        //    XYZ maxPoint = boundingBoxForSolid.Max;
        //    double heightOfSimpleSolid = maxPoint.Z - minPoint.Z;

        //    XYZ pointOne = new XYZ(minPoint.X, minPoint.Y, minPoint.Z);
        //    XYZ pointTwo = new XYZ(maxPoint.X, maxPoint.Y, minPoint.Z);
        //    XYZ pointThree = new XYZ(minPoint.X, maxPoint.Y, minPoint.Z);
        //    XYZ pointFour = new XYZ(maxPoint.X, minPoint.Y, minPoint.Z);

        //    Line edgeOne = Line.CreateBound(pointOne, pointFour);
        //    Line edgeTwo = Line.CreateBound(pointFour, pointTwo);
        //    Line edgeThree = Line.CreateBound(pointTwo, pointThree);
        //    Line edgeFour = Line.CreateBound(pointThree, pointOne);

        //    List<Curve> edgesForSolid = new List<Curve> { edgeOne, edgeTwo, edgeThree, edgeFour };

        //    CurveLoop curveLoopForSolid = CurveLoop.Create(edgesForSolid);

        //    List<CurveLoop> curvesForSolid = new List<CurveLoop> { curveLoopForSolid };

        //    Solid simpleSolidFromBoundingBoxXYZ = GeometryCreationUtilities
        //        .CreateExtrusionGeometry(curvesForSolid, XYZ.BasisZ, heightOfSimpleSolid);
        //    return simpleSolidFromBoundingBoxXYZ;

        //}
        private Solid UnionSolids(List<Solid> solidsForUnion)
        {
            Solid unionSolid = null;
            foreach (Solid solidForUnion in solidsForUnion)
            {
                if (null == unionSolid)
                {
                    unionSolid = solidForUnion;
                }
                else
                {
                    try
                    {
                        unionSolid = BooleanOperationsUtils
                        .ExecuteBooleanOperation(unionSolid, solidForUnion, BooleanOperationsType.Union);

                    }
                    catch
                    {
                        CreateDirectShapeElementFromSolid(solidForUnion, doc);
                    }

                }
            }
            return unionSolid;
        }
        private List<Solid> GetSolidsFromElement(Element elementForSolid, Options optionForGeometryOfElement)
        {
            List<GeometryObject> listofGeometryObjectOfElement = elementForSolid.get_Geometry(optionForGeometryOfElement).ToList();
            List<Solid> solidsFromElement = new List<Solid> { };
            foreach (GeometryObject geometryObjectFromElement in listofGeometryObjectOfElement)
            {
                if (geometryObjectFromElement is GeometryInstance)
                {
                    List<GeometryObject> listOfGeometryObjectFromGeometryInstance = (geometryObjectFromElement as GeometryInstance).GetInstanceGeometry().ToList();
                    foreach (GeometryObject geometryObjectFromGeometryInstance in listOfGeometryObjectFromGeometryInstance)
                    {
                        if (geometryObjectFromGeometryInstance is Solid)
                        {
                            Solid tempSolid = geometryObjectFromGeometryInstance as Solid;
                            if (tempSolid.SurfaceArea > 0)
                            {
                                solidsFromElement.Add(tempSolid);
                            }
                        }
                    }
                }
                if (geometryObjectFromElement is Solid)
                {
                    Solid tempSolid = geometryObjectFromElement as Solid;
                    if (tempSolid.SurfaceArea > 0)
                    {
                        solidsFromElement.Add(tempSolid);
                    }
                }
            }
            return solidsFromElement;
        }


        public List<FamilyInstance> GetAllSubFamily(FamilyInstance ownFamilyForSearch, Document docOfFamilyInstance)
        {
            List<FamilyInstance> tempSubFamilyInstance = new List<FamilyInstance>();
            if (ownFamilyForSearch is FamilyInstance)
            {
                List<FamilyInstance> tempListOfFamilyInstance = GetAllSubFamilyRecursion(ownFamilyForSearch, docOfFamilyInstance);

                if (tempListOfFamilyInstance.Count > 0)
                {
                    tempSubFamilyInstance.AddRange(tempListOfFamilyInstance);
                    foreach (FamilyInstance tempFamilyInstance in tempListOfFamilyInstance)
                    {
                        tempSubFamilyInstance.AddRange(GetAllSubFamily(tempFamilyInstance, docOfFamilyInstance));

                    }
                }
            }
            return tempSubFamilyInstance;
        }
        public List<FamilyInstance> GetAllSubFamilyRecursion(FamilyInstance recursionOwnFamilyForSearch, Document docOfFamilyInstance)
        {

            List<FamilyInstance> subelementsFromRecursionOwnFamilyForSearch = new List<FamilyInstance>();
            if (recursionOwnFamilyForSearch is FamilyInstance)
            {
                subelementsFromRecursionOwnFamilyForSearch.AddRange(recursionOwnFamilyForSearch.GetSubComponentIds()
                .Select(subFamilyInstanceId => docOfFamilyInstance
                .GetElement(subFamilyInstanceId))
                .Cast<FamilyInstance>()
                .ToList());
            }
            return subelementsFromRecursionOwnFamilyForSearch;



        }

        public void RegisterPlugsInformation(Element fenceElement)
        {
            if (FenceElement is Wall)
            {
                Family horizontalFamilyOfPlug = FamilySymbolOfHorizontalFamiliyOfPlug.Family;
                string horizontalFamilyNameLowerValue = horizontalFamilyOfPlug.Name.ToLower();

                PlugInfo = new PlugInformation(DirectionOfPlug.Horizontal);

                if (horizontalFamilyNameLowerValue.Contains("круглое"))
                {
                    PlugInfo.Shape = ShapeOfPlug.Circle;
                }
                else if (horizontalFamilyNameLowerValue.Contains("прямоугольное"))
                {
                    PlugInfo.Shape = ShapeOfPlug.Rectagular;
                }
            }
            else if (FenceElement is Floor | FenceElement is FootPrintRoof)
            {
                Family verticalFamilyOfPlug = FamilySymbolOfVerticalFamiliyOfPlug.Family;
                string verticalFamilyNameLowerValue = verticalFamilyOfPlug.Name.ToLower();

                PlugInfo = new PlugInformation(DirectionOfPlug.Vertical);

                if (verticalFamilyNameLowerValue.Contains("круглое"))
                {
                    PlugInfo.Shape = ShapeOfPlug.Circle;
                }
                else if (verticalFamilyNameLowerValue.Contains("прямоугольное"))
                {
                    PlugInfo.Shape = ShapeOfPlug.Rectagular;
                }
            }
        }
        private List<Level> GetAllLevelsOfProjectSortedToProjectElevation()
        {
            return new FilteredElementCollector(doc)
            .OfClass(typeof(Level))
            .ToElements()
            .Cast<Level>()
            .OrderByDescending(level => level.ProjectElevation)
            .ToList();


        }

        public void Run()
        {
            ResultElement result = RunWithResult();
            TaskDialog.Show(result.OperationTitle, result.OperationMessage);
        }

        public ResultElement RunWithResult()
        {
            using (var t = new Transaction(doc))
            {
                t.Start("Расчет геометрии и размеров заглушки");
                FamilySymbolOfHorizontalFamiliyOfPlug.Activate();
                FamilySymbolOfVerticalFamiliyOfPlug.Activate();
                AllLevelsForPlugInfo = GetAllLevelsOfProjectSortedToProjectElevation();


                MepElementsFilterSelection mepSelectionFilter = new MepElementsFilterSelection();
                FenceElementsFilterSelection fenceSelectionFilter = new FenceElementsFilterSelection(doc);

                Reference fenceLinkReferenceForPlug = uidoc.Selection
                    .PickObject(ObjectType.LinkedElement, fenceSelectionFilter,
                    "Выберите ограждение (стена,крыша,перекрытие) в котором необходиом поставить заглушку");
                RevitLinkInstance revitlinkinstance = doc.GetElement(fenceLinkReferenceForPlug) as RevitLinkInstance;
                Document docLink = revitlinkinstance.GetLinkDocument();
                FenceElement = docLink.GetElement(fenceLinkReferenceForPlug.LinkedElementId);


                RegisterPlugsInformation(FenceElement);

                Parameter parameterOfAreaOfFenceElement = FenceElement.get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED);

                double parameterOfAreaOfFenceElementValueDouble = parameterOfAreaOfFenceElement.AsDouble();

                Transform transformOfrevitlinkinstance = revitlinkinstance.GetTotalTransform();

                Options optionForFenceElement = new Options();
                optionForFenceElement.ComputeReferences = true;
                optionForFenceElement.DetailLevel = ViewDetailLevel.Fine;
                optionForFenceElement.IncludeNonVisibleObjects = false;

                Solid fenceSolid = GetSolidsFromElement(FenceElement, optionForFenceElement)[0];

                Solid newFenceSolid = SolidUtils.CreateTransformed(fenceSolid, transformOfrevitlinkinstance);

                FaceArray newFaceArray = newFenceSolid.Faces;

                List<PlanarFace> newFenceSolidFaces = new List<PlanarFace>();


                foreach (Face face in newFaceArray)
                {
                    if (face is PlanarFace)
                    {
                        newFenceSolidFaces.Add(face as PlanarFace);
                    }
                }

                PlanarFace mainPlanarFaceOfFence = null;

                foreach (PlanarFace face in newFenceSolidFaces)
                {
                    if (face.Area == parameterOfAreaOfFenceElementValueDouble)
                    {
                        mainPlanarFaceOfFence = face;
                    }
                    else
                    {
                        continue;
                    }
                }


                List<Element> mepElementsForPlugs = uidoc.Selection
                    .PickObjects(ObjectType.Element, mepSelectionFilter,
                    "Выберите MEP элементы для которых нужно создать заглушки")
                    .Select(reference => doc.GetElement(reference)).ToList();
                List<Element> mepElementsForPlugsWithSubElements = new List<Element> { };



                foreach (Element e1 in mepElementsForPlugs)
                {
                    mepElementsForPlugsWithSubElements.Add(e1);
                    if (e1 is FamilyInstance)
                    {
                        List<FamilyInstance> lsit = GetAllSubFamily(e1 as FamilyInstance, doc);
                        mepElementsForPlugsWithSubElements.AddRange(lsit);

                    }
                }

                Options optionForMepElement = new Options();
                optionForMepElement.ComputeReferences = true;
                optionForMepElement.DetailLevel = ViewDetailLevel.Fine;
                optionForMepElement.IncludeNonVisibleObjects = false;

                List<Solid> allMepSolids = new List<Solid> { };

                foreach (Element mepElementForSolid in mepElementsForPlugsWithSubElements)
                {
                    List<Solid> tempListOfSolids = GetSolidsFromElement(mepElementForSolid, optionForMepElement);
                    if (mepElementForSolid is FamilyInstance)
                    {
                        foreach (Solid solidForSimpleSolid in tempListOfSolids)
                        {
                            Solid solidFromBoundingBox = CreateRotatedSolidFromBoundingBoxXYZ(solidForSimpleSolid, mainPlanarFaceOfFence);
                            allMepSolids.Add(solidFromBoundingBox);
                        }
                    }
                    else
                    {
                        allMepSolids.AddRange(tempListOfSolids);
                    }
                }
                Solid unionSolidOfMepElements = UnionSolids(allMepSolids);
                Solid solidForPlug = BooleanOperationsUtils.ExecuteBooleanOperation(newFenceSolid, unionSolidOfMepElements, BooleanOperationsType.Intersect);
                if (solidForPlug.Volume == 0)
                {
                    return new ResultElement(false, "Элементы не пересекаются с ограждением");
                }
                Solid simpliestSolidForPlugInfo = CreateRotatedSolidFromBoundingBoxXYZAndSetPlugInfo(solidForPlug, mainPlanarFaceOfFence);

                CreateDirectShapeElementFromSolid(newFenceSolid, doc);
                CreateDirectShapeElementFromSolid(unionSolidOfMepElements, doc);
                CreateDirectShapeElementFromSolid(simpliestSolidForPlugInfo, doc);

                t.Commit();

                using (var t2 = new Transaction(doc))
                {
                    t2.Start("Вставка семейства заглушки");

                    if (PlugInfo.Direction == DirectionOfPlug.Horizontal)
                    {
                        PlugInfo.PlugInstance = doc.Create
                        .NewFamilyInstance(PlugInfo.Center
                        , FamilySymbolOfHorizontalFamiliyOfPlug
                        , PlugInfo.LevelOfPlug
                        , Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    }
                    else if (PlugInfo.Direction == DirectionOfPlug.Vertical)
                    {
                        PlugInfo.PlugInstance = doc.Create
                        .NewFamilyInstance(PlugInfo.Center
                        , FamilySymbolOfVerticalFamiliyOfPlug
                        , PlugInfo.LevelOfPlug
                        , Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                    }


                    if (PlugInfo.Shape == ShapeOfPlug.Circle)
                    {
                        PlugInfo.PlugInstance.LookupParameter(NameOfParamOfPlugDepth)
                            .Set(PlugInfo.Depth);
                        PlugInfo.PlugInstance.LookupParameter(NameOfParamOfPlugDiameter)
                            .Set(PlugInfo.Diameter + UnitUtils.ConvertToInternalUnits(ValueOfStockInt, DisplayUnitType.DUT_MILLIMETERS));
                    }
                    else if (PlugInfo.Shape == ShapeOfPlug.Rectagular)
                    {
                        PlugInfo.PlugInstance.LookupParameter(NameOfParamOfPlugDepth)
                            .Set(PlugInfo.Depth);
                        PlugInfo.PlugInstance.LookupParameter(NameOfParamOfPlugWidth)
                            .Set(PlugInfo.Width + UnitUtils.ConvertToInternalUnits(ValueOfStockInt, DisplayUnitType.DUT_MILLIMETERS));
                        PlugInfo.PlugInstance.LookupParameter(NameOfParamOfPlugHeight)
                            .Set(PlugInfo.Height + UnitUtils.ConvertToInternalUnits(ValueOfStockInt, DisplayUnitType.DUT_MILLIMETERS));
                    }

                    if (NotRoundPlug == false)
                    {
                        PlugInfo.PlugInstance.LookupParameter(NameOfParamOfRound).Set(1);
                    }
                    else
                    {
                        PlugInfo.PlugInstance.LookupParameter(NameOfParamOfRound).Set(0);
                        PlugInfo.PlugInstance.LookupParameter(NameOfParamOfRoundValueInt).Set(UnitUtils.ConvertToInternalUnits(ValueOfRoundInt, DisplayUnitType.DUT_MILLIMETERS));
                    }

                    t2.Commit();

                }

                using (var t3 = new Transaction(doc))
                {
                    t3.Start("Поворот заглушки и  растановка ее по высоте");
                    LocationPoint plugLocationPoint = PlugInfo.PlugInstance.Location as LocationPoint;
                    XYZ plugLocationPointXYZ = plugLocationPoint.Point;
                    Line lineRotationOfPlug = Line.CreateUnbound(plugLocationPointXYZ, XYZ.BasisZ);

                    PlugInfo.PlugInstance.Location.Rotate(lineRotationOfPlug, PlugInfo.Angle);

                    if (PlugInfo.Direction == DirectionOfPlug.Vertical)
                    {
                        if (plugLocationPointXYZ.Z < 0)
                        {
                            PlugInfo.PlugInstance.Location.Move(new XYZ(0, 0, -plugLocationPointXYZ.Z + PlugInfo.Center.Z));
                        }
                        else if (plugLocationPointXYZ.Z == 0)
                        {
                            PlugInfo.PlugInstance.Location.Move(new XYZ(0, 0, PlugInfo.Center.Z));
                        }
                        else if (plugLocationPointXYZ.Z > 0)
                        {
                            PlugInfo.PlugInstance.Location.Move(new XYZ(0, 0, -plugLocationPointXYZ.Z + PlugInfo.Center.Z));
                        }
                    }
                    else if (PlugInfo.Direction == DirectionOfPlug.Horizontal)
                    {
                        if (plugLocationPointXYZ.Z < 0)
                        {
                            PlugInfo.PlugInstance.Location.Move(new XYZ(0, 0, -plugLocationPointXYZ.Z + PlugInfo.Center.Z));
                        }
                        else if (plugLocationPointXYZ.Z == 0)
                        {
                            PlugInfo.PlugInstance.Location.Move(new XYZ(0, 0, PlugInfo.Center.Z));
                        }
                        else if (plugLocationPointXYZ.Z > 0)
                        {
                            PlugInfo.PlugInstance.Location.Move(new XYZ(0, 0, -plugLocationPointXYZ.Z + PlugInfo.Center.Z));
                        }
                    }
                    t3.Commit();
                }
                //uidoc.ShowElements(PlugInfo.PlugInstance);
                return new ResultElement(true, "Операция успешна");
            }
        }
    }
}
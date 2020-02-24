using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace CEGVN.TMP
{
    [Transaction(TransactionMode.Manual)]
    class CalloutDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Selection choice = uidoc.Selection;
            Plane plane = Plane.CreateByNormalAndOrigin(doc.ActiveView.ViewDirection, doc.ActiveView.Origin);
            Dimension ele1 = null;
            Dimension ele2 = null;
            Reference r1 = choice.PickObject(ObjectType.Element, "Select First Dimension");
            Reference r2 = choice.PickObject(ObjectType.Element, "Select Second Dimension");

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");

                Dimension dim1 = doc.GetElement(r1) as Dimension;
                Line curve1 = dim1.Curve as Line;
                curve1.MakeBound(0, 1);
                Line line1 = Support.ProjectLineOnPlane(plane, curve1);

                Dimension dim2 = doc.GetElement(r2) as Dimension;
                Line curve2 = dim2.Curve as Line;
                curve2.MakeBound(0, 1);
                Line line2 = Support.ProjectLineOnPlane(plane, curve2);
                XYZ vector1 = line1.GetEndPoint(0) - line1.GetEndPoint(1);
                XYZ vector2 = line2.GetEndPoint(0) - line2.GetEndPoint(1);
                if (Support.IsHorizontal(vector1, doc.ActiveView) == true)
                {
                    ele1 = dim1;
                    ele2 = dim2;
                }
                else
                {
                    ele1 = dim2;
                    ele2 = dim1;
                }

                List<Element> list1 = new List<Element>();
                List<Element> list2 = new List<Element>();

                for (int aa = 1; aa < ele1.Segments.Size; aa++)
                {
                    Reference ref1 = ele1.References.get_Item(aa);
                    Element element1 = doc.GetElement(ref1);
                    if (element1.Name != null && element1.Category.Name == "Specialty Equipment")
                    {
                        list1.Add(element1);
                    }
                }

                for (int bb = 1; bb < ele2.Segments.Size; bb++)
                {
                    Reference ref2 = ele2.References.get_Item(bb);
                    Element element2 = doc.GetElement(ref2);
                    if (element2.Name != null && element2.Category.Name == "Specialty Equipment")
                    {
                        list2.Add(element2);
                    }
                }
                if (list1.First().Name != list2.First().Name)
                {
                    TaskDialog.Show("ERROR", "PLEASE SELECT TWO DIM FOR THE SAME OBJECT");
                }
                else
                {
                    if (clinename(doc) != "CSAZ" && clinename(doc) != "CSFL" && clinename(doc)!= "STRESSCON")
                    {
                        if (ele1.Id != ele2.Id)
                        {
                            //giao diem:
                            XYZ giaodiem = Support.ExtendLineIntersection(line1, line2);

                            ////Getendpoint:
                            List<XYZ> listendpoint1 = Support.GetStartEndDimensio(ele1);
                            List<XYZ> listendpoint2 = Support.GetStartEndDimensio(ele2);
                            List<XYZ> TwoPointEndDim = Support.GetDimensionPointsEnd(doc, ele1);

                            XYZ test1 = TwoPointEndDim[0];
                            test1 = Support.ProjectOnto(plane, test1);
                            XYZ test2 = TwoPointEndDim[1];
                            test2 = Support.ProjectOnto(plane, test2);

                            //Create line:                
                            var min1 = double.MaxValue;
                            XYZ Point1 = XYZ.Zero;
                            XYZ Point2 = XYZ.Zero;

                            foreach (var point in listendpoint1)
                            {
                                var distance = giaodiem.DistanceTo(point);
                                if (distance < min1)
                                {
                                    Point1 = point;
                                    min1 = distance;
                                }
                            }
                            var min2 = double.MaxValue;
                            foreach (var point in listendpoint2)
                            {
                                var distance = giaodiem.DistanceTo(point);
                                if (distance < min2)
                                {
                                    Point2 = point;
                                    min2 = distance;
                                }
                            }

                            Point1 = Support.ProjectOnto(plane, Point1);
                            Point2 = Support.ProjectOnto(plane, Point2);

                            XYZ ep21 = Support.ProjectOnto(plane, listendpoint2[0]);
                            XYZ ep222 = Support.ProjectOnto(plane, listendpoint2[1]);

                            DetailCurve cur1 = Support.createdetailcurve(doc, Point1, giaodiem, plane);
                            DetailCurve cur3 = Support.createdetailcurve(doc, Point2, giaodiem, plane);

                            #region: Create text
                            XYZ kc = new XYZ(0, 0, 0);
                            XYZ pos = new XYZ(0, 0, 0);

                            //if (line1.Direction.DotProduct(XYZ.BasisX) == 1)
                            //{
                            //    pos = new XYZ(10, 10, 10);
                            //}

                            if (giaodiem.DistanceTo(test1) < giaodiem.DistanceTo(test2))
                            {
                                if (line1.Direction.DotProduct(XYZ.BasisY) > 0)
                                {
                                    kc = new XYZ(0, 0, -0.45);
                                }
                                else
                                {
                                    kc = new XYZ(0, 0, -0.45);
                                }
                                pos = Point1 - kc;
                            }
                            else
                            {
                                if (line1.Direction.DotProduct(XYZ.BasisY) > 0)
                                {
                                    kc = new XYZ(0, 0, -0.45);
                                }
                                else
                                {
                                    kc = new XYZ(0, 0, -0.45);
                                }
                                pos = Point1 - kc;
                            }
                            ElementId defaultTextTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                            TextNoteOptions opts = new TextNoteOptions(defaultTextTypeId);
                            opts.HorizontalAlignment = HorizontalTextAlignment.Left;
                            //XYZ pos = Point1 - kc;

                            string s1 = "CL (" + list1.Count * list2.Count + ") " + list1.First().Name;

                            #endregion

                            #region: Create location
                            FilteredElementCollector filter = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_MultiCategoryTags);
                            List<Element> listtag = filter.OfCategory(BuiltInCategory.OST_MultiCategoryTags).WhereElementIsNotElementType().ToElements().ToList();
                            List<Element> sametag = new List<Element>();
                            foreach (var item in listtag)
                            {
                                IndependentTag el = doc.GetElement(item.Id) as IndependentTag;
                                Element tag1 = el.GetTaggedLocalElement();
                                string tag = tag1.Name;

                                if (tag == list1.First().Name)
                                {
                                    sametag.Add(item);
                                }
                            }

                            if (sametag.Count > 0)
                            {
                                Element elem1 = doc.GetElement(sametag.First().Id) as Element;
                                string samename = elem1.Name;
                                int gachngang = samename.LastIndexOf("-");
                                string lastname = samename.Substring(gachngang + 2);
                                string s11 = s1 + "\n" + "(" + lastname + ")";
                                TextNote s2 = TextNote.Create(doc, doc.ActiveView.Id, pos, s11, opts);
                            }
                            else
                            {
                                TextNote s2 = TextNote.Create(doc, doc.ActiveView.Id, pos, s1, opts);
                            }
                            #endregion
                        }
                        else
                        {
                            List<XYZ> listendpoint1 = Support.GetStartEndDimensio(ele1);

                            #region: Create line                
                            var min1 = double.MaxValue;
                            XYZ Point1 = XYZ.Zero;
                            Double scale = doc.ActiveView.Scale;
                            XYZ kc1 = new XYZ(0, 0.09 * scale, 0);
                            XYZ kc11 = new XYZ(0, -0.09 * scale, 0);
                            XYZ kc2 = new XYZ(0, 0, 0.09 * scale);
                            XYZ kc22 = new XYZ(0, 0, 0.09 * scale);
                            XYZ kc23 = new XYZ(-0.09 * scale, 0, 0);
                            XYZ kc24 = new XYZ(0.09 * scale, 0, 0);
                            XYZ kc25 = new XYZ(0.09 * scale, 0, 0.09 * scale);
                            kc2 = Support.ProjectOnto(plane, kc2);
                            kc23 = Support.ProjectOnto(plane, kc23);
                            kc24 = Support.ProjectOnto(plane, kc24);
                            kc25 = Support.ProjectOnto(plane, kc25);
                            List<XYZ> TwoPointEndDim = Support.GetDimensionPointsEnd(doc, ele1);

                            XYZ test1 = TwoPointEndDim[0];
                            test1 = Support.ProjectOnto(plane, test1);
                            XYZ test2 = TwoPointEndDim[1];
                            test2 = Support.ProjectOnto(plane, test2);
                            XYZ Point2 = XYZ.Zero;

                            if (line1.Direction.DotProduct(XYZ.BasisY) == -1)
                            {
                                Point2 = test1 + kc1;
                                Point2 = Support.ProjectOnto(plane, Point2);
                                DetailCurve cur1 = Support.createdetailcurve(doc, test1, Point2, plane);
                            }

                            if (line1.Direction.DotProduct(XYZ.BasisY) == 1)
                            {
                                Point2 = test1 + kc11;
                                Point2 = Support.ProjectOnto(plane, Point2);
                                DetailCurve cur1 = Support.createdetailcurve(doc, test1, Point2, plane);
                            }

                            if (line1.Direction.DotProduct(XYZ.BasisZ) == 1)
                            {
                                Point2 = test2 + kc2;
                                Point2 = Support.ProjectOnto(plane, Point2);
                                test1 = Support.ProjectOnto(plane, test1);
                                DetailCurve cur1 = Support.createdetailcurve(doc, test2, Point2, plane);
                            }

                            if (line1.Direction.DotProduct(XYZ.BasisZ) == -1)
                            {
                                Point2 = test1 + kc22;
                                Point2 = Support.ProjectOnto(plane, Point2);
                                test1 = Support.ProjectOnto(plane, test1);
                                DetailCurve cur1 = Support.createdetailcurve(doc, test1, Point2, plane);
                            }

                            if (line1.Direction.DotProduct(XYZ.BasisX) == 1)
                            {
                                Point1 = test1 + kc23;
                                Point1 = Support.ProjectOnto(plane, Point1);
                                test1 = Support.ProjectOnto(plane, test1);
                                DetailCurve cur1 = Support.createdetailcurve(doc, test1, Point1, plane);
                            }

                            if (line1.Direction.DotProduct(XYZ.BasisX) == -1)
                            {
                                Point1 = test1 + kc24;
                                Point1 = Support.ProjectOnto(plane, Point1);
                                test1 = Support.ProjectOnto(plane, test1);
                                DetailCurve cur1 = Support.createdetailcurve(doc, test1, Point1, plane);
                            }

                            if (line1.Direction.DotProduct(XYZ.BasisX) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisY) != -1 && line1.Direction.DotProduct(XYZ.BasisY) != 1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1 && line1.Direction.DotProduct(XYZ.BasisZ) != 1)
                            {
                                CreateExtenline(doc, plane, ele1, 3.5);
                            }

                            foreach (var point in listendpoint1)
                            {
                                var distance = kc1.DistanceTo(point);
                                if (distance < min1)
                                {
                                    Point1 = point;
                                    min1 = distance;
                                }
                            }
                            #endregion


                            #region: Create text
                            for (int aa = 1; aa < ele1.Segments.Size; aa++)
                            {
                                Reference ref1 = ele1.References.get_Item(aa);
                                Element element1 = doc.GetElement(ref1);
                                if (element1.Name != null && element1.Category.Name == "Specialty Equipment")
                                {
                                    list1.Add(element1);
                                }
                            }

                            XYZ pos = null;
                            ElementId defaultTextTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                            TextNoteOptions opts = new TextNoteOptions(defaultTextTypeId);
                            string s1 = null;

                            XYZ vector = test1 - test2;
                            Double x = vector.DotProduct(XYZ.BasisX);
                            Double y = vector.DotProduct(XYZ.BasisY);
                            Double z = vector.DotProduct(XYZ.BasisZ);

                            if (Support.IsHorizontal(vector, doc.ActiveView) == true)
                            {
                                XYZ kc = null;
                                TextNoteOptions opts1 = new TextNoteOptions();

                                opts.HorizontalAlignment = HorizontalTextAlignment.Left;
                                if (line1.Direction.DotProduct(XYZ.BasisY) == -1)
                                {
                                    kc = new XYZ(0, 0, -0.44);
                                    pos = test1 - kc;
                                }
                                if (line1.Direction.DotProduct(XYZ.BasisY) == 1)
                                {
                                    kc = new XYZ(0, 2.6, -0.44);
                                    pos = test1 - kc;
                                }
                                if (line1.Direction.DotProduct(XYZ.BasisX) == -1)
                                {
                                    kc = new XYZ(-2.6, 0, -0.44);
                                    pos = test1 - kc;
                                }
                                if (line1.Direction.DotProduct(XYZ.BasisX) == 1)
                                {
                                    kc = new XYZ(0.35, 0, -0.44);
                                    pos = test1 - kc;
                                }
                                if (line1.Direction.DotProduct(XYZ.BasisZ) == -1)
                                {
                                    pos = test1;
                                }
                                if (line1.Direction.DotProduct(XYZ.BasisZ) == 1)
                                {
                                    pos = test2;
                                }
                                if (line1.Direction.DotProduct(XYZ.BasisZ) != 1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1 && line1.Direction.DotProduct(XYZ.BasisY) != 1 && line1.Direction.DotProduct(XYZ.BasisY) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisX) != -1)
                                {
                                    pos = test1;
                                }

                                s1 = "CL (" + (list1.Count) / 2 + ") " + list1.First().Name;
                            }

                            if (Support.IsVertical(vector, doc.ActiveView) == true)
                            {
                                XYZ kc = null;
                                TextNoteOptions opts1 = new TextNoteOptions();

                                opts.HorizontalAlignment = HorizontalTextAlignment.Left;
                                opts.Rotation = 0.5 * Math.PI;
                                if (line1.Direction.DotProduct(XYZ.BasisZ) == -1)
                                {
                                    kc = new XYZ(-0.42, 0, -0.35);
                                    pos = test1 - kc;
                                }
                                if (line1.Direction.DotProduct(XYZ.BasisZ) == 1)
                                {
                                    kc = new XYZ(-0.42, 0.4, -0.35);
                                    pos = test1 - kc;
                                }
                                pos = test1;
                                s1 = "CL (" + (list1.Count) / 2 + ") " + list1.First().Name;
                            }
                            if (Support.IsVertical(vector, doc.ActiveView) == false && Support.IsHorizontal(vector, doc.ActiveView) == false)
                            {
                                if (line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisX) != -1 && line1.Direction.DotProduct(XYZ.BasisY) != 1 && line1.Direction.DotProduct(XYZ.BasisY) != -1)
                                {
                                    pos = test1;
                                    s1 = "CL (" + (list1.Count) / 2 + ") " + list1.First().Name;
                                }
                            }

                            #endregion

                            #region: Create location
                            FilteredElementCollector filter = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_MultiCategoryTags);
                            List<Element> listtag = filter.OfCategory(BuiltInCategory.OST_MultiCategoryTags).WhereElementIsNotElementType().ToElements().ToList();
                            List<Element> sametag = new List<Element>();
                            foreach (var item in listtag)
                            {
                                IndependentTag el = doc.GetElement(item.Id) as IndependentTag;
                                Element tag1 = el.GetTaggedLocalElement();
                                string tag = tag1.Name;

                                if (tag == list1.First().Name)
                                {
                                    sametag.Add(item);
                                }
                            }

                            if (sametag.Count > 0)
                            {
                                Element elem1 = doc.GetElement(sametag.First().Id) as Element;
                                //string samename =elem1.GetParameters("CONTROL_MARK").ToString();
                                string samename = elem1.Name;
                                int gachngang = samename.LastIndexOf("-");
                                string lastname = samename.Substring(gachngang + 2);
                                string s11 = s1 + "\n" + "(" + lastname + ")";
                                TextNote s2 = TextNote.Create(doc, doc.ActiveView.Id, pos, s11, opts);
                            }
                            else
                            {
                                TextNote s2 = TextNote.Create(doc, doc.ActiveView.Id, pos, s1, opts);
                            }
                            #endregion

                        }
                    }
                    //if (clinename(doc) == "")
                    //{
                    //    TaskDialog.Show("Error", "PLEASE CHECK PROJECT_CLIENT_PRECAST_MANUFACTURER");
                    //}
                    if (clinename(doc) == "CSFL")
                    {
                        Setsymbol(doc, ele1, ele2, "AKSDADA1", "CS Miami Dummy Part Callout 3_16", "AKSDADA2", "CSS Miami Dummy Part Callout 3_16", plane);
                    }
                    if (clinename(doc) == "CSAZ")
                    {
                        Setsymbol(doc, ele1, ele2, "ARI1", "CS Arizon Dummy Part Callout 3_16", "ARI2", "CSS Arizon Dummy Part Callout 3_16", plane);
                    }
                    if (clinename(doc) == "STRESSCON")
                    {
                        Setsymbol(doc, ele1, ele2, "TAG TEXT NOTE", "NONE", "TAG TEXT NOTE1", "NONE", plane);
                    }
                }



                tx.Commit();
            }
            return Result.Succeeded;
        }
        private string clinename(Document doc)
        {
            ProjectInfo proinfo = doc.ProjectInformation;
            string projectclient = proinfo.LookupParameter("PROJECT_CLIENT_PRECAST_MANUFACTURER").AsString();
            return projectclient;
        }

        private void Setsymbol(Document doc, Dimension ele1, Dimension ele2, string family, string type, string family1, string type1, Plane plane)
        {
            Line curve1 = ele1.Curve as Line;
            curve1.MakeBound(0, 1);
            Line line1 = Support.ProjectLineOnPlane(plane, curve1);
            Line curve2 = ele2.Curve as Line;
            curve2.MakeBound(0, 1);
            Line line2 = Support.ProjectLineOnPlane(plane, curve2);
            FamilySymbol symbol = Support.GetSymbol(doc, family, type);
            FamilySymbol symbol1 = Support.GetSymbol(doc, family1, type1);
            //SUPPORT.PARAMETER.PARAMETER para = new SUPPORT.PARAMETER.PARAMETER();

            if (ele1.Id != ele2.Id)
            {
                //giao diem:
                XYZ giaodiem = Support.ExtendLineIntersection(line1, line2);

                ////Getendpoint:
                List<XYZ> listendpoint1 = Support.GetStartEndDimensio(ele1);
                List<XYZ> listendpoint2 = Support.GetStartEndDimensio(ele2);
                List<XYZ> TwoPointEndDim = Support.GetDimensionPointsEnd(doc, ele1);

                XYZ test1 = TwoPointEndDim[0];
                test1 = Support.ProjectOnto(plane, test1);
                XYZ test2 = TwoPointEndDim[1];
                test2 = Support.ProjectOnto(plane, test2);
                XYZ tp = new XYZ(2, 0, 0);

                Plane plane1 = Plane.CreateByNormalAndOrigin(line1.Direction.Normalize(), test2 - tp);
                #region Create line:                
                var min1 = double.MaxValue;
                XYZ Point1 = XYZ.Zero;
                XYZ Point2 = XYZ.Zero;

                foreach (var point in listendpoint1)
                {
                    var distance = giaodiem.DistanceTo(point);
                    if (distance < min1)
                    {
                        Point1 = point;
                        min1 = distance;
                    }
                }
                var min2 = double.MaxValue;
                foreach (var point in listendpoint2)
                {
                    var distance = giaodiem.DistanceTo(point);
                    if (distance < min2)
                    {
                        Point2 = point;
                        min2 = distance;
                    }
                }
                Point1 = Support.ProjectOnto(plane, Point1);
                Point2 = Support.ProjectOnto(plane, Point2);

                XYZ ep21 = Support.ProjectOnto(plane, listendpoint2[0]);
                XYZ ep222 = Support.ProjectOnto(plane, listendpoint2[1]);

                DetailCurve cur1 = Support.createdetailcurve(doc, Point1, giaodiem, plane);
                DetailCurve cur3 = Support.createdetailcurve(doc, Point2, giaodiem, plane);
                #endregion

                #region: Create text
                List<Element> list1 = new List<Element>();
                List<Element> list2 = new List<Element>();

                for (int aa = 1; aa < ele1.Segments.Size; aa++)
                {
                    Reference ref1 = ele1.References.get_Item(aa);
                    Element element1 = doc.GetElement(ref1);
                    if (element1.Name != null && element1.Category.Name == "Specialty Equipment")
                    {
                        list1.Add(element1);
                    }
                }

                for (int bb = 1; bb < ele2.Segments.Size; bb++)
                {
                    Reference ref2 = ele2.References.get_Item(bb);
                    Element element2 = doc.GetElement(ref2);
                    if (element2.Name != null && element2.Category.Name == "Specialty Equipment")
                    {
                        list2.Add(element2);
                    }
                }

                XYZ kc = new XYZ(0, 0, 0);
                XYZ pos = new XYZ(0, 0, 0);
                ElementId defaultTextTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                TextNoteOptions opts = new TextNoteOptions(defaultTextTypeId);
                opts.HorizontalAlignment = HorizontalTextAlignment.Left;
                string s1 = list1.First().Name;

                #region: Create location
                FilteredElementCollector filter = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_MultiCategoryTags);
                List<Element> listtag = filter.OfCategory(BuiltInCategory.OST_MultiCategoryTags).WhereElementIsNotElementType().ToElements().ToList();
                List<Element> sametag = new List<Element>();

                foreach (var item in listtag)
                {
                    IndependentTag el = doc.GetElement(item.Id) as IndependentTag;
                    Element tag1 = el.GetTaggedLocalElement();
                    string tag = tag1.Name;
                    if (tag == list1.First().Name)
                    {
                        sametag.Add(item);
                    }
                }

                string lastname = "";
                if (sametag.Count > 0)
                {
                    Element elem1 = doc.GetElement(sametag.First().Id) as Element;
                    string samename = elem1.Name;
                    int gachngang = samename.LastIndexOf("-");
                    lastname = samename.Substring(gachngang + 1);
                    string s11 = s1 + "\n" + "(" + lastname + ")";
                }
                if (sametag.Count == 0)
                {
                    lastname = " ";
                }

                #endregion

                if (giaodiem.DistanceTo(test1) < giaodiem.DistanceTo(test2))
                {
                    FamilyInstance newinstance = null; ;
                    XYZ kt = new XYZ(0, 0, 0);
                    if (line1.Direction.DotProduct(XYZ.BasisX) == -1)
                    {
                        kt = new XYZ(-3.3, 0, 0);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisX) == 1)
                    {
                        kt = new XYZ(3.3, 0, 0);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisZ) == -1)
                    {
                        kt = new XYZ(0, 0, -3.3);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisZ) == 1)
                    {
                        kt = new XYZ(0, 0, 3.3);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisY) == 1)
                    {
                        kt = new XYZ(0, 4, 0);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisY) == -1)
                    {
                        kt = new XYZ(0, -4, 0);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisX) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisY) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1)
                    {
                        kt = new XYZ(0, 0, 0);

                    }
                    newinstance = doc.Create.NewFamilyInstance(test1 - kt, symbol, doc.ActiveView);
                    #region: Create symbol    
                    newinstance.LookupParameter("Part No.").Set(s1);
                    newinstance.LookupParameter("TEXT 3").Set("CL" + " (" + list1.Count * list2.Count + ")");
                    newinstance.LookupParameter("TEXT 2").Set(lastname);
                    #endregion
                }

                else
                {
                    FamilyInstance newinstance = null;
                    #region: Create symbol
                    if (line1.Direction.DotProduct(XYZ.BasisX) == -1)
                    {
                        XYZ kt = new XYZ(0.7, 0, 0);
                        newinstance = doc.Create.NewFamilyInstance(test2 - kt, symbol, doc.ActiveView);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisX) == 1)
                    {
                        XYZ kt = new XYZ(0, 0, 0);
                        newinstance = doc.Create.NewFamilyInstance(test2 - kt, symbol, doc.ActiveView);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisZ) == 1)
                    {
                        XYZ kt = new XYZ(0, 0, 0);
                        newinstance = doc.Create.NewFamilyInstance(test2 - kt, symbol, doc.ActiveView);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisY) == 1)
                    {
                        XYZ kt = new XYZ(0, 3.2, 0);
                        newinstance = doc.Create.NewFamilyInstance(test2 - kt, symbol, doc.ActiveView);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisY) == -1)
                    {
                        XYZ kt = new XYZ(0, 0, 0);
                        newinstance = doc.Create.NewFamilyInstance(test2 - kt, symbol, doc.ActiveView);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisX) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisY) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1)
                    {
                        XYZ kt = new XYZ(0, 0, 0);
                        newinstance = doc.Create.NewFamilyInstance(test2 - kt, symbol, doc.ActiveView);
                    }

                    ICollection<ElementId> result = new List<ElementId>();
                    ICollection<ElementId> newresult = new List<ElementId>();
                    result.Add(newinstance.Id);
                    ElementTransformUtils.RotateElement(doc, newinstance.Id, line2, Math.PI * 0.5);
                    newresult = ElementTransformUtils.MirrorElements(doc, result, plane1, true);
                    doc.Delete(newinstance.Id);
                    FamilyInstance famiin = doc.GetElement(newresult.First()) as FamilyInstance;
                    famiin.LookupParameter("Part No.").Set(s1);
                    famiin.LookupParameter("TEXT 3").Set("CL" + " (" + list1.Count * list2.Count + ")");
                    famiin.LookupParameter("TEXT 2").Set(lastname);
                    #endregion
                }
                #endregion
            }

            else
            {
                List<XYZ> listendpoint1 = Support.GetStartEndDimensio(ele1);

                List<XYZ> TwoPointEndDim = Support.GetDimensionPointsEnd(doc, ele1);

                XYZ test1 = TwoPointEndDim[0];
                test1 = Support.ProjectOnto(plane, test1);
                XYZ test2 = TwoPointEndDim[1];
                test2 = Support.ProjectOnto(plane, test2);
                XYZ Point2 = XYZ.Zero;
                #region
                //if (line1.Direction.DotProduct(XYZ.BasisY) == -1)
                //{
                //    Point2 = test1 + kc1;
                //    Point2 = SUPPORT.PLANE.PlaneHelper.ProjectOnto(plane, Point2);
                //    DetailCurve cur1 = SUPPORT.LINE.LINE.createdetailcurve(doc, test1, Point2, plane);
                //}

                //if (Math.Round(line1.Direction.DotProduct(XYZ.BasisY), 2) == 1)
                //{
                //    Point2 = test1 + kc11;
                //    Point2 = SUPPORT.PLANE.PlaneHelper.ProjectOnto(plane, Point2);

                //    DetailCurve cur1 = SUPPORT.LINE.LINE.createdetailcurve(doc, test1, Point2, plane);
                //}

                //if (line1.Direction.DotProduct(XYZ.BasisZ) == 1)
                //{
                //    Point2 = test2 + kc2;
                //    Point2 = SUPPORT.PLANE.PlaneHelper.ProjectOnto(plane, Point2);
                //    test1 = SUPPORT.PLANE.PlaneHelper.ProjectOnto(plane, test1);
                //    DetailCurve cur1 = SUPPORT.LINE.LINE.createdetailcurve(doc, test2, Point2, plane);
                //}

                //if (line1.Direction.DotProduct(XYZ.BasisZ) == -1)
                //{
                //    Point2 = test1 + kc22;
                //    Point2 = SUPPORT.PLANE.PlaneHelper.ProjectOnto(plane, Point2);
                //    test1 = SUPPORT.PLANE.PlaneHelper.ProjectOnto(plane, test1);
                //    DetailCurve cur1 = SUPPORT.LINE.LINE.createdetailcurve(doc, test1, Point2, plane);
                //}

                //if (line1.Direction.DotProduct(XYZ.BasisX) == 1)
                //{
                //    Point1 = test2 - kc23;
                //    Point1 = SUPPORT.PLANE.PlaneHelper.ProjectOnto(plane, Point1);
                //    test1 = SUPPORT.PLANE.PlaneHelper.ProjectOnto(plane, test1);
                //    DetailCurve cur1 = SUPPORT.LINE.LINE.createdetailcurve(doc, test2, Point1, plane);
                //}

                //if (line1.Direction.DotProduct(XYZ.BasisX) == -1)
                //{
                //    Point1 = test1 + kc23;
                //    Point1 = SUPPORT.PLANE.PlaneHelper.ProjectOnto(plane, Point1);
                //    test1 = SUPPORT.PLANE.PlaneHelper.ProjectOnto(plane, test1);
                //    DetailCurve cur1 = SUPPORT.LINE.LINE.createdetailcurve(doc, test1, Point1, plane);
                //}
                ////if (line1.Direction.DotProduct(XYZ.BasisX) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisY) != -1 && line1.Direction.DotProduct(XYZ.BasisY) != 1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1 && line1.Direction.DotProduct(XYZ.BasisZ) != 1)
                ////{
                ////    CreateExtenline(doc, plane, ele1, 3.5);
                ////}

                //foreach (var point in listendpoint1)
                //{
                //    var distance = kc1.DistanceTo(point);
                //    if (distance < min1)
                //    {
                //        Point1 = point;
                //        min1 = distance;
                //    }
                //}
                #endregion

                #region: Create text
                List<Element> list1 = new List<Element>();

                for (int aa = 1; aa < ele1.Segments.Size; aa++)
                {
                    Reference ref1 = ele1.References.get_Item(aa);
                    Element element1 = doc.GetElement(ref1);
                    if (element1.Name != null && element1.Category.Name == "Specialty Equipment")
                    {
                        list1.Add(element1);
                    }
                }

                //XYZ pos = null;
                ElementId defaultTextTypeId = doc.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                TextNoteOptions opts = new TextNoteOptions(defaultTextTypeId);
                string s1 = list1.First().Name;

                XYZ vector = test1 - test2;
                Double x = vector.DotProduct(XYZ.BasisX);
                Double y = vector.DotProduct(XYZ.BasisY);
                Double z = vector.DotProduct(XYZ.BasisZ);

                FamilyInstance newinstance = null;

                if (Support.IsHorizontal(vector, doc.ActiveView) == true)
                {
                    XYZ kt = new XYZ(0, 0, 0);
                    if (line1.Direction.DotProduct(XYZ.BasisX) == -1)
                    {
                        kt = new XYZ(-3.2, 0, 0);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisX) == 1)
                    {
                        kt = new XYZ(3.2, 0, 0);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisZ) == -1)
                    {
                        kt = new XYZ(0, 0, -3.2);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisZ) == 1)
                    {
                        kt = new XYZ(0, 0, 0);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisY) == 1)
                    {
                        kt = new XYZ(0, 3.2, 0);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisY) == -1)
                    {
                        kt = new XYZ(0, (3.2 + Support.Totaldim(ele1)), 0);
                    }
                    if (line1.Direction.DotProduct(XYZ.BasisX) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisY) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1)
                    {
                        kt = new XYZ(0, (3.2 + Support.Totaldim(ele1)), 0);
                    }
                    newinstance = doc.Create.NewFamilyInstance(test1, symbol, doc.ActiveView);
                }

                if (Support.IsVertical(vector, doc.ActiveView) == true)
                {
                    XYZ kt = new XYZ(0, 0, 0);
                    if (line1.Direction.DotProduct(XYZ.BasisX) == 1)
                    {
                        kt = new XYZ(-(Support.Totaldim(ele1) + 3.2), 0, 0);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisX) == -1)
                    {
                        kt = new XYZ(-3.2, 0, 0);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisZ) == 1)
                    {
                        kt = new XYZ(0, 0, -(Support.Totaldim(ele1) + 3.2));

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisZ) == -1)
                    {
                        kt = new XYZ(0, 0, -3.2);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisY) == -1)
                    {
                        kt = new XYZ(0, 3.2, 0);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisY) == 1)
                    {
                        kt = new XYZ(0, 3.2, 0);

                    }
                    if (line1.Direction.DotProduct(XYZ.BasisX) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisY) != -1 && line1.Direction.DotProduct(XYZ.BasisX) != 1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1 && line1.Direction.DotProduct(XYZ.BasisZ) != -1)
                    {
                        kt = new XYZ(0, 0, 0);
                    }
                    newinstance = doc.Create.NewFamilyInstance(test1, symbol1, doc.ActiveView);
                }

                if (Support.IsHorizontal(vector, doc.ActiveView) == false && Support.IsVertical(vector, doc.ActiveView) == false)
                {
                    newinstance = doc.Create.NewFamilyInstance(test1, symbol, doc.ActiveView);
                }
                #endregion
                #region: Create location
                FilteredElementCollector filter = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_MultiCategoryTags);
                List<Element> listtag = filter.OfCategory(BuiltInCategory.OST_MultiCategoryTags).WhereElementIsNotElementType().ToElements().ToList();
                List<Element> sametag = new List<Element>();
                foreach (var item in listtag)
                {
                    IndependentTag el = doc.GetElement(item.Id) as IndependentTag;
                    Element tag1 = el.GetTaggedLocalElement();
                    string tag = tag1.Name;

                    if (tag == list1.First().Name)
                    {
                        sametag.Add(item);
                    }
                }
                try
                {
                    if (sametag.Count > 0)
                    {
                        Element elem1 = doc.GetElement(sametag.First().Id) as Element;
                        string samename = elem1.Name;
                        int gachngang = samename.LastIndexOf("-");
                        string lastname = samename.Substring(gachngang + 1);
                        string s11 = s1 + "\n" + "(" + lastname + ")";
                        #region: Create symbol    
                        newinstance.LookupParameter("Part No.").Set(s1);
                        newinstance.LookupParameter("TEXT 3").Set("CL" + " (" + list1.Count + ")");
                        newinstance.LookupParameter("TEXT 2").Set(lastname);
                        #endregion
                    }
                    else
                    {
                        newinstance.LookupParameter("Part No.").Set(s1);
                        newinstance.LookupParameter("TEXT 3").Set("CL" + " (" + list1.Count + ")");
                        newinstance.LookupParameter("TEXT 2").Set(" ");
                    }
                }
                catch { };

                #endregion
            }
        }

        private void CreateExtenline(Document doc, Plane plane, Dimension dim, Double a)
        {
            List<XYZ> listendpoint1 = Support.GetStartEndDimensio(dim);

            XYZ vectorAB = listendpoint1[0];
            vectorAB = Support.ProjectOnto(plane, vectorAB);
            XYZ vectorAC = listendpoint1[1];
            vectorAC = Support.ProjectOnto(plane, vectorAC);
            XYZ vectorCB = vectorAB - vectorAC;
            XYZ directionToB = vectorCB.Normalize();
            XYZ newB = vectorAB + (directionToB * a);
            newB = Support.ProjectOnto(plane, newB);
            Line line1 = Line.CreateBound(vectorAB, newB);
            line1 = Support.ProjectLineOnPlane(plane, line1);
            doc.Create.NewDetailCurve(doc.ActiveView, line1);
        }
    }
}

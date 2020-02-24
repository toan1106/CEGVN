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
    class FrozenSection : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string messages, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            Transaction trans = new Transaction(doc, "ExComm");
            trans.Start();

            DWGExportOptions options = new DWGExportOptions();
            //SUPPORT.PARAMETER.PARAMETER Para = new SUPPORT.PARAMETER.PARAMETER();
            View view = doc.ActiveView;
            ElementId eleid = view.Id;

            ICollection<ElementId> views = new List<ElementId>();
            FilteredElementCollector filter = new FilteredElementCollector(doc);
            FilteredElementCollector Filter = filter.OfCategory(BuiltInCategory.OST_Views).WhereElementIsNotElementType();
            IList<Element> list = Filter.ToElements();
            IList<Element> lishname = new List<Element>();
            ViewDrafting drafting = null;
            ElementId newlegend = null;
            string currentview = doc.ActiveView.ViewName;
            string test = "Zz_" + currentview + "##";
            ElementId id = null;
            View viewlegend = null;

            foreach (var ele1 in list)
            {
                Parameter parameter = ele1.get_Parameter(BuiltInParameter.VIEW_NAME);
                string viewname = parameter.AsString();
                ViewFamilyType vd = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
                .FirstOrDefault(q => q.ViewFamily == ViewFamily.Drafting);
                views.Add(doc.ActiveView.Id);
                doc.Export(@"C:\Autodesk", "exportdwg.dwg", views, options);
                drafting = ViewDrafting.Create(doc, vd.Id);
                doc.Regenerate();

                DWGImportOptions importOptions = new DWGImportOptions();
                importOptions.ColorMode = ImportColorMode.BlackAndWhite;
                doc.Import(@"C:\Autodesk\\exportdwg.dwg", importOptions, drafting, out id);
                try
                {
                    drafting.Name = test;
                    trans.Commit();
                    commandData.Application.ActiveUIDocument.ActiveView = drafting;
                }
                catch
                {
                    TaskDialog.Show("ERROR", "SECTION NÀY ĐÃ ĐƯỢC TẠO FROZEN");

                    trans.RollBack();
                    break;
                }
                break;
            }

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Transaction Name");

                Element curElem = doc.GetElement(id);
                ImportInstance curLink = (ImportInstance)curElem;
                List<GeometryObject> curveobject = GetLinkedDWGCurves(curLink, doc);

                foreach (GeometryObject curGeom in curveobject)
                {
                    if (curGeom.GetType() == typeof(PolyLine))
                    {
                        // create polyline in current view
                        PolyLine curPolyline = (PolyLine)curGeom;

                        // get polyline coordinate points
                        IList<XYZ> ptsList = curPolyline.GetCoordinates();

                        for (var i = 0; i <= ptsList.Count - 2; i++)
                        {
                            // create detail curve from polyline coordinates
                            try
                            {
                                DetailCurve newDetailLine = doc.Create.NewDetailCurve(doc.ActiveView, Line.CreateBound(ptsList[i], ptsList[i + 1]));
                            }
                            catch
                            {
                            }
                        }
                    }
                    else
                    {
                        try { DetailCurve newDetailLine = doc.Create.NewDetailCurve(doc.ActiveView, (Curve)curGeom); }
                        catch { }
                    }
                }

                FilteredElementCollector legCollector = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType();
                List<View> alllegends = new List<View>();
                foreach (ElementId eid in legCollector.ToElementIds())
                {
                    View v = doc.GetElement(eid) as View;
                    if (v.ViewType == ViewType.Legend)
                        alllegends.Add(v);
                }

                newlegend = alllegends.Last().Duplicate(ViewDuplicateOption.WithDetailing);
                viewlegend = doc.GetElement(newlegend) as View;
                tx.Commit();
            }

            using (Form_FrozenSection form = new Form_FrozenSection(doc, uiapp))
            {
                form.ShowDialog();

                if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return Result.Cancelled;
                }

                if (form.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    using (Transaction tx1 = new Transaction(doc))
                    {
                        tx1.Start("Transaction Name");



                        tx1.Commit();
                        uiapp.ActiveUIDocument.ActiveView = form.vsheet();
                    }
                }
                using (Transaction tx2 = new Transaction(doc))
                {
                    tx2.Start("Transaction Name");

                    XYZ pos = uidoc.Selection.PickPoint(ObjectSnapTypes.None, "Chon diem dat legend");
                    Viewport.Create(doc, form.vsheet().Id, newlegend, pos);

                    FilteredElementCollector allElementsInView = new FilteredElementCollector(doc, viewlegend.Id).WhereElementIsNotElementType();
                    ICollection<ElementId> elementsInView = allElementsInView.WhereElementIsNotElementType().Where(x => x.Category != null).Select(x => x.Id).ToList();
                    doc.Delete(elementsInView);

                    FilteredElementCollector detailine = new FilteredElementCollector(doc, drafting.Id);
                    IList<ElementId> listdetailine = detailine.OfCategory(BuiltInCategory.OST_Lines).WhereElementIsNotElementType().ToElementIds().ToList();

                    Transform tranform = ElementTransformUtils.GetTransformFromViewToView(viewlegend, doc.ActiveView);
                    ElementTransformUtils.CopyElements(drafting, listdetailine, viewlegend, tranform, new CopyPasteOptions());

                    viewlegend.Scale = 16;
                    Parameter viewname = viewlegend.LookupParameter("View Name");
                    string name = form.Viewname();
                    viewname.Set(name);
                    doc.Delete(drafting.Id);
                    tx2.Commit();
                }
            }
            return Result.Succeeded;
        }
        public List<GeometryObject> GetLinkedDWGCurves(ImportInstance curLink, Document curDoc)
        {
            // returns list of curves for linked DWG file
            List<GeometryObject> curveList = new List<GeometryObject>();
            Options curOptions = new Options();
            GeometryElement geoElement;
            GeometryInstance geoInstance;
            GeometryElement geoElem2;

            // get geometry from current link
            geoElement = curLink.get_Geometry(curOptions);

            foreach (GeometryObject geoObject in geoElement)
            {
                // convert geoObject to geometry instance
                geoInstance = (GeometryInstance)geoObject;
                geoElem2 = geoInstance.GetInstanceGeometry();

                foreach (GeometryObject curObj in geoElem2)
                    // add object to list
                    curveList.Add(curObj);
            }

            // return list of geometry
            return curveList;
        }
    }
}

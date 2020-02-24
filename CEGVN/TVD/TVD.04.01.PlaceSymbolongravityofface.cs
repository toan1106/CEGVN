#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;
#endregion

namespace TVDCEG
{
    [Transaction(TransactionMode.Manual)]
    public class PlaceSymbolongravityFacecmd : IExternalCommand
    {
        public List<FamilySymbol> listsymbol = new List<FamilySymbol>();
        public IList<CurveLoop> curves = new List<CurveLoop>();
        public XYZ center;
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            listsymbol = ListSymbol(doc);
            Reference rf = sel.PickObject(ObjectType.Face);
            curves = GetCurveLoops(doc, rf);
            var form = new FrmPlaceSymbolonGravity(this, doc);
            form.ShowDialog();
            return Result.Succeeded;
        }
        public void Placesymbol(Document doc, IList<CurveLoop> curveloop, FamilySymbol symbol)
        {
            XYZ point = Createsolid(doc, curveloop);
            using (Transaction tr = new Transaction(doc, "Place Symbol"))
            {
                tr.Start();
                doc.Create.NewFamilyInstance(point, symbol, doc.ActiveView);
                tr.Commit();
            }
        }
        XYZ Createsolid(Document doc, IList<CurveLoop> looplist)
        {
            View view = doc.ActiveView;
            XYZ point = view.ViewDirection;
            Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(looplist, point, 0.1);
            XYZ center = solid.ComputeCentroid();
            return center;
        }
        IList<CurveLoop> GetCurveLoops(Document doc, Reference rf)
        {
            IList<CurveLoop> curveloop = new List<CurveLoop>();
            GeometryObject geometryObject = doc.GetElement(rf).GetGeometryObjectFromReference(rf);
            try
            {
                PlanarFace planarFace = geometryObject as PlanarFace;
                curveloop = planarFace.GetEdgesAsCurveLoops();
            }
            catch
            {
                HermiteFace hermiteFace = geometryObject as HermiteFace;
                if (hermiteFace == null)
                {
                    RuledFace ruledFace = geometryObject as RuledFace;
                    curveloop = ruledFace.GetEdgesAsCurveLoops();
                }
                else
                {
                    curveloop = hermiteFace.GetEdgesAsCurveLoops();
                }
            }
            return curveloop;
        }
        List<FamilySymbol> ListSymbol(Document doc)
        {
            var symbol = (from FamilySymbol x in new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_GenericAnnotation).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>() where x.Category.CategoryType==CategoryType.Annotation select x).ToList();
            return symbol;
        }
    }
}

using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CEGVN.TVD
{
    public class lbr_
    {
        public static lbr_ _instance;
        private lbr_()
        {

        }
        public static lbr_ Instance => _instance ?? (_instance = new lbr_());
        public static IList<Solid> CreateSolid(Document doc, FamilyInstance familyInstance, Transform transform)
        {
            IList<Solid> solids = new List<Solid>();
            try
            {
                Curve curve = GetCurvemaxfamily(familyInstance);
                Element ele = doc.GetElement(familyInstance.Id);
                string[] parameters = new string[]
                {
                "DIM_WWF_YY",
                "DIM_LENGTH"
                };
                Parameter pa = LookupElementParameter(ele, parameters);
                double lengthcurve = pa.AsDouble();
                XYZ starpointfa = new XYZ(lengthcurve / 2, 0, 0);
                XYZ endpointfa = new XYZ(-lengthcurve / 2, 0, 0);
                XYZ startpoint = transform.OfPoint(starpointfa);
                XYZ endpoint = transform.OfPoint(endpointfa);
                XYZ norm = new XYZ((startpoint.X - endpoint.X), (startpoint.Y - endpoint.Y), (startpoint.Z - endpoint.Z));
                Plane plane = Plane.CreateByNormalAndOrigin(norm, startpoint);
                Transaction newtran = new Transaction(doc, "ss");
                newtran.Start();
                SketchPlane stk = SketchPlane.Create(doc, plane);
                XYZ pt1 = startpoint.Add(0.01 * plane.XVec).Add(0.01 * plane.YVec);
                XYZ pt2 = startpoint.Add(0.01 * plane.YVec);
                XYZ pt3 = startpoint.Add(-0.01 * plane.YVec);
                XYZ pt4 = startpoint.Add(0.01 * plane.XVec).Add(-0.01 * plane.YVec);
                XYZ pt5 = (-1) * norm;
                Line lineleft = Line.CreateBound(pt1, pt2);
                Line linetop = Line.CreateBound(pt2, pt3);
                Line lineright = Line.CreateBound(pt3, pt4);
                Line linebot = Line.CreateBound(pt4, pt1);
                CurveLoop profile = new CurveLoop();
                profile.Append(lineleft);
                profile.Append(linetop);
                profile.Append(lineright);
                profile.Append(linebot);
                IList<CurveLoop> listloop1 = new List<CurveLoop>();
                listloop1.Add(profile);
                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(listloop1, pt5, lengthcurve);
                newtran.Commit();
                solids.Add(solid);
            }
            catch
            {
                solids = null;
            }
            return solids;
        }
        public static Parameter LookupElementParameter(Element element, string[] parameters)
        {
            Parameter parameter = null;
            foreach (string name in parameters)
            {
                parameter = element.LookupParameter(name);
                bool flag = parameter != null;
                if (flag)
                {
                    break;
                }
            }
            return parameter;
        }
        public static Curve GetCurvemaxfamily(FamilyInstance familyInstance)
        {
            List<Curve> alllines = new List<Curve>();
            Curve curvesmax = null;
            Options option = new Options();
            option.ComputeReferences = true;
            option.IncludeNonVisibleObjects = false;
            if (familyInstance != null)
            {
                GeometryElement geoElement = familyInstance.get_Geometry(option);
                foreach (GeometryObject geoObject in geoElement)
                {
                    GeometryInstance instance = geoObject as GeometryInstance;
                    if (null != instance)
                    {
                        GeometryElement instanceGeometryElement = instance.GetSymbolGeometry();
                        foreach (GeometryObject o in instanceGeometryElement)
                        {
                            try
                            {
                                Solid solid = o as Solid;
                                if (solid != null)
                                {
                                    foreach (Edge item in solid.Edges)
                                    {
                                        alllines.Add(item.AsCurve());
                                    }
                                }
                            }
                            catch
                            {
                                Line line = o as Line;
                                alllines.Add(line as Curve);
                            }
                        }
                        curvesmax = Findcurvemax(alllines);
                    }
                }
            }
            return curvesmax;
        }
        static Curve Findcurvemax(List<Curve> curves)
        {
            Curve max = curves.First();
            foreach (var item in curves)
            {
                if (max.ApproximateLength < item.ApproximateLength)
                {
                    max = item;
                }
            }
            return max;
        }
        public static Curve GetCurveminfamily(FamilyInstance familyInstance)
        {
            IList<Curve> alllines = new List<Curve>();
            Curve curvesmin = null;
            Options option = new Options();
            option.ComputeReferences = true;
            option.IncludeNonVisibleObjects = false;
            if (familyInstance != null)
            {
                GeometryElement geoElement = familyInstance.get_Geometry(option);
                foreach (GeometryObject geoObject in geoElement)
                {
                    GeometryInstance instance = geoObject as GeometryInstance;
                    if (null != instance)
                    {
                        GeometryElement instanceGeometryElement = instance.GetSymbolGeometry();
                        foreach (GeometryObject o in instanceGeometryElement)
                        {
                            Line curve = o as Line;
                            if (curve != null)
                            {
                                alllines.Add(curve);
                            }
                        }
                        var membercurvmax = alllines.OrderByDescending(x => x.Length).LastOrDefault();
                        curvesmin = membercurvmax;
                    }
                }
            }
            return curvesmin;
        }

        public List<ReferencePlane> GetSketchPlanes(Document doc)
        {
            List<ReferencePlane> listskp = new List<ReferencePlane>();
            FilteredElementCollector colec = new FilteredElementCollector(doc).OfClass(typeof(ReferencePlane));
            var lop = colec.ToElements();
            foreach (var i in lop)
            {
                var i2 = i as ReferencePlane;
                listskp.Add(i2);
            }
            listskp.OrderBy(x => x.Name).ToList();
            return listskp;
        }

        public Dictionary<double, Level> GetLevelnearly(Document doc, FamilyInstance familyInstance)
        {
            Dictionary<double, Level> diclevel = new Dictionary<double, Level>();
            Level level = null;
            double min = 300;
            var listlevels = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>();
            Transform transform = familyInstance.GetTransform();
            XYZ xYZ1 = transform.BasisZ;
            LocationPoint locationPoint = familyInstance.Location as LocationPoint;
            BoundingBoxXYZ boundingBoxXYZ = familyInstance.get_BoundingBox(doc.ActiveView);
            XYZ poitbou = boundingBoxXYZ.Min;
            if (xYZ1.Z != 0)
            {
                foreach (var i in listlevels)
                {
                    var Elevationlevel = i.Elevation;
                    double space = Math.Abs(poitbou.Z) - Math.Abs(Elevationlevel);
                    if (space > 0 && space < min)
                    {
                        min = space;
                        level = i;
                    }
                }
                if (level != null)
                {
                    diclevel.Add(min, level);
                }
            }
            return diclevel;
        }
        public static List<FamilySymbol> StrandsPo(Document doc, List<Family> families)
        {
            List<FamilySymbol> Conns = new List<FamilySymbol>();
            foreach (Family Instance in families)
            {
                ElementId eleid = Instance.GetFamilySymbolIds().ElementAt(0);
                Element elemtype = doc.GetElement(eleid);
                FamilySymbol fasy = elemtype as FamilySymbol;
                Conns.Add(fasy);
            }
            return Conns;
        }
    }
    public static class Solidhelper
    {
        public static void GetSolidsFromSymbol(FamilyInstance familyInstance, ref List<Solid> solids)
        {
            Options option = new Options();
            option.ComputeReferences = true;
            option.IncludeNonVisibleObjects = true;
            option.DetailLevel = ViewDetailLevel.Undefined;
            if (familyInstance != null)
            {
                GeometryElement geoElement = familyInstance.get_Geometry(option);
                foreach (GeometryObject geoObject in geoElement)
                {
                    GeometryInstance instance = geoObject as Autodesk.Revit.DB.GeometryInstance;
                    if (null != instance)
                    {
                        Transform transform = familyInstance.GetTransform();
                        GeometryElement instanTVDCEGeometryElement = instance.GetSymbolGeometry(transform);
                        foreach (GeometryObject instObj in instanTVDCEGeometryElement)
                        {
                            // Try to find solid
                            Solid solid = instObj as Solid;
                            if (null == solid || 0 == solid.Faces.Size || 0 == solid.Edges.Size) continue;
                            if (!solids.Contains(solid)) solids.Add(solid);
                        }
                    }
                }
            }
        }
        public static void GetSolids(FamilyInstance familyInstance, ref List<Solid> solids)
        {
            Options option = new Options();
            option.ComputeReferences = true;
            option.IncludeNonVisibleObjects = true;
            option.DetailLevel = ViewDetailLevel.Undefined;
            if (familyInstance != null)
            {
                GeometryElement geoElement = familyInstance.get_Geometry(option);
                foreach (GeometryObject geoObject in geoElement)
                {
                    // Try to find solid
                    Solid solid = geoObject as Solid;
                    if (null == solid || 0 == solid.Faces.Size || 0 == solid.Edges.Size) continue;
                    if (!solids.Contains(solid)) solids.Add(solid);
                }
            }
        }
        public static void GetSolidsFromNested(FamilyInstance familyInstance, ref List<Solid> solids)
        {
            Document doc = familyInstance.Document;
            foreach (ElementId id in familyInstance.GetSubComponentIds())
            {
                FamilyInstance instance = doc.GetElement(id) as FamilyInstance;
                if (instance == null) continue;
                GetSolids(instance, ref solids);
                GetSolidsFromSymbol(instance, ref solids);
            }
        }
        public static List<Solid> AllSolids(FamilyInstance familyInstance)
        {
            List<Solid> allSolids = new List<Solid>();
            GetSolids(familyInstance, ref allSolids);
            GetSolidsFromSymbol(familyInstance, ref allSolids);
            GetSolidsFromNested(familyInstance, ref allSolids);
            return allSolids;
        }
    }
}



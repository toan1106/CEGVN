using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace CEGVN.TMP
{
    internal static class Support
    {
        public static Line ProjectLineOnPlane(Plane plane, Line line)
        {
            Line l = null;
            var p0 = line.GetEndPoint(0);
            var p1 = line.GetEndPoint(1);
            p0 = p0.ProjectOnto(plane);
            p1 = p1.ProjectOnto(plane);
            l = Line.CreateBound(p0, p1);
            return l;
        }
        public static XYZ ProjectOnto(this XYZ p, Plane plane)
        {
            XYZ source = p - plane.Origin;
            double d = plane.Normal.DotProduct(source);
            XYZ q = p - d * plane.Normal;
            return q;
        }
        public static bool IsHorizontal(this XYZ vecThis, View view)
        {
            return vecThis.IsPerpendicular(view.UpDirection);
        }
        public static bool IsVertical(this XYZ vecThis, View view)
        {
            return vecThis.IsPerpendicular(view.RightDirection);
        }
        public static bool IsPerpendicular(this XYZ v, XYZ w)
        {
            return 1E-09 < v.GetLength() && 1E-09 < v.GetLength() && 1E-09 > Math.Abs(v.DotProduct(w));
        }
        public static XYZ ExtendLineIntersection(Line l1, Line l2)
        {
            IntersectionResultArray resultArray;
            if (Line.CreateBound(l1.Origin + 10000.0 * l1.Direction, l1.Origin - 10000.0 * l1.Direction).Intersect((Curve)Line.CreateBound(l2.Origin + 10000.0 * l2.Direction, l2.Origin - 10000.0 * l2.Direction), out resultArray) != SetComparisonResult.Overlap)
                throw new InvalidOperationException("Input lines did not intersect.");
            if (resultArray == null || resultArray.Size != 1)
                throw new InvalidOperationException("Could not extract line intersection point.");
            return resultArray.get_Item(0).XYZPoint;
        }
        public static List<XYZ> GetStartEndDimensio(Dimension dim)
        {
            Line dimLine = dim.Curve as Line;
            if (dimLine == null) return null;
            List<XYZ> pts = new List<XYZ>();

            dimLine.MakeBound(0, 1);
            XYZ pt1 = dimLine.GetEndPoint(0);
            XYZ pt2 = dimLine.GetEndPoint(1);
            XYZ direction = pt2.Subtract(pt1).Normalize();

            if (0 == dim.Segments.Size)
            {
                XYZ v = 0.5 * (double)dim.Value * direction;
                pts.Add(GetDimensionStartPoint(dim) - v);
                pts.Add(GetDimensionStartPoint(dim) + v);
            }
            else
            {
                XYZ p = GetDimensionStartPoint(dim);
                foreach (DimensionSegment seg in dim.Segments)
                {
                    XYZ v = (double)seg.Value * direction;
                    if (0 == pts.Count)
                    {
                        pts.Add(p = (GetDimensionStartPoint(dim) - 0.5 * v));
                    }
                    pts.Add(p = p.Add(v));
                }
            }
            return pts;
        }
        public static XYZ GetDimensionStartPoint(Dimension dim)
        {
            XYZ p = null;

            try
            {
                p = dim.Origin;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException ex)
            {
                Debug.Assert(ex.Message.Equals("Cannot access this method if this dimension has more than one segment."));

                foreach (DimensionSegment seg in dim.Segments)
                {
                    p = seg.Origin;
                    break;
                }
            }
            return p;
        }
        public static List<XYZ> GetDimensionPointsEnd(Document doc, Dimension dim)
        {
            XYZ p = null;

            try
            {
                p = dim.Origin;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException ex)
            {
                Debug.Assert(ex.Message.Equals("Cannot access this method if this dimension has more than one segment."));

                foreach (DimensionSegment seg in dim.Segments)
                {
                    p = seg.Origin;
                    break;
                }
            }

            Line dimLine = dim.Curve as Line;
            if (dimLine == null) return null;
            List<XYZ> pts = new List<XYZ>();
            List<XYZ> last = new List<XYZ>();
            dimLine.MakeBound(0, 1);
            XYZ pt1 = dimLine.GetEndPoint(0);
            XYZ pt2 = dimLine.GetEndPoint(1);
            XYZ direction = pt2.Subtract(pt1).Normalize();

            if (0 == dim.Segments.Size)
            {
                XYZ v = 0.5 * (double)dim.Value * direction;
                pts.Add(p - v);
                pts.Add(p + v);
            }
            else
            {
                foreach (DimensionSegment seg in dim.Segments)
                {
                    XYZ v = (double)seg.Value * direction;
                    if (0 == pts.Count)
                    {
                        pts.Add(p = (p - 0.5 * v));
                    }
                    pts.Add(p = p.Add(v));
                }
                foreach (var item in pts)
                {
                    last.Add(pts[0]);
                    last.Add(pts[dim.Segments.Size]);
                }
            }
            return last;
        }
        public static XYZ ProjectOnto(this Plane plane, XYZ p)
        {
            XYZ source = p - plane.Origin;

            double d = plane.Normal.DotProduct(source);

            XYZ q = p - d * plane.Normal;

            return q;
        }
        public static DetailCurve createdetailcurve(Document doc, XYZ xyz1, XYZ xyz2, Plane plane)
        {
            Line line = Line.CreateBound(xyz1, xyz2);
            line = ProjectLineOnPlane(plane, line);
            DetailCurve detailcurve = doc.Create.NewDetailCurve(doc.ActiveView, line);
            return detailcurve;

        }
        public static FamilySymbol GetSymbol(Document document, string familyName, string symbolName)
        {
            return new FilteredElementCollector(document).OfClass(typeof(Family)).OfType<Family>().FirstOrDefault(f => f.Name.Equals(familyName))?.GetFamilySymbolIds().Select(id => document.GetElement(id)).OfType<FamilySymbol>().FirstOrDefault(symbol => symbol.Name.Equals(symbolName));
        }
        public static Double Totaldim(Dimension dim)
        {
            double total = 0;

            foreach (DimensionSegment dimSeg in dim.Segments)
            {
                total += (double)dimSeg.Value;
            }
            return total;
        }
    }
}

#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#endregion

namespace CEGVN.TVD
{
    public class CutVoidConnection
    {
        private static CutVoidConnection _instance;
        private CutVoidConnection()
        {

        }
        public static CutVoidConnection Instance => _instance ?? (_instance = new CutVoidConnection());
        public Dictionary<string, List<FamilyInstance>> GetAllConnection(Document doc)
        {
            Dictionary<string, List<FamilyInstance>> dic = new Dictionary<string, List<FamilyInstance>>();
            var Conn = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_SpecialityEquipment).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().ToList();
            foreach (var i in Conn)
            {
                ElementId faid = i.GetTypeId();
                Element elemtype = doc.GetElement(faid);
                Autodesk.Revit.DB.Parameter pa = elemtype.LookupParameter("MANUFACTURE_COMPONENT");
                if (pa != null)
                {
                    var vla = pa.AsString();
                    if (vla.Contains("CONNECTION"))
                    {
                        if (dic.ContainsKey(i.Name))
                        {
                            dic[i.Name].Add(i);
                        }
                        else
                        {
                            dic.Add(i.Name, new List<FamilyInstance> { i });
                        }
                    }
                }
            }
            return dic;
        }
        public List<FamilyInstance> FindVoidConnection(Document doc, FamilyInstance familyInstance)
        {
            List<FamilyInstance> list = new List<FamilyInstance>();
            var fg = familyInstance.GetSubComponentIds();
            foreach (var item in fg)
            {
                FamilyInstance element = doc.GetElement(item) as FamilyInstance;
                if (element.Category.IsCuttable)
                {
                    list.Add(element);
                }
            }
            return list;
        }
        public List<FamilyInstance> FindProductVoidcut(Document doc, List<FamilyInstance> Voids)
        {
            List<FamilyInstance> list = new List<FamilyInstance>();
            foreach (var item in Voids)
            {
                var solids = Solidhelper.AllSolids(item);
                foreach (var solid in solids)
                {
                    ICollection<ElementId> col = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).WherePasses(new ElementIntersectsSolidFilter(solid)).ToElementIds();
                }
            }
            return list;
        }
        public void Cutting(Document doc, List<FamilyInstance> listcut, List<FamilyInstance> listconn)
        {
            ProgressBarform progressBarform = new ProgressBarform(listconn.Count, "Cutting");
            progressBarform.Show();
            foreach (var conn in listconn)
            {
                progressBarform.giatri();
                if (progressBarform.iscontinue == false)
                {
                    break;
                }
                using (Transaction tran = new Transaction(doc, "Cut Void"))
                {
                    tran.Start();
                    foreach (var framming in listcut)
                    {
                        try
                        {
                            if (InstanceVoidCutUtils.CanBeCutWithVoid(framming))
                            {
                                InstanceVoidCutUtils.AddInstanceVoidCut(doc, framming, conn);
                            }
                        }
                        catch
                        {

                        }
                    }
                    tran.Commit();
                }
            }
            progressBarform.Close();
        }
        public Dictionary<string, List<FamilyInstance>> Getallframing(Document doc)
        {
            Dictionary<string, List<FamilyInstance>> dic = new Dictionary<string, List<FamilyInstance>>();
            List<FamilyInstance> list = new List<FamilyInstance>();
            var structural = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFraming).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().ToList();
            foreach (var item in structural)
            {
                if (item.GetSubComponentIds().Count == 1 || item.GetSubComponentIds().Count == 0)
                {
                    list.Add(item);
                }
            }
            foreach (var item in list)
            {
                if (dic.ContainsKey(item.Symbol.FamilyName))
                {
                    dic[item.Symbol.FamilyName].Add(item);
                }
                else
                {
                    dic.Add(item.Symbol.FamilyName, new List<FamilyInstance> { item });
                }
            }
            return dic;
        }
    }
    public class CegProduct
    {
        public string CONSTRUCTION_PRODUCT { get; set; }
        public CegProduct(Document doc, Element element)
        {
            ElementId faid = element.GetTypeId();
            Element elemtype = doc.GetElement(faid);
            Parameter pa = elemtype.LookupParameter("CONSTRUCTION_PRODUCT");
            if (pa != null)
            {
                CONSTRUCTION_PRODUCT = pa.AsString();
            }
        }
    }
}

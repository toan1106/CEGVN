using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using c= Autodesk.Revit.ApplicationServices;

namespace CEGVN.TVD
{
    public class lbr_view
    {
        public List<View> GetViews(Document doc)
        {
            List<View> listview = new List<View>();
            List<View> listview_after = new List<View>();
            listview = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfClass(typeof(View)).ToElements().Cast<View>().ToList();
            foreach (View view in listview)
            {
                if (view.ViewType == ViewType.DraftingView ||
                    view.ViewType == ViewType.Legend)
                {
                    if (view.IsTemplate) continue;
                    if (view.ViewType == ViewType.Schedule && view.Name.Contains("Revision Schedule"))
                    {
                        continue;
                    }
                    listview_after.Add(view);
                }
            }
            return listview_after;
        }
    }

}

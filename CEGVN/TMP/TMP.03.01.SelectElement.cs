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
    class SelectElement : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Selection sel = uidoc.Selection;
            List<AssemblyInstance> listchosen = new List<AssemblyInstance>();
            List<Element> listchosen2 = new List<Element>();
            List<AssemblyInstance> listassemblies = new List<AssemblyInstance>();
            List<Element> listelement = new List<Element>();
            ICollection<ElementId> listkq = new List<ElementId>();
            ICollection<ElementId> listkq2 = new List<ElementId>();

            using (FormSelectElements form = new FormSelectElements(doc, uiapp))
            {
                form.ShowDialog();

                if (form.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                {
                    return Result.Cancelled;
                }

                if (form.DialogResult == System.Windows.Forms.DialogResult.OK)

                {
                    using (Transaction tx = new Transaction(doc))
                    {
                        int i = 0;
                        int j = 0;
                        tx.Start("Transaction Name");

                        listchosen = form.newlist();

                        FilteredElementCollector assemblies = new FilteredElementCollector(doc).OfClass(typeof(AssemblyInstance));
                        foreach (var ass in assemblies)
                        {
                            AssemblyInstance assinstance = doc.GetElement(ass.Id) as AssemblyInstance;
                            if (assinstance != null)
                            {
                                listassemblies.Add(assinstance);
                            }
                        }

                        for (i = 0; i < listchosen.Count; i++)
                        {
                            for (j = i; j < listassemblies.Count; j++)
                            {
                                if (listchosen[i].AssemblyTypeName == listassemblies[j].AssemblyTypeName)
                                {
                                    listkq.Add(listassemblies[j].Id);
                                }
                            }
                        }
                        //uidoc.Selection.SetElementIds(listkq);


                        listchosen2 = form.newlist2();
                        ElementClassFilter familyinstance = new ElementClassFilter(typeof(FamilyInstance));
                        ElementCategoryFilter framingfilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
                        FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
                        List<Element> framing = collector.WherePasses(framingfilter).WhereElementIsNotElementType().ToElements().ToList();
                        foreach (var ele in framing)
                        {
                            if (ele != null)
                            {
                                listelement.Add(ele);
                            }
                        }
                        for (i = 0; i < listchosen2.Count; i++)
                        {
                            for (j = i; j < framing.Count; j++)
                            {
                                try
                                {
                                    if ((listchosen2[i].LookupParameter("CONTROL_MARK").AsString()) == (framing[j].LookupParameter("CONTROL_MARK").AsString()))
                                    {
                                        listkq.Add(framing[j].Id);
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }
                        //TaskDialog.Show("so luong", listkq2.Count + "");
                        uidoc.Selection.SetElementIds(listkq);

                        tx.Commit();
                    }
                }
                return Result.Succeeded;
            }
        }
    }
}

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CEGVN.TMP
{
    public partial class FormSelectElements : System.Windows.Forms.Form
    {
        List<AssemblyInstance> listassemblies = new List<AssemblyInstance>();
        List<Element> listcontrolmark = new List<Element>();
        List<Element> framing = new List<Element>();
        UIApplication uiapp;
        Document doc;
        public FormSelectElements(Document doc, UIApplication uiapp)
        {
            InitializeComponent();
            this.uiapp = uiapp;
            this.doc = doc;
            FilteredElementCollector assemblies = new FilteredElementCollector(doc).OfClass(typeof(AssemblyInstance));
            listView1.View = System.Windows.Forms.View.Details;

            ElementClassFilter familyinstance = new ElementClassFilter(typeof(FamilyInstance));
            ElementCategoryFilter framingfilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming);
            FilteredElementCollector collector = new FilteredElementCollector(doc, doc.ActiveView.Id);
            framing = collector.WherePasses(framingfilter).WhereElementIsNotElementType().ToElements().ToList();
            listView2.View = System.Windows.Forms.View.Details;
            foreach (var ass in assemblies)
            {
                ListViewItem item = new ListViewItem();
                AssemblyInstance assinstance = doc.GetElement(ass.Id) as AssemblyInstance;
                if (assinstance != null)
                {
                    listassemblies.Add(assinstance);
                    item.SubItems.Add(assinstance.AssemblyTypeName);
                }
                listView1.Items.Add(item);
            }
            foreach (var fra in framing)
            {
                ListViewItem item = new ListViewItem();
                if (fra != null)
                {
                    try
                    {
                        Parameter controlmark = fra.LookupParameter("CONTROL_MARK");
                        listcontrolmark.Add(fra);
                        item.SubItems.Add(controlmark.AsString());
                        Parameter controlnumber = fra.LookupParameter("CONTROL_NUMBER");
                        item.SubItems.Add(controlnumber.AsString());
                    }
                    catch
                    {

                    }
                    string name = fra.Name;
                    item.SubItems.Add(name);
                }
                listView2.Items.Add(item);
            }
        }
        List<AssemblyInstance> filterList1 = new List<AssemblyInstance>();
        List<Element> filterList2 = new List<Element>();
        List<AssemblyInstance> sheetList1 = new List<AssemblyInstance>();
        List<Element> sheetList2 = new List<Element>();

        private void Assemblies_TextChanged(object sender, EventArgs e)
        {
            filterList1 = filterDataFromListView(Assemblies.Text);
            listView1.Items.Clear();
            try
            {
                sheetList1 = filterList1.Cast<AssemblyInstance>().OrderBy(x => x.Name).ToList();
            }
            catch
            {

            }

            foreach (AssemblyInstance el in sheetList1)
            {
                ListViewItem item = new ListViewItem();
                item.SubItems.Add(el.AssemblyTypeName);
                listView1.Items.Add(item);
            }
        }
        private void Control_Mark_TextChanged(object sender, EventArgs e)
        {
            filterList2 = filterDataFromListViewElement(Control_Mark.Text);
            listView2.Items.Clear();
            try
            {
                sheetList2 = filterList2.Cast<Element>().OrderBy(x => x.LookupParameter("CONTROL_MARK").AsString()).ToList();
            }
            catch
            {

            }

            foreach (Element el in sheetList2)
            {
                ListViewItem item = new ListViewItem();
                try
                {
                    item.SubItems.Add(el.LookupParameter("CONTROL_MARK").AsString());
                    item.SubItems.Add(el.LookupParameter("CONTROL_NUMBER").AsString());
                }
                catch
                {

                }
                item.SubItems.Add(el.Name);
                listView2.Items.Add(item);
            }
        }

        private void loadAllDataToListView(IList<Element> list, ListView lv)
        {
            foreach (Element e in list)
            {
                ListViewItem item = new ListViewItem();

                item.SubItems.Add(e.Name);

                lv.Items.Add(item);
            }
        }

        private void Form_SearchAssemblies_Load(object sender, EventArgs e)
        {
            this.Assemblies.Select();
        }

        List<AssemblyInstance> list1 = new List<AssemblyInstance>();
        private List<AssemblyInstance> filterDataFromListView(string text)
        {
            list1.Clear();
            foreach (AssemblyInstance el in listassemblies)
            {
                if (el.AssemblyTypeName.Contains(text) && text != "")
                {
                    list1.Add(el);
                }
            }
            return text == "" ? listassemblies : list1;
        }

        List<Element> list2 = new List<Element>();
        private List<Element> filterDataFromListViewElement(string text)
        {
            list2.Clear();
            foreach (Element el in framing)
            {
                try
                {
                    if ((el.LookupParameter("CONTROL_MARK").AsString()).Contains(text))
                    {
                        list2.Add(el);
                    }
                }
                catch
                {

                }
            }
            return text == "" ? framing : list2;
        }

        private List<int> listIndex = new List<int>();
        private List<int> listIndex2 = new List<int>();
        private void OK_Click(object sender, EventArgs e)
        {
            var listItem = listView1.SelectedItems;
            foreach (ListViewItem item in listItem)
            {
                listIndex.Add(item.Index);
            }
            var listItem2 = listView2.SelectedItems;
            foreach (ListViewItem item in listItem2)
            {
                listIndex2.Add(item.Index);
            }
        }

        public List<AssemblyInstance> newlist()
        {
            List<AssemblyInstance> nlist = new List<AssemblyInstance>();
            foreach (int index in listIndex)
            {
                if (Assemblies.Text == "")
                {
                    nlist.Add(listassemblies[index]);
                }
                else
                {
                    nlist.Add(sheetList1[index]);
                }
            }
            return nlist;
        }

        public List<Element> newlist2()
        {
            List<Element> nlist = new List<Element>();
            foreach (int index in listIndex2)
            {
                if (Control_Mark.Text == "")
                {
                    nlist.Add(framing[index]);
                }
                else
                {
                    nlist.Add(sheetList2[index]);
                }
            }
            return nlist;
        }

        private void FormSelectElements_Load(object sender, EventArgs e)
        {

        }
    }
}

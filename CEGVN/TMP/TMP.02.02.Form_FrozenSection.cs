using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace CEGVN.TMP
{
    public partial class Form_FrozenSection : System.Windows.Forms.Form
    {
        List<Element> allSheet = new List<Element>();
        UIApplication uiapp;
        Document doc;
        public Boolean radioButton1_Checked()
        {
            if (radioButton1.Checked == false)
            {
                return false;
            }
            radioButton1.Checked = true;

            return true;
        }

        public Boolean radioButton2_Checked()
        {
            if (radioButton2.Checked == false)
            {
                return false;
            }
            radioButton2.Checked = true;

            return true;
        }
        public Form_FrozenSection(Document doc, UIApplication uiapp)
        {
            InitializeComponent();
            this.uiapp = uiapp;
            this.doc = doc;

            allSheet = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Sheets).ToElements().ToList();

            lvSheet.View = System.Windows.Forms.View.Details;

            int stt = 0;

            foreach (Element e in allSheet)
            {
                ViewSheet sheet = e as ViewSheet;

                ListViewItem item = new ListViewItem();

                stt++;

                item.Text = stt + "";

                item.SubItems.Add(sheet.SheetNumber);
                item.SubItems.Add(sheet.Name);

                lvSheet.Items.Add(item);
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

        List<Element> filterList = new List<Element>();

        List<ViewSheet> sheetList = new List<ViewSheet>();

        private void tbSearch_TextChanged(object sender, EventArgs e)
        {
            filterList = filterDataFromListView(tbSearch.Text);

            if (radioButton1_Checked())
            {
                sheetList = filterList.Cast<ViewSheet>().OrderBy(x => x.Name).ToList();
            }

            if (radioButton2_Checked())
            {
                sheetList = filterList.Cast<ViewSheet>().OrderBy(x => x.SheetNumber).ToList();
            }

            lvSheet.Items.Clear();

            int stt = 0;

            ViewSheet sheet = null;

            foreach (Element el in sheetList)
            {
                sheet = el as ViewSheet;

                ListViewItem item = new ListViewItem();

                stt++;

                item.Text = stt + "";

                item.SubItems.Add(sheet.Name);

                item.SubItems.Add(sheet.SheetNumber);

                lvSheet.Items.Add(item);

                //this.lvSheet.Refresh();
            }

        }

        private void Form_SearchSheet_Load(object sender, EventArgs e)
        {
            this.tbSearch.Select();
        }

        private List<Element> filterDataFromListView(string text)
        {

            List<Element> list = new List<Element>();

            foreach (Element el in allSheet)
            {

                ViewSheet vs = el as ViewSheet;

                if (radioButton1_Checked())
                {
                    if (el.Name.Contains(text))
                    {
                        list.Add(el);
                    }
                }

                if (radioButton2_Checked())
                {
                    if (vs.SheetNumber.Contains(text))
                    {
                        list.Add(el);
                    }
                }

            }

            return list.Count == 0 ? allSheet : list;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (filterList.Count == 0) filterList = allSheet;

            var listItem = lvSheet.SelectedItems;

            foreach (var item in listItem)
            {
                ListViewItem lvitem = item as ListViewItem;
                int index = int.Parse(lvitem.Text);

                //get viewsheet
                ViewSheet vs = sheetList[index - 1];

                if (vs != null) openView(vs, uiapp);
            }
        }

        public ViewSheet vsheet()
        {
            ViewSheet vs = null;
            if (filterList.Count == 0) filterList = allSheet;

            var listItem = lvSheet.SelectedItems;

            foreach (var item in listItem)
            {
                ListViewItem lvitem = item as ListViewItem;
                int index = int.Parse(lvitem.Text);

                //get viewsheet
                vs = sheetList[index - 1];
            }
            return vs;
        }

        private void openView(Autodesk.Revit.DB.View view, UIApplication uiapp)
        {
            uiapp.ActiveUIDocument.ActiveView = view;
        }

        public string Viewname()
        {
            return textBox1.Text;
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form_FrozenSection_Load(object sender, EventArgs e)
        {

        }
    }
}

#region Namespaces
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace CEGVN.TVD
{
    public class CheckWorkPlaneEvent : EventRegisterHandler
    {
        private CheckWorkPlanecmd _data;
        private Document _doc;
        public CheckWorkPlaneEvent(CheckWorkPlanecmd data,Document doc)
        {
            this._data = data;
            _doc = doc;
        }
        public override void DoingSomething()
        {
            _data.Showview(_doc, _data.list);
        }
    }
}
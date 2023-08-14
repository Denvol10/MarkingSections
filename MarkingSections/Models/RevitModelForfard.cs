using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using BridgeDeck.Models;
using MarkingSections.Models;
using MarkingSections.Infrastructure;

namespace MarkingSections
{
    public class RevitModelForfard
    {
        private UIApplication Uiapp { get; set; } = null;
        private Application App { get; set; } = null;
        private UIDocument Uidoc { get; set; } = null;
        private Document Doc { get; set; } = null;

        public RevitModelForfard(UIApplication uiapp)
        {
            Uiapp = uiapp;
            App = uiapp.Application;
            Uidoc = uiapp.ActiveUIDocument;
            Doc = uiapp.ActiveUIDocument.Document;
        }

        #region Проверка на то существуют линии оси и линии на поверхности в модели
        public bool IsLinesExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(DirectShape));
        }
        #endregion

        #region Ось трассы
        public PolyCurve RoadAxis { get; set; }

        private string _roadAxisElemIds;

        public string RoadAxisElemIds
        {
            get => _roadAxisElemIds;
            set => _roadAxisElemIds = value;
        }

        public void GetPolyCurve()
        {
            var curves = RevitGeometryUtils.GetCurvesByRectangle(Uiapp, out _roadAxisElemIds);
            RoadAxis = new PolyCurve(curves);
        }
        #endregion

        #region Получение оси трассы из Settings
        public void GetAxisBySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            var lines = RevitGeometryUtils.GetCurvesById(Doc, elemIds);
            RoadAxis = new PolyCurve(lines);
        }
        #endregion

        #region Начальная линия
        public Curve StartLine { get; set; }

        private string _startLineElemIds;
        public string StartLineElemIds
        {
            get => _startLineElemIds;
            set => _startLineElemIds = value;
        }

        public void GetStartLine()
        {
            StartLine = RevitGeometryUtils.GetStartLine(Uiapp, out _startLineElemIds);
        }

        public void GetStartLineBySettings(string elementId)
        {
            StartLine = RevitGeometryUtils.GetStartLineById(Doc, elementId);
        }
        #endregion

        #region Проверка на то существует начальная линия
        public bool IsStartLineExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(ModelLine));
        }
        #endregion

        #region Создание линий
        public void CreateLines(double distBetweenLines, bool isChangeDirection, int countLines)
        {
            double startParameter;
            bool isStartLineIntersectAxis = RoadAxis.Intersect(StartLine, out _, out startParameter);
            if(!isStartLineIntersectAxis)
            {
                TaskDialog.Show("Предупреждение", "Линия не пересекается с осью");
                return;
            }

            var newLineParameters = new List<double>();

            double dist = UnitUtils.ConvertToInternalUnits(distBetweenLines, UnitTypeId.Meters);
            for(int i = 1; i <= countLines; i++)
            {
                if(isChangeDirection)
                {
                    newLineParameters.Add(startParameter + dist * i);
                }
                else
                {
                    newLineParameters.Add(startParameter - dist * i);
                }
            }

            if(!(newLineParameters.Min() >= 0 && newLineParameters.Max() <= RoadAxis.GetLength()))
            {
                TaskDialog.Show("Предупреждение", "Линии выходят за границы оси");
                return;
            }

            using(Transaction trans = new Transaction(Doc, "Created Model Lines"))
            {
                trans.Start();
                foreach(var parameter in newLineParameters)
                {
                    XYZ point = RoadAxis.GetPointOnPolycurve(parameter, out _);
                    var refPoint = Doc.FamilyCreate.NewReferencePoint(point);
                }
                trans.Commit();
            }
        }
        #endregion

    }
}

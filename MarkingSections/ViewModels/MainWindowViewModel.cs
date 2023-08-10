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
using System.Windows.Input;
using MarkingSections.Infrastructure;

namespace MarkingSections.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private RevitModelForfard _revitModel;

        internal RevitModelForfard RevitModel
        {
            get => _revitModel;
            set => _revitModel = value;
        }

        #region Заголовок
        private string _title = "Разметка секций";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        #region Элементы оси трассы
        private string _roadAxisElemIds;

        public string RoadAxisElemIds
        {
            get => _roadAxisElemIds;
            set => Set(ref _roadAxisElemIds, value);
        }
        #endregion

        #region Начальная линия
        private string _startLineElemIds;

        public string StartLineElemIds
        {
            get => _startLineElemIds;
            set => Set(ref _startLineElemIds, value);
        }
        #endregion

        #region Команды

        #region Получение оси трассы
        public ICommand GetRoadAxisCommand { get; }

        private void OnGetRoadAxisCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetPolyCurve();
            RoadAxisElemIds = RevitModel.RoadAxisElemIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadAxisCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение начальной линии
        public ICommand GetStartLineCommand { get; }

        private void OnGetStartLineCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetStartLine();
            StartLineElemIds = RevitModel.StartLineElemIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetStartLineCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Закрыть окно
        public ICommand CloseWindowCommand { get; }

        private void OnCloseWindowCommandExecuted(object parameter)
        {
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        private void SaveSettings()
        {
            Properties.Settings.Default["RoadAxisElemIds"] = RoadAxisElemIds;
            Properties.Settings.Default["StartLineElemIds"] = StartLineElemIds;
            Properties.Settings.Default.Save();
        }


        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Инициализация значения оси из Settings
            if (!(Properties.Settings.Default["RoadAxisElemIds"] is null))
            {
                string axisElemIdInSettings = Properties.Settings.Default["RoadAxisElemIds"].ToString();
                if(RevitModel.IsLinesExistInModel(axisElemIdInSettings) && !string.IsNullOrEmpty(axisElemIdInSettings))
                {
                    RoadAxisElemIds = axisElemIdInSettings;
                    RevitModel.GetAxisBySettings(axisElemIdInSettings);
                }
            }
            #endregion

            #region Инициализация значения стартовой линии
            if (!(Properties.Settings.Default["StartLineElemIds"] is null))
            {
                string startLineElemIdInSettings = Properties.Settings.Default["StartLineElemIds"].ToString();
                if(RevitModel.IsStartLineExistInModel(startLineElemIdInSettings) && !string.IsNullOrEmpty(startLineElemIdInSettings))
                {
                    StartLineElemIds = startLineElemIdInSettings;
                    RevitModel.GetStartLineBySettings(startLineElemIdInSettings);
                }
            }
            #endregion

            #region Команды
            GetRoadAxisCommand = new LambdaCommand(OnGetRoadAxisCommandExecuted, CanGetRoadAxisCommandExecute);
            GetStartLineCommand = new LambdaCommand(OnGetStartLineCommandExecuted, CanGetStartLineCommandExecute);
            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            #endregion
        }

        public MainWindowViewModel() { }
        #endregion
    }
}

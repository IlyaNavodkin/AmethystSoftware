using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
namespace AmethystSoftware
{
    public class AmethystRedMainViewModel : INotifyPropertyChanged
    {

        #region 1. Properties of MainViewModel 

        public ExternalCommandData CommandData;
        public Document doc;
        public UIApplication app;
        public UIDocument uidoc;


        private RevitTask _revitTask = new RevitTask();

        public AmethystRedModel AmethystRedCommand;

        public ObservableCollection<Family> AllFamiliesOfHorizontalPlug { get; set; }
        public ObservableCollection<Family> AllFamiliesOfVerticalPlug { get; set; }



        #region 1.2. Select Horizontal type of family of plug 

        private Family _horizontalFamiliyOfPlug { get; set; }
        public Family HorizontalFamiliyOfPlug
        {
            get { return _horizontalFamiliyOfPlug; }
            set
            {
                _horizontalFamiliyOfPlug = value;
                OnPropertyChanged("HorizontalFamiliyOfPlug");
            }
        }
        private List<FamilySymbol> _familySymbolsOfHorizontalFamiliyOfPlug { get; set; }
        public List<FamilySymbol> FamilySymbolsOfHorizontalFamiliyOfPlug
        {
            get { return _familySymbolsOfHorizontalFamiliyOfPlug; }
            set
            {
                _familySymbolsOfHorizontalFamiliyOfPlug = value;
                OnPropertyChanged("FamilySymbolsOfHorizontalFamiliyOfPlug");
            }
        }
        private FamilySymbol _familySymbolOfHorizontalFamiliyOfPlug { get; set; }
        public FamilySymbol FamilySymbolOfHorizontalFamiliyOfPlug
        {
            get { return _familySymbolOfHorizontalFamiliyOfPlug; }
            set
            {
                _familySymbolOfHorizontalFamiliyOfPlug = value;
                OnPropertyChanged("FamilySymbolOfHorizontalFamiliyOfPlug");
            }
        }

        private RelayCommand _selectHorizontalFamiliyOfPlug;
        public RelayCommand SelectHorizontalFamiliyOfPlug
        {
            get
            {
                return _selectHorizontalFamiliyOfPlug ??
                (_selectHorizontalFamiliyOfPlug = new RelayCommand(obj =>
                {
                    FamilySymbolsOfHorizontalFamiliyOfPlug = HorizontalFamiliyOfPlug
                    .GetFamilySymbolIds()
                    .Select(fsid => doc.GetElement(fsid))
                    .Cast<FamilySymbol>().ToList();
                    //FamilySymbolsOfHorizontalFamiliyOfPlug.Select(i => MessageBox.Show($"{i.Name}"));
                    //MessageBox.Show($"{HorizontalFamiliyOfPlug.Name}");

                }));
            }
        }
        private RelayCommand _selectHorizontalFamiliySymbolOfPlug;
        public RelayCommand SelectHorizontalFamiliySymbolOfPlug
        {
            get
            {
                return _selectHorizontalFamiliySymbolOfPlug ??
                (_selectHorizontalFamiliySymbolOfPlug = new RelayCommand(obj =>
                {
                    if (FamilySymbolOfHorizontalFamiliyOfPlug != null)
                    {
                        //MessageBox.Show($"{FamilySymbolOfHorizontalFamiliyOfPlug.Name}");
                    }

                }));
            }
        }
        #endregion
        #region 1.3. Select Vertical type of family of plug 

        private Family _verticalFamiliyOfPlug { get; set; }
        public Family VerticalFamiliyOfPlug
        {
            get { return _verticalFamiliyOfPlug; }
            set
            {
                _verticalFamiliyOfPlug = value;
                OnPropertyChanged("VerticalFamiliyOfPlug");
            }
        }
        private List<FamilySymbol> _familySymbolsOfVerticalFamiliyOfPlug { get; set; }
        public List<FamilySymbol> FamilySymbolsOfVerticalFamiliyOfPlug
        {
            get { return _familySymbolsOfVerticalFamiliyOfPlug; }
            set
            {
                _familySymbolsOfVerticalFamiliyOfPlug = value;
                OnPropertyChanged("FamilySymbolsOfVerticalFamiliyOfPlug");
            }
        }
        private FamilySymbol _familySymbolOfVerticalFamiliyOfPlug { get; set; }
        public FamilySymbol FamilySymbolOfVerticalFamiliyOfPlug
        {
            get { return _familySymbolOfVerticalFamiliyOfPlug; }
            set
            {
                _familySymbolOfVerticalFamiliyOfPlug = value;
                OnPropertyChanged("FamilySymbolOfVerticalFamiliyOfPlug");
            }
        }

        private RelayCommand _selectVerticalFamiliyOfPlug;
        public RelayCommand SelectVerticalFamiliyOfPlug
        {
            get
            {
                return _selectVerticalFamiliyOfPlug ??
                (_selectVerticalFamiliyOfPlug = new RelayCommand(obj =>
                {
                    FamilySymbolsOfVerticalFamiliyOfPlug = VerticalFamiliyOfPlug
                    .GetFamilySymbolIds()
                    .Select(fsid => doc.GetElement(fsid))
                    .Cast<FamilySymbol>().ToList();

                }));
            }
        }
        private RelayCommand _selectVerticalFamiliySymbolOfPlug;
        public RelayCommand SelectVerticalFamiliySymbolOfPlug
        {
            get
            {
                return _selectVerticalFamiliySymbolOfPlug ??
                (_selectVerticalFamiliySymbolOfPlug = new RelayCommand(obj =>
                {
                    if (FamilySymbolOfVerticalFamiliyOfPlug != null)
                    {
                        //MessageBox.Show($"{FamilySymbolOfVerticalFamiliyOfPlug.Name}");
                    }

                }));
            }
        }

        private RelayCommand _runTheFirstCommand;
        public RelayCommand RunTheFirstCommand
        {
            get
            {
                return _runTheFirstCommand ??
                (_runTheFirstCommand = new RelayCommand(obj =>
                {
                    if (_familySymbolOfVerticalFamiliyOfPlug == null |
                    _familySymbolOfHorizontalFamiliyOfPlug == null |
                    _horizontalFamiliyOfPlug == null |
                    _verticalFamiliyOfPlug == null
                    )
                    {
                        TaskDialog.Show("Ошибка", "Выберите семейство или тип семейства отверстия");
                    }

                    else
                    {
                        AmethystRedCommand.FamilySymbolOfHorizontalFamiliyOfPlug = FamilySymbolOfHorizontalFamiliyOfPlug;
                        AmethystRedCommand.FamilySymbolOfVerticalFamiliyOfPlug = FamilySymbolOfVerticalFamiliyOfPlug;
                        RunFirstCommand();
                    }

                }));
            }
        }
        #endregion
        #region 1.4. Select round function and round count 

        private bool _roundPlug { get; set; }
        public bool RoundPlug
        {
            get { return _roundPlug; }
            set
            {

                _roundPlug = value;
                OnPropertyChanged("RoundPlug");
                this.AmethystRedCommand.NotRoundPlug = value;
            }
        }
        private string _valueOfStockString { get; set; }
        public string ValueOfStockString
        {
            get { return _valueOfStockString; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _valueOfStockString = value;
                    OnPropertyChanged("ValueOfStockString");
                }
                else
                {
                    if (value.All(char.IsDigit))
                    {
                        _valueOfStockString = value;
                        OnPropertyChanged("ValueOfStockString");
                        ValueOfStockInt = int.Parse(value);
                    }
                    else
                    {
                        TaskDialog.Show("Ошибка!", "Введите целое числовое значение для запаса отверстия!");
                    }
                }

            }
        }
        private int _valueOfStockInt { get; set; }
        public int ValueOfStockInt
        {
            get { return _valueOfStockInt; }
            set
            {
                _valueOfStockInt = value;
                OnPropertyChanged("ValueOfStockInt");
                this.AmethystRedCommand.ValueOfStockInt = value;
            }
        }

        private string _valueOfRoundString { get; set; }
        public string ValueOfRoundString
        {
            get { return _valueOfRoundString; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _valueOfRoundString = value;
                    OnPropertyChanged("ValueOfRoundString");
                }
                else
                {
                    if (value.All(char.IsDigit))
                    {
                        _valueOfRoundString = value;
                        OnPropertyChanged("ValueOfRoundString");
                        ValueOfRoundInt = int.Parse(value);
                    }
                    else
                    {
                        TaskDialog.Show("Ошибка!", "Введите целое числовое значение для запаса отверстия!");
                    }
                }

            }
        }
        private int _valueOfRoundInt { get; set; }
        public int ValueOfRoundInt
        {
            get { return _valueOfRoundInt; }
            set
            {
                _valueOfRoundInt = value;
                OnPropertyChanged("ValueOfRoundInt");
                this.AmethystRedCommand.ValueOfRoundInt = value;
            }
        }

        #endregion


        #endregion



        #region 2. Initilization of MainViewModel 

        public AmethystRedMainViewModel(ExternalCommandData commandData)
        {
            this.CommandData = commandData;
            this.doc = CommandData.Application.ActiveUIDocument.Document;
            this.app = CommandData.Application;
            this.uidoc = app.ActiveUIDocument;

            this.AmethystRedCommand = new AmethystRedModel(commandData);
            this.AllFamiliesOfHorizontalPlug = new ObservableCollection<Family>(AmethystRedCommand.AllFamiliesOfHorizontalPlug);
            this.AllFamiliesOfVerticalPlug = new ObservableCollection<Family>(AmethystRedCommand.AllFamiliesOfVerticalPlug);



            //Команда кнопки  -  запуск
            //this.UICommandInsertPlug = new UiCommand(RunUICommandInsertPlug);

        }
        #endregion



        #region 3. Run commands in context Revit API 
        private void RunFirstCommand()
        {
            //CommandResult resultOfFirstCommand = FirstCommandElement.Run();
            //Запуск команды в  контексте API через RevitTask
            _revitTask.Run((uiApp) => AmethystRedCommand.Run());
        }


        # endregion

        #region 4. MVVM events and methods of MainViewModel 

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}
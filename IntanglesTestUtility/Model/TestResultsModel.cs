using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntanglesTestUtility.Model
{
    public class TestResultsModel : INotifyPropertyChanged
    {
        private bool _isSelected;
        private string _parameter;
        private string _result;

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsSelected {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                RaisePropertyChange("IsSelected");
            }
        }
        public string Parameter
        {
            get
            {
                return _parameter;
            }

            set
            {
                _parameter = value;
                RaisePropertyChange("Parameter");
            }
        }
        public string Result
        { get
            {
                return  _result;
            }
            set {
                _result = value;
                RaisePropertyChange("Result");
            }
        }

        public void RaisePropertyChange(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
    }
}

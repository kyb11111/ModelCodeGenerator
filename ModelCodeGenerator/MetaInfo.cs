using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ModelCodeGenerator
{
    public class MetaInfo : INotifyPropertyChanged
    {
        private bool m_IsGenerateCode = true;
        public bool IsGenerateCode
        {
            get { return m_IsGenerateCode; }
            set 
            {
                if (m_IsGenerateCode == value)
                    return;
                m_IsGenerateCode = value; 
                OnPropertyChanged("IsGenerateCode"); 
            }
        }

        private bool m_HasError = false;
        public bool HasError
        {
            get { return m_HasError; }
            set
            {
                if (m_HasError != value)
                {
                    m_HasError = value;
                    OnPropertyChanged("HasError");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

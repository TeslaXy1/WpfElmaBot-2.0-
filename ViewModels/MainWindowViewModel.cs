using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfElmaBot_2._0_.ViewModels.Base;

namespace WpfElmaBot_2._0_.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        #region Статус состояния
        
        private string _Status;
        /// <summary>
        /// состояние программы
        /// </summary>
        public string Status
        {
            get => _Status;
            set=>Set(ref _Status, value);   
        }
        #endregion

        #region Консоль 
        private string _Consol;
        /// <summary>
        /// свойство консоли
        /// </summary>
        public string Consol
        {
            get => _Consol;
            set => Set(ref _Consol, value);
        }
        #endregion
    }
}

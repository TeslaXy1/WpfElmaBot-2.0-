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
        #region заголовок окна
        /// <summary>
        /// Заголовок
        /// </summary>
        private string _Tittle="Its a tittle";
        public string Tittle
        {
            get => _Tittle;
            set=>Set(ref _Tittle, value);   
        }
        #endregion
    }
}

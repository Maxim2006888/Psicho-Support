using Psicho_Support.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Psicho_Support.Views.Pages
{
    public class BasePage : UserControl
    {
        protected Users CurrentUser => AppSession.CurrentUser;
        public BasePage()
        {
            // Все страницы будут получать пользователя через AppSession
        }
    }
}

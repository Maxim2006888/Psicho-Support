using Psicho_Support.Data;
using System.Linq;

namespace Psicho_Support.Views.Pages
{
    public partial class NotesPage : BasePage
    {
        public NotesPage()
        {
            InitializeComponent();
            LoadNotes();
        }

        private void LoadNotes()
        {
            using (var db = new HealthPsicho_DBEntities())
            {
                var notes = db.Notes
                    .Where(n => n.UserID == CurrentUser.UserID)
                    .ToList();

                NotesList.Items.Clear();

                if (notes.Any())
                {
                    foreach (var n in notes)
                        NotesList.Items.Add($"📝 {n.Title}");
                }
                else
                {
                    NotesList.Items.Add("У вас пока нет заметок.");
                }
            }
        }
    }
}

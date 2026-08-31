using System;
using System.Windows.Forms;

namespace TiendaUNNE
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Crea el administrador inicial si la tabla Usuario está vacía.
            try
            {
                Bootstrap.AsegurarAdministradorInicial();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo verificar / crear el usuario inicial:\n\n" + ex.Message,
                    "TiendaUNNE", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            using (var login = new frmLogin())
            {
                if (login.ShowDialog() != DialogResult.OK || !SesionActual.HaySesion)
                    return;   // canceló o falló el login -> se cierra la aplicación
            }

            // El rol queda disponible en SesionActual.Usuario.Rol dentro de frmPrincipal.
            Application.Run(new frmPrincipal());
        }
    }
}

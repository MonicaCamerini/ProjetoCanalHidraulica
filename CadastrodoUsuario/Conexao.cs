using System.Data.SQLite;

namespace ProjetoCanaldeHidraulica.CadastrodoUsuario
{
    public class Conexao
    {
        public SQLiteConnection conn = new SQLiteConnection("Data Source=CadastroLogin.db3"); //conn é um objeto
        public SQLiteConnection conn2 = new SQLiteConnection("Data Source=Relatório.db");
        public SQLiteConnection conn3 = new SQLiteConnection("Data Source=AlarmeseFalhas.db");


        public void conectar()
        {
            conn.Open();
            conn2.Open();
            conn3.Open();

        }

        public void desconectar()
        {
            conn.Close();
            conn2.Close();
            conn3.Close();

        }
    }
}

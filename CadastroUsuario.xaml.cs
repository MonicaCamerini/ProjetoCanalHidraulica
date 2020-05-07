using System;
using System.Windows;
using System.Data.SQLite;
using ProjetoCanaldeHidraulica.CadastrodoUsuario;


namespace ProjetoCanaldeHidraulica
{
    /// <summary>
    /// Lógica interna para CadastroUsuario.xaml
    /// </summary>
    public partial class CadastroUsuario : Window
    {

        public CadastroUsuario()
        {
            InitializeComponent();
        }

        private void CadastrarUsuario_Click(object sender, RoutedEventArgs e)
        {
            if (CadastrodoUsuario.Text == "" || CadastroSenha.Password == "" || CadastroConfirmarSenha.Password == "" || CadastrodoNome.Text == "")
            {
                MessageBox.Show("Confirme as informações preenchidas. Todos os campos são obrigatórios!", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                CadastrodoUsuario.Focus();
            }

            if (CadastroSenha.Password != CadastroConfirmarSenha.Password)
            {
                MessageBox.Show("As senhas não coincidem!", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                CadastroSenha.Clear();
                CadastroConfirmarSenha.Clear();
                CadastrodoUsuario.Focus();
            }

            else
            {
                Conexao con = new Conexao();

                try
                {
                    con.conectar();
                    String sqlite = "INSERT INTO Login(Usuario, Senha, ConfirmarSenha, Nome) VALUES ('" + CadastrodoUsuario.Text + "', '" + CadastroSenha.Password + "', '" + CadastroConfirmarSenha.Password + "', '"+ CadastrodoNome.Text + "')";
                    SQLiteCommand comando = new SQLiteCommand(sqlite, con.conn);
                    _ = comando.ExecuteNonQuery();

                    MessageBox.Show("Registro efetuado com sucesso!", "Cadastrado(a)", MessageBoxButton.OK, MessageBoxImage.Information);
                    CadastroSenha.Clear();
                    CadastroConfirmarSenha.Clear();

                    con.desconectar();

                    

                }

                catch (Exception)
                {
                    MessageBox.Show("Erro de cadastro! Não pode haver usuários cadastrados com o mesmo nome.", "Atenção!", MessageBoxButton.OK, MessageBoxImage.Error);
                    CadastroSenha.Clear();
                    CadastroConfirmarSenha.Clear();
                }

            }
        }
    }
}

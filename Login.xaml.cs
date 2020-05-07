using System.Windows;
using System;
using System.Data.SQLite;
using ProjetoCanaldeHidraulica.CadastrodoUsuario;
using System.Data;

namespace ProjetoCanaldeHidraulica
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window                                     
    {
       public MainWindow()
       {
           InitializeComponent();                                              
        }

        public static string usuarioConectado;



        #region Botão para Login                                                      

        private void BotaoLogin_Click(object sender, RoutedEventArgs e)
        {
            if (CaixaUsuario.Text == "" || CaixaSenha.Password == "")
            {
                MessageBox.Show("Preencha com as informações de Usuário e Senha!", "Formulário Incompleto", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                CaixaSenha.Clear();
                CaixaUsuario.Focus();
            }

            else
            {
                Conexao con = new Conexao(); //criando objeto para a chamada da classe Conexão
                try //tentar acessar a base de dados
                {
                    con.conectar(); //função conectar ques esta dentro da classe Conexão
                    string sqlite = "SELECT * FROM Login WHERE Usuario = '" + CaixaUsuario.Text + "' AND Senha = '" + CaixaSenha.Password + "' "; //Atenção com o uso das aspas!
                    SQLiteDataAdapter dados = new SQLiteDataAdapter(sqlite, con.conn); //Realizando a Query de consulta
                    DataTable Login = new DataTable(); //Criando DataTable para receber dados do banco
                    dados.Fill(Login); //passando os meus dados do DataAdapter para o DataTable

                    if (Login.Rows.Count == 0) //testando se existe algum registro
                    {
                        MessageBox.Show("Registro não encontrado!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        CaixaSenha.Clear();
                        CaixaUsuario.Focus();
                        this.Focus();
                    }


                    else
                    {
                        MessageBox.Show("Login realizado com sucesso!", "Bem Vindo(a)" + " " + Login.Rows[0]["Usuario"].ToString(), MessageBoxButton.OK, MessageBoxImage.Information);
                        usuarioConectado = CaixaUsuario.Text;         //Pega o valor de "Usuario" da linha que foi lida
                        this.Hide();                                                    //torna a janela de login invisível 
                        Window1 window = new Window1();                                 //criação da variável window que "armazena" a classe Window1 que pertence a janela da Página Principal
                        window.ShowDialog();                                            //mostra a janela da Página Principal  
                        this.Close();                                                   //fecha a janela de Login 
                    }
                }
                catch (Exception E)
                {
                    MessageBox.Show(E.Message.ToString(), "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            con.desconectar();
            }
        }


        #endregion Botão para Login                                             //encerra o bloco de código "Botão para Login"

        private void CadastrarNovoUsuario_Click(object sender, RoutedEventArgs e)
        {
            CadastroUsuario cadastro = new CadastroUsuario();
            cadastro.ShowDialog();
        }

    }
}

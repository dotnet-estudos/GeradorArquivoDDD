using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GeradorArquivoDDD.Dominio;

namespace WFGeradorArquivoDDD
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            this.txtCaminhoArquivo.Text = Path.GetDirectoryName(Application.ExecutablePath);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IList<string> entidades;
            try
            {
                entidades = GeradorClasses.ParseEntidades(ListaClassesTXT.Text);
            }
            catch (ArgumentException exception)
            {
                MessageBox.Show(exception.Message, "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (entidades.Count == 0)
            {
                MessageBox.Show("Digite os nomes das classes", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var arquivos = new GeradorClasses().Gerar(CarregarArquivo());

                progressBar1.Minimum = 0;
                progressBar1.Maximum = arquivos.Count;
                progressBar1.Value = 0;
                progressBar1.Step = 1;

                foreach (var arquivo in arquivos)
                {
                    StatusLabel1.Text = String.Format("Gerando {0} ...", Path.GetFileName(arquivo.Caminho));
                    GerarArquivo(arquivo.Caminho, arquivo.Conteudo);
                    progressBar1.PerformStep();
                    StatusLabel2.Text = String.Format("{0}%", progressBar1.Value * 100 / progressBar1.Maximum);
                    statusStrip1.Refresh();
                }

                MessageBox.Show(String.Format("{0} arquivo(s) gerado(s) com sucesso.", arquivos.Count),
                    "Sucesso!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static void GerarArquivo(string fileName, string conteudo)
        {
            var diretorio = Path.GetDirectoryName(fileName);
            if (!String.IsNullOrEmpty(diretorio) && !Directory.Exists(diretorio))
            {
                Directory.CreateDirectory(diretorio);
            }

            File.WriteAllText(fileName, conteudo, new UTF8Encoding(true));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = @"Arquivo Xml|*.xml";
            openFileDialog1.FileName = txtCaminhoArquivo.Text;
            openFileDialog1.InitialDirectory = Path.GetDirectoryName(txtCaminhoArquivo.Text);

            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtCaminhoArquivo.Text = openFileDialog1.FileName;

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SalvarArquivo();
        }

        private void SalvarArquivo()
        {
            var arquivoSave =  CarregarArquivo();

            var conteudoXml = Global.Instance.Serializar(arquivoSave);

            if (!txtCaminhoArquivo.Text.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            {
                txtCaminhoArquivo.Text = Path.Combine(txtCaminhoArquivo.Text, "Configuracao.xml");
            }

            GerarArquivo(txtCaminhoArquivo.Text, conteudoXml);

            MessageBox.Show("Arquivo salvo com sucesso!");

        }

        private Configuracao CarregarArquivo()
        {
            var ArquivoSave = new Configuracao();

            ArquivoSave.DirInterfaceApplication = this.CaminhoInterfaceApplicationtxt.Text;
            ArquivoSave.DirInterfaceRepository = this.CaminhoInterfaceRepositoryTxt.Text;
            ArquivoSave.DirInterfaceService = this.CaminhoIterfaceServiceTxt.Text;
            ArquivoSave.DirRepository = this.CaminhoRepositoryTxt.Text;
            ArquivoSave.DirApplication = this.CaminhoApplicationTxt.Text;
            ArquivoSave.DirService = this.CaminhoServiceTxt.Text;

            ArquivoSave.SufixoApplication = this.SufixoApplicaion.Text;
            ArquivoSave.SufixoRepository = this.SufixoRepository.Text;
            ArquivoSave.SufixoService = this.SufixoService.Text;
            ArquivoSave.SufixoBase = this.SufixoBase.Text;

            ArquivoSave.UsingApplication = this.UsingApplication.Text;
            ArquivoSave.UsingEntities = this.UsingEntites.Text;
            ArquivoSave.UsingInterfaceApplication = this.UsingIApplication.Text;
            ArquivoSave.UsingInterfaceRepository = this.UsingIRepository.Text;
            ArquivoSave.UsingInterfaceService = this.UsingIService.Text;
            ArquivoSave.UsingRepository = this.UsingRepository.Text;
            ArquivoSave.UsingService = this.UsingService.Text;

            ArquivoSave.Entidades = this.ListaClassesTXT.Text;

            return ArquivoSave;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CarregarArquivoXml();
        }

        private void CarregarArquivoXml()
        {
            if (!File.Exists(txtCaminhoArquivo.Text))
            {
                MessageBox.Show("Arquivo de configuração não encontrado.", "Alerta", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var ArquivoSave = GetArquivoXML(txtCaminhoArquivo.Text);
            if (ArquivoSave == null)
            {
                MessageBox.Show("O arquivo selecionado não é uma configuração válida.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.CaminhoInterfaceApplicationtxt.Text = ArquivoSave.DirInterfaceApplication;
            this.CaminhoInterfaceRepositoryTxt.Text = ArquivoSave.DirInterfaceRepository;
            this.CaminhoIterfaceServiceTxt.Text = ArquivoSave.DirInterfaceService;
            this.CaminhoRepositoryTxt.Text = ArquivoSave.DirRepository;
            this.CaminhoApplicationTxt.Text = ArquivoSave.DirApplication;
            this.CaminhoServiceTxt.Text = ArquivoSave.DirService;

            this.SufixoApplicaion.Text = ArquivoSave.SufixoApplication;
            this.SufixoRepository.Text = ArquivoSave.SufixoRepository;
            this.SufixoService.Text = ArquivoSave.SufixoService;
            this.SufixoBase.Text = ArquivoSave.SufixoBase;

            this.UsingApplication.Text = ArquivoSave.UsingApplication;
            this.UsingEntites.Text = ArquivoSave.UsingEntities;
            this.UsingIApplication.Text = ArquivoSave.UsingInterfaceApplication;
            this.UsingIRepository.Text = ArquivoSave.UsingInterfaceRepository;
            this.UsingIService.Text = ArquivoSave.UsingInterfaceService;
            this.UsingRepository.Text = ArquivoSave.UsingRepository;
            this.UsingService.Text = ArquivoSave.UsingService;

            this.ListaClassesTXT.Text = ArquivoSave.Entidades;
        }

        private Configuracao GetArquivoXML(string fileName)
        {

            var conteudoxml = File.ReadAllText(fileName);
            return Global.Instance.FromXml<Configuracao>(conteudoxml);
        }
    }
}

using System;
using System.IO;
using System.Linq;
using GeradorArquivoDDD.Dominio;
using NUnit.Framework;

namespace GeradorArquivoDDD.Tests
{
    [TestFixture]
    public class GeradorClassesTests
    {
        private static Configuracao ConfiguracaoPadrao(string entidades)
        {
            return new Configuracao
            {
                DirInterfaceRepository = Path.Combine("saida", "Domain", "Interfaces", "Repositories"),
                DirRepository = Path.Combine("saida", "Infra", "Repositories"),
                DirInterfaceService = Path.Combine("saida", "Domain", "Interfaces", "Services"),
                DirService = Path.Combine("saida", "Domain", "Services"),
                DirInterfaceApplication = Path.Combine("saida", "Application", "Interface"),
                DirApplication = Path.Combine("saida", "Application"),
                UsingEntities = "Loja.Domain.Entities",
                UsingInterfaceRepository = "Loja.Domain.Interfaces.Repositories",
                UsingRepository = "Loja.Infra.Data.Repositories",
                UsingInterfaceService = "Loja.Domain.Interfaces.Services",
                UsingService = "Loja.Domain.Services",
                UsingInterfaceApplication = "Loja.Application.Interface",
                UsingApplication = "Loja.Application",
                SufixoRepository = "Repository",
                SufixoService = "Service",
                SufixoApplication = "AppService",
                SufixoBase = "Base",
                Entidades = entidades
            };
        }

        [Test]
        public void ParseEntidades_RemoveEspacosVaziosERepetidos()
        {
            var entidades = GeradorClasses.ParseEntidades(" Cliente ; Produto;;Cliente; ");

            CollectionAssert.AreEqual(new[] { "Cliente", "Produto" }, entidades);
        }

        [Test]
        public void ParseEntidades_AceitaQuebraDeLinhaEVirgula()
        {
            var entidades = GeradorClasses.ParseEntidades("Cliente\nProduto,Pedido");

            CollectionAssert.AreEqual(new[] { "Cliente", "Produto", "Pedido" }, entidades);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ;  ")]
        public void ParseEntidades_ListaVazia_RetornaNenhuma(string lista)
        {
            Assert.That(GeradorClasses.ParseEntidades(lista), Is.Empty);
        }

        [TestCase("Cliente Novo")]
        [TestCase("1Cliente")]
        [TestCase("Cliente-Novo")]
        public void ParseEntidades_NomeInvalido_LancaExcecao(string nome)
        {
            var ex = Assert.Throws<ArgumentException>(() => GeradorClasses.ParseEntidades(nome));
            StringAssert.Contains(nome, ex.Message);
        }

        [Test]
        public void Gerar_CriaSeisArquivosPorEntidade()
        {
            var arquivos = new GeradorClasses().Gerar(ConfiguracaoPadrao("Cliente;Produto"));

            Assert.That(arquivos.Count, Is.EqualTo(2 * GeradorClasses.ArquivosPorEntidade));
        }

        [Test]
        public void Gerar_NomeiaArquivosPelaEntidadeESufixo()
        {
            var nomes = new GeradorClasses().Gerar(ConfiguracaoPadrao("Cliente"))
                .Select(a => Path.GetFileName(a.Caminho))
                .ToArray();

            CollectionAssert.AreEqual(new[]
            {
                "IClienteRepository.cs",
                "ClienteRepository.cs",
                "IClienteService.cs",
                "ClienteService.cs",
                "IClienteAppService.cs",
                "ClienteAppService.cs"
            }, nomes);
        }

        [Test]
        public void Gerar_UsaDiretorioConfiguradoParaCadaCamada()
        {
            var config = ConfiguracaoPadrao("Cliente");
            var arquivos = new GeradorClasses().Gerar(config);

            Assert.That(arquivos[0].Caminho, Is.EqualTo(Path.Combine(config.DirInterfaceRepository, "IClienteRepository.cs")));
            Assert.That(arquivos[3].Caminho, Is.EqualTo(Path.Combine(config.DirService, "ClienteService.cs")));
            Assert.That(arquivos[5].Caminho, Is.EqualTo(Path.Combine(config.DirApplication, "ClienteAppService.cs")));
        }

        [Test]
        public void Gerar_InterfaceRepository_HerdaDaInterfaceBase()
        {
            var conteudo = new GeradorClasses().Gerar(ConfiguracaoPadrao("Cliente"))[0].Conteudo;

            StringAssert.Contains("using Loja.Domain.Entities;", conteudo);
            StringAssert.Contains("namespace Loja.Domain.Interfaces.Repositories", conteudo);
            StringAssert.Contains("public interface IClienteRepository : IRepositoryBase<Cliente>", conteudo);
        }

        [Test]
        public void Gerar_Service_InjetaRepositoryNoConstrutor()
        {
            var conteudo = new GeradorClasses().Gerar(ConfiguracaoPadrao("Cliente"))[3].Conteudo;

            StringAssert.Contains("public class ClienteService : ServiceBase<Cliente>, IClienteService", conteudo);
            StringAssert.Contains("private readonly IClienteRepository _clienteRepository;", conteudo);
            StringAssert.Contains("public ClienteService(IClienteRepository clienteRepository)", conteudo);
            StringAssert.Contains(": base(clienteRepository)", conteudo);
            StringAssert.Contains("_clienteRepository = clienteRepository;", conteudo);
        }

        [Test]
        public void Gerar_Application_InjetaServiceNoConstrutor()
        {
            var conteudo = new GeradorClasses().Gerar(ConfiguracaoPadrao("Produto"))[5].Conteudo;

            StringAssert.Contains("public class ProdutoAppService : AppServiceBase<Produto>, IProdutoAppService", conteudo);
            StringAssert.Contains("public ProdutoAppService(IProdutoService produtoService)", conteudo);
            StringAssert.Contains(": base(produtoService)", conteudo);
        }

        [Test]
        public void Gerar_NaoDeixaEspacosNoFimDasLinhas()
        {
            var arquivos = new GeradorClasses().Gerar(ConfiguracaoPadrao("Cliente"));

            foreach (var arquivo in arquivos)
            {
                var linhas = arquivo.Conteudo.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                Assert.That(linhas.Where(l => l != l.TrimEnd()), Is.Empty, arquivo.Caminho);
            }
        }

        [Test]
        public void Gerar_ChavesBalanceadas()
        {
            foreach (var arquivo in new GeradorClasses().Gerar(ConfiguracaoPadrao("Cliente")))
            {
                Assert.That(arquivo.Conteudo.Count(c => c == '{'),
                    Is.EqualTo(arquivo.Conteudo.Count(c => c == '}')), arquivo.Caminho);
            }
        }

        [Test]
        public void Gerar_SemConfiguracao_LancaExcecao()
        {
            Assert.Throws<ArgumentNullException>(() => new GeradorClasses().Gerar(null));
        }

        [TestCase("Cliente", "cliente")]
        [TestCase("ItemPedido", "itemPedido")]
        [TestCase("X", "x")]
        public void NomeVariavel_PrimeiraLetraMinuscula(string entidade, string esperado)
        {
            Assert.That(GeradorClasses.NomeVariavel(entidade), Is.EqualTo(esperado));
        }
    }
}

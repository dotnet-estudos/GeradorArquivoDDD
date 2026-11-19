using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GeradorArquivoDDD.Dominio
{
    /// <summary>
    /// Arquivo de código gerado: caminho de destino e conteúdo.
    /// </summary>
    public class ArquivoGerado
    {
        public ArquivoGerado(string caminho, string conteudo)
        {
            Caminho = caminho;
            Conteudo = conteudo;
        }

        public string Caminho { get; private set; }

        public string Conteudo { get; private set; }
    }

    /// <summary>
    /// Gera as classes e interfaces de Repository, Service e Application
    /// para cada entidade informada, seguindo a configuração de diretórios,
    /// namespaces e sufixos.
    /// </summary>
    public class GeradorClasses
    {
        /// <summary>Quantidade de arquivos gerados por entidade.</summary>
        public const int ArquivosPorEntidade = 6;

        private static readonly Regex IdentificadorValido = new Regex(@"^[A-Za-z_][A-Za-z0-9_]*$");

        /// <summary>
        /// Converte a lista separada por ";" em nomes de entidade, removendo
        /// espaços, entradas vazias e repetidas.
        /// </summary>
        public static IList<string> ParseEntidades(string lista)
        {
            if (string.IsNullOrWhiteSpace(lista))
            {
                return new List<string>();
            }

            var entidades = lista
                .Split(new[] { ';', ',', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            var invalidas = entidades.Where(x => !IdentificadorValido.IsMatch(x)).ToList();
            if (invalidas.Any())
            {
                throw new ArgumentException(
                    "Nome de entidade inválido: " + string.Join(", ", invalidas) +
                    ". Use apenas letras, números e _, começando por letra.");
            }

            return entidades;
        }

        /// <summary>
        /// Gera os seis arquivos de cada entidade da configuração.
        /// </summary>
        public IList<ArquivoGerado> Gerar(Configuracao config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            var arquivos = new List<ArquivoGerado>();
            foreach (var entidade in ParseEntidades(config.Entidades))
            {
                arquivos.AddRange(GerarEntidade(config, entidade));
            }

            return arquivos;
        }

        /// <summary>
        /// Gera os arquivos de uma única entidade, na ordem:
        /// IRepository, Repository, IService, Service, IApplication, Application.
        /// </summary>
        public IList<ArquivoGerado> GerarEntidade(Configuracao config, string entidade)
        {
            return new List<ArquivoGerado>
            {
                InterfaceRepository(config, entidade),
                Repository(config, entidade),
                InterfaceService(config, entidade),
                Service(config, entidade),
                InterfaceApplication(config, entidade),
                Application(config, entidade)
            };
        }

        private static ArquivoGerado InterfaceRepository(Configuracao c, string e)
        {
            var codigo = new Codigo()
                .Using(c.UsingEntities)
                .Linha()
                .Linha("namespace {0}", c.UsingInterfaceRepository)
                .Linha("{")
                .Linha("    public interface I{0}{1} : I{1}{2}<{0}>", e, c.SufixoRepository, c.SufixoBase)
                .Linha("    {")
                .Linha("    }")
                .Linha("}");

            return Arquivo(c.DirInterfaceRepository, "I" + e + c.SufixoRepository, codigo);
        }

        private static ArquivoGerado Repository(Configuracao c, string e)
        {
            var codigo = new Codigo()
                .Using(c.UsingEntities)
                .Using(c.UsingInterfaceRepository)
                .Linha()
                .Linha("namespace {0}", c.UsingRepository)
                .Linha("{")
                .Linha("    public class {0}{1} : {1}{2}<{0}>, I{0}{1}", e, c.SufixoRepository, c.SufixoBase)
                .Linha("    {")
                .Linha("    }")
                .Linha("}");

            return Arquivo(c.DirRepository, e + c.SufixoRepository, codigo);
        }

        private static ArquivoGerado InterfaceService(Configuracao c, string e)
        {
            var codigo = new Codigo()
                .Using(c.UsingEntities)
                .Linha()
                .Linha("namespace {0}", c.UsingInterfaceService)
                .Linha("{")
                .Linha("    public interface I{0}{1} : I{1}{2}<{0}>", e, c.SufixoService, c.SufixoBase)
                .Linha("    {")
                .Linha("    }")
                .Linha("}");

            return Arquivo(c.DirInterfaceService, "I" + e + c.SufixoService, codigo);
        }

        private static ArquivoGerado Service(Configuracao c, string e)
        {
            var campo = NomeVariavel(e) + c.SufixoRepository;
            var codigo = new Codigo()
                .Using(c.UsingEntities)
                .Using(c.UsingInterfaceRepository)
                .Using(c.UsingInterfaceService)
                .Linha()
                .Linha("namespace {0}", c.UsingService)
                .Linha("{")
                .Linha("    public class {0}{1} : {1}{2}<{0}>, I{0}{1}", e, c.SufixoService, c.SufixoBase)
                .Linha("    {")
                .Linha("        private readonly I{0}{1} _{2};", e, c.SufixoRepository, campo)
                .Linha()
                .Linha("        public {0}{1}(I{0}{2} {3})", e, c.SufixoService, c.SufixoRepository, campo)
                .Linha("            : base({0})", campo)
                .Linha("        {")
                .Linha("            _{0} = {0};", campo)
                .Linha("        }")
                .Linha("    }")
                .Linha("}");

            return Arquivo(c.DirService, e + c.SufixoService, codigo);
        }

        private static ArquivoGerado InterfaceApplication(Configuracao c, string e)
        {
            var codigo = new Codigo()
                .Using(c.UsingEntities)
                .Linha()
                .Linha("namespace {0}", c.UsingInterfaceApplication)
                .Linha("{")
                .Linha("    public interface I{0}{1} : I{1}{2}<{0}>", e, c.SufixoApplication, c.SufixoBase)
                .Linha("    {")
                .Linha("    }")
                .Linha("}");

            return Arquivo(c.DirInterfaceApplication, "I" + e + c.SufixoApplication, codigo);
        }

        private static ArquivoGerado Application(Configuracao c, string e)
        {
            var campo = NomeVariavel(e) + c.SufixoService;
            var codigo = new Codigo()
                .Using(c.UsingInterfaceApplication)
                .Using(c.UsingEntities)
                .Using(c.UsingInterfaceService)
                .Linha()
                .Linha("namespace {0}", c.UsingApplication)
                .Linha("{")
                .Linha("    public class {0}{1} : {1}{2}<{0}>, I{0}{1}", e, c.SufixoApplication, c.SufixoBase)
                .Linha("    {")
                .Linha("        private readonly I{0}{1} _{2};", e, c.SufixoService, campo)
                .Linha()
                .Linha("        public {0}{1}(I{0}{2} {3})", e, c.SufixoApplication, c.SufixoService, campo)
                .Linha("            : base({0})", campo)
                .Linha("        {")
                .Linha("            _{0} = {0};", campo)
                .Linha("        }")
                .Linha("    }")
                .Linha("}");

            return Arquivo(c.DirApplication, e + c.SufixoApplication, codigo);
        }

        /// <summary>Primeira letra minúscula: Cliente → cliente.</summary>
        public static string NomeVariavel(string entidade)
        {
            return char.ToLowerInvariant(entidade[0]) + entidade.Substring(1);
        }

        private static ArquivoGerado Arquivo(string diretorio, string nomeClasse, Codigo codigo)
        {
            var caminho = Path.Combine(diretorio ?? string.Empty, nomeClasse + ".cs");
            return new ArquivoGerado(caminho, codigo.ToString());
        }

        /// <summary>Monta o código linha a linha, sem espaços sobrando no fim.</summary>
        private class Codigo
        {
            private readonly StringBuilder _sb = new StringBuilder();

            public Codigo Using(string ns)
            {
                return string.IsNullOrWhiteSpace(ns) ? this : Linha("using {0};", ns.Trim());
            }

            public Codigo Linha()
            {
                _sb.AppendLine();
                return this;
            }

            public Codigo Linha(string formato, params object[] args)
            {
                _sb.AppendLine(args.Length == 0 ? formato : string.Format(formato, args));
                return this;
            }

            public override string ToString()
            {
                return _sb.ToString();
            }
        }
    }
}

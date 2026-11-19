using GeradorArquivoDDD.Dominio;
using NUnit.Framework;

namespace GeradorArquivoDDD.Tests
{
    [TestFixture]
    public class ConfiguracaoXmlTests
    {
        [Test]
        public void SerializarEDesserializar_PreservaOsCampos()
        {
            var original = new Configuracao
            {
                DirRepository = @"C:\projeto\Infra\Repositories",
                UsingEntities = "Loja.Domain.Entities",
                SufixoRepository = "Repository",
                SufixoBase = "Base",
                Entidades = "Cliente;Produto"
            };

            var xml = Global.Instance.Serializar(original);
            var copia = Global.Instance.FromXml<Configuracao>(xml);

            Assert.That(copia, Is.Not.Null);
            Assert.That(copia.DirRepository, Is.EqualTo(original.DirRepository));
            Assert.That(copia.UsingEntities, Is.EqualTo(original.UsingEntities));
            Assert.That(copia.SufixoRepository, Is.EqualTo(original.SufixoRepository));
            Assert.That(copia.SufixoBase, Is.EqualTo(original.SufixoBase));
            Assert.That(copia.Entidades, Is.EqualTo(original.Entidades));
        }

        [TestCase("isto não é xml")]
        [TestCase("<outro><elemento/></outro>")]
        public void FromXml_ConteudoInvalido_RetornaNulo(string conteudo)
        {
            Assert.That(Global.Instance.FromXml<Configuracao>(conteudo), Is.Null);
        }
    }
}

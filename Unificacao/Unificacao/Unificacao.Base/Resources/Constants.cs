using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furnas.GestaoSPE.Unificacao.Base.Resources
{
    public class Constants
    {
        public static class NameLibraryLegado
        {
            public const string DocsSPELegado = "Documentos SPE";
            public const string DocsSPENew = "Documento SPE";
            public const string ListSPE = "Lista Documentos SPE";
        }

        public static class NameSiteCollectionDocsDest
        {
            public const string SiteCollectionDest = "docs/furnas";
        }

        public static List<string> libraries()
        {
            List<string> libraries = new List<string>();

            libraries.Add("Documentos Contrato");
            libraries.Add("Documento Aporte SPE");
            libraries.Add("Anexo");
            libraries.Add("Documentos Acompanhamento Empreendimento");
            libraries.Add("Documentos Acompanhamento Obra");
            libraries.Add("Documentos Balanço Patrimonial");
            libraries.Add("Documento Dividendos SPE");
            libraries.Add("Documentos Empreendimento");
            libraries.Add("Documento Financiamento SPE");
            libraries.Add("Documentos Licença");
            libraries.Add("Documentos Obra");
            libraries.Add("Documento Pessoa");
            libraries.Add("Documentos Plano de Negócio");
            libraries.Add("Documentos Remuneração Global");
            libraries.Add("Documentos SPE");

            return libraries;
        }
    }
}

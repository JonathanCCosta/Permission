using Furnas.GestaoSPE.Unificacao.Base.Infrastruture;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furnas.GestaoSPE.Unificacao.Base.Service
{
    public class ServicoGrupoEspecifico
    {
        public ServicoGrupoEspecifico(string nome, SPWeb web)
        {
            _Nome = nome;
            _Web = web;
        }

        public ServicoGrupoEspecifico(SPItemEventProperties properties)
        {
            _Nome = Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Title")]);
            _NomeNovo = Convert.ToString(properties.ListItem[SPEncode.UrlDecodeAsUrl("Title")]);
            _Web = properties.Web;
        }

        public SPWeb _Web { get; set; }
        public string _Nome { get; set; }
        public string _NomeNovo { get; set; }

        public void AtualizaUsuarios(SPItemEventProperties properties)
        {

            List<SPUser> users = new List<SPUser>();

            string grupo = _NomeNovo;
            string grupoNovo = _Nome;

            SPGroup group = _Web.ParentWeb.SiteGroups[grupo];
            SPGroup groupNew = _Web.ParentWeb.SiteGroups[grupoNovo];

            foreach (SPUser user in group.Users)
            {
                groupNew.AddUser(user);
            }
        }

        public static void RemoverGrupo(SPWeb web, string nomeGrupo)
        {
            if (web.SiteGroups[nomeGrupo] != null)
            {
                web.SiteGroups.Remove(nomeGrupo);
            }
        }

        public bool ExisteGrupoEspecifico(SPGroupCollection groups, string name)
        {
            if (string.IsNullOrEmpty(name) ||
                (name.Length > 255) ||
                (groups == null) ||
                (groups.Count == 0))
                return false;
            else
                return (groups.GetCollection(new String[] { name }).Count > 0);
        }
    }
}

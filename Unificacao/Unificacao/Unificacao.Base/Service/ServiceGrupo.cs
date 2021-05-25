using Furnas.GestaoSPE.Unificacao.Base.Infrastruture;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furnas.GestaoSPE.Unificacao.Base.Service
{
    public class ServiceGrupo
    {
        public ServiceGrupo(string nome, SPWeb web)
        {
            _Nome = nome;
            _Web = web;
        }

        public ServiceGrupo(SPItemEventProperties properties)
        {
            _Web = properties.Web.ParentWeb;

            if (properties.EventType == SPEventReceiverType.ItemAdded || properties.EventType == SPEventReceiverType.ItemDeleting)
            {
                _Perfil = new SPFieldLookupValue(Convert.ToString(properties.ListItem["Perfil"])).LookupValue;
                _Empresa = new SPFieldLookupValue(Convert.ToString(properties.ListItem["Empresa"])).LookupValue;
                _Nome = properties.ListItem.Title;
                _NomeGrupo = _Empresa + " " + _Nome;
                
            }
            else if (properties.EventType == SPEventReceiverType.ItemAdding || properties.EventType == SPEventReceiverType.ItemUpdating)
            {
                _Perfil = ObterValorLookupParaItemAdding(Convert.ToInt32(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Perfil")]), "Perfil", properties.Web);
                //_Perfil = new SPFieldLookupValue(Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Perfil")])).LookupValue;]
                _Empresa = ObterValorLookupParaItemAdding(Convert.ToInt32(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Empresa")]), "Empresa", properties.Web);
                //_Empresa = new SPFieldLookupValue(Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Empresa")])).LookupValue;
                _Nome = Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Title")]);
                _NomeGrupoAfterProperties = _Empresa + " " + _Nome;
               
            }
        }

        public string _NomeGrupo { get; set; }
        public string _NomeGrupoAfterProperties { get; set; }
        public string _Nome { get; set; }
        public string _Perfil { get; set; }
        public string _Empresa { get; set; }
        public SPWeb _Web { get; set; }

        private string ObterValorLookupParaItemAdding(int id, string nomeLista, SPWeb web)
        {
            SPList list = web.Lists[nomeLista];
            SPQuery query = new SPQuery();
            query.Query = "<Where><Eq><FieldRef Name='ID' /><Value Type='Counter'>" + id + "</Value></Eq></Where>";
            query.ViewFields = "<FieldRef Name='Title' />";

            SPListItemCollectionPosition collPoss;
            DataTable table = list.GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);

            if (table != null)
                return table.Rows[0].Field<string>("Title");
            else
                return "";
        }

        /// <summary>
        /// Adiciona os usuários no grupo.
        /// </summary>
        /// <param name="Campo">Campo do tipo pessoa ou grupo onde os usuários foram inseridos.</param>
        public void AddUsuarios(SPListItem Campo)
        {
            string usuariosSomenteLeitura = Convert.ToString(Campo["Usuarios"]);
            SPFieldUserValueCollection UsuariosGrupoLeitura = new SPFieldUserValueCollection(_Web, usuariosSomenteLeitura);
            foreach (SPFieldUserValue userValue in UsuariosGrupoLeitura)
            {
                if (userValue.User != null)
                    Seguranca.ConcederPermissao(_Web, userValue.User, _NomeGrupo);
            }
        }

        /// <summary>
        /// Uma vez que o grupo for criado, não é permitido alterar o nome dele.
        /// </summary>
        private void ValidaGrupo(SPItemEventProperties properties)
        {
            SPQuery query = new SPQuery();
            query.Query = "<Where><And><Eq><FieldRef Name='Title' /><Value Type='Text'>" + _Nome + "</Value></Eq><And><Eq><FieldRef Name='Perfil' LookupId='FALSE'/><Value Type='Lookup'>" + _Perfil + "</Value></Eq><Eq><FieldRef Name='Empresa' LookupId='FALSE'/><Value Type='Lookup'>" + _Empresa + "</Value></Eq></And></And></Where>";
            query.ViewFields = "<FieldRef Name='Title' />";
            SPListItemCollectionPosition collPoss;
            DataTable table = properties.Web.Lists["Grupo"].GetDataTable(query, SPListGetDataTableOptions.None, out collPoss);

            //Se for alteração do item
            if (properties.EventType == SPEventReceiverType.ItemUpdating)
            {
                //if (string.Compare(_NomeGrupo, _NomeGrupoAfterProperties, false) != 0)
                //    throw new Exception("Não é permitido alterar o nome do Grupo.");

                if (table != null && table.Rows.Count > 1)
                    throw new Exception("Existe um grupo cadastrado com este nome para a mesma empresa. Favor alterar o nome e tentar novamente.");
            }
            else if (properties.EventType == SPEventReceiverType.ItemAdding)
            {
                //Verifica se ja existe uma empresa cadastrada
                if (table != null && table.Rows.Count > 0)
                    throw new Exception("Existe um grupo cadastrado com este nome para a mesma empresa. Favor alterar o nome e tentar novamente.");
            }
        }

        /// <summary>
        /// Atualiza os usuários do grupo
        /// </summary>
        /// <param name="properties"></param>
        public void AtualizaUsuarios(SPItemEventProperties properties)
        {
            ValidaGrupo(properties);

            string valorAtualCampo = Convert.ToString(properties.ListItem[SPEncode.UrlDecodeAsUrl("Usuarios")]);
            SPFieldUserValueCollection colecaoUsuariosAtuais = new SPFieldUserValueCollection(_Web, valorAtualCampo);

            string valorNovoDoCampo = Convert.ToString(properties.AfterProperties[SPEncode.UrlDecodeAsUrl("Usuarios")]);
            SPFieldUserValueCollection colecaoTodosUsuarios = new SPFieldUserValueCollection(_Web, valorNovoDoCampo);

            string grupo = _NomeGrupo == null ? _NomeGrupoAfterProperties : _NomeGrupo;

            AtualizaUsuarios(colecaoUsuariosAtuais, colecaoTodosUsuarios, grupo);
        }

        private void AtualizaUsuarios(SPFieldUserValueCollection colecaoUsuariosAtuais, SPFieldUserValueCollection colecaoTodosUsuarios, string nomeGrupo)
        {
            string idsUsuariosAnteriores = string.Empty;
            foreach (SPFieldUserValue userValue in colecaoUsuariosAtuais)
            {
                if (userValue.User.LoginName.Contains("|"))
                    idsUsuariosAnteriores += _Web.EnsureUser(userValue.User.LoginName.Split('|')[1]).ID;
                else
                    idsUsuariosAnteriores += userValue.LookupId;
            }

            string idsUsuariosNovos = string.Empty;
            foreach (SPFieldUserValue userValue in colecaoTodosUsuarios)
            {
                if (userValue.User != null && userValue.User.LoginName.Contains("|"))
                    idsUsuariosNovos += _Web.EnsureUser(userValue.User.LoginName.Split('|')[1]).ID;
                else if (userValue.ToString().Contains("|"))
                    idsUsuariosNovos += _Web.EnsureUser(userValue.ToString().Split('|')[1]).ID;
                else
                {
                    if (userValue.LookupId > 0)
                        idsUsuariosNovos += userValue.LookupId;
                }
            }

            if (idsUsuariosAnteriores != idsUsuariosNovos)
            {
                SPSecurity.RunWithElevatedPrivileges(delegate()
                {
                    using (SPSite site = new SPSite(_Web.Url))
                    {
                        using (SPWeb web = site.OpenWeb())
                        {
                            foreach (SPFieldUserValue userValue in colecaoUsuariosAtuais)
                            {
                                SPUser user = web.SiteUsers.GetByID(userValue.LookupId);
                                Seguranca.RemoverPermissao(web, user, nomeGrupo);
                            }

                            foreach (SPFieldUserValue userValue in colecaoTodosUsuarios)
                            {
                                SPUser user = null;
                                if (userValue.User != null && userValue.User.LoginName.Contains("|"))
                                    user = web.EnsureUser(userValue.User.LoginName.Split('|')[1]);
                                else if (userValue.ToString().Contains("|"))
                                    user = web.EnsureUser(userValue.ToString().Split('|')[1]);
                                else
                                    user = web.SiteUsers.GetByID(userValue.LookupId);

                                Seguranca.ConcederPermissao(web, user, nomeGrupo);
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Verifica se existe algum grupo de acesso para esta empresa
        /// </summary>
        /// <returns></returns>
        public bool GrupoExiste(SPItemEventProperties properties)
        {
            ValidaGrupo(properties);

            SPGroupCollection collGroups = _Web.SiteGroups;
            string grupo = _NomeGrupo == string.Empty ? _NomeGrupoAfterProperties : _NomeGrupo;
            bool grupoExiste = Seguranca.GrupoExiste(collGroups, grupo);
            return grupoExiste;
        }

        public SPRoleType Perfil()
        {
            switch (_Perfil)
            {
                case "Leitura":
                    return SPRoleType.Reader;
                case "Colaboração":
                    return SPRoleType.Contributor;
                case "Administrador":
                    return SPRoleType.Administrator;
                default:
                    return SPRoleType.None;
            }
        }
    }
}
